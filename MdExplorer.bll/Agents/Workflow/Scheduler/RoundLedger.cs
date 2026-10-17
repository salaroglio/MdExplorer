using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MdExplorer.Features.Services;

namespace MdExplorer.Features.Agents.Workflow.Scheduler
{
    /// <summary>
    /// Il registro dei giri su disco, dentro il progetto (quindi in git, W16):
    /// <code>
    /// .mde/giri/&lt;giro&gt;/giro.json      chi ha lanciato: id, workflow, quando, chi
    /// .mde/giri/&lt;giro&gt;/&lt;passo&gt;.json   gli eventi di quel passo
    /// .mde/giri/&lt;giro&gt;/&lt;passo&gt;.approvazione   le decisioni di chi approva per ruolo (approval.by)
    /// </code>
    /// Un file per passo perché ogni file abbia <b>un solo scrittore</b>, il computer del responsabile del passo: due
    /// computer non scrivono mai lo stesso file, e git unisce i loro commit senza conflitti. Gli eventi si aggiungono
    /// soltanto. La lettura è a tolleranza zero: un file che non si capisce è un errore con il suo nome, mai ignorato.
    /// </summary>
    public static class RoundLedger
    {
        public const string Folder = ".mde/giri";
        public const string HeaderFile = "giro.json";
        /// <summary>
        /// Il workflow con cui il giro è nato, testo com'era (16/10/2026): il giro va avanti con quello anche se il workflow
        /// cambia. Non è <c>.json</c> di proposito: i passi sono i <c>*.json</c> della cartella, e un computer con una versione
        /// di MdExplorer di prima questo file lo ignora senza confonderlo con un passo.
        /// </summary>
        public const string WorkflowSnapshotFile = "workflow.snapshot";
        /// <summary>
        /// Le interfacce delle schede con cui il giro è nato (workflow v3, 17/10/2026): uscite, documenti letti e ingressi
        /// dei lavori, uniti al workflow. Una scheda cambiata a giro aperto non cambia il giro.
        /// </summary>
        public const string CardsSnapshotFile = "schede.snapshot";

        /// <summary>
        /// Le decisioni sul passo di chi lo approva per ruolo (<c>approval.by</c>, workflow v3 F5): chi approva non è chi
        /// risponde dell'agente, e scrive un suo file perché ogni file abbia un solo scrittore. Non è <c>.json</c>: i passi sono
        /// i <c>*.json</c> della cartella. Alla lettura le decisioni entrano negli eventi del passo.
        /// </summary>
        public const string DecisionsExtension = ".approvazione";

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
                throw new ArgumentException(UiText.Say($"'{roundId}' non è l'id di un giro (minuscole, cifre e trattini).", $"'{roundId}' is not a round id (lowercase letters, digits and hyphens)."));
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
                throw new InvalidOperationException(UiText.Say($"Il giro '{header.Id}' esiste già.", $"The round '{header.Id}' already exists."));
            Directory.CreateDirectory(folder);
            WriteAtomically(file, JsonSerializer.Serialize(header, Strict));
            return header;
        }

        /// <summary>Salva il workflow con cui nasce il giro. Una volta sola: la versione del giro non cambia più.</summary>
        public static void SaveWorkflowSnapshot(string projectRoot, string roundId, string workflowJson) =>
            SaveSnapshot(projectRoot, roundId, WorkflowSnapshotFile, workflowJson);

        /// <summary>Il workflow con cui il giro è nato, o null per un giro aperto prima che si salvasse (vale quello attuale).</summary>
        public static string LoadWorkflowSnapshot(string projectRoot, string roundId) =>
            LoadSnapshot(projectRoot, roundId, WorkflowSnapshotFile);

        /// <summary>Salva le schede con cui nasce il giro, accanto al workflow. Una volta sola, come il workflow.</summary>
        public static void SaveCardsSnapshot(string projectRoot, string roundId, string cardsJson) =>
            SaveSnapshot(projectRoot, roundId, CardsSnapshotFile, cardsJson);

