using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MdExplorer.Features.Agents
{
    /// <summary>Una voce del diario di un turno: un pezzo di risposta, di ragionamento, uno strumento usato, un errore.</summary>
    public sealed class AgentTurnDiaryEntry
    {
        public int Seq { get; set; }
        /// <summary>Uno dei tipi di <see cref="AgentTurnEvent"/>.</summary>
        public string Kind { get; set; }
        public string Text { get; set; }
        public DateTime AtUtc { get; set; }
        /// <summary>La versione del diario all'ultima modifica di questa voce: una risposta che cresce resta la stessa voce.</summary>
        public long Revision { get; set; }
    }

    /// <summary>Il diario di un turno, o la sua sola testata (<see cref="Entries"/> vuota) negli elenchi.</summary>
    public sealed class AgentTurnDiaryView
    {
        public Guid Id { get; set; }
        public string AgentName { get; set; }
        public string ProjectPath { get; set; }
        public string Engine { get; set; }
        public DateTime StartedAtUtc { get; set; }
        /// <summary>null finché il turno è in corso.</summary>
        public DateTime? FinishedAtUtc { get; set; }
        /// <summary>null finché il turno è in corso; poi l'esito (<see cref="AgentTurnOutcome"/>) come testo.</summary>
        public string Outcome { get; set; }
        public string Diagnostic { get; set; }
        /// <summary>Cresce a ogni modifica: chi rilegge chiede «dopo la versione N».</summary>
        public long Version { get; set; }
        /// <summary>Il turno ha scritto più di quanto il diario tiene: la fine manca.</summary>
        public bool Truncated { get; set; }
        public List<AgentTurnDiaryEntry> Entries { get; set; } = new();
    }

    /// <summary>
    /// Che cosa sta facendo, e che cosa ha fatto, un agente in un turno: la risposta mentre la scrive, il ragionamento,
    /// gli strumenti che usa. Lo riempie chi fa girare il turno (un solo punto: <c>EngineTurnRunner</c>), lo legge il
    /// pannello che si apre dall'indicatore «agenti al lavoro». A turno finito resta su disco, fuori dal progetto.
    /// </summary>
    public interface IAgentTurnDiary
    {
        void Open(AgentActivity activity);
        void Append(Guid id, string kind, string text);
        /// <param name="finalText">La risposta intera: entra nel diario solo se il motore non l'ha mandata a pezzi.</param>
        void Close(Guid id, string outcome, string diagnostic, string finalText);
        /// <summary>Il diario con le voci cambiate dopo <paramref name="afterVersion"/> (0 = tutte); null se non esiste.</summary>
        AgentTurnDiaryView Read(Guid id, long afterVersion);
        /// <summary>Le testate dei turni di un progetto, i più recenti prima: quelli in corso e quelli finiti.</summary>
        IReadOnlyList<AgentTurnDiaryView> Recent(string projectPath, int max);
        /// <summary>Un diario è cambiato: (id, progetto).</summary>
        event Action<Guid, string> Changed;
    }

    public sealed class AgentTurnDiary : IAgentTurnDiary
    {
        /// <summary>Oltre questo testo il diario smette di registrare (un turno molto lungo): lo dichiara con <c>Truncated</c>.</summary>
        public const int MaxChars = 1_000_000;
        /// <summary>Uno strumento si mostra col suo comando o il suo file, non col contenuto: oltre, si taglia.</summary>
        public const int MaxToolChars = 600;
        /// <summary>Quanti turni finiti restano su disco.</summary>
        public const int MaxKept = 200;

        private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private readonly string _folder;
        private readonly ConcurrentDictionary<Guid, Live> _live = new();
        private readonly object _indexLock = new();
        private List<AgentTurnDiaryView> _index;
        // Gli ultimi turni finiti, in memoria: chi ha il pannello aperto rilegge subito la fine senza passare dal disco
        // (e senza una cartella — nei test — sono l'unico posto dove restano).
        private readonly ConcurrentDictionary<Guid, AgentTurnDiaryView> _justFinished = new();
        private readonly ConcurrentQueue<Guid> _justFinishedOrder = new();
        private const int JustFinishedKept = 20;

        public event Action<Guid, string> Changed;

        /// <param name="folder">Dove restano i diari dei turni finiti (dati dell'utente, non il progetto); null = solo in memoria.</param>
        public AgentTurnDiary(string folder)
        {
            _folder = folder;
        }

        private sealed class Live
        {
            public readonly AgentTurnDiaryView View;
            public int Chars;
            public Live(AgentTurnDiaryView view) { View = view; }
        }

        public void Open(AgentActivity activity)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));
            _live[activity.Id] = new Live(new AgentTurnDiaryView
            {
                Id = activity.Id,
                AgentName = activity.AgentName,
                ProjectPath = activity.ProjectPath,
                Engine = activity.Engine,
                StartedAtUtc = activity.StartedAtUtc,
                Version = 1,
            });
            Changed?.Invoke(activity.Id, activity.ProjectPath);
        }

        public void Append(Guid id, string kind, string text)
        {
            if (string.IsNullOrEmpty(text) || !_live.TryGetValue(id, out var live)) return;
            lock (live)
            {
                if (live.View.FinishedAtUtc != null) return;
                AppendLocked(live, kind, text);
            }
            Changed?.Invoke(id, live.View.ProjectPath);
        }

        private static void AppendLocked(Live live, string kind, string text)
        {
            var view = live.View;
            var isError = kind == AgentTurnEvent.KindError;
            if (live.Chars >= MaxChars && !isError)
            {
                if (!view.Truncated) { view.Truncated = true; view.Version++; }
                return;
            }
            if (kind == AgentTurnEvent.KindTool && text.Length > MaxToolChars) text = text.Substring(0, MaxToolChars) + "…";
            live.Chars += text.Length;
            view.Version++;
            var last = view.Entries.Count > 0 ? view.Entries[view.Entries.Count - 1] : null;
            // La risposta e il ragionamento arrivano a pezzi: restano una voce sola, che cresce.
            var grows = kind == AgentTurnEvent.KindMessage || kind == AgentTurnEvent.KindThinking;
            if (grows && last != null && last.Kind == kind)
            {
                last.Text += text;
                last.Revision = view.Version;
                return;
            }
            view.Entries.Add(new AgentTurnDiaryEntry
            {
                Seq = view.Entries.Count + 1,
                Kind = kind,
                Text = text,
                AtUtc = DateTime.UtcNow,
                Revision = view.Version,
            });
        }

        public void Close(Guid id, string outcome, string diagnostic, string finalText)
        {
            if (!_live.TryGetValue(id, out var live)) return;
            AgentTurnDiaryView copy;
            lock (live)
            {
                if (live.View.FinishedAtUtc != null) return;
                // Un motore che non manda la risposta a pezzi (un CLI di prima): la si mette intera, alla fine.
                if (!string.IsNullOrWhiteSpace(finalText) && !live.View.Entries.Any(e => e.Kind == AgentTurnEvent.KindMessage))
                    AppendLocked(live, AgentTurnEvent.KindMessage, finalText);
                live.View.FinishedAtUtc = DateTime.UtcNow;
                live.View.Outcome = outcome;
                live.View.Diagnostic = diagnostic;
                live.View.Version++;
                copy = Copy(live.View, 0);
            }
            Persist(copy);
            _justFinished[id] = copy;
            _justFinishedOrder.Enqueue(id);
            while (_justFinishedOrder.Count > JustFinishedKept && _justFinishedOrder.TryDequeue(out var oldest)) _justFinished.TryRemove(oldest, out _);
            _live.TryRemove(id, out _);
            Changed?.Invoke(id, copy.ProjectPath);
        }

        public AgentTurnDiaryView Read(Guid id, long afterVersion)
        {
            if (_live.TryGetValue(id, out var live))
                lock (live) return Copy(live.View, afterVersion);
            var stored = _justFinished.TryGetValue(id, out var recent) ? recent : Load(id);
            return stored == null ? null : Copy(stored, afterVersion);
        }

        public IReadOnlyList<AgentTurnDiaryView> Recent(string projectPath, int max)
        {
            var running = _live.Values.Select(l => { lock (l) return Header(l.View); });
            List<AgentTurnDiaryView> finished;
            lock (_indexLock) finished = Index().ToList();
            return running.Concat(finished)
                .Where(v => AgentPathComparer.Equals(v.ProjectPath, projectPath))
                .GroupBy(v => v.Id).Select(g => g.First())
                .OrderByDescending(v => v.StartedAtUtc)
                .Take(Math.Max(0, max))
                .ToList();
        }

        private static AgentTurnDiaryView Header(AgentTurnDiaryView v) => new()
        {
            Id = v.Id, AgentName = v.AgentName, ProjectPath = v.ProjectPath, Engine = v.Engine, StartedAtUtc = v.StartedAtUtc,
            FinishedAtUtc = v.FinishedAtUtc, Outcome = v.Outcome, Diagnostic = v.Diagnostic, Version = v.Version, Truncated = v.Truncated,
        };

        private static AgentTurnDiaryView Copy(AgentTurnDiaryView v, long afterVersion)
        {
            var copy = Header(v);
            copy.Entries = v.Entries.Where(e => e.Revision > afterVersion)
                .Select(e => new AgentTurnDiaryEntry { Seq = e.Seq, Kind = e.Kind, Text = e.Text, AtUtc = e.AtUtc, Revision = e.Revision })
                .ToList();
            return copy;
        }

        // ── Su disco: un file per turno finito, più l'indice delle testate ──────────────────────────────

        private string FileOf(Guid id) => Path.Combine(_folder, id.ToString("N") + ".json");
        private string IndexFile => Path.Combine(_folder, "indice.json");

        private List<AgentTurnDiaryView> Index()
        {
            if (_index != null) return _index;
            _index = new List<AgentTurnDiaryView>();
            if (_folder == null || !File.Exists(IndexFile)) return _index;
            // Un indice illeggibile non deve fermare i turni: lo si dice e si riparte da vuoto (i file dei turni restano).
            try { _index = JsonSerializer.Deserialize<List<AgentTurnDiaryView>>(File.ReadAllText(IndexFile), Json) ?? _index; }
            catch (Exception ex) when (ex is JsonException || ex is IOException)
            {
                Console.WriteLine($"[AgentTurnDiary] indice dei diari illeggibile ({IndexFile}): {ex.Message}. Si riparte da un elenco vuoto.");
            }
            return _index;
        }

        private void Persist(AgentTurnDiaryView finished)
        {
            lock (_indexLock)
            {
                var index = Index();
                index.RemoveAll(v => v.Id == finished.Id);
                index.Insert(0, Header(finished));
                var dropped = index.Skip(MaxKept).ToList();
                if (dropped.Count > 0) index.RemoveRange(MaxKept, index.Count - MaxKept);
                if (_folder == null) return;
                Directory.CreateDirectory(_folder);
                File.WriteAllText(FileOf(finished.Id), JsonSerializer.Serialize(finished, Json));
                File.WriteAllText(IndexFile, JsonSerializer.Serialize(index, Json));
                foreach (var old in dropped)
                    try { File.Delete(FileOf(old.Id)); } catch (IOException) { /* resta un file in più: lo toglie il prossimo giro */ }
            }
        }

        private AgentTurnDiaryView Load(Guid id)
        {
            if (_folder == null || !File.Exists(FileOf(id))) return null;
            return JsonSerializer.Deserialize<AgentTurnDiaryView>(File.ReadAllText(FileOf(id)), Json);
        }
    }
}
