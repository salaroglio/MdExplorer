using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using MdExplorer.Features.Services;
using MdExplorer.Abstractions.Services;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services.AI;
using Ad.Tools.Dal.Extensions;

namespace MdExplorer.Services
{
    public interface IGitCommitAiService
    {
        Task<string> GenerateCommitMessageAsync(string repositoryPath, string language);
    }

    public class GitCommitAiService : IGitCommitAiService
    {
        private readonly ILogger<GitCommitAiService> _logger;
        private readonly IAiChatService _aiChatService;
        private readonly IGeminiApiService _geminiService;
        private readonly IModernGitService _modernGitService;
        private readonly IEnumerable<IAiProvider> _aiProviders;
        private readonly IUserSettingsDB _userSettingsDB;
        private const int MaxDiffLinesPerFile = 100;
        private const int MaxFilesToAnalyze = 20;
        private const string DefaultProviderKey = "AI_DefaultProvider";

        public GitCommitAiService(
            ILogger<GitCommitAiService> logger,
            IAiChatService aiChatService,
            IGeminiApiService geminiService,
            IModernGitService modernGitService,
            IEnumerable<IAiProvider> aiProviders,
            IUserSettingsDB userSettingsDB)
        {
            _logger = logger;
            _aiChatService = aiChatService;
            _geminiService = geminiService;
            _modernGitService = modernGitService;
            _aiProviders = aiProviders;
            _userSettingsDB = userSettingsDB;
        }

        public async Task<string> GenerateCommitMessageAsync(string repositoryPath, string language)
        {
            var lang = NormalizeLanguage(language);
            try
            {
                _logger.LogInformation("Generating commit message for repository: {RepositoryPath} (lang={Lang})",
                    repositoryPath, lang);

                // Get repository status
                var status = await _modernGitService.GetStatusAsync(repositoryPath);

                if (!HasChanges(status))
                {
                    _logger.LogInformation("No changes detected in repository");
                    return lang == "it" ? "Nessuna modifica da committare" : "No changes to commit";
                }

                // Collect changes information
                var changesInfo = await CollectChangesInfo(repositoryPath, status);

                // Generate prompt for AI
                var prompt = BuildCommitPrompt(changesInfo, lang);

                // Call AI to generate message
                var commitMessage = await CallAiForCommitMessage(prompt, repositoryPath, lang);

                _logger.LogInformation("Successfully generated commit message");
                return commitMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating commit message for repository: {RepositoryPath}", repositoryPath);
                return GenerateFallbackMessage(lang);
            }
        }

