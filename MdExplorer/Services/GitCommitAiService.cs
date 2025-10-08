using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using MdExplorer.Features.Services;

namespace MdExplorer.Services
{
    public interface IGitCommitAiService
    {
        Task<string> GenerateCommitMessageAsync(string repositoryPath);
    }

    public class GitCommitAiService : IGitCommitAiService
    {
        private readonly ILogger<GitCommitAiService> _logger;
        private readonly IAiChatService _aiChatService;
        private readonly IGeminiApiService _geminiService;
        private readonly IModernGitService _modernGitService;
        private const int MaxDiffLinesPerFile = 100;
        private const int MaxFilesToAnalyze = 20;

        public GitCommitAiService(
            ILogger<GitCommitAiService> logger,
            IAiChatService aiChatService,
            IGeminiApiService geminiService,
            IModernGitService modernGitService)
        {
            _logger = logger;
            _aiChatService = aiChatService;
            _geminiService = geminiService;
            _modernGitService = modernGitService;
        }

        public async Task<string> GenerateCommitMessageAsync(string repositoryPath)
        {
            try
            {
                _logger.LogInformation("Generating commit message for repository: {RepositoryPath}", repositoryPath);

                // Get repository status
                var status = await _modernGitService.GetStatusAsync(repositoryPath);
                
                if (!HasChanges(status))
                {
                    _logger.LogInformation("No changes detected in repository");
                    return "No changes to commit";
                }

                // Collect changes information
                var changesInfo = await CollectChangesInfo(repositoryPath, status);
                
                // Generate prompt for AI
                var prompt = BuildCommitPrompt(changesInfo);
                
                // Call AI to generate message
                var commitMessage = await CallAiForCommitMessage(prompt);
                
                _logger.LogInformation("Successfully generated commit message");
                return commitMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating commit message for repository: {RepositoryPath}", repositoryPath);
                return "Update from MdExplorer";
            }
        }

        private bool HasChanges(GitRepositoryStatus status)
        {
            return status.Added.Any() || status.Modified.Any() || status.Removed.Any() || status.Untracked.Any();
        }

