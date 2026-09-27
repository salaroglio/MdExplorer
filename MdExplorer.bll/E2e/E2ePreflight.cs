using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.E2e
{
    public sealed class E2ePreflightResult
    {
        /// <summary>The test as read, or null when the file cannot be read at all.</summary>
        public E2eTestDocument Document { get; init; }

        /// <summary>What blocks the run, each with what to do.</summary>
        public IReadOnlyList<string> Errors { get; init; }

        /// <summary>What the run survives but the user should know.</summary>
        public IReadOnlyList<string> Warnings { get; init; }

        public bool CanRun => Errors.Count == 0;
    }

    /// <summary>
    /// The checks MdExplorer makes before handing a test to an agent (F2). They are deterministic on
    /// purpose: a missing credential key is a silent failure once the test runs (the Playwright server
    /// types the key's name when it does not know it, verified 27/09/2026), and a credentials file that
    /// ends up in git cannot be taken back.
    /// </summary>
    public static class E2ePreflight
    {
        private static readonly System.Text.RegularExpressions.Regex ValidKey = new(@"^[A-Za-z0-9_.-]+$");

        public static E2ePreflightResult Check(string testFilePath)
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var fileName = Path.GetFileName(testFilePath);

            if (!testFilePath.EndsWith(".e2e.md", StringComparison.OrdinalIgnoreCase))
                errors.Add($"{fileName}: un file di test deve finire in '.e2e.md'.");

            E2eTestDocument document;
            try
            {
                document = E2eTestParser.Parse(File.ReadAllText(testFilePath), fileName);
            }
            catch (E2eFormatException ex)
            {
                errors.Add(ex.Message);
                return new E2ePreflightResult { Errors = errors, Warnings = warnings };
            }
            errors.AddRange(document.Problems);

            var folder = Path.GetDirectoryName(Path.GetFullPath(testFilePath));

            if (document.SiteMap != null && !File.Exists(Path.Combine(folder, document.SiteMap)))
                warnings.Add($"{fileName}: la mappa del sito '{document.SiteMap}' non esiste ancora: la creerà l'agente alla prima esecuzione.");

            var keys = document.CredentialKeys.ToList();
            if (document.Credentials != null)
            {
                var credentialsPath = Path.Combine(folder, document.Credentials);
                if (File.Exists(credentialsPath))
                {
                    var known = ReadCredentialKeys(credentialsPath);
                    // The Playwright server's dotenv drops, in silence, any key outside [\w.-] (ASCII): it would
                    // then type the key's name (@playwright/mcp 0.0.82, utilsBundle.js). Checked here instead.
                    foreach (var key in keys.Concat(known).Distinct().Where(k => !ValidKey.IsMatch(k)))
                        errors.Add($"{fileName}: la chiave '{key}' ha caratteri che il server Playwright non accetta: usa solo lettere, cifre, '_', '.' e '-' (senza accenti).");
                    foreach (var key in keys.Where(k => !known.Contains(k)))
                        errors.Add($"{fileName}: la chiave '{key}' non c'è in '{document.Credentials}'. Aggiungi la riga '{key}=<valore>' oppure correggi il nome nel test.");
                    CheckNotInGit(credentialsPath, document.Credentials, fileName, errors);
                }
                else if (keys.Count > 0)
                {
                    errors.Add($"{fileName}: il file delle credenziali '{document.Credentials}' non esiste. Crealo accanto al test, con una riga chiave=valore per: {string.Join(", ", keys)}.");
                }
            }

            return new E2ePreflightResult { Document = document, Errors = errors, Warnings = warnings };
        }

        /// <summary>The keys of a credentials file: lines <c>key=value</c>, <c>#</c> for comments (as E2eSupport.cs reads it).</summary>
        public static HashSet<string> ReadCredentialKeys(string path) =>
            File.ReadAllLines(path)
                .Select(l => l.Trim())
                .Where(l => l.Length > 0 && !l.StartsWith("#", StringComparison.Ordinal) && l.IndexOf('=') > 0)
                .Select(l => l.Substring(0, l.IndexOf('=')).Trim())
                .ToHashSet(StringComparer.Ordinal);

        private static void CheckNotInGit(string credentialsPath, string credentialsName, string fileName, List<string> errors)
        {
            var gitDir = Repository.Discover(Path.GetDirectoryName(credentialsPath));
            if (gitDir == null) return; // not a git repository: nothing can be committed

            using var repo = new Repository(gitDir);
            var relative = Path.GetRelativePath(repo.Info.WorkingDirectory, credentialsPath).Replace('\\', '/');

            if (repo.Index[relative] != null)
                errors.Add($"{fileName}: il file delle credenziali '{credentialsName}' è già nel repository git. Toglilo dall'indice (git rm --cached \"{relative}\") e aggiungi 'credenziali-*.txt' al .gitignore.");
            else if (!repo.Ignore.IsPathIgnored(relative))
                errors.Add($"{fileName}: il file delle credenziali '{credentialsName}' non è escluso da git. Aggiungi la riga 'credenziali-*.txt' al .gitignore del progetto.");
        }
    }
}
