using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Services
{
    /// <summary>
    /// Sends real-time notifications about AI file operations to specific clients via SignalR.
    /// </summary>
    public interface IAiFileOperationNotifier
    {
        /// <summary>
        /// Sends notification to a specific client about an AI file operation.
        /// </summary>
        /// <param name="connectionId">SignalR connection ID of the target client</param>
        /// <param name="operationType">Type of operation (create, read, update)</param>
        /// <param name="filePath">Relative path of the file</param>
        /// <param name="success">Whether the operation succeeded</param>
        /// <param name="message">Human-readable message about the operation</param>
        Task SendNotificationAsync(string connectionId, string operationType, string filePath, bool success, string message);
    }
}
