using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.TeamChat
{
    /// <summary>
    /// Service that manages Server-Sent Events (SSE) connections to Firebase Realtime Database.
    /// When a message arrives from Firebase, it broadcasts to local SignalR clients.
    /// This enables cross-PC communication without Redis backplane.
    /// </summary>
    public class FirebaseStreamingService : IDisposable
    {
        private readonly ILogger<FirebaseStreamingService> _logger;
        private readonly IHubContext<TeamChatHub> _hubContext;
        private readonly HttpClient _httpClient;
        private readonly string _firebaseDatabaseUrl;

        // Track active SSE connections per room for messages: roomId -> RoomSubscription
        private readonly ConcurrentDictionary<string, RoomSubscription> _roomSubscriptions = new();

        // Track active SSE connections for project users: roomId -> RoomSubscription
        private readonly ConcurrentDictionary<string, RoomSubscription> _projectUsersSubscriptions = new();

        // Track active SSE connections for presence: roomId -> RoomSubscription
        private readonly ConcurrentDictionary<string, RoomSubscription> _presenceSubscriptions = new();

        // Track last seen message timestamp per room to avoid duplicates
        private readonly ConcurrentDictionary<string, long> _lastMessageTimestamp = new();

        // Lock for subscription management
        private readonly object _subscriptionLock = new();

        public FirebaseStreamingService(
            ILogger<FirebaseStreamingService> logger,
            IHubContext<TeamChatHub> hubContext,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _hubContext = hubContext;
            _httpClient = httpClientFactory.CreateClient("FirebaseStreaming");

            // Set timeout to infinite for SSE streaming
            _httpClient.Timeout = Timeout.InfiniteTimeSpan;

            _firebaseDatabaseUrl = configuration["Firebase:DatabaseUrl"]
                ?? "https://mdexplorer-chat-default-rtdb.europe-west1.firebasedatabase.app";

            _logger.LogInformation("FirebaseStreamingService initialized with URL: {Url}", _firebaseDatabaseUrl);
        }

        /// <summary>
        /// Subscribe to a room's messages via SSE.
        /// Called only by TeamChatService when the first client joins a room.
        /// </summary>
        public async Task SubscribeToRoom(string roomId)
        {
            RoomSubscription subscription;
            bool needsNewTask = false;

            lock (_subscriptionLock)
            {
                if (_roomSubscriptions.TryGetValue(roomId, out var existing))
                {
                    // Already subscribed - check if task is dead and needs restart
                    if (!existing.IsTaskRunning || existing.ListenerTask?.IsCompleted == true)
                    {
                        _logger.LogWarning("SSE task for room {RoomId} was dead, restarting", roomId);
                        existing.CancellationTokenSource?.Dispose();
                        existing.CancellationTokenSource = new CancellationTokenSource();
                        existing.IsTaskRunning = true;
                        needsNewTask = true;
                        subscription = existing;
                    }
                    else
                    {
                        // Already active, nothing to do
                        return;
                    }
                }
                else
                {
                    // Create new subscription
                    subscription = new RoomSubscription
                    {
                        CancellationTokenSource = new CancellationTokenSource(),
                        IsTaskRunning = true
                    };
                    _roomSubscriptions[roomId] = subscription;
                    needsNewTask = true;

                    // Initialize last timestamp to now to avoid receiving old messages
                    _lastMessageTimestamp[roomId] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }

            // Start SSE listener outside the lock
            if (needsNewTask)
            {
                subscription.ListenerTask = Task.Run(() => ListenToFirebaseSSE(roomId));
                _logger.LogInformation("Started SSE listener task for room {RoomId}", roomId);
            }
        }

        /// <summary>
        /// Unsubscribe from a room and close the SSE connection.
        /// Called only by TeamChatService when the last client leaves a room.
        /// </summary>
        public void UnsubscribeFromRoom(string roomId)
        {
            lock (_subscriptionLock)
            {
                if (_roomSubscriptions.TryRemove(roomId, out var subscription))
                {
                    subscription.CancellationTokenSource?.Cancel();
                    subscription.IsTaskRunning = false;
                    _lastMessageTimestamp.TryRemove(roomId, out _);
                    _logger.LogInformation("Closed SSE subscription for room {RoomId}", roomId);
                }
            }
        }

        /// <summary>
        /// Listen to Firebase SSE stream for a specific room
        /// </summary>
        private async Task ListenToFirebaseSSE(string roomId)
        {
            RoomSubscription subscription;
            CancellationToken cancellationToken;

            lock (_subscriptionLock)
            {
                if (!_roomSubscriptions.TryGetValue(roomId, out subscription))
                {
                    return;
                }
                cancellationToken = subscription.CancellationTokenSource.Token;
            }

            var retryCount = 0;
            const int maxRetries = 10;
            const int baseDelayMs = 1000;

            try
            {
                while (!cancellationToken.IsCancellationRequested && retryCount < maxRetries)
                {
                    try
                    {
                        var url = $"{_firebaseDatabaseUrl}/chatRooms/{roomId}/messages.json";

                        using var request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

                        using var response = await _httpClient.SendAsync(
                            request,
                            HttpCompletionOption.ResponseHeadersRead,
                            cancellationToken);

                        response.EnsureSuccessStatusCode();

                        using var stream = await response.Content.ReadAsStreamAsync();
                        using var reader = new StreamReader(stream);

                        _logger.LogInformation("SSE connection established for room {RoomId}", roomId);
                        retryCount = 0; // Reset retry count on successful connection

                        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                        {
                            var line = await reader.ReadLineAsync();

                            if (string.IsNullOrEmpty(line))
                            {
                                continue;
                            }

                            await ProcessSSELine(roomId, line);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogDebug("SSE connection cancelled for room {RoomId}", roomId);
                        break;
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        var delay = Math.Min(baseDelayMs * (int)Math.Pow(2, retryCount), 30000);

                        _logger.LogWarning(ex,
                            "SSE connection error for room {RoomId}, retry {RetryCount}/{MaxRetries} in {Delay}ms",
                            roomId, retryCount, maxRetries, delay);

                        if (retryCount < maxRetries && !cancellationToken.IsCancellationRequested)
                        {
                            try
                            {
                                await Task.Delay(delay, cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                // Mark task as not running when it exits
                lock (_subscriptionLock)
                {
                    if (_roomSubscriptions.TryGetValue(roomId, out var sub))
                    {
                        sub.IsTaskRunning = false;
                        _logger.LogInformation("SSE listener stopped for room {RoomId}", roomId);
                    }
                }
            }
        }

        /// <summary>
        /// Process a line from the SSE stream
        /// </summary>
        private async Task ProcessSSELine(string roomId, string line)
        {
            // Firebase SSE format:
            // event: put
            // data: {"path":"/messageId","data":{...}}

            if (!line.StartsWith("data:"))
            {
                return;
            }

            var jsonData = line.Substring(5).Trim();

            if (string.IsNullOrEmpty(jsonData) || jsonData == "null")
            {
                return;
            }

            try
            {
                using var doc = JsonDocument.Parse(jsonData);
                var root = doc.RootElement;

                // Check if this is a "put" event with message data
                if (!root.TryGetProperty("path", out var pathElement) ||
                    !root.TryGetProperty("data", out var dataElement))
                {
                    return;
                }

                var path = pathElement.GetString();

                // Skip if path is "/" (initial data load) or data is null
                if (path == "/" || dataElement.ValueKind == JsonValueKind.Null)
                {
                    // Initial load - process all messages if it's an object
                    if (path == "/" && dataElement.ValueKind == JsonValueKind.Object)
                    {
                        await ProcessInitialMessages(roomId, dataElement);
                    }
                    return;
                }

                // Extract message ID from path (e.g., "/-ONotABC123")
                var messageId = path.TrimStart('/');

                // Parse the message data
                var message = ParseMessageFromJson(messageId, dataElement);

                if (message != null)
                {
                    await BroadcastMessageIfNew(roomId, message);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogDebug(ex, "Failed to parse SSE JSON for room {RoomId}: {Data}", roomId, jsonData);
            }
        }

        /// <summary>
        /// Process initial batch of messages when SSE connection is established
        /// </summary>
        private async Task ProcessInitialMessages(string roomId, JsonElement dataElement)
        {
            // We don't broadcast initial messages - they're already loaded via REST API
            // Just update the last timestamp to the newest message
            _lastMessageTimestamp.TryGetValue(roomId, out long maxTimestamp);

            foreach (var prop in dataElement.EnumerateObject())
            {
                if (prop.Value.TryGetProperty("timestamp", out var timestampElement))
                {
                    var timestamp = timestampElement.GetInt64();
                    if (timestamp > maxTimestamp)
                    {
                        maxTimestamp = timestamp;
                    }
                }
            }

            _lastMessageTimestamp[roomId] = maxTimestamp;
            _logger.LogDebug("Updated last timestamp for room {RoomId} to {Timestamp}", roomId, maxTimestamp);
        }

        /// <summary>
        /// Parse a chat message from JSON element
        /// </summary>
        private ChatMessageDto ParseMessageFromJson(string messageId, JsonElement element)
        {
            try
            {
                return new ChatMessageDto
                {
                    Id = messageId,
                    Content = element.TryGetProperty("content", out var c) ? c.GetString() : "",
                    SenderName = element.TryGetProperty("senderName", out var sn) ? sn.GetString() : "",
                    SenderEmail = element.TryGetProperty("senderEmail", out var se) ? se.GetString() : "",
                    Timestamp = element.TryGetProperty("timestamp", out var t) ? t.GetInt64() : 0,
                    Type = element.TryGetProperty("type", out var ty) ? ty.GetString() : "message"
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to parse message {MessageId}", messageId);
                return null;
            }
        }

        /// <summary>
        /// Broadcast message to SignalR clients if it's new (not already seen)
        /// </summary>
        private async Task BroadcastMessageIfNew(string roomId, ChatMessageDto message)
        {
            // Check if this is a new message (timestamp > last seen)
            _lastMessageTimestamp.TryGetValue(roomId, out long lastTimestamp);

            if (message.Timestamp <= lastTimestamp)
            {
                // Already seen this message or older
                return;
            }

            // Update last seen timestamp
            _lastMessageTimestamp[roomId] = message.Timestamp;

            // Broadcast to all clients in this room via SignalR
            await _hubContext.Clients.Group(roomId).SendAsync("ReceiveMessage", message);

            _logger.LogDebug("Broadcasted message {MessageId} to room {RoomId} via SSE",
                message.Id, roomId);
        }

        /// <summary>
        /// Subscribe to project users changes via SSE.
        /// </summary>
        public async Task SubscribeToProjectUsers(string roomId)
        {
            RoomSubscription subscription;
            bool needsNewTask = false;

            lock (_subscriptionLock)
            {
                if (_projectUsersSubscriptions.TryGetValue(roomId, out var existing))
                {
                    if (!existing.IsTaskRunning || existing.ListenerTask?.IsCompleted == true)
                    {
                        _logger.LogWarning("Project users SSE task for room {RoomId} was dead, restarting", roomId);
                        existing.CancellationTokenSource?.Dispose();
                        existing.CancellationTokenSource = new CancellationTokenSource();
                        existing.IsTaskRunning = true;
                        needsNewTask = true;
                        subscription = existing;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    subscription = new RoomSubscription
                    {
                        CancellationTokenSource = new CancellationTokenSource(),
                        IsTaskRunning = true
                    };
                    _projectUsersSubscriptions[roomId] = subscription;
                    needsNewTask = true;
                }
            }

            if (needsNewTask)
            {
                subscription.ListenerTask = Task.Run(() => ListenToFirebaseProjectUsersSSE(roomId));
                _logger.LogInformation("Started project users SSE listener for room {RoomId}", roomId);
            }
        }

        /// <summary>
        /// Unsubscribe from project users SSE.
        /// </summary>
        public void UnsubscribeFromProjectUsers(string roomId)
        {
            lock (_subscriptionLock)
            {
                if (_projectUsersSubscriptions.TryRemove(roomId, out var subscription))
                {
                    subscription.CancellationTokenSource?.Cancel();
                    subscription.IsTaskRunning = false;
                    _logger.LogInformation("Closed project users SSE subscription for room {RoomId}", roomId);
                }
            }
        }

        /// <summary>
        /// Listen to Firebase SSE stream for project users
        /// </summary>
        private async Task ListenToFirebaseProjectUsersSSE(string roomId)
        {
            RoomSubscription subscription;
            CancellationToken cancellationToken;

            lock (_subscriptionLock)
            {
                if (!_projectUsersSubscriptions.TryGetValue(roomId, out subscription))
                {
                    return;
                }
                cancellationToken = subscription.CancellationTokenSource.Token;
            }

            var retryCount = 0;
            const int maxRetries = 10;
            const int baseDelayMs = 1000;

            try
            {
                while (!cancellationToken.IsCancellationRequested && retryCount < maxRetries)
                {
                    try
                    {
                        var url = $"{_firebaseDatabaseUrl}/chatRooms/{roomId}/projectUsers.json";

                        using var request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

                        using var response = await _httpClient.SendAsync(
                            request,
                            HttpCompletionOption.ResponseHeadersRead,
                            cancellationToken);

                        response.EnsureSuccessStatusCode();

                        using var stream = await response.Content.ReadAsStreamAsync();
                        using var reader = new StreamReader(stream);

                        _logger.LogInformation("Project users SSE connection established for room {RoomId}", roomId);
                        retryCount = 0;

                        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                        {
                            var line = await reader.ReadLineAsync();

                            if (string.IsNullOrEmpty(line))
                            {
                                continue;
                            }

                            await ProcessProjectUsersSSELine(roomId, line);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogDebug("Project users SSE connection cancelled for room {RoomId}", roomId);
                        break;
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        var delay = Math.Min(baseDelayMs * (int)Math.Pow(2, retryCount), 30000);

                        _logger.LogWarning(ex,
                            "Project users SSE error for room {RoomId}, retry {RetryCount}/{MaxRetries} in {Delay}ms",
                            roomId, retryCount, maxRetries, delay);

                        if (retryCount < maxRetries && !cancellationToken.IsCancellationRequested)
                        {
                            try
                            {
                                await Task.Delay(delay, cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                lock (_subscriptionLock)
                {
                    if (_projectUsersSubscriptions.TryGetValue(roomId, out var sub))
                    {
                        sub.IsTaskRunning = false;
                        _logger.LogInformation("Project users SSE listener stopped for room {RoomId}", roomId);
                    }
                }
            }
        }

        /// <summary>
        /// Process a line from the project users SSE stream
        /// </summary>
        private async Task ProcessProjectUsersSSELine(string roomId, string line)
        {
            if (!line.StartsWith("data:"))
            {
                return;
            }

            var jsonData = line.Substring(5).Trim();

            if (string.IsNullOrEmpty(jsonData) || jsonData == "null")
            {
                // No project users - broadcast 0
                await _hubContext.Clients.Group(roomId).SendAsync("ProjectUsersCountUpdate", 0);
                return;
            }

            try
            {
                using var doc = JsonDocument.Parse(jsonData);
                var root = doc.RootElement;

                // Count the number of users
                int count = 0;

                if (root.TryGetProperty("data", out var dataElement))
                {
                    if (dataElement.ValueKind == JsonValueKind.Object)
                    {
                        count = CountJsonProperties(dataElement);
                    }
                    else if (dataElement.ValueKind == JsonValueKind.Null)
                    {
                        count = 0;
                    }
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    // Initial data load - the root is the data
                    count = CountJsonProperties(root);
                }

                // Broadcast the count to all clients in the room
                await _hubContext.Clients.Group(roomId).SendAsync("ProjectUsersCountUpdate", count);
                _logger.LogDebug("Broadcasted project users count {Count} to room {RoomId}", count, roomId);
            }
            catch (JsonException ex)
            {
                _logger.LogDebug(ex, "Failed to parse project users SSE JSON for room {RoomId}", roomId);
            }
        }

        /// <summary>
        /// Count the number of properties in a JSON object
        /// </summary>
        private int CountJsonProperties(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object)
                return 0;

            int count = 0;
            foreach (var _ in element.EnumerateObject())
            {
                count++;
            }
            return count;
        }

        /// <summary>
        /// Subscribe to presence changes via SSE.
        /// </summary>
        public async Task SubscribeToRoomPresence(string roomId)
        {
            RoomSubscription subscription;
            bool needsNewTask = false;

            lock (_subscriptionLock)
            {
                if (_presenceSubscriptions.TryGetValue(roomId, out var existing))
                {
                    if (!existing.IsTaskRunning || existing.ListenerTask?.IsCompleted == true)
                    {
                        _logger.LogWarning("Presence SSE task for room {RoomId} was dead, restarting", roomId);
                        existing.CancellationTokenSource?.Dispose();
                        existing.CancellationTokenSource = new CancellationTokenSource();
                        existing.IsTaskRunning = true;
                        needsNewTask = true;
                        subscription = existing;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    subscription = new RoomSubscription
                    {
                        CancellationTokenSource = new CancellationTokenSource(),
                        IsTaskRunning = true
                    };
                    _presenceSubscriptions[roomId] = subscription;
                    needsNewTask = true;
                }
            }

            if (needsNewTask)
            {
                subscription.ListenerTask = Task.Run(() => ListenToFirebasePresenceSSE(roomId));
                _logger.LogInformation("Started presence SSE listener for room {RoomId}", roomId);
            }
        }

        /// <summary>
        /// Unsubscribe from presence SSE.
        /// </summary>
        public void UnsubscribeFromRoomPresence(string roomId)
        {
            lock (_subscriptionLock)
            {
                if (_presenceSubscriptions.TryRemove(roomId, out var subscription))
                {
                    subscription.CancellationTokenSource?.Cancel();
                    subscription.IsTaskRunning = false;
                    _logger.LogInformation("Closed presence SSE subscription for room {RoomId}", roomId);
                }
            }
        }

        /// <summary>
        /// Listen to Firebase SSE stream for presence
        /// </summary>
        private async Task ListenToFirebasePresenceSSE(string roomId)
        {
            RoomSubscription subscription;
            CancellationToken cancellationToken;

            lock (_subscriptionLock)
            {
                if (!_presenceSubscriptions.TryGetValue(roomId, out subscription))
                {
                    return;
                }
                cancellationToken = subscription.CancellationTokenSource.Token;
            }

            var retryCount = 0;
            const int maxRetries = 10;
            const int baseDelayMs = 1000;

            try
            {
                while (!cancellationToken.IsCancellationRequested && retryCount < maxRetries)
                {
                    try
                    {
                        var url = $"{_firebaseDatabaseUrl}/chatRooms/{roomId}/presence.json";

                        using var request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

                        using var response = await _httpClient.SendAsync(
                            request,
                            HttpCompletionOption.ResponseHeadersRead,
                            cancellationToken);

                        response.EnsureSuccessStatusCode();

                        using var stream = await response.Content.ReadAsStreamAsync();
                        using var reader = new StreamReader(stream);

                        _logger.LogInformation("Presence SSE connection established for room {RoomId}", roomId);
                        retryCount = 0;

                        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                        {
                            var line = await reader.ReadLineAsync();

                            if (string.IsNullOrEmpty(line))
                            {
                                continue;
                            }

                            await ProcessPresenceSSELine(roomId, line);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogDebug("Presence SSE connection cancelled for room {RoomId}", roomId);
                        break;
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        var delay = Math.Min(baseDelayMs * (int)Math.Pow(2, retryCount), 30000);

                        _logger.LogWarning(ex,
                            "Presence SSE error for room {RoomId}, retry {RetryCount}/{MaxRetries} in {Delay}ms",
                            roomId, retryCount, maxRetries, delay);

                        if (retryCount < maxRetries && !cancellationToken.IsCancellationRequested)
                        {
                            try
                            {
                                await Task.Delay(delay, cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                lock (_subscriptionLock)
                {
                    if (_presenceSubscriptions.TryGetValue(roomId, out var sub))
                    {
                        sub.IsTaskRunning = false;
                        _logger.LogInformation("Presence SSE listener stopped for room {RoomId}", roomId);
                    }
                }
            }
        }

        /// <summary>
        /// Process a line from the presence SSE stream
        /// </summary>
        private async Task ProcessPresenceSSELine(string roomId, string line)
        {
            if (!line.StartsWith("data:"))
            {
                return;
            }

            var jsonData = line.Substring(5).Trim();

            if (string.IsNullOrEmpty(jsonData) || jsonData == "null")
            {
                // No users - broadcast empty presence
                await _hubContext.Clients.Group(roomId).SendAsync("PresenceUpdate", new PresenceInfoDto
                {
                    Users = new List<ChatUserInfo>(),
                    TotalOnline = 0
                });
                return;
            }

            try
            {
                using var doc = JsonDocument.Parse(jsonData);
                var root = doc.RootElement;

                var users = new List<ChatUserInfo>();
                JsonElement dataElement;

                // Firebase SSE can send data in different formats
                if (root.TryGetProperty("data", out dataElement))
                {
                    // Update event with path and data
                    if (dataElement.ValueKind == JsonValueKind.Object)
                    {
                        users = ParseUsersFromJson(dataElement);
                    }
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    // Initial data load - root is the data
                    users = ParseUsersFromJson(root);
                }

                var presence = new PresenceInfoDto
                {
                    Users = users,
                    TotalOnline = users.Count
                };

                // Broadcast to all clients in the room
                await _hubContext.Clients.Group(roomId).SendAsync("PresenceUpdate", presence);
                _logger.LogDebug("Broadcasted presence update to room {RoomId}: {Count} users", roomId, users.Count);
            }
            catch (JsonException ex)
            {
                _logger.LogDebug(ex, "Failed to parse presence SSE JSON for room {RoomId}", roomId);
            }
        }

        /// <summary>
        /// Parse users from JSON element
        /// </summary>
        private List<ChatUserInfo> ParseUsersFromJson(JsonElement element)
        {
            var users = new List<ChatUserInfo>();

            if (element.ValueKind != JsonValueKind.Object)
                return users;

            foreach (var prop in element.EnumerateObject())
            {
                try
                {
                    var userElement = prop.Value;
                    if (userElement.ValueKind == JsonValueKind.Object)
                    {
                        var user = new ChatUserInfo
                        {
                            OderId = userElement.TryGetProperty("oderId", out var o) ? o.GetString() : prop.Name,
                            UserId = userElement.TryGetProperty("userId", out var ui) ? ui.GetString() : "",
                            UserName = userElement.TryGetProperty("userName", out var un) ? un.GetString() : "Unknown",
                            UserEmail = userElement.TryGetProperty("userEmail", out var ue) ? ue.GetString() : "",
                            Online = userElement.TryGetProperty("online", out var on) && on.GetBoolean(),
                            LastSeen = userElement.TryGetProperty("lastSeen", out var ls) ? ls.GetInt64() : 0
                        };

                        if (user.Online)
                        {
                            users.Add(user);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to parse user from presence data");
                }
            }

            return users;
        }

        public void Dispose()
        {
            foreach (var subscription in _roomSubscriptions.Values)
            {
                subscription.CancellationTokenSource.Cancel();
                subscription.CancellationTokenSource.Dispose();
            }
            _roomSubscriptions.Clear();

            foreach (var subscription in _projectUsersSubscriptions.Values)
            {
                subscription.CancellationTokenSource.Cancel();
                subscription.CancellationTokenSource.Dispose();
            }
            _projectUsersSubscriptions.Clear();

            foreach (var subscription in _presenceSubscriptions.Values)
            {
                subscription.CancellationTokenSource.Cancel();
                subscription.CancellationTokenSource.Dispose();
            }
            _presenceSubscriptions.Clear();

            _lastMessageTimestamp.Clear();
        }

        private class RoomSubscription
        {
            public CancellationTokenSource CancellationTokenSource { get; set; }
            public bool IsTaskRunning { get; set; }
            public Task ListenerTask { get; set; }
        }
    }
}