        /// <summary>Le schede con cui il giro è nato, o null se il giro non le ha salvate.</summary>
        public static string LoadCardsSnapshot(string projectRoot, string roundId) =>
            LoadSnapshot(projectRoot, roundId, CardsSnapshotFile);

        private static void SaveSnapshot(string projectRoot, string roundId, string name, string text)
        {
            var folder = RoundFolder(projectRoot, roundId);
            if (!File.Exists(Path.Combine(folder, HeaderFile)))
                throw new InvalidOperationException(UiText.Say($"Il giro '{roundId}' non esiste: va aperto prima di salvarne il workflow.", $"The round '{roundId}' does not exist: it must be opened before saving its workflow."));
            var file = Path.Combine(folder, name);
            if (File.Exists(file)) return;
            // Il testo esatto, senza l'a capo che gli altri file del registro aggiungono: è una copia, non un file nostro.
            var aside = file + ".scrittura";
            File.WriteAllText(aside, text ?? throw new ArgumentNullException(nameof(text)), new UTF8Encoding(false));
            File.Move(aside, file, overwrite: true);
        }

        private static string LoadSnapshot(string projectRoot, string roundId, string name)
        {
            var file = Path.Combine(RoundFolder(projectRoot, roundId), name);
            return File.Exists(file) ? File.ReadAllText(file) : null;
        }

        /// <summary>
        /// Aggiunge un evento al file del passo (creandolo se serve). Chi chiama deve essere il computer del responsabile del
        /// passo: è ciò che tiene un solo scrittore per file.
        /// </summary>
        public static StepRecord Append(string projectRoot, string roundId, string stepId, string agent, RoundEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            if (!RoundEventType.All.Contains(evt.Type))
                throw new ArgumentException(UiText.Say($"'{evt.Type}' non è un evento di un passo.", $"'{evt.Type}' is not a step event."));
            var folder = RoundFolder(projectRoot, roundId);
            if (!File.Exists(Path.Combine(folder, HeaderFile)))
                throw new InvalidOperationException(UiText.Say($"Il giro '{roundId}' non esiste: va aperto prima di scriverci.", $"The round '{roundId}' does not exist: it must be opened before writing to it."));
            if (string.IsNullOrWhiteSpace(stepId) || !RoundId.IsMatch(stepId) || stepId + ".json" == HeaderFile)
                throw new ArgumentException(UiText.Say($"'{stepId}' non è l'id di un passo.", $"'{stepId}' is not a step id."));

            var file = Path.Combine(folder, stepId + ".json");
            var record = File.Exists(file) ? ReadStep(file) : new StepRecord { Step = stepId, Agent = agent };
            record.Events.Add(evt);
            WriteAtomically(file, JsonSerializer.Serialize(record, Strict));
            return record;
        }

