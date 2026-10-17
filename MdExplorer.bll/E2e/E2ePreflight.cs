using MdExplorer.Features.Services;
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

        /// <summary>
        /// The credentials file is in a git repository and not excluded by .gitignore: the dialog offers the button that
        /// adds the line. A flag, not a search in <see cref="Errors"/>, whose text follows the interface language.
        /// </summary>
        public bool CredentialsNotIgnored { get; init; }

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
                errors.Add(UiText.Say($"{fileName}: un file di test deve finire in '.e2e.md'.", $"{fileName}: a test file must end in '.e2e.md'."));

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

            // D10 of sprint 2026-09-29-Motore-LLM-Unico: the tests always run in a session of their own.
            if (document.Run?.ObsoleteDedicatedSession == true)
                warnings.Add(UiText.Say($"{fileName}: 'e2e.run.dedicatedSession' non si usa più: i test girano sempre in una sessione dedicata. Puoi togliere la riga (la finestra dei test la toglie quando salva).",
                    $"{fileName}: 'e2e.run.dedicatedSession' is no longer used: the tests always run in a dedicated session. You can remove the line (the tests window removes it when it saves)."));

            // P7: without a check the script can assert, its replay passes whatever the page does (seen on 28/09/2026).
            foreach (var test in document.Tests.Where(t => !t.Steps.Any(s => s.Check is { } kind && kind != E2eCheckKind.Judgment)))
                warnings.Add(UiText.Say($"{fileName}: T{test.Number} non ha verifiche ✔ che lo script possa controllare: il rigioco risulterà superato qualunque cosa faccia la pagina. Aggiungi almeno una verifica nelle forme della skill mde-e2e (es. ✔ Compare il testo \"…\").",
                    $"{fileName}: T{test.Number} has no ✔ checks the script can verify: the replay will pass whatever the page does. Add at least one check in the forms of the mde-e2e skill (e.g. ✔ Compare il testo \"…\")."));

            if (document.SiteMap != null && !File.Exists(Path.Combine(folder, document.SiteMap)))
                warnings.Add(UiText.Say($"{fileName}: la mappa del sito '{document.SiteMap}' non esiste ancora: la creerà l'agente alla prima esecuzione.",
                    $"{fileName}: the site map '{document.SiteMap}' does not exist yet: the agent will create it on the first run."));

            var keys = document.CredentialKeys.ToList();
            var notIgnored = false;
            if (document.Credentials == null && keys.Count > 0)
                errors.Add(UiText.Say($"{fileName}: il test usa delle credenziali ({string.Join(", ", keys)}) ma nel front matter manca 'e2e.credentials' (il file .txt con le righe chiave=valore).",
                    $"{fileName}: the test uses credentials ({string.Join(", ", keys)}) but 'e2e.credentials' is missing from the front matter (the .txt file with key=value lines)."));
            if (document.Credentials != null)
            {
                var credentialsPath = Path.Combine(folder, document.Credentials);
                if (File.Exists(credentialsPath))
                {
                    var known = ReadCredentialKeys(credentialsPath);
                    // The Playwright server's dotenv drops, in silence, any key outside [\w.-] (ASCII): it would
                    // then type the key's name (@playwright/mcp 0.0.82, utilsBundle.js). Checked here instead.
                    foreach (var key in keys.Concat(known).Distinct().Where(k => !ValidKey.IsMatch(k)))
                        errors.Add(UiText.Say($"{fileName}: la chiave '{key}' ha caratteri che il server Playwright non accetta: usa solo lettere, cifre, '_', '.' e '-' (senza accenti).",
                            $"{fileName}: the key '{key}' has characters the Playwright server does not accept: use only letters, digits, '_', '.' and '-' (no accents)."));
                    foreach (var key in keys.Where(k => known.Contains(k) && EmptyValue(credentialsPath, k)))
                        errors.Add(UiText.Say($"{fileName}: la chiave '{key}' in '{document.Credentials}' non ha un valore.", $"{fileName}: the key '{key}' in '{document.Credentials}' has no value."));
                    foreach (var key in keys.Where(k => !known.Contains(k)))
                        errors.Add(UiText.Say($"{fileName}: la chiave '{key}' non c'è in '{document.Credentials}'. Aggiungi la riga '{key}=<valore>' oppure correggi il nome nel test.",
                            $"{fileName}: the key '{key}' is not in '{document.Credentials}'. Add the line '{key}=<value>' or fix the name in the test."));
                    notIgnored = CheckNotInGit(credentialsPath, document.Credentials, fileName, errors);
                }
                else if (keys.Count > 0)
                {
                    errors.Add(UiText.Say($"{fileName}: il file delle credenziali '{document.Credentials}' non esiste. Crealo accanto al test, con una riga chiave=valore per: {string.Join(", ", keys)}.",
                        $"{fileName}: the credentials file '{document.Credentials}' does not exist. Create it next to the test, with a key=value line for: {string.Join(", ", keys)}."));
                }
            }

            return new E2ePreflightResult { Document = document, Errors = errors, Warnings = warnings, CredentialsNotIgnored = notIgnored };
        }

        /// <summary>The keys of a credentials file: lines <c>key=value</c>, <c>#</c> for comments (as E2eSupport.cs reads it).</summary>
        public static HashSet<string> ReadCredentialKeys(string path) =>
            File.ReadAllLines(path)
                .Select(l => l.Trim())
                .Where(l => l.Length > 0 && !l.StartsWith("#", StringComparison.Ordinal) && l.IndexOf('=') > 0)
                .Select(l => l.Substring(0, l.IndexOf('=')).Trim())
                .ToHashSet(StringComparer.Ordinal);

        /// <summary>The .gitignore line that excludes a credentials file: the skill's pattern when the name follows it.</summary>
        public static string GitIgnoreLine(string credentialsName)
        {
            var name = Path.GetFileName(credentialsName);
            return System.Text.RegularExpressions.Regex.IsMatch(name, @"^credenziali-.+\.txt$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                ? "credenziali-*.txt" : name;
        }

        private static bool EmptyValue(string path, string key) =>
            File.ReadAllLines(path).Select(l => l.Trim())
                .Where(l => !l.StartsWith("#", StringComparison.Ordinal) && l.IndexOf('=') > 0)
                .Any(l => l.Substring(0, l.IndexOf('=')).Trim() == key && l.Substring(l.IndexOf('=') + 1).Trim().Length == 0);

        /// <returns>True when the file is not excluded by .gitignore (the dialog's button fixes it).</returns>
        private static bool CheckNotInGit(string credentialsPath, string credentialsName, string fileName, List<string> errors)
        {
            var gitDir = Repository.Discover(Path.GetDirectoryName(credentialsPath));
            if (gitDir == null) return false; // not a git repository: nothing can be committed

            using var repo = new Repository(gitDir);
            var relative = Path.GetRelativePath(repo.Info.WorkingDirectory, credentialsPath).Replace('\\', '/');

            if (repo.Index[relative] != null)
                errors.Add(UiText.Say($"{fileName}: il file delle credenziali '{credentialsName}' è già nel repository git. Toglilo dall'indice (git rm --cached \"{relative}\") e aggiungi 'credenziali-*.txt' al .gitignore.",
                    $"{fileName}: the credentials file '{credentialsName}' is already in the git repository. Remove it from the index (git rm --cached \"{relative}\") and add 'credenziali-*.txt' to .gitignore."));
            else if (!repo.Ignore.IsPathIgnored(relative))
            {
                errors.Add(UiText.Say($"{fileName}: il file delle credenziali '{credentialsName}' non è escluso da git. Aggiungi la riga '{GitIgnoreLine(credentialsName)}' al .gitignore del progetto (c'è un pulsante nella finestra dei test).",
                    $"{fileName}: the credentials file '{credentialsName}' is not excluded from git. Add the line '{GitIgnoreLine(credentialsName)}' to the project's .gitignore (there is a button in the tests window)."));
                return true;
            }
            return false;
        }
    }
}
