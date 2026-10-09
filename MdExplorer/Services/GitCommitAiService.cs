using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using MdExplorer.Abstractions.Services;

namespace MdExplorer.Services
{
    /// <summary>
    /// The deterministic half of the AI commit message: the changes, the prompt, the cleaning of the
    /// answer. The question is asked by the client on a channel of the AI chat
    /// (<c>AiChatService.askOnChannel</c>), so it goes into the same CLI session as the MarkAgent
    /// tab: the agent that did the work, or talked about it, writes the WHY (user's decision,
    /// 25/09/2026 — before, a one-shot call with no memory, through a chain of providers that
    /// fell back to the next, and then to a generic message, in silence).
    /// Sprint: docs-internal/Sprints/2026-09-25-Commit-AI-Sessione-Del-Tab.md
    /// </summary>
    public interface IGitCommitAiService
    {
        /// <summary>The prompt for the changes of <paramref name="repositoryPath"/>; null when there is nothing to commit.</summary>
        Task<string> BuildCommitPromptAsync(string repositoryPath, string language);

        /// <summary>
        /// The agent's answer as a commit message: a Conventional Commits header, a blank line, the
        /// body wrapped at 72. Empty when nothing is left.
        /// </summary>
        string CleanCommitMessage(string aiResponse);
    }

    public class GitCommitAiService : IGitCommitAiService
    {
        private readonly ILogger<GitCommitAiService> _logger;
        private readonly IModernGitService _modernGitService;
        private const int MaxDiffLinesPerFile = 100;
        private const int MaxFilesToAnalyze = 20;
        private const int MaxHeaderLength = 72;

        /// <summary>The types of Conventional Commits the prompt asks for and the cleaning recognises.</summary>
        private static readonly string[] ConventionalTypes =
            { "feat", "fix", "docs", "style", "refactor", "perf", "test", "build", "ci", "chore", "revert" };
        private static readonly string ConventionalTypesList = string.Join(", ", ConventionalTypes);

        /// <summary><c>type(scope)!: description</c> — the header of a Conventional Commit.</summary>
        private static readonly System.Text.RegularExpressions.Regex ConventionalHeaderRegex =
            new System.Text.RegularExpressions.Regex(
                @"^(?<type>[A-Za-z]+)\s*(?:\(\s*(?<scope>[^()]*?)\s*\))?\s*(?<bang>!)?\s*:\s*(?<description>\S.*)$");

        public GitCommitAiService(
            ILogger<GitCommitAiService> logger,
            IModernGitService modernGitService)
        {
            _logger = logger;
            _modernGitService = modernGitService;
        }