        private async Task<ChangesInfo> CollectChangesInfo(string repositoryPath, GitRepositoryStatus status)
        {
            var info = new ChangesInfo
            {
                AddedFiles = status.Added.Take(MaxFilesToAnalyze).ToList(),
                ModifiedFiles = status.Modified.Take(MaxFilesToAnalyze).ToList(),
                RemovedFiles = status.Removed.Take(MaxFilesToAnalyze).ToList(),
                UntrackedFiles = status.Untracked.Take(MaxFilesToAnalyze).ToList(),
                FileDiffs = new Dictionary<string, string>()
            };

            try
            {
                using var repo = new Repository(repositoryPath);
                
                // Collect diffs for modified files
                foreach (var modifiedFile in info.ModifiedFiles.Take(10)) // Limit to 10 files for performance
                {
                    try
                    {
                        var diff = GetFileDiff(repo, modifiedFile);
                        if (!string.IsNullOrEmpty(diff))
                        {
                            info.FileDiffs[modifiedFile] = diff;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not get diff for file: {File}", modifiedFile);
                    }
                }

                // Collect content preview for new files
                foreach (var addedFile in info.AddedFiles.Take(5)) // Limit to 5 new files
                {
                    try
                    {
                        var content = GetFilePreview(repo, addedFile);
                        if (!string.IsNullOrEmpty(content))
                        {
                            info.FileDiffs[addedFile] = content;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not get content for new file: {File}", addedFile);
                    }
                }

                // Collect content preview for untracked files
                foreach (var untrackedFile in info.UntrackedFiles.Take(5)) // Limit to 5 untracked files
                {
                    try
                    {
                        var fullPath = System.IO.Path.Combine(repositoryPath, untrackedFile);
                        if (System.IO.File.Exists(fullPath))
                        {
                            var content = System.IO.File.ReadAllText(fullPath);
                            if (!string.IsNullOrEmpty(content))
                            {
                                var lines = content.Split('\n').Take(50); // First 50 lines
                                info.FileDiffs[untrackedFile] = $"Nuovo file (non tracciato):\n{string.Join("\n", lines)}";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not get content for untracked file: {File}", untrackedFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting changes info");
            }

            return info;
        }

        private string GetFileDiff(Repository repo, string filePath)
        {
            try
            {
                var patch = repo.Diff.Compare<Patch>(
                    repo.Head.Tip.Tree,
                    DiffTargets.Index | DiffTargets.WorkingDirectory,
                    new[] { filePath });

                if (patch != null && patch.Count() > 0)
                {
                    var content = patch.Content;
                    var lines = content.Split('\n').Take(MaxDiffLinesPerFile);
                    return string.Join("\n", lines);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting diff for file: {File}", filePath);
            }

            return string.Empty;
        }

        private string GetFilePreview(Repository repo, string filePath)
        {
            try
            {
                var indexEntry = repo.Index[filePath];
                if (indexEntry != null)
                {
                    var blob = repo.ObjectDatabase.CreateBlob(System.IO.Path.Combine(repo.Info.WorkingDirectory, filePath));
                    var content = blob.GetContentText();
                    
                    if (!string.IsNullOrEmpty(content))
                    {
                        var lines = content.Split('\n').Take(50); // First 50 lines
                        return $"New file preview:\n{string.Join("\n", lines)}";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting content for file: {File}", filePath);
            }

            return string.Empty;
        }

        private string BuildCommitPrompt(ChangesInfo changesInfo)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("Analizza questi cambiamenti Git e genera un messaggio di commit conciso e descrittivo in italiano.");
            prompt.AppendLine("Il messaggio deve essere chiaro, professionale e seguire le best practice dei messaggi di commit.");
            prompt.AppendLine("Usa tempo presente e imperativo quando appropriato.");
            prompt.AppendLine();

            // Added files
            if (changesInfo.AddedFiles.Any())
            {
                prompt.AppendLine($"FILE AGGIUNTI ({changesInfo.AddedFiles.Count}):");
                foreach (var file in changesInfo.AddedFiles.Take(10))
                {
                    prompt.AppendLine($"  - {file}");
                    if (changesInfo.FileDiffs.ContainsKey(file))
                    {
                        prompt.AppendLine($"    Preview: {changesInfo.FileDiffs[file].Substring(0, Math.Min(200, changesInfo.FileDiffs[file].Length))}...");
                    }
                }
                prompt.AppendLine();
            }

            // Modified files
            if (changesInfo.ModifiedFiles.Any())
            {
                prompt.AppendLine($"FILE MODIFICATI ({changesInfo.ModifiedFiles.Count}):");
                foreach (var file in changesInfo.ModifiedFiles.Take(10))
                {
                    prompt.AppendLine($"  - {file}");
                    if (changesInfo.FileDiffs.ContainsKey(file))
                    {
                        var diff = changesInfo.FileDiffs[file];
                        var diffPreview = diff.Length > 500 ? diff.Substring(0, 500) + "..." : diff;
                        prompt.AppendLine($"    Modifiche:\n{diffPreview}");
                    }
                }
                prompt.AppendLine();
            }

            // Removed files
            if (changesInfo.RemovedFiles.Any())
            {
                prompt.AppendLine($"FILE RIMOSSI ({changesInfo.RemovedFiles.Count}):");
                foreach (var file in changesInfo.RemovedFiles.Take(10))
                {
                    prompt.AppendLine($"  - {file}");
                }
                prompt.AppendLine();
            }

            // Untracked files (new files not yet added to git)
            if (changesInfo.UntrackedFiles.Any())
            {
                prompt.AppendLine($"NUOVI FILE NON TRACCIATI ({changesInfo.UntrackedFiles.Count}):");
                foreach (var file in changesInfo.UntrackedFiles.Take(10))
                {
                    prompt.AppendLine($"  - {file}");
                    if (changesInfo.FileDiffs.ContainsKey(file))
                    {
                        prompt.AppendLine($"    Preview: {changesInfo.FileDiffs[file].Substring(0, Math.Min(200, changesInfo.FileDiffs[file].Length))}...");
                    }
                }
                prompt.AppendLine();
            }

            prompt.AppendLine("Genera un messaggio di commit che:");
            prompt.AppendLine("1. Descriva il cambiamento principale in una riga (max 50 caratteri)");
            prompt.AppendLine("2. Se necessario, aggiungi una riga vuota e poi dettagli aggiuntivi");
            prompt.AppendLine("3. Sia specifico ma conciso");
            prompt.AppendLine("4. Non includere prefissi come 'commit:' o 'git:'");
            prompt.AppendLine();
            prompt.AppendLine("Rispondi SOLO con il messaggio di commit, senza spiegazioni aggiuntive.");

            return prompt.ToString();
        }

        private async Task<string> CallAiForCommitMessage(string prompt)
        {
            try
            {
                // First try Gemini if configured
                if (_geminiService.IsConfigured())
                {
                    _logger.LogInformation("Using Gemini API for commit message generation");
                    var response = await _geminiService.ChatAsync(prompt, "gemini-1.5-flash");
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        return CleanCommitMessage(response);
                    }
                }
                
                // Fallback to local AI if loaded
                if (_aiChatService.IsModelLoaded())
                {
                    _logger.LogInformation("Using local AI model for commit message generation");
                    var response = await _aiChatService.ChatAsync(prompt);
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        return CleanCommitMessage(response);
                    }
                }

                _logger.LogWarning("No AI service available for commit message generation");
                return GenerateFallbackMessage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling AI for commit message");
                return GenerateFallbackMessage();
            }
        }

        private string CleanCommitMessage(string aiResponse)
        {
            // Remove any markdown formatting
            aiResponse = aiResponse.Replace("```", "").Trim();
            
            // Remove common prefixes
            var prefixesToRemove = new[] { "commit:", "git:", "message:", "Commit:", "Git:", "Message:" };
            foreach (var prefix in prefixesToRemove)
            {
                if (aiResponse.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    aiResponse = aiResponse.Substring(prefix.Length).Trim();
                }
            }

            // Ensure reasonable length
            var lines = aiResponse.Split('\n');
            if (lines.Length > 0 && lines[0].Length > 72)
            {
                // Truncate first line if too long
                lines[0] = lines[0].Substring(0, 72) + "...";
            }

            return string.Join("\n", lines).Trim();
        }

        private string GenerateFallbackMessage()
        {
            return $"Aggiornamento del {DateTime.Now:yyyy-MM-dd HH:mm}";
        }

        private class ChangesInfo
        {
            public List<string> AddedFiles { get; set; } = new List<string>();
            public List<string> ModifiedFiles { get; set; } = new List<string>();
            public List<string> RemovedFiles { get; set; } = new List<string>();
            public List<string> UntrackedFiles { get; set; } = new List<string>();
            public Dictionary<string, string> FileDiffs { get; set; } = new Dictionary<string, string>();
        }
    }
}