using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Una skill che un pacchetto di linguaggio porta con sé, per gli agenti che lavorano su quel codice.</summary>
    public sealed class LanguagePackageSkill
    {
        /// <summary>Il nome della skill = il nome della sua cartella, col prefisso <c>&lt;id&gt;-</c>.</summary>
        public string Name { get; init; }
        public string Description { get; init; }
        /// <summary>I file della skill, per percorso relativo alla sua cartella (<c>SKILL.md</c> e gli altri <c>.md</c>).</summary>
        public IReadOnlyDictionary<string, string> Files { get; init; }
    }

    /// <summary>
    /// Un linguaggio come pacchetto modulare: una cartella <c>.mde-languages/&lt;id&gt;/</c> con le regole
    /// (<c>language.yml</c>, <see cref="DeclarativeLanguage"/>), il prompt che si aggiunge a quello di Mark per i sorgenti
    /// di quel linguaggio (<c>prompts/code-doc.md</c>) e le skill per gli agenti (<c>skills/&lt;id&gt;-…/SKILL.md</c>).
    /// Aggiungere un linguaggio = copiare una cartella, toglierlo = cancellarla; MdExplorer non ne porta con sé, i pacchetti
    /// stanno fuori dal suo repository. Lettura <b>rigida</b> come per le regole: un file che il formato non conosce, una
    /// skill senza nome o descrizione, o con un nome senza il prefisso, sono errori con il file e il motivo.
    /// </summary>
    public sealed class LanguagePackage
    {
        public const string DefinitionFile = "language.yml";
        public const string ReadmeFile = "README.md";
        public const string PromptsFolder = "prompts";
        public const string CodeDocPromptFile = "code-doc.md";
        public const string SkillsFolder = "skills";
        public const string SkillFile = "SKILL.md";
        /// <summary>Quanto può essere grande un file del pacchetto: sono regole e testi, non sorgenti.</summary>
        public const int MaxFileBytes = 256 * 1024;

        private static readonly Regex IdPattern = new("^[a-z][a-z0-9-]*$", RegexOptions.CultureInvariant);

        public string Id { get; private init; }
        /// <summary>La cartella del pacchetto (assoluta, sul disco).</summary>
        public string FolderPath { get; private init; }
        public DeclarativeLanguage Language { get; private init; }
        /// <summary>Il testo di <c>prompts/code-doc.md</c>, o null se il pacchetto non ne ha.</summary>
        public string CodeDocPrompt { get; private init; }
        /// <summary>Le prime 8 cifre dello SHA-256 del prompt: entra nella versione del prompt di una descrizione (B6b).</summary>
        public string CodeDocPromptHash { get; private init; }
        public IReadOnlyList<LanguagePackageSkill> Skills { get; private init; }

        /// <summary>Le cartelle dei pacchetti del progetto, in ordine; nessuna se <c>.mde-languages/</c> non c'è.</summary>
        public static IReadOnlyList<string> FoldersOf(string projectPath)
        {
            var root = DeclarativeLanguage.FolderOf(projectPath ?? string.Empty);
            if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(root)) return Array.Empty<string>();
            return Directory.EnumerateDirectories(root).OrderBy(d => d, StringComparer.Ordinal).ToList();
        }

        /// <summary>Un pacchetto dal disco: tutti i suoi file letti, poi controllati da <see cref="FromFiles"/>.</summary>
        public static LanguagePackage Load(string folder)
        {
            var files = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(folder, file).Replace('\\', '/');
                if (new FileInfo(file).Length > MaxFileBytes)
                    throw new LanguageDefinitionException(file, UiText.Say($"più di {MaxFileBytes / 1024} KB: un pacchetto ha regole e testi, non sorgenti", $"larger than {MaxFileBytes / 1024} KB: a package holds rules and texts, not sources"));
                files[rel] = File.ReadAllText(file, Encoding.UTF8);
            }
            return FromFiles(Path.GetFileName(folder), folder, files);
        }

        /// <summary>
        /// Un pacchetto dai suoi file (percorso relativo → testo): dal disco, e domani da uno zip da importare. Gli errori
        /// nominano il file sotto <paramref name="folder"/>.
        /// </summary>
        public static LanguagePackage FromFiles(string id, string folder, IReadOnlyDictionary<string, string> files)
        {
            string PathOf(string rel) => Path.Combine(folder, rel.Replace('/', Path.DirectorySeparatorChar));
            LanguageDefinitionException Fail(string rel, string italian, string english) => new(PathOf(rel), UiText.Say(italian, english));

            if (!IdPattern.IsMatch(id ?? string.Empty))
                throw new LanguageDefinitionException(folder, UiText.Say($"'{id}' non è un nome di pacchetto: solo minuscole, cifre e trattini, e comincia con una lettera", $"'{id}' is not a package name: lowercase letters, digits and dashes only, starting with a letter"));
            if (!files.TryGetValue(DefinitionFile, out var yaml))
                throw Fail(DefinitionFile, "manca: è il file con le regole del linguaggio", "missing: it is the file with the language rules");

            var skillFiles = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            string prompt = null;
            foreach (var (rel, text) in files.OrderBy(f => f.Key, StringComparer.Ordinal))
            {
                if (rel == DefinitionFile || rel == ReadmeFile) continue;
                if (rel == PromptsFolder + "/" + CodeDocPromptFile) { prompt = text; continue; }
                var parts = rel.Split('/');
                if (parts.Length >= 3 && parts[0] == SkillsFolder && rel.EndsWith(".md", StringComparison.Ordinal))
                {
                    if (!skillFiles.TryGetValue(parts[1], out var mine)) skillFiles[parts[1]] = mine = new Dictionary<string, string>(StringComparer.Ordinal);
                    mine[string.Join('/', parts.Skip(2))] = text;
                    continue;
                }
                throw Fail(rel, $"file non previsto in un pacchetto di linguaggio (ammessi: {DefinitionFile}, {ReadmeFile}, {PromptsFolder}/{CodeDocPromptFile}, {SkillsFolder}/{id}-…/*.md)",
                    $"file not expected in a language package (allowed: {DefinitionFile}, {ReadmeFile}, {PromptsFolder}/{CodeDocPromptFile}, {SkillsFolder}/{id}-…/*.md)");
            }
            if (prompt != null && string.IsNullOrWhiteSpace(prompt))
                throw Fail(PromptsFolder + "/" + CodeDocPromptFile, "è vuoto: se il linguaggio non ha niente da aggiungere al prompt, il file non serve", "it is empty: if the language has nothing to add to the prompt, the file is not needed");

            var skills = skillFiles.OrderBy(k => k.Key, StringComparer.Ordinal).Select(k => ReadSkill(id, k.Key, k.Value, PathOf)).ToList();
            return new LanguagePackage
            {
                Id = id, FolderPath = folder,
                Language = DeclarativeLanguage.Parse(yaml, PathOf(DefinitionFile)),
                CodeDocPrompt = prompt?.Trim(), CodeDocPromptHash = prompt == null ? null : Hash(prompt.Trim()),
                Skills = skills,
            };
        }

        /// <summary>
        /// Una skill del pacchetto: <c>SKILL.md</c> con l'intestazione YAML <c>name</c> (uguale alla cartella, col prefisso
        /// <c>&lt;id&gt;-</c>, così non si scontra con le skill di MdExplorer né con quelle di un altro pacchetto) e
        /// <c>description</c>. Il blocco <c>mde:</c> lo scrive MdExplorer quando la installa (B6c), non chi scrive il pacchetto.
        /// </summary>
        private static LanguagePackageSkill ReadSkill(string id, string name, Dictionary<string, string> files, Func<string, string> pathOf)
        {
            var skillRel = $"{SkillsFolder}/{name}/{SkillFile}";
            LanguageDefinitionException Fail(string italian, string english) => new(pathOf(skillRel), UiText.Say(italian, english));
            if (!name.StartsWith(id + "-", StringComparison.Ordinal) || !IdPattern.IsMatch(name))
                throw Fail($"la cartella della skill si chiama '{name}': il nome comincia con '{id}-' ed è fatto di minuscole, cifre e trattini", $"the skill folder is named '{name}': the name starts with '{id}-' and is made of lowercase letters, digits and dashes");
            if (!files.TryGetValue(SkillFile, out var text)) throw Fail("manca: ogni skill ha il suo SKILL.md", "missing: every skill has its SKILL.md");
            var m = Regex.Match(text, @"\A---\r?\n(?<yaml>.*?)\r?\n---\r?\n", RegexOptions.Singleline);
            if (!m.Success) throw Fail("manca l'intestazione YAML fra due righe '---' (name e description)", "the YAML header between two '---' lines (name and description) is missing");
            Dictionary<string, object> header;
            try { header = new DeserializerBuilder().Build().Deserialize<Dictionary<string, object>>(m.Groups["yaml"].Value) ?? new Dictionary<string, object>(); }
            catch (YamlException ex) { throw Fail($"intestazione YAML, riga {ex.Start.Line}: {ex.InnerException?.Message ?? ex.Message}", $"YAML header, line {ex.Start.Line}: {ex.InnerException?.Message ?? ex.Message}"); }
            if (header.ContainsKey("mde")) throw Fail("niente blocco mde: lo scrive MdExplorer quando installa la skill", "no mde block: MdExplorer writes it when it installs the skill");
            if (!(header.TryGetValue("name", out var n) && n is string sn && sn == name))
                throw Fail($"name: deve essere '{name}', il nome della cartella", $"name: must be '{name}', the folder name");
            if (!(header.TryGetValue("description", out var d) && d is string sd && !string.IsNullOrWhiteSpace(sd)))
                throw Fail("description: obbligatoria, è quella che fa scegliere la skill all'agente", "description: required, it is what makes the agent pick the skill");
            return new LanguagePackageSkill { Name = name, Description = sd.Trim(), Files = files };
        }

        private static string Hash(string text)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(text)), 0, 4).ToLowerInvariant();
        }

        /// <summary>
        /// Il formato di prima (<c>.mde-languages/&lt;id&gt;.lang.yml</c>) diventa un pacchetto (<c>&lt;id&gt;/language.yml</c>):
        /// una volta sola, e lo si dice (le frasi tornate). Se la cartella c'è già, il file piatto resta dov'è ed è un errore
        /// per quell'id (tornato in <paramref name="problems"/>): due definizioni dello stesso linguaggio non si fondono a caso.
        /// </summary>
        public static List<string> MigrateFlat(string projectPath, Dictionary<string, LanguageDefinitionException> problems)
        {
            var notices = new List<string>();
            var root = DeclarativeLanguage.FolderOf(projectPath ?? string.Empty);
            if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(root)) return notices;
            foreach (var flat in Directory.EnumerateFiles(root, "*" + DeclarativeLanguage.LegacyFileSuffix).OrderBy(f => f, StringComparer.Ordinal))
            {
                var name = Path.GetFileName(flat);
                var id = name[..^DeclarativeLanguage.LegacyFileSuffix.Length];
                var folder = Path.Combine(root, id);
                if (Directory.Exists(folder))
                {
                    problems[id] = new LanguageDefinitionException(flat, UiText.Say($"c'è anche il pacchetto {DeclarativeLanguage.FolderName}/{id}/: tieni uno dei due (il formato è la cartella) e cancella l'altro",
                        $"there is also the package {DeclarativeLanguage.FolderName}/{id}/: keep one of the two (the format is the folder) and delete the other"));
                    continue;
                }
                try
                {
                    Directory.CreateDirectory(folder);
                    File.Move(flat, Path.Combine(folder, DefinitionFile));
                }
                catch (FileNotFoundException) { continue; }   // l'ha spostato un'altra lettura del catalogo nello stesso momento
                notices.Add(UiText.Say($"{DeclarativeLanguage.FolderName}/{name} spostato in {DeclarativeLanguage.FolderName}/{id}/{DefinitionFile}: un linguaggio ora è una cartella, con regole, prompt e skill",
                    $"{DeclarativeLanguage.FolderName}/{name} moved to {DeclarativeLanguage.FolderName}/{id}/{DefinitionFile}: a language is now a folder, with rules, prompt and skills"));
            }
            return notices;
        }
    }
}