        public async Task<string> BuildCommitPromptAsync(string repositoryPath, string language)
        {
            var lang = NormalizeLanguage(language);
            var status = await _modernGitService.GetStatusAsync(repositoryPath);
            if (!HasChanges(status))
            {
                _logger.LogInformation("No changes detected in repository {RepositoryPath}", repositoryPath);
                return null;
            }
            var changesInfo = await CollectChangesInfo(repositoryPath, status);
            return BuildCommitPrompt(changesInfo, lang);
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

            // The format is the user's decision of 01/10/2026: the language chosen in the app, a
            // Conventional Commits header (the short line a log shows) and a body of two very short
            // sentences. Type and scope stay in English in every language: they are the convention's
            // keywords, tools read them.
            if (isIt)
            {
                prompt.AppendLine("Analizza questi cambiamenti Git e scrivi il messaggio di commit.");
                prompt.AppendLine();
                prompt.AppendLine("LINGUA: scrivi in ITALIANO la descrizione e il corpo, anche se la nostra conversazione è in un'altra lingua.");
                prompt.AppendLine("Restano in inglese solo il tipo e lo scope, che sono parole chiave della convenzione.");
                prompt.AppendLine();
                prompt.AppendLine("FORMATO: Conventional Commits 1.0.0.");
                prompt.AppendLine("  1. Prima riga: <tipo>(<scope>): <descrizione>. È la frase sintetica che si legge nell'elenco dei commit.");
                prompt.AppendLine($"  2. Il tipo è uno fra: {ConventionalTypesList}.");
                prompt.AppendLine("     feat = funzione nuova, fix = correzione di un difetto, docs = solo documentazione, refactor = stesso comportamento.");
                prompt.AppendLine("  3. Lo scope è facoltativo: una parola minuscola che dice l'area toccata. Se non c'è un'area chiara, omettilo insieme alle parentesi.");
                prompt.AppendLine("  4. La descrizione è al modo imperativo (\"aggiungi\", \"correggi\", \"rimuovi\"), con iniziale minuscola e senza punto finale.");
                prompt.AppendLine($"  5. L'intera prima riga non supera {MaxHeaderLength} caratteri.");
                prompt.AppendLine("  6. Se la modifica rompe la compatibilità, metti \"!\" subito prima dei due punti.");
                prompt.AppendLine("  7. Poi una riga vuota e il corpo: ESATTAMENTE due frasi molto sintetiche, al massimo 15 parole l'una.");
                prompt.AppendLine("     La prima dice COSA cambia, la seconda PERCHÉ. Niente elenchi, niente nomi di file in fila, niente terza frase.");
                prompt.AppendLine();
            }
            else
            {
                prompt.AppendLine("Analyze these Git changes and write the commit message.");
                prompt.AppendLine();
                prompt.AppendLine("LANGUAGE: write the description and the body in ENGLISH, even if our conversation is in another language.");
                prompt.AppendLine();
                prompt.AppendLine("FORMAT: Conventional Commits 1.0.0.");
                prompt.AppendLine("  1. First line: <type>(<scope>): <description>. It is the short sentence shown in the list of commits.");
                prompt.AppendLine($"  2. The type is one of: {ConventionalTypesList}.");
                prompt.AppendLine("     feat = new feature, fix = bug fix, docs = documentation only, refactor = same behaviour.");
                prompt.AppendLine("  3. The scope is optional: one lowercase word for the area touched. With no clear area, omit it and its parentheses.");
                prompt.AppendLine("  4. The description is in the imperative mood (\"add\", \"fix\", \"remove\"), starts lowercase, has no trailing period.");
                prompt.AppendLine($"  5. The whole first line is at most {MaxHeaderLength} characters.");
                prompt.AppendLine("  6. For a breaking change, put \"!\" right before the colon.");
                prompt.AppendLine("  7. Then a blank line and the body: EXACTLY two very short sentences, 15 words each at most.");
                prompt.AppendLine("     The first says WHAT changes, the second WHY. No lists, no rows of file names, no third sentence.");
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
                prompt.AppendLine("Il PERCHÉ: se nella nostra conversazione abbiamo fatto o discusso queste modifiche, usa quello che");
                prompt.AppendLine("sai (lo scopo, la decisione presa, il problema risolto) per la seconda frase. Non inventare: se non ne");
                prompt.AppendLine("sai niente, descrivi solo quello che si vede dalle modifiche. Non usare i tuoi tool: le modifiche sono qui.");
                prompt.AppendLine();
                prompt.AppendLine("Vincoli aggiuntivi:");
                prompt.AppendLine("  - Niente etichette davanti al messaggio, tipo \"commit:\" o \"messaggio:\".");
                prompt.AppendLine("  - Niente blocchi markdown, backtick di apertura/chiusura, virgolette di contorno.");
                prompt.AppendLine("  - Niente frasi introduttive tipo \"Ecco il messaggio:\" o \"Il commit message è:\".");
                prompt.AppendLine("  - NON aggiungere trailer tipo \"Co-authored-by:\", \"Signed-off-by:\", \"Generated by:\" o link \"(mailto:...)\".");
                prompt.AppendLine();
                prompt.AppendLine("Formato della risposta (ESATTO, nient'altro):");
                prompt.AppendLine("<tipo>(<scope>): <descrizione in italiano>");
                prompt.AppendLine("");
                prompt.AppendLine("<Prima frase: cosa cambia.> <Seconda frase: perché.>");
            }
            else
            {
                prompt.AppendLine("The WHY: if these changes were made or discussed in our conversation, use what you know (the aim,");
                prompt.AppendLine("the decision taken, the problem solved) for the second sentence. Do not invent: if you know nothing about");
                prompt.AppendLine("them, describe only what the changes show. Do not use your tools: the changes are here.");
                prompt.AppendLine();
                prompt.AppendLine("Additional constraints:");
                prompt.AppendLine("  - No labels in front of the message, like \"commit:\" or \"message:\".");
                prompt.AppendLine("  - No markdown fences, wrapping backticks or quotes.");
                prompt.AppendLine("  - No lead-ins like \"Here's the commit message:\" or \"Commit message:\".");
                prompt.AppendLine("  - DO NOT append trailers like \"Co-authored-by:\", \"Signed-off-by:\", \"Generated by:\" or \"(mailto:...)\" links.");
                prompt.AppendLine();
                prompt.AppendLine("Response format (EXACT — nothing else):");
                prompt.AppendLine("<type>(<scope>): <description in English>");
                prompt.AppendLine("");
                prompt.AppendLine("<First sentence: what changes.> <Second sentence: why.>");
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

        public string CleanCommitMessage(string aiResponse)
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

            // --- Header: no leading markers, no trailing punctuation. A Conventional Commits header
            // is put in its canonical spelling (lowercase type and scope, one space after the colon);
            // anything else is left as the agent wrote it, for the user to see and correct in the box.
            var subject = rawLines[0].Trim().TrimStart('-', '*', '•', ' ', '\t');
            subject = subject.TrimEnd('.', '!', '?', ';', ':', ' ');
            subject = NormalizeConventionalHeader(subject);

            // Pull body lines (skip blank separator)
            var bodyLines = rawLines.Skip(1).ToList();
            while (bodyLines.Count > 0 && string.IsNullOrWhiteSpace(bodyLines[0])) bodyLines.RemoveAt(0);

            if (bodyLines.Count == 0)
            {
                // Subject-only: rule 1 doesn't apply (no body → no blank line needed)
                return subject;
            }

            // --- Body: blank line separator + wrap at 72 chars
            var wrappedBody = WrapBody(bodyLines, 72);
            return subject + "\n\n" + wrappedBody;
        }

        private static string NormalizeConventionalHeader(string subject)
        {
            var match = ConventionalHeaderRegex.Match(subject);
            if (!match.Success) return subject;
            var type = match.Groups["type"].Value.ToLowerInvariant();
            if (!ConventionalTypes.Contains(type)) return subject;

            var scope = match.Groups["scope"].Success ? match.Groups["scope"].Value.Trim().ToLowerInvariant() : string.Empty;
            var header = type;
            if (scope.Length > 0) header += "(" + scope + ")";
            if (match.Groups["bang"].Success) header += "!";
            return header + ": " + match.Groups["description"].Value.Trim();
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