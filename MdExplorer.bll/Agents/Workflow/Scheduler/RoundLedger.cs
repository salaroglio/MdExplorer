using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Agents.Workflow.Scheduler
{
    /// <summary>
    /// Il registro dei giri su disco, dentro il progetto (quindi in git, W16):
    /// <code>
    /// .mde/giri/&lt;giro&gt;/giro.json      chi ha lanciato: id, workflow, quando, chi
    /// .mde/giri/&lt;giro&gt;/&lt;passo&gt;.json   gli eventi di quel passo
    /// </code>
    /// Un file per passo perché ogni file abbia <b>un solo scrittore</b>, il computer del responsabile del passo: due
    /// computer non scrivono mai lo stesso file, e git unisce i loro commit senza conflitti. Gli eventi si aggiungono
    /// soltanto. La lettura è a tolleranza zero: un file che non si capisce è un errore con il suo nome, mai ignorato.
    /// </summary>
    public static class RoundLedger
    {
        public const string Folder = ".mde/giri";
        public const string HeaderFile = "giro.json";

        private static readonly Regex RoundId = new(@"^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

        private static readonly JsonSerializerOptions Strict = new()
        {
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        /// <summary>La cartella di un giro, assoluta.</summary>
        public static string RoundFolder(string projectRoot, string roundId)
        {
            if (string.IsNullOrWhiteSpace(roundId) || !RoundId.IsMatch(roundId))
                throw new ArgumentException($"'{roundId}' non è l'id di un giro (minuscole, cifre e trattini).");
            return Path.Combine(projectRoot, Folder.Replace('/', Path.DirectorySeparatorChar), roundId);
        }

        /// <summary>
        /// Un id nuovo, leggibile nella cartella e nella storia di git: data, nome del workflow, sei caratteri a caso
        /// (due giri dello stesso workflow nello stesso giorno, su due computer, non si scontrano).
        /// </summary>
        public static string NewRoundId(string workflowPath, DateTime when, Random random = null)
        {
            var name = Path.GetFileName(workflowPath ?? "giro");
            name = name.EndsWith(WorkflowFile.Extension, StringComparison.OrdinalIgnoreCase) ? name[..^WorkflowFile.Extension.Length] : Path.GetFileNameWithoutExtension(name);
            var slug = Regex.Replace(name.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
            if (slug.Length == 0) slug = "giro";
            random ??= Random.Shared;
            var suffix = string.Concat(Enumerable.Range(0, 6).Select(_ => "0123456789abcdef"[random.Next(16)]));
            return $"{when:yyyy-MM-dd}-{slug}-{suffix}";
        }

        /// <summary>Apre un giro: scrive <c>giro.json</c>. Un giro che esiste già non si riapre.</summary>
        public static RoundHeader Open(string projectRoot, RoundHeader header)
        {
            var folder = RoundFolder(projectRoot, header.Id);
            var file = Path.Combine(folder, HeaderFile);
            if (File.Exists(file))
                throw new InvalidOperationException($"Il giro '{header.Id}' esiste già.");
            Directory.CreateDirectory(folder);
            WriteAtomically(file, JsonSerializer.Serialize(header, Strict));
            return header;
        }

        /// <summary>
        /// Aggiunge un evento al file del passo (creandolo se serve). Chi chiama deve essere il computer del responsabile del
        /// passo: è ciò che tiene un solo scrittore per file.
        /// </summary>
        public static StepRecord Append(string projectRoot, string roundId, string stepId, string agent, RoundEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            if (!RoundEventType.All.Contains(evt.Type))
                throw new ArgumentException($"'{evt.Type}' non è un evento di un passo.");
            var folder = RoundFolder(projectRoot, roundId);
            if (!File.Exists(Path.Combine(folder, HeaderFile)))
                throw new InvalidOperationException($"Il giro '{roundId}' non esiste: va aperto prima di scriverci.");
            if (string.IsNullOrWhiteSpace(stepId) || !RoundId.IsMatch(stepId) || stepId + ".json" == HeaderFile)
                throw new ArgumentException($"'{stepId}' non è l'id di un passo.");

            var file = Path.Combine(folder, stepId + ".json");
            var record = File.Exists(file) ? ReadStep(file) : new StepRecord { Step = stepId, Agent = agent };
            record.Events.Add(evt);
            WriteAtomically(file, JsonSerializer.Serialize(record, Strict));
            return record;
        }

        /// <summary>Legge un giro intero. Un file che non si capisce solleva, con il suo nome.</summary>
        public static RoundState Load(string projectRoot, string roundId)
        {
            var folder = RoundFolder(projectRoot, roundId);
            var headerFile = Path.Combine(folder, HeaderFile);
            if (!File.Exists(headerFile))
                throw new InvalidOperationException($"Il giro '{roundId}' non esiste ({Relative(projectRoot, headerFile)}).");

            var state = new RoundState { Header = Read<RoundHeader>(headerFile) };
            if (state.Header.Id != roundId)
                throw new InvalidDataException($"{Relative(projectRoot, headerFile)}: l'id dentro ('{state.Header.Id}') non è quello della cartella ('{roundId}').");
            foreach (var file in Directory.EnumerateFiles(folder, "*.json").Where(f => Path.GetFileName(f) != HeaderFile).OrderBy(f => f, StringComparer.Ordinal))
            {
                var record = ReadStep(file);
                var expected = Path.GetFileNameWithoutExtension(file);
                if (record.Step != expected)
                    throw new InvalidDataException($"{Relative(projectRoot, file)}: il passo dentro ('{record.Step}') non è quello del nome del file.");
                state.Steps[record.Step] = record;
            }
            return state;
        }

        /// <summary>Gli id dei giri presenti nel progetto.</summary>
        public static IReadOnlyList<string> RoundIds(string projectRoot)
        {
            var root = Path.Combine(projectRoot, Folder.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(root)) return Array.Empty<string>();
            return Directory.EnumerateDirectories(root)
                .Where(d => File.Exists(Path.Combine(d, HeaderFile)))
                .Select(Path.GetFileName)
                .Where(id => RoundId.IsMatch(id))
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
        }

        private static StepRecord ReadStep(string file)
        {
            var record = Read<StepRecord>(file);
            var unknown = record.Events?.FirstOrDefault(e => !RoundEventType.All.Contains(e?.Type));
            if (unknown != null)
                throw new InvalidDataException($"{file}: '{unknown.Type}' non è un evento di un passo.");
            record.Events ??= new List<RoundEvent>();
            return record;
        }

        private static T Read<T>(string file)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(File.ReadAllText(file), Strict)
                       ?? throw new InvalidDataException($"{file}: è vuoto.");
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"{file}: non si legge ({ex.Message}).", ex);
            }
        }

        /// <summary>Scritto a parte e spostato al suo posto: chi osserva i file non lo vede mai a metà.</summary>
        private static void WriteAtomically(string file, string content)
        {
            var aside = file + ".scrittura";
            File.WriteAllText(aside, content + "\n", new UTF8Encoding(false));
            File.Move(aside, file, overwrite: true);
        }

        private static string Relative(string projectRoot, string file)
            => Path.GetRelativePath(projectRoot, file).Replace('\\', '/');
    }
}
