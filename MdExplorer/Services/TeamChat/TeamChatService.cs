using MdExplorer.Hubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MdExplorer.Services.TeamChat
{
    /// <summary>
    /// Team Chat service implementation using Firebase Realtime Database REST API.
    /// Uses FirebaseStreamingService for real-time cross-PC communication via SSE.
    /// </summary>
    public class TeamChatService : ITeamChatService
    {
        private readonly ILogger<TeamChatService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _firebaseDatabaseUrl;
        private readonly FirebaseStreamingService _streamingService;

        // Track presence per room: roomId -> (connectionId -> userInfo)
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ChatUserInfo>> _roomPresence = new();

        // Track project users per room: roomId -> (oderId -> userInfo)
        // This tracks who has the project open, not who has the chat open
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ChatUserInfo>> _projectUsers = new();

        // JSON serializer options
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public TeamChatService(
            ILogger<TeamChatService> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            FirebaseStreamingService streamingService)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("Firebase");
            _streamingService = streamingService;

            // Get Firebase URL from configuration
            _firebaseDatabaseUrl = configuration["Firebase:DatabaseUrl"]
                ?? "https://mdexplorer-chat-default-rtdb.europe-west1.firebasedatabase.app";

            _logger.LogInformation("TeamChatService initialized with Firebase URL: {Url}", _firebaseDatabaseUrl);
        }

        /// <summary>
        /// Register a connection to a chat room
        /// </summary>
        public async Task JoinRoom(string roomId, string connectionId, ChatUserInfo user)
        {
            // Add to local presence tracking
            var roomUsers = _roomPresence.GetOrAdd(roomId, _ => new ConcurrentDictionary<string, ChatUserInfo>());

            // Check if this is the first client in the room (before adding)
            bool isFirstClient = roomUsers.IsEmpty;

            user.Online = true;
            user.LastSeen = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            roomUsers[connectionId] = user;

            // Update presence in Firebase
            await UpdateFirebasePresence(roomId, connectionId, user);

            // Subscribe to Firebase SSE only for the first client (one SSE per room per backend)
            if (isFirstClient)
            {
                await _streamingService.SubscribeToRoom(roomId);
                await _streamingService.SubscribeToRoomPresence(roomId);
            }

            _logger.LogInformation("User {UserName} joined room {RoomId} (connection: {ConnectionId}, isFirst: {IsFirst})",
                user.UserName, roomId, connectionId, isFirstClient);
        }

        /// <summary>
        /// Unregister a connection from a chat room
        /// </summary>
        public async Task LeaveRoom(string roomId, string connectionId)
        {
            bool roomEmpty = false;
            string userName = "Unknown";

            // Remove from local presence tracking
            if (_roomPresence.TryGetValue(roomId, out var roomUsers))
            {
                roomUsers.TryRemove(connectionId, out var user);
                userName = user?.UserName ?? "Unknown";

                // Check if room is now empty
                roomEmpty = roomUsers.IsEmpty;
                if (roomEmpty)
                {
                    _roomPresence.TryRemove(roomId, out _);
                }

                _logger.LogInformation("User {UserName} left room {RoomId} (connection: {ConnectionId}, roomEmpty: {RoomEmpty})",
                    userName, roomId, connectionId, roomEmpty);
            }

            // Remove presence from Firebase
            await RemoveFirebasePresence(roomId, connectionId);

            // Unsubscribe from Firebase SSE only when the last client leaves
            if (roomEmpty)
            {
                _streamingService.UnsubscribeFromRoom(roomId);
                _streamingService.UnsubscribeFromRoomPresence(roomId);
            }
        }

        /// <summary>
        /// Send a message to a room (saves to Firebase)
        /// </summary>
        public async Task<ChatMessageDto> SendMessage(string roomId, ChatMessageDto message)
        {
            message.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var url = $"{_firebaseDatabaseUrl}/chatRooms/{roomId}/messages.json";

            var json = JsonSerializer.Serialize(new
            {
                content = message.Content,
                senderName = message.SenderName,
                senderEmail = message.SenderEmail,
                timestamp = message.Timestamp,
                type = message.Type
            }, _jsonOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FirebasePostResponse>(responseJson, _jsonOptions);

                message.Id = result?.Name ?? Guid.NewGuid().ToString();

                _logger.LogDebug("Message sent to room {RoomId}: {MessageId}", roomId, message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to Firebase for room {RoomId}", roomId);
                // Generate local ID if Firebase fails
                message.Id = Guid.NewGuid().ToString();
            }

            return message;
        }

        /// <summary>
        /// Get recent messages from a room
        /// </summary>
        public async Task<List<ChatMessageDto>> GetRecentMessages(string roomId, int limit)
        {
            var url = $"{_firebaseDatabaseUrl}/chatRooms/{roomId}/messages.json?orderBy=\"timestamp\"&limitToLast={limit}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get messages from Firebase: {StatusCode}", response.StatusCode);
                    return new List<ChatMessageDto>();
                }

                var json = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(json) || json == "null")
                {
                    return new List<ChatMessageDto>();
                }

                var messages = JsonSerializer.Deserialize<Dictionary<string, FirebaseMessage>>(json, _jsonOptions);

                if (messages == null)
                {
                    return new List<ChatMessageDto>();
                }

                return messages
                    .Select(kvp => new ChatMessageDto
                    {
                        Id = kvp.Key,
                        Content = kvp.Value.Content,
                        SenderName = kvp.Value.SenderName,
                        SenderEmail = kvp.Value.SenderEmail,
                        Timestamp = kvp.Value.Timestamp,
                        Type = kvp.Value.Type ?? "message"
                    })
                    .OrderBy(m => m.Timestamp)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get messages from Firebase for room {RoomId}", roomId);
                return new List<ChatMessageDto>();
            }
        }

        /// <summary>
        /// Get current presence info for a room (from local cache)
        /// </summary>
        public PresenceInfoDto GetPresence(string roomId)
        {
            if (_roomPresence.TryGetValue(roomId, out var roomUsers))
            {
                var users = roomUsers.Values.Where(u => u.Online).ToList();
                return new PresenceInfoDto
                {
                    Users = users,
                    TotalOnline = users.Count
                };
            }

            return new PresenceInfoDto { Users = new List<ChatUserInfo>(), TotalOnline = 0 };
        }

        private async Task UpdateFirebasePresence(string roomId, string connectionId, ChatUserInfo user)
        {
            var url = $"{_firebaseDatabaseUrl}/chatRooms/{roomId}/presence/{connectionId}.json";

            var json = JsonSerializer.Serialize(new
            {
                oderId = user.OderId,
                userId = user.UserId,
                userName = user.UserName,
                userEmail = user.UserEmail,
                online = true,
                lastSeen = user.LastSeen
            }, _jsonOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update Firebase presence for room {RoomId}", roomId);
            }
        }

        private async Task RemoveFirebasePresence(string roomId, string connectionId)
        {
            var url = $"{_firebaseDatabaseUrl}/chatRooms/{roomId}/presence/{connectionId}.json";

            try
            {
                var response = await _httpClient.DeleteAsync(url);
                // Don't throw on failure - presence cleanup is best effort
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove Firebase presence for room {RoomId}", roomId);
            }
        }

        /// <summary>
        /// Register a user opening a project (not chat, just the project)
        /// </summary>
        public async Task RegisterProjectOpen(string roomId, string oderId, ChatUserInfo user)
        {
            var roomUsers = _projectUsers.GetOrAdd(roomId, _ => new ConcurrentDictionary<string, ChatUserInfo>());

            // Check if this is the first user for this room
            bool isFirstUser = roomUsers.IsEmpty;

            user.Online = true;
            user.LastSeen = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            roomUsers[oderId] = user;

            // Update in Firebase
            await UpdateFirebaseProjectUser(roomId, oderId, user);

            // Subscribe to project users SSE if first user
            if (isFirstUser)
            {
                await _streamingService.SubscribeToProjectUsers(roomId);
            }

            _logger.LogInformation("User {UserName} opened project for room {RoomId} (oderId: {OderId})",
                user.UserName, roomId, oderId);
        }

        /// <summary>
        /// Unregister a user closing a project
        /// </summary>
        public async Task UnregisterProjectOpen(string roomId, string oderId)
        {
            bool roomEmpty = false;

            if (_projectUsers.TryGetValue(roomId, out var roomUsers))
            {
                roomUsers.TryRemove(oderId, out var user);
                var userName = user?.UserName ?? "Unknown";

                roomEmpty = roomUsers.IsEmpty;
                if (roomEmpty)
                {
                    _projectUsers.TryRemove(roomId, out _);
                }

                _logger.LogInformation("User {UserName} closed project for room {RoomId} (oderId: {OderId})",
                    userName, roomId, oderId);
            }

            // Remove from Firebase
            await RemoveFirebaseProjectUser(roomId, oderId);

            // Unsubscribe from SSE if no more users
            if (roomEmpty)
            {
                _streamingService.UnsubscribeFromProjectUsers(roomId);
            }
        }

        /// <summary>
        /// Get count of users who have the project open
        /// </summary>
        public int GetProjectUsersCount(string roomId)
        {
            if (_projectUsers.TryGetValue(roomId, out var roomUsers))
            {
                return roomUsers.Count;
            }
            return 0;
        }

        /// <summary>
        /// Update project user count from remote SSE
        /// Called by FirebaseStreamingService when remote presence changes
        /// </summary>
        public void UpdateRemoteProjectUsers(string roomId, int count)
        {
            // This will be used to merge local and remote counts
            // For now, we'll handle this in the SSE streaming service
            _logger.LogDebug("Remote project users update for room {RoomId}: {Count}", roomId, count);
        }

        private async Task UpdateFirebaseProjectUser(string roomId, string oderId, ChatUserInfo user)
        {
            var url = $"{_firebaseDatabaseUrl}/chatRooms/{roomId}/projectUsers/{oderId}.json";

            var json = JsonSerializer.Serialize(new
            {
                oderId = user.OderId,
                userId = user.UserId,
                userName = user.UserName,
                userEmail = user.UserEmail,
                online = true,
                lastSeen = user.LastSeen
            }, _jsonOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update Firebase project user for room {RoomId}", roomId);
            }
        }

        private async Task RemoveFirebaseProjectUser(string roomId, string oderId)
        {
            var url = $"{_firebaseDatabaseUrl}/chatRooms/{roomId}/projectUsers/{oderId}.json";

            try
            {
                var response = await _httpClient.DeleteAsync(url);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove Firebase project user for room {RoomId}", roomId);
            }
        }

        // Firebase response models
        private class FirebasePostResponse
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        private class FirebaseMessage
        {
            [JsonPropertyName("content")]
            public string Content { get; set; }

            [JsonPropertyName("senderName")]
            public string SenderName { get; set; }

            [JsonPropertyName("senderEmail")]
            public string SenderEmail { get; set; }

            [JsonPropertyName("timestamp")]
            public long Timestamp { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }
        }
    }
}
