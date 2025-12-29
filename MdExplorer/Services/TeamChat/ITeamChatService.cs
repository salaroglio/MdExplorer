using MdExplorer.Hubs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MdExplorer.Services.TeamChat
{
    /// <summary>
    /// Service interface for Team Chat functionality.
    /// Handles communication with Firebase Realtime Database.
    /// </summary>
    public interface ITeamChatService
    {
        /// <summary>
        /// Register a connection to a chat room
        /// </summary>
        Task JoinRoom(string roomId, string connectionId, ChatUserInfo user);

        /// <summary>
        /// Unregister a connection from a chat room
        /// </summary>
        Task LeaveRoom(string roomId, string connectionId);

        /// <summary>
        /// Send a message to a room (saves to Firebase)
        /// </summary>
        Task<ChatMessageDto> SendMessage(string roomId, ChatMessageDto message);

        /// <summary>
        /// Get recent messages from a room
        /// </summary>
        Task<List<ChatMessageDto>> GetRecentMessages(string roomId, int limit);

        /// <summary>
        /// Get current presence info for a room
        /// </summary>
        PresenceInfoDto GetPresence(string roomId);

        /// <summary>
        /// Register a user opening a project (not chat, just the project)
        /// </summary>
        Task RegisterProjectOpen(string roomId, string oderId, ChatUserInfo user);

        /// <summary>
        /// Unregister a user closing a project
        /// </summary>
        Task UnregisterProjectOpen(string roomId, string oderId);

        /// <summary>
        /// Get count of users who have the project open
        /// </summary>
        int GetProjectUsersCount(string roomId);
    }
}
