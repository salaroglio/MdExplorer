namespace MdExplorer.bll.Models.AI
{
    /// <summary>
    /// Represents a single message in an AI conversation.
    /// Used to maintain conversation history across multiple turns.
    /// </summary>
    public class ConversationMessage
    {
        /// <summary>
        /// Role of the message sender: "user" or "model"
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Content of the message
        /// </summary>
        public string Content { get; set; }
    }
}
