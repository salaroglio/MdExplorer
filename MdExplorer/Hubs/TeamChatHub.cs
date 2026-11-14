using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Services.TeamChat;

namespace MdExplorer.Hubs
{
    /// <summary>
    /// SignalR Hub for Team Chat functionality.
    /// Each connectionId is associated with a project/room.
    /// Messages are proxied to/from Firebase Realtime Database.
    /// </summary>
    public class TeamChatHub : Hub
    {
        private readonly ILogger<TeamChatHub> _logger;
        private readonly ITeamChatService _chatService;
        private readonly IChatRoomKeys _roomKeys;

        // Track which connectionId is in which room
        private static readonly ConcurrentDictionary<string, string> _connectionRooms = new();

        // Track user info per connection
        private static readonly ConcurrentDictionary<string, ChatUserInfo> _connectionUsers = new();

        public TeamChatHub(
            ILogger<TeamChatHub> logger,
            ITeamChatService chatService,
            IChatRoomKeys roomKeys)
        {
            _logger = logger;
            _chatService = chatService;
            _roomKeys = roomKeys;
        }

        /// <summary>
        /// Get the connection ID for this client
        /// </summary>
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        /// <summary>
        /// Join a chat room for a specific repository
        /// </summary>
        /// <param name="roomId">The room ID (hash of repository URL)</param>
        /// <param name="user">User information</param>
        public async Task JoinRoom(string roomId, ChatUserInfo user)
        {
            var connectionId = Context.ConnectionId;

            // La chiave della stanza la registra room-info (dal progetto): senza, la chat sarebbe muta
            // in silenzio (messaggi non letti né spediti). Succede se il servizio è ripartito sotto la UI.
            if (_roomKeys.Find(roomId) == null)
                throw new HubException(MdExplorer.Features.Services.UiText.Say("La stanza non ha la chiave del progetto: riapri la chat dal progetto.", "The room has no project key: open the chat again from the project."));

            _logger.LogInformation("Client {ConnectionId} joining room {RoomId} as {UserName}",
                connectionId, roomId, user.UserName);

            // Leave previous room if any
            if (_connectionRooms.TryGetValue(connectionId, out var previousRoom))
            {
                await LeaveRoomInternal(connectionId, previousRoom);
            }

            // Join the SignalR group for this room
            await Groups.AddToGroupAsync(connectionId, roomId);

            // Track the connection
            _connectionRooms[connectionId] = roomId;
            _connectionUsers[connectionId] = user;

            await Relayed(async () =>
            {
                // Register with the chat service (connects to the relay)
                await _chatService.JoinRoom(roomId, connectionId, user);

                // Get message history and send to client
                var messages = await _chatService.GetRecentMessages(roomId, 50);
                await Clients.Caller.SendAsync("ReceiveMessageHistory", messages);

                // Get current presence and broadcast to all in room
                var presence = _chatService.GetPresence(roomId);
                await Clients.Group(roomId).SendAsync("PresenceUpdate", presence);

                // Send system message
                await SendSystemMessage(roomId, $"{user.UserName} joined the chat");
            });
        }

        /// <summary>
        /// Un errore del relay (rifiuto, relay che non risponde, socket che non si apre) arriva al client col suo
        /// testo: SignalR mostra solo il messaggio di una <see cref="HubException"/>, le altre diventano «errore inatteso».
        /// </summary>
        private static async Task Relayed(Func<Task> work)
        {
            try
            {
                await work();
            }
            catch (Exception ex) when (ex is not HubException)
            {
                throw new HubException(ex.Message);
            }
        }

        /// <summary>
        /// Leave the current chat room
        /// </summary>
        public async Task LeaveRoom(string roomId)
        {
            var connectionId = Context.ConnectionId;
            await LeaveRoomInternal(connectionId, roomId);
        }

        private async Task LeaveRoomInternal(string connectionId, string roomId)
        {
            if (_connectionUsers.TryGetValue(connectionId, out var user))
            {
                // Send leave message before removing. Se il relay non lo prende si dice nel log, ma l'uscita
                // dalla stanza va finita comunque: altrimenti la connessione resterebbe iscritta.
                try
                {
                    await SendSystemMessage(roomId, $"{user.UserName} left the chat");
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning("Leave notice for room {RoomId} not delivered: {Error}", roomId, ex.Message);
                }
            }

            // Remove from SignalR group
            await Groups.RemoveFromGroupAsync(connectionId, roomId);

            // Unregister from chat service
            await _chatService.LeaveRoom(roomId, connectionId);

            // Remove tracking
            _connectionRooms.TryRemove(connectionId, out _);
            _connectionUsers.TryRemove(connectionId, out _);

            // Broadcast updated presence
            var presence = _chatService.GetPresence(roomId);
            await Clients.Group(roomId).SendAsync("PresenceUpdate", presence);

            _logger.LogInformation("Client {ConnectionId} left room {RoomId}", connectionId, roomId);
        }

        /// <summary>
        /// Send a message to the current room
        /// </summary>
        public async Task SendMessage(string roomId, ChatMessageDto message)
        {
            var connectionId = Context.ConnectionId;

            // Verify the client is in this room: un messaggio scartato qui va detto a chi scrive, non solo al log.
            if (!_connectionRooms.TryGetValue(connectionId, out var currentRoom) || currentRoom != roomId)
            {
                _logger.LogWarning("Client {ConnectionId} tried to send to room {RoomId} but is in {CurrentRoom}",
                    connectionId, roomId, currentRoom ?? "no room");
                throw new HubException(MdExplorer.Features.Services.UiText.Say(
                    "Messaggio non inviato: questa finestra non è più nella stanza. Riapri la chat dal progetto.",
                    "Message not sent: this window is no longer in the room. Open the chat again from the project."));
            }

            await Relayed(async () =>
            {
                // Save to the relay and get the message with ID
                var savedMessage = await _chatService.SendMessage(roomId, message);

                // Broadcast to all clients in the room
                await Clients.Group(roomId).SendAsync("ReceiveMessage", savedMessage);
            });
        }

        private async Task SendSystemMessage(string roomId, string content)
        {
            var systemMessage = new ChatMessageDto
            {
                Content = content,
                SenderName = "System",
                SenderEmail = "",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Type = "system"
            };

            var savedMessage = await _chatService.SendMessage(roomId, systemMessage);
            await Clients.Group(roomId).SendAsync("ReceiveMessage", savedMessage);
        }

        /// <summary>
        /// Called when a client disconnects
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;

            _logger.LogInformation("Client {ConnectionId} disconnecting from TeamChat", connectionId);

            // Leave the room if in one
            if (_connectionRooms.TryGetValue(connectionId, out var roomId))
            {
                await LeaveRoomInternal(connectionId, roomId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }

    /// <summary>
    /// User information for chat
    /// </summary>
    public class ChatUserInfo
    {
        public string OderId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public bool Online { get; set; } = true;
        public long LastSeen { get; set; }
    }

    /// <summary>
    /// Chat message DTO
    /// </summary>
    public class ChatMessageDto
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public long Timestamp { get; set; }
        public string Type { get; set; } = "message";
    }

    /// <summary>
    /// Presence information DTO
    /// </summary>
    public class PresenceInfoDto
    {
        public List<ChatUserInfo> Users { get; set; } = new();
        public int TotalOnline { get; set; }
    }
}
