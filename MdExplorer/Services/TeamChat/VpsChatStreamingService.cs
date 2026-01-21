using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SocketIOClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace MdExplorer.Services.TeamChat
{
    /// <summary>
    /// Service that manages WebSocket connections to VPS Chat Server.
    /// When a message arrives from VPS, it broadcasts to local SignalR clients.
    /// This enables cross-PC communication without Redis backplane.
    /// </summary>
    public class VpsChatStreamingService : IDisposable
    {
        private readonly ILogger<VpsChatStreamingService> _logger;
        private readonly IHubContext<TeamChatHub> _hubContext;
        private readonly string _vpsWebSocketUrl;
        private readonly string _apiKey;

        // Single Socket.io connection (shared across all rooms)
        private SocketIOClient.SocketIO _socket;
        private readonly object _socketLock = new();
        private bool _isConnected;
        private TaskCompletionSource<bool> _connectionTcs;

        // Track subscribed rooms per channel type
        private readonly ConcurrentDictionary<string, HashSet<string>> _subscribedRooms = new()
        {
            ["messages"] = new HashSet<string>(),
            ["presence"] = new HashSet<string>(),
            ["project-users"] = new HashSet<string>()
        };

        // Track seen message IDs per room to avoid duplicates
        private readonly ConcurrentDictionary<string, HashSet<string>> _seenMessageIds = new();
        private readonly ConcurrentDictionary<string, object> _seenMessageLocks = new();

        public VpsChatStreamingService(
            ILogger<VpsChatStreamingService> logger,
            IHubContext<TeamChatHub> hubContext,
            IConfiguration configuration)
        {
            _logger = logger;
            _hubContext = hubContext;

            _vpsWebSocketUrl = configuration["MdChat:WebSocketUrl"]
                ?? "wss://errantia.net/mdchat";
            _apiKey = configuration["MdChat:ApiKey"]
                ?? throw new InvalidOperationException("MdChat:ApiKey is required");

            _logger.LogInformation("VpsChatStreamingService initialized with URL: {Url}", _vpsWebSocketUrl);
        }

        /// <summary>
        /// Ensure Socket.io connection is established
        /// </summary>
        private async Task EnsureConnectedAsync()
        {
            if (_isConnected && _socket?.Connected == true)
                return;

            TaskCompletionSource<bool> tcs;

            lock (_socketLock)
            {
                if (_isConnected && _socket?.Connected == true)
                    return;

                // If already connecting, wait for that connection
                if (_connectionTcs != null && !_connectionTcs.Task.IsCompleted)
                {
                    tcs = _connectionTcs;
                }
                else
                {
                    _socket?.Dispose();
                    _connectionTcs = new TaskCompletionSource<bool>();
                    tcs = _connectionTcs;

                    // SocketIOClient needs base URL (without /mdchat) and path set to /mdchat/socket.io
                    var baseUrl = _vpsWebSocketUrl
                        .Replace("wss://", "https://")
                        .Replace("ws://", "http://")
                        .Replace("/mdchat", ""); // Remove /mdchat from URL, will be in path

                    _logger.LogInformation("Connecting to VPS WebSocket - Base URL: {BaseUrl}, Path: /mdchat/socket.io", baseUrl);

                    _socket = new SocketIOClient.SocketIO(baseUrl, new SocketIOOptions
                    {
                        Path = "/mdchat/socket.io",
                        Auth = new Dictionary<string, string>
                        {
                            ["apiKey"] = _apiKey
                        },
                        ExtraHeaders = new Dictionary<string, string>
                        {
                            ["X-API-Key"] = _apiKey
                        },
                        Reconnection = true,
                        ReconnectionAttempts = 10,
                        ReconnectionDelay = 1000,
                        ReconnectionDelayMax = 30000,
                        ConnectionTimeout = TimeSpan.FromSeconds(30)
                        // Let it use default transport (polling first, then upgrade to websocket)
                    });

                    // Setup event handlers
                    _socket.OnConnected += OnConnected;
                    _socket.OnDisconnected += OnDisconnected;
                    _socket.OnError += OnError;
                    _socket.OnReconnectAttempt += (s, attempt) =>
                        _logger.LogWarning("Socket.io reconnect attempt #{Attempt}", attempt);
                    _socket.OnReconnectFailed += (s, e) =>
                        _logger.LogError("Socket.io reconnect failed");

                    // Message events
                    _socket.On("new_message", OnNewMessage);
                    _socket.On("presence_update", OnPresenceUpdate);
                    _socket.On("project_users_update", OnProjectUsersUpdate);

                    // Start connection (don't await, will complete via OnConnected)
                    _ = ConnectInternalAsync();
                }
            }

            // Wait for connection to complete
            try
            {
                var timeout = Task.Delay(TimeSpan.FromSeconds(15));
                var completed = await Task.WhenAny(tcs.Task, timeout);

                if (completed == timeout)
                {
                    _logger.LogError("Connection to VPS WebSocket timed out");
                    throw new TimeoutException("Connection to VPS WebSocket timed out");
                }

                if (!tcs.Task.Result)
                {
                    throw new Exception("Failed to connect to VPS WebSocket");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to VPS WebSocket at {Url}", _vpsWebSocketUrl);
                throw;
            }
        }

        private async Task ConnectInternalAsync()
        {
            try
            {
                _logger.LogInformation("Starting WebSocket connection to VPS...");
                await _socket.ConnectAsync();
                _logger.LogInformation("ConnectAsync completed, waiting for OnConnected event...");
                // OnConnected will set the result
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConnectAsync failed with exception: {Message}", ex.Message);
                _connectionTcs?.TrySetResult(false);
            }
        }

        private void OnConnected(object sender, EventArgs e)
        {
            _logger.LogInformation("Socket.io connected to VPS (Connected={Connected})", _socket?.Connected);
            _isConnected = true;
            _connectionTcs?.TrySetResult(true);

            // Re-subscribe to all rooms after reconnection
            Task.Run(async () => await ResubscribeAllRooms());
        }

        private void OnDisconnected(object sender, string reason)
        {
            _logger.LogWarning("Socket.io disconnected from VPS: {Reason}", reason);
            _isConnected = false;
        }

        private void OnError(object sender, string error)
        {
            _logger.LogError("Socket.io error: {Error}", error);
            // If we're still connecting, set failure
            _connectionTcs?.TrySetResult(false);
        }

        private async Task ResubscribeAllRooms()
        {
            foreach (var channel in _subscribedRooms.Keys)
            {
                if (_subscribedRooms.TryGetValue(channel, out var rooms))
                {
                    foreach (var roomId in rooms)
                    {
                        await SubscribeToChannel(channel, roomId);
                    }
                }
            }
        }

        /// <summary>
        /// Subscribe to a channel for a room
        /// </summary>
        private async Task SubscribeToChannel(string channel, string roomId)
        {
            try
            {
                if (_socket == null || !_socket.Connected)
                {
                    _logger.LogWarning("Cannot subscribe to {Channel} for room {RoomId}: socket not connected (socket={Socket}, connected={Connected})",
                        channel, roomId, _socket != null ? "exists" : "null", _socket?.Connected);
                    return;
                }

                var data = new Dictionary<string, string>
                {
                    ["channel"] = channel,
                    ["roomId"] = roomId
                };
                await _socket.EmitAsync("subscribe", data);
                _logger.LogDebug("Subscribed to {Channel} for room {RoomId}", channel, roomId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to subscribe to {Channel} for room {RoomId}", channel, roomId);
            }
        }

        /// <summary>
        /// Unsubscribe from a channel for a room
        /// </summary>
        private async Task UnsubscribeFromChannel(string channel, string roomId)
        {
            try
            {
                if (_socket == null || !_socket.Connected)
                {
                    _logger.LogDebug("Cannot unsubscribe from {Channel} for room {RoomId}: socket not connected", channel, roomId);
                    return;
                }

                var data = new Dictionary<string, string>
                {
                    ["channel"] = channel,
                    ["roomId"] = roomId
                };
                await _socket.EmitAsync("unsubscribe", data);
                _logger.LogDebug("Unsubscribed from {Channel} for room {RoomId}", channel, roomId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to unsubscribe from {Channel} for room {RoomId}", channel, roomId);
            }
        }

        /// <summary>
        /// Subscribe to a room's messages
        /// </summary>
        public async Task SubscribeToRoom(string roomId)
        {
            await EnsureConnectedAsync();

            // Initialize seen message tracking
            _seenMessageIds[roomId] = new HashSet<string>();
            _seenMessageLocks[roomId] = new object();

            if (_subscribedRooms["messages"].Add(roomId))
            {
                await SubscribeToChannel("messages", roomId);
                _logger.LogInformation("Started message subscription for room {RoomId}", roomId);
            }
        }

        /// <summary>
        /// Unsubscribe from a room's messages
        /// </summary>
        public void UnsubscribeFromRoom(string roomId)
        {
            if (_subscribedRooms["messages"].Remove(roomId))
            {
                _ = UnsubscribeFromChannel("messages", roomId);
                _seenMessageIds.TryRemove(roomId, out _);
                _seenMessageLocks.TryRemove(roomId, out _);
                _logger.LogInformation("Stopped message subscription for room {RoomId}", roomId);
            }

            // Disconnect if no more subscriptions
            CheckAndDisconnect();
        }

        /// <summary>
        /// Subscribe to a room's presence updates
        /// </summary>
        public async Task SubscribeToRoomPresence(string roomId)
        {
            await EnsureConnectedAsync();

            if (_subscribedRooms["presence"].Add(roomId))
            {
                await SubscribeToChannel("presence", roomId);
                _logger.LogInformation("Started presence subscription for room {RoomId}", roomId);
            }
        }

        /// <summary>
        /// Unsubscribe from a room's presence updates
        /// </summary>
        public void UnsubscribeFromRoomPresence(string roomId)
        {
            if (_subscribedRooms["presence"].Remove(roomId))
            {
                _ = UnsubscribeFromChannel("presence", roomId);
                _logger.LogInformation("Stopped presence subscription for room {RoomId}", roomId);
            }

            CheckAndDisconnect();
        }

        /// <summary>
        /// Subscribe to project users updates
        /// </summary>
        public async Task SubscribeToProjectUsers(string roomId)
        {
            await EnsureConnectedAsync();

            if (_subscribedRooms["project-users"].Add(roomId))
            {
                await SubscribeToChannel("project-users", roomId);
                _logger.LogInformation("Started project users subscription for room {RoomId}", roomId);
            }
        }

        /// <summary>
        /// Unsubscribe from project users updates
        /// </summary>
        public void UnsubscribeFromProjectUsers(string roomId)
        {
            if (_subscribedRooms["project-users"].Remove(roomId))
            {
                _ = UnsubscribeFromChannel("project-users", roomId);
                _logger.LogInformation("Stopped project users subscription for room {RoomId}", roomId);
            }

            CheckAndDisconnect();
        }

        /// <summary>
        /// Disconnect if no more subscriptions
        /// </summary>
        private void CheckAndDisconnect()
        {
            bool hasSubscriptions = false;
            foreach (var rooms in _subscribedRooms.Values)
            {
                if (rooms.Count > 0)
                {
                    hasSubscriptions = true;
                    break;
                }
            }

            if (!hasSubscriptions && _isConnected)
            {
                _logger.LogInformation("No more subscriptions, disconnecting from VPS");
                _ = _socket?.DisconnectAsync();
                _isConnected = false;
            }
        }

        /// <summary>
        /// Handle new message from VPS
        /// </summary>
        private async void OnNewMessage(SocketIOResponse response)
        {
            try
            {
                var data = response.GetValue<JsonElement>();

                if (!data.TryGetProperty("roomId", out var roomIdElement) ||
                    !data.TryGetProperty("message", out var messageElement))
                {
                    return;
                }

                var roomId = roomIdElement.GetString();
                var message = ParseMessage(messageElement);

                if (message == null || string.IsNullOrEmpty(roomId))
                    return;

                // Check for duplicate
                if (!IsNewMessage(roomId, message.Id))
                {
                    _logger.LogDebug("Skipping duplicate message {MessageId} for room {RoomId}", message.Id, roomId);
                    return;
                }

                // Broadcast to SignalR clients
                await _hubContext.Clients.Group(roomId).SendAsync("ReceiveMessage", message);
                _logger.LogDebug("Broadcasted message {MessageId} to room {RoomId}", message.Id, roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing new message from VPS");
            }
        }

        /// <summary>
        /// Handle presence update from VPS
        /// </summary>
        private async void OnPresenceUpdate(SocketIOResponse response)
        {
            try
            {
                var data = response.GetValue<JsonElement>();

                if (!data.TryGetProperty("roomId", out var roomIdElement))
                    return;

                var roomId = roomIdElement.GetString();
                var users = new List<ChatUserInfo>();
                var totalOnline = 0;

                if (data.TryGetProperty("users", out var usersElement) &&
                    usersElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var userElement in usersElement.EnumerateArray())
                    {
                        var user = ParseUser(userElement);
                        if (user != null)
                            users.Add(user);
                    }
                    totalOnline = users.Count;
                }

                if (data.TryGetProperty("totalOnline", out var totalElement))
                {
                    totalOnline = totalElement.GetInt32();
                }

                var presence = new PresenceInfoDto
                {
                    Users = users,
                    TotalOnline = totalOnline
                };

                await _hubContext.Clients.Group(roomId).SendAsync("PresenceUpdate", presence);
                _logger.LogDebug("Broadcasted presence update to room {RoomId}: {Count} users", roomId, totalOnline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing presence update from VPS");
            }
        }

        /// <summary>
        /// Handle project users update from VPS
        /// </summary>
        private async void OnProjectUsersUpdate(SocketIOResponse response)
        {
            try
            {
                var data = response.GetValue<JsonElement>();

                if (!data.TryGetProperty("roomId", out var roomIdElement) ||
                    !data.TryGetProperty("count", out var countElement))
                {
                    return;
                }

                var roomId = roomIdElement.GetString();
                var count = countElement.GetInt32();

                await _hubContext.Clients.Group(roomId).SendAsync("ProjectUsersCountUpdate", count);
                _logger.LogDebug("Broadcasted project users count {Count} to room {RoomId}", count, roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing project users update from VPS");
            }
        }

        /// <summary>
        /// Check if message is new (not already seen)
        /// </summary>
        private bool IsNewMessage(string roomId, string messageId)
        {
            if (!_seenMessageIds.TryGetValue(roomId, out var seenIds) ||
                !_seenMessageLocks.TryGetValue(roomId, out var lockObj))
            {
                return true;
            }

            lock (lockObj)
            {
                if (seenIds.Contains(messageId))
                    return false;

                seenIds.Add(messageId);

                // Limit size to prevent memory growth
                if (seenIds.Count > 1000)
                {
                    seenIds.Clear();
                    seenIds.Add(messageId);
                }

                return true;
            }
        }

        /// <summary>
        /// Parse message from JSON
        /// </summary>
        private ChatMessageDto ParseMessage(JsonElement element)
        {
            try
            {
                return new ChatMessageDto
                {
                    Id = element.TryGetProperty("id", out var id) ? id.GetString() : Guid.NewGuid().ToString(),
                    Content = element.TryGetProperty("content", out var c) ? c.GetString() : "",
                    SenderName = element.TryGetProperty("senderName", out var sn) ? sn.GetString() : "",
                    SenderEmail = element.TryGetProperty("senderEmail", out var se) ? se.GetString() : "",
                    Timestamp = element.TryGetProperty("timestamp", out var t) ? t.GetInt64() : 0,
                    Type = element.TryGetProperty("type", out var ty) ? ty.GetString() : "message"
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parse user from JSON
        /// </summary>
        private ChatUserInfo ParseUser(JsonElement element)
        {
            try
            {
                return new ChatUserInfo
                {
                    OderId = element.TryGetProperty("oderId", out var o) ? o.GetString() : "",
                    UserId = element.TryGetProperty("userId", out var ui) ? ui.GetString() : "",
                    UserName = element.TryGetProperty("userName", out var un) ? un.GetString() : "Unknown",
                    UserEmail = element.TryGetProperty("userEmail", out var ue) ? ue.GetString() : "",
                    Online = element.TryGetProperty("online", out var on) && on.GetBoolean(),
                    LastSeen = element.TryGetProperty("lastSeen", out var ls) ? ls.GetInt64() : 0
                };
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            foreach (var rooms in _subscribedRooms.Values)
            {
                rooms.Clear();
            }

            _seenMessageIds.Clear();
            _seenMessageLocks.Clear();

            _socket?.Dispose();
            _socket = null;
            _isConnected = false;
        }
    }
}