        private static string NormalizeLanguage(string language)
        {
            if (string.IsNullOrWhiteSpace(language)) return "en";
            var lower = language.Trim().ToLowerInvariant();
            // Accept "it", "it-IT", "italian" etc. — default to "en" for anything else
            if (lower.StartsWith("it")) return "it";
            return "en";
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

        private string BuildCommitPrompt(ChangesInfo changesInfo, string language)
        {
            var isIt = language == "it";
            var labels = GetPromptLabels(isIt);
            var prompt = new StringBuilder();

            if (isIt)
            {
                prompt.AppendLine("Analizza questi cambiamenti Git e genera un messaggio di commit in ITALIANO.");
                prompt.AppendLine("DEVI seguire le 7 regole di un buon commit message (Chris Beams):");
                prompt.AppendLine("  1. Separa il subject dal body con una riga vuota.");
                prompt.AppendLine("  2. Limita il subject a 50 caratteri (massimo assoluto).");
                prompt.AppendLine("  3. Inizia il subject con lettera maiuscola.");
                prompt.AppendLine("  4. Non terminare il subject con un punto.");
                prompt.AppendLine("  5. Usa il modo imperativo nel subject (es. \"Aggiungi\", \"Correggi\", \"Rimuovi\", non \"Aggiunto\"/\"Aggiunge\").");
                prompt.AppendLine("  6. Wrappa il body a 72 caratteri per riga.");
                prompt.AppendLine("  7. Nel body spiega COSA e PERCHÉ, non COME.");
                prompt.AppendLine();
                prompt.AppendLine("Il body è opzionale: ometti l'intero body (subject only) se le modifiche sono minori o auto-esplicative.");
                prompt.AppendLine();
            }
            else
            {
                prompt.AppendLine("Analyze these Git changes and generate a commit message in ENGLISH.");
                prompt.AppendLine("You MUST follow the 7 rules of a great Git commit message (Chris Beams):");
                prompt.AppendLine("  1. Separate subject from body with a blank line.");
                prompt.AppendLine("  2. Limit the subject line to 50 characters (hard cap).");
                prompt.AppendLine("  3. Capitalize the subject line.");
                prompt.AppendLine("  4. Do not end the subject line with a period.");
                prompt.AppendLine("  5. Use the imperative mood in the subject (e.g. \"Add\", \"Fix\", \"Remove\" — not \"Added\"/\"Adds\").");
                prompt.AppendLine("  6. Wrap the body at 72 characters.");
                prompt.AppendLine("  7. Use the body to explain WHAT and WHY vs. HOW.");
                prompt.AppendLine();
                prompt.AppendLine("The body is optional: omit it (subject only) when the change is small or self-explanatory.");
                prompt.AppendLine();
            }

            // Added files
            if (changesInfo.AddedFiles.Any())
            {
                prompt.AppendLine($"{labels.Added} ({changesInfo.AddedFiles.Count}):");
                foreach (var file in changesInfo.AddedFiles.Take(10))
                {
                    prompt.AppendLine($"  - {file}");
                    if (changesInfo.FileDiffs.ContainsKey(file))
                    {
                        prompt.AppendLine($"    {labels.Preview}: {changesInfo.FileDiffs[file].Substring(0, Math.Min(200, changesInfo.FileDiffs[file].Length))}...");
                    }
                }
                prompt.AppendLine();
            }

            // Modified files
            if (changesInfo.ModifiedFiles.Any())
            {
                prompt.AppendLine($"{labels.Modified} ({changesInfo.ModifiedFiles.Count}):");
                foreach (var file in changesInfo.ModifiedFiles.Take(10))
                {
                    prompt.AppendLine($"  - {file}");
                    if (changesInfo.FileDiffs.ContainsKey(file))
                    {
                        var diff = changesInfo.FileDiffs[file];
                        var diffPreview = diff.Length > 500 ? diff.Substring(0, 500) + "..." : diff;
                        prompt.AppendLine($"    {labels.Changes}:\n{diffPreview}");
                    }
                }
                prompt.AppendLine();
            }

            // Removed files
            if (changesInfo.RemovedFiles.Any())
            {
                prompt.AppendLine($"{labels.Removed} ({changesInfo.RemovedFiles.Count}):");
                foreach (var file in changesInfo.RemovedFiles.Take(10))
                {
                    prompt.AppendLine($"  - {file}");
                }
                prompt.AppendLine();
            }

            // Untracked files (new files not yet added to git)
            if (changesInfo.UntrackedFiles.Any())
            {
                prompt.AppendLine($"{labels.Untracked} ({changesInfo.UntrackedFiles.Count}):");
                foreach (var file in changesInfo.UntrackedFiles.Take(10))
                {
                    prompt.AppendLine($"  - {file}");
                    if (changesInfo.FileDiffs.ContainsKey(file))
                    {
                        prompt.AppendLine($"    {labels.Preview}: {changesInfo.FileDiffs[file].Substring(0, Math.Min(200, changesInfo.FileDiffs[file].Length))}...");
                    }
                }
                prompt.AppendLine();
            }

            if (isIt)
            {
                prompt.AppendLine("Vincoli aggiuntivi:");
                prompt.AppendLine("  - Niente prefissi tipo \"commit:\", \"git:\", \"message:\".");
                prompt.AppendLine("  - Niente blocchi markdown, backtick di apertura/chiusura, virgolette di contorno.");
                prompt.AppendLine("  - Niente frasi introduttive tipo \"Ecco il messaggio:\" o \"Il commit message è:\".");
                prompt.AppendLine("  - NON aggiungere trailer tipo \"Co-authored-by:\", \"Signed-off-by:\", \"Generated by:\" o link \"(mailto:...)\". SOLO subject e body puri.");
                prompt.AppendLine();
                prompt.AppendLine("Formato della risposta (ESATTO, nient'altro):");
                prompt.AppendLine("<Subject in imperativo, ≤50 char, prima lettera maiuscola, senza punto finale>");
                prompt.AppendLine("");
                prompt.AppendLine("<Body opzionale: paragrafi che spiegano COSA e PERCHÉ, righe ≤72 char. Ometti se non serve.>");
            }
            else
            {
                prompt.AppendLine("Additional constraints:");
                prompt.AppendLine("  - No prefixes like \"commit:\", \"git:\", \"message:\".");
                prompt.AppendLine("  - No markdown fences, wrapping backticks or quotes.");
                prompt.AppendLine("  - No lead-ins like \"Here's the commit message:\" or \"Commit message:\".");
                prompt.AppendLine("  - DO NOT append trailers like \"Co-authored-by:\", \"Signed-off-by:\", \"Generated by:\" or \"(mailto:...)\" links. Output ONLY the pure subject and body.");
                prompt.AppendLine();
                prompt.AppendLine("Response format (EXACT — nothing else):");
                prompt.AppendLine("<Imperative subject, ≤50 chars, capitalized, no trailing period>");
                prompt.AppendLine("");
                prompt.AppendLine("<Optional body: paragraphs explaining WHAT and WHY, lines ≤72 chars. Omit if unnecessary.>");
            }

            return prompt.ToString();
        }

        private struct PromptLabels
        {
            public string Added;
            public string Modified;
            public string Removed;
            public string Untracked;
            public string Preview;
            public string Changes;
        }

        private static PromptLabels GetPromptLabels(bool isIt) => isIt
            ? new PromptLabels
            {
                Added = "FILE AGGIUNTI",
                Modified = "FILE MODIFICATI",
                Removed = "FILE RIMOSSI",
                Untracked = "NUOVI FILE NON TRACCIATI",
                Preview = "Preview",
                Changes = "Modifiche"
            }
            : new PromptLabels
            {
                Added = "ADDED FILES",
                Modified = "MODIFIED FILES",
                Removed = "REMOVED FILES",
                Untracked = "UNTRACKED NEW FILES",
                Preview = "Preview",
                Changes = "Changes"
            };

        private async Task<string> CallAiForCommitMessage(string prompt, string repositoryPath, string language)
        {
            try
            {
                // Resolve preferred provider order:
                //   1. user's explicit AI_DefaultProvider setting (if set and available)
                //   2. Copilot CLI (matches the per-project auto-select behavior used by Mark Agent)
                //   3. Gemini
                //   4. Local (LLamaSharp)
                var preferredProvider = TryReadDefaultProviderSetting();
                var copilotProvider = _aiProviders?
                    .FirstOrDefault(p => p.GetProviderType() == ProviderType.CopilotCli) as CopilotCliProvider;

                var order = BuildProviderOrder(preferredProvider, copilotProvider);

                foreach (var candidate in order)
                {
                    var response = await TryInvokeProviderAsync(candidate, prompt, repositoryPath, copilotProvider);
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        return CleanCommitMessage(response);
                    }
                }

                _logger.LogWarning("No AI service available for commit message generation");
                return GenerateFallbackMessage(language);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling AI for commit message");
                return GenerateFallbackMessage(language);
            }
        }