        /// <summary>
        /// Aggiunge una decisione (approvato o rifiutato) al file delle approvazioni del passo. Chi chiama è il computer di chi
        /// approva per ruolo: il file del passo resta del responsabile dell'agente.
        /// </summary>
        public static StepRecord AppendDecision(string projectRoot, string roundId, string stepId, RoundEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            if (evt.Type != RoundEventType.Approved && evt.Type != RoundEventType.Rejected)
                throw new ArgumentException(UiText.Say($"'{evt.Type}' non è una decisione: chi approva scrive solo approvato o rifiutato.", $"'{evt.Type}' is not a decision: the approver writes only approved or rejected."));
            var folder = RoundFolder(projectRoot, roundId);
            if (!File.Exists(Path.Combine(folder, HeaderFile)))
                throw new InvalidOperationException(UiText.Say($"Il giro '{roundId}' non esiste: va aperto prima di scriverci.", $"The round '{roundId}' does not exist: it must be opened before writing to it."));
            if (string.IsNullOrWhiteSpace(stepId) || !RoundId.IsMatch(stepId))
                throw new ArgumentException(UiText.Say($"'{stepId}' non è l'id di un passo.", $"'{stepId}' is not a step id."));
            var file = Path.Combine(folder, stepId + DecisionsExtension);
            var record = File.Exists(file) ? ReadDecisions(file) : new StepRecord { Step = stepId };
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
                throw new InvalidOperationException(UiText.Say($"Il giro '{roundId}' non esiste ({Relative(projectRoot, headerFile)}).", $"The round '{roundId}' does not exist ({Relative(projectRoot, headerFile)})."));

            var state = new RoundState { Header = Read<RoundHeader>(headerFile) };
            if (state.Header.Id != roundId)
                throw new InvalidDataException(UiText.Say($"{Relative(projectRoot, headerFile)}: l'id dentro ('{state.Header.Id}') non è quello della cartella ('{roundId}').", $"{Relative(projectRoot, headerFile)}: the id inside ('{state.Header.Id}') is not the folder's ('{roundId}')."));
            foreach (var file in Directory.EnumerateFiles(folder, "*.json").Where(f => Path.GetFileName(f) != HeaderFile).OrderBy(f => f, StringComparer.Ordinal))
            {
                var record = ReadStep(file);
                var expected = Path.GetFileNameWithoutExtension(file);
                if (record.Step != expected)
                    throw new InvalidDataException(UiText.Say($"{Relative(projectRoot, file)}: il passo dentro ('{record.Step}') non è quello del nome del file.", $"{Relative(projectRoot, file)}: the step inside ('{record.Step}') is not the one in the file name."));
                state.Steps[record.Step] = record;
            }
            foreach (var file in Directory.EnumerateFiles(folder, "*" + DecisionsExtension).OrderBy(f => f, StringComparer.Ordinal))
            {
                var decisions = ReadDecisions(file);
                var expected = Path.GetFileName(file)[..^DecisionsExtension.Length];
                if (decisions.Step != expected)
                    throw new InvalidDataException(UiText.Say($"{Relative(projectRoot, file)}: il passo dentro ('{decisions.Step}') non è quello del nome del file.", $"{Relative(projectRoot, file)}: the step inside ('{decisions.Step}') is not the one in the file name."));
                if (!state.Steps.TryGetValue(expected, out var record))
                    throw new InvalidDataException(UiText.Say($"{Relative(projectRoot, file)}: decisioni su un passo che non ha consegnato niente.", $"{Relative(projectRoot, file)}: decisions on a step that has delivered nothing."));
                foreach (var decision in decisions.Events)
                {
                    // Subito dopo la consegna che giudica (stesso giro e tentativo): gli orologi di due computer non decidono l'ordine.
                    var delivered = record.Events.FindLastIndex(e => e.Type == RoundEventType.Delivered && e.Round == decision.Round && e.Attempt == decision.Attempt);
                    if (delivered < 0)
                        throw new InvalidDataException(UiText.Say($"{Relative(projectRoot, file)}: una decisione sul tentativo {decision.Attempt} del giro {decision.Round}, che non ha una consegna.", $"{Relative(projectRoot, file)}: a decision on attempt {decision.Attempt} of round {decision.Round}, which has no delivery."));
                    record.Events.Insert(delivered + 1, decision);
                }
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
                throw new InvalidDataException(UiText.Say($"{file}: '{unknown.Type}' non è un evento di un passo.", $"{file}: '{unknown.Type}' is not a step event."));
            record.Events ??= new List<RoundEvent>();
            return record;
        }

        private static StepRecord ReadDecisions(string file)
        {
            var record = Read<StepRecord>(file);
            record.Events ??= new List<RoundEvent>();
            var other = record.Events.FirstOrDefault(e => e?.Type != RoundEventType.Approved && e?.Type != RoundEventType.Rejected);
            if (other != null)
                throw new InvalidDataException(UiText.Say($"{file}: '{other?.Type}' non è una decisione di chi approva.", $"{file}: '{other?.Type}' is not an approver's decision."));
            return record;
        }

        private static T Read<T>(string file)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(File.ReadAllText(file), Strict)
                       ?? throw new InvalidDataException(UiText.Say($"{file}: è vuoto.", $"{file}: is empty."));
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException(UiText.Say($"{file}: non si legge ({ex.Message}).", $"{file}: cannot be read ({ex.Message})."), ex);
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
