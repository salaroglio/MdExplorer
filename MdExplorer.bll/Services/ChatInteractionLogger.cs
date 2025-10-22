using System;
using System.Collections.Generic;
using System.IO;

namespace MdExplorer.Features.Services
{
    /// <summary>
    /// Dedicated logger for chat interactions to debug AI tool calling behavior.
    /// Writes all interactions to Logs/chat-interactions-{Date}.log
    /// </summary>
    public class ChatInteractionLogger
    {
        private readonly object _lock = new object();
        private readonly string _logFilePath;

        public ChatInteractionLogger()
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            Directory.CreateDirectory(logPath);
            _logFilePath = Path.Combine(logPath, $"chat-interactions-{DateTime.Now:yyyy-MM-dd}.log");
        }

        private void WriteLog(string message)
        {
            lock (_lock)
            {
                try
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    File.AppendAllText(_logFilePath, $"[{timestamp}] {message}\n");
                }
                catch
                {
                    // Fail silently to not disrupt chat functionality
                }
            }
        }

        public void LogUserMessage(string connectionId, string message, string currentDoc, int historyCount)
        {
            WriteLog("========================================");
            WriteLog("NEW USER MESSAGE");
            WriteLog($"ConnectionId: {connectionId}");
            WriteLog($"Message: {message}");
            WriteLog($"Current Document: {currentDoc ?? "NONE"}");
            WriteLog($"Conversation History: {historyCount} messages");
        }

        public void LogGeminiRequest(string prompt, string toolGuidance, List<string> availableTools, string currentDoc, int historyCount)
        {
            WriteLog("--- GEMINI REQUEST ---");
            WriteLog($"Prompt Length: {prompt?.Length ?? 0} chars");
            WriteLog($"Prompt (first 200 chars): {(prompt?.Length > 200 ? prompt.Substring(0, 200) + "..." : prompt)}");
            WriteLog($"Current Document: {currentDoc ?? "NONE"}");
            WriteLog($"Available Tools: {string.Join(", ", availableTools ?? new List<string>())}");
            WriteLog($"History Count: {historyCount}");
            WriteLog("Tool Guidance:");
            WriteLog(toolGuidance ?? "NONE");
        }

        public void LogGeminiResponse(string responseJson, bool hasFunctionCalls)
        {
            WriteLog("--- GEMINI RESPONSE ---");
            WriteLog($"Response JSON Length: {responseJson?.Length ?? 0} chars");

            if (responseJson != null && responseJson.Length <= 2000)
            {
                WriteLog($"Response JSON: {responseJson}");
            }
            else
            {
                WriteLog($"Response JSON (truncated): {responseJson?.Substring(0, 2000)}...");
            }

            if (!hasFunctionCalls)
            {
                WriteLog("⚠️ NO FUNCTION CALLS - AI decided to only generate text without calling tools");
            }
        }

        public void LogFunctionCall(string functionName, string arguments)
        {
            WriteLog("✅ FUNCTION CALL DETECTED");
            WriteLog($"Function: {functionName}");
            WriteLog($"Arguments: {arguments}");
        }

        public void LogFunctionResult(string functionName, bool success, string path, string message)
        {
            WriteLog($"Tool Execution Result:");
            WriteLog($"  Function: {functionName}");
            WriteLog($"  Success: {success}");
            WriteLog($"  Path: {path ?? "N/A"}");
            WriteLog($"  Message: {message ?? "N/A"}");
        }

        public void LogFinalResponse(string response)
        {
            WriteLog("--- FINAL RESPONSE ---");

            if (response != null && response.Length <= 500)
            {
                WriteLog($"Final Text: {response}");
            }
            else
            {
                WriteLog($"Final Text (truncated): {response?.Substring(0, 500)}...");
            }
        }

        public void LogWarning(string message)
        {
            WriteLog($"⚠️ WARNING: {message}");
        }

        public void LogError(string message, Exception ex = null)
        {
            WriteLog($"❌ ERROR: {message}");
            if (ex != null)
            {
                WriteLog($"Exception: {ex.Message}");
                WriteLog($"StackTrace: {ex.StackTrace}");
            }
        }

        public void LogInfo(string message)
        {
            WriteLog($"ℹ️ INFO: {message}");
        }
    }
}