        private static IEnumerable<string> BuildProviderOrder(string preferred, CopilotCliProvider copilot)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(preferred) && seen.Add(preferred))
                yield return preferred;

            // Copilot CLI first (silent auto-select path), then Gemini, then Local
            foreach (var fallback in new[] { "copilotcli", "gemini", "local" })
            {
                if (seen.Add(fallback)) yield return fallback;
            }
        }

        private async Task<string> TryInvokeProviderAsync(
            string providerKey,
            string prompt,
            string repositoryPath,
            CopilotCliProvider copilotProvider)
        {
            try
            {
                switch (providerKey?.ToLowerInvariant())
                {
                    case "copilotcli":
                        if (copilotProvider != null && copilotProvider.IsAvailable())
                        {
                            _logger.LogInformation("Using Copilot CLI for commit message generation");
                            // Ensure the CLI runs inside the repo (singleton WD may have drifted)
                            if (!string.IsNullOrEmpty(repositoryPath) && System.IO.Directory.Exists(repositoryPath))
                            {
                                copilotProvider.WorkingDirectory = repositoryPath;
                            }
                            return await copilotProvider.ChatAsync(prompt);
                        }
                        break;

                    case "gemini":
                        if (_geminiService.IsConfigured())
                        {
                            _logger.LogInformation("Using Gemini API for commit message generation");
                            return await _geminiService.ChatAsync(prompt, "gemini-1.5-flash");
                        }
                        break;

                    case "local":
                        if (_aiChatService.IsModelLoaded())
                        {
                            _logger.LogInformation("Using local AI model for commit message generation");
                            return await _aiChatService.ChatAsync(prompt);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Provider '{Provider}' failed generating commit message, trying next", providerKey);
            }
            return null;
        }

        private string TryReadDefaultProviderSetting()
        {
            // IUserSettingsDB is a shared NHibernate session (ReplaceDalFeatures);
            // even pure reads must happen inside an explicit BeginTransaction/Commit
            // or other controllers' Commit() will fail with TransactionException.
            try
            {
                _userSettingsDB.BeginTransaction();
                var settings = _userSettingsDB.GetDal<Setting>().GetList().ToList();
                _userSettingsDB.Commit();
                return settings.FirstOrDefault(s => s.Name == DefaultProviderKey)?.ValueString;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read AI_DefaultProvider setting (non-fatal)");
                try { _userSettingsDB.Rollback(); } catch { }
                return null;
            }
        }

        private string CleanCommitMessage(string aiResponse)
        {
            // Normalize line endings and strip markdown fences + wrapping quotes
            aiResponse = aiResponse.Replace("\r\n", "\n").Replace("\r", "\n");
            aiResponse = aiResponse.Replace("```", "").Trim();
            aiResponse = aiResponse.Trim('"', '\'', '`');

            // Strip common chatty prefixes ("commit:", "Ecco il messaggio:" ...)
            var prefixesToRemove = new[]
            {
                "commit:", "git:", "message:",
                "ecco il messaggio di commit:", "ecco il messaggio:",
                "il commit message è:", "messaggio di commit:",
                "here's the commit message:", "here is the commit message:",
                "commit message:"
            };
            bool stripped;
            do
            {
                stripped = false;
                foreach (var prefix in prefixesToRemove)
                {
                    if (aiResponse.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        aiResponse = aiResponse.Substring(prefix.Length).TrimStart();
                        stripped = true;
                    }
                }
            } while (stripped);

            var rawLines = aiResponse.Split('\n').Select(l => l.TrimEnd()).ToList();
            while (rawLines.Count > 0 && string.IsNullOrWhiteSpace(rawLines[0])) rawLines.RemoveAt(0);
            while (rawLines.Count > 0 && string.IsNullOrWhiteSpace(rawLines[rawLines.Count - 1]))
                rawLines.RemoveAt(rawLines.Count - 1);

            // Strip git-style trailers that Copilot CLI (and other providers) sometimes append
            // on their own: "Co-authored-by:", "Signed-off-by:", "(mailto:...)" fragments, etc.
            // We only scrub the trailing block — not mid-body text that happens to look similar.
            rawLines = StripTrailers(rawLines);

            if (rawLines.Count == 0) return string.Empty;

            // --- Subject (rules 1-5): capitalize, strip trailing punctuation, no leading markers
            var subject = rawLines[0].Trim().TrimStart('-', '*', '•', ' ', '\t');
            subject = subject.TrimEnd('.', '!', '?', ';', ':', ' ');
            if (subject.Length > 0 && char.IsLower(subject[0]))
            {
                subject = char.ToUpper(subject[0]) + subject.Substring(1);
            }

            // Pull body lines (skip blank separator)
            var bodyLines = rawLines.Skip(1).ToList();
            while (bodyLines.Count > 0 && string.IsNullOrWhiteSpace(bodyLines[0])) bodyLines.RemoveAt(0);

            if (bodyLines.Count == 0)
            {
                // Subject-only: rule 1 doesn't apply (no body → no blank line needed)
                return subject;
            }

            // --- Body (rules 1, 6, 7): blank line separator + wrap at 72 chars
            var wrappedBody = WrapBody(bodyLines, 72);
            return subject + "\n\n" + wrappedBody;
        }

        private static readonly System.Text.RegularExpressions.Regex TrailerRegex =
            new System.Text.RegularExpressions.Regex(
                @"^(Co-authored-by|Signed-off-by|Reported-by|Suggested-by|Reviewed-by|Tested-by|Acked-by|Helped-by|Mentored-by|Generated[- ]by|Assisted-by):\s",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        private static readonly System.Text.RegularExpressions.Regex MailtoNoiseRegex =
            new System.Text.RegularExpressions.Regex(
                @"^\(?\s*mailto:[^\s)]+\)?$|users\.noreply\.github\.com",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        private static List<string> StripTrailers(List<string> lines)
        {
            // Walk from the end; drop trailer-looking lines, blank separators adjacent to them,
            // and orphan "(mailto:...)" / "users.noreply.github.com" lines.
            // Stop as soon as we hit a real content line.
            int lastKeep = lines.Count - 1;
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                if (TrailerRegex.IsMatch(line) || MailtoNoiseRegex.IsMatch(line))
                {
                    lastKeep = i - 1;
                    continue;
                }
                break;
            }

            if (lastKeep < lines.Count - 1)
            {
                lines = lines.Take(lastKeep + 1).ToList();
                // Re-trim trailing blanks left behind
                while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[lines.Count - 1]))
                    lines.RemoveAt(lines.Count - 1);
            }

            return lines;
        }

        private static string WrapBody(IEnumerable<string> bodyLines, int maxWidth)
        {
            var output = new StringBuilder();
            bool firstBlock = true;
            var paragraph = new List<string>();

            void FlushParagraph()
            {
                if (paragraph.Count == 0) return;
                if (!firstBlock) output.Append('\n');
                // Preserve list-style lines (start with "-", "*", "•", digit+".") without re-wrapping aggressively
                var isList = paragraph.All(l =>
                {
                    var t = l.TrimStart();
                    return t.StartsWith("- ") || t.StartsWith("* ") || t.StartsWith("• ")
                           || System.Text.RegularExpressions.Regex.IsMatch(t, @"^\d+\.\s");
                });
                if (isList)
                {
                    foreach (var l in paragraph)
                    {
                        output.Append(WrapLine(l, maxWidth));
                        output.Append('\n');
                    }
                    // drop trailing newline, let the next block or end-of-body decide
                    if (output.Length > 0 && output[output.Length - 1] == '\n')
                        output.Length--;
                }
                else
                {
                    var joined = string.Join(" ", paragraph.Select(l => l.Trim()));
                    output.Append(WrapLine(joined, maxWidth));
                }
                firstBlock = false;
                paragraph.Clear();
            }

            foreach (var line in bodyLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    FlushParagraph();
                    output.Append('\n');
                }
                else
                {
                    paragraph.Add(line);
                }
            }
            FlushParagraph();
            return output.ToString().TrimEnd('\n');
        }

        private static string WrapLine(string text, int maxWidth)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxWidth) return text;

            var words = text.Split(' ');
            var sb = new StringBuilder();
            var current = new StringBuilder();
            foreach (var word in words)
            {
                if (current.Length == 0)
                {
                    current.Append(word);
                }
                else if (current.Length + 1 + word.Length <= maxWidth)
                {
                    current.Append(' ').Append(word);
                }
                else
                {
                    sb.Append(current).Append('\n');
                    current.Clear();
                    current.Append(word);
                }
            }
            if (current.Length > 0) sb.Append(current);
            return sb.ToString();
        }

        private string GenerateFallbackMessage(string language)
        {
            return language == "it"
                ? $"Aggiornamento del {DateTime.Now:yyyy-MM-dd HH:mm}"
                : $"Update {DateTime.Now:yyyy-MM-dd HH:mm}";
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