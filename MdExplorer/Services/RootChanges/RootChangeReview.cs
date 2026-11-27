using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Services.RootChanges;
using MdExplorer.RemoteProtocol;
using MdExplorer.Services.CodeGraph;
using MdExplorer.Services.RemoteFolders;

namespace MdExplorer.Services.RootChanges
{
    /// <summary>Un file da rivedere, per l'elenco: che cosa gli è successo e quanto.</summary>
    public sealed class RootChangeSummary
    {
        public string Alias { get; init; }
        public string Relative { get; init; }
        public string LogicalPath { get; init; }
        /// <summary><c>modified</c>, <c>created</c>, <c>deleted</c>.</summary>
        public string Status { get; init; }
        public int Added { get; init; }
        public int Removed { get; init; }
        public int Hunks { get; init; }
        /// <summary>Il file non è quello dell'ultima scrittura di MarkAgent: qualcuno l'ha cambiato dopo (D8).</summary>
        public bool ChangedOutside { get; init; }
        public DateTime UpdatedAt { get; init; }
    }

    /// <summary>Un file da rivedere, per il pannello: i due testi e i pezzi.</summary>
    public sealed class RootChangeDetail
    {
        public RootChangeSummary Summary { get; init; }
        public string OriginalText { get; init; }
        public string CurrentText { get; init; }
        public string Encoding { get; init; }
        public IReadOnlyList<ChangeHunk> Hunks { get; init; }
    }

    /// <summary>
    /// La revisione dei file delle radici cambiati da MarkAgent (sprint 2026-11-27-Modifiche-Da-Approvare-Sulle-Radici, F3, D7-D8):
    /// l'originale tenuto dal cancello contro il file di adesso, a pezzi. Tieni sposta il pezzo nell'originale; Annulla lo
    /// riscrive nel file, con la stessa via delle scritture (MDE Remote o le stesse regole sul disco) e la stessa codifica.
    /// Un pezzo si indica con indice e impronta: se il file è cambiato nel frattempo l'impronta non combacia e si rifiuta.
    /// </summary>
    public sealed class RootChangeReview
    {
        public const string Modified = "modified";
        public const string Created = "created";
        public const string Deleted = "deleted";

        private readonly RootChangeStore _store;
        private readonly ICodeRootService _roots;
        private readonly RemoteCodeRootFiles _remoteFiles;

        public RootChangeReview(RootChangeStore store, ICodeRootService roots, RemoteCodeRootFiles remoteFiles)
        {
            _store = store; _roots = roots; _remoteFiles = remoteFiles;
        }

        private sealed class Place
        {
            public CodeRootState Root;
            public Func<RemoteRequest, Task<RemoteResponse>> Real;
        }

        private async Task<Place> PlaceAsync(string projectPath, string alias, CancellationToken ct)
        {
            var resolver = await _roots.CreateResolverAsync(projectPath, ct);
            var root = resolver.Roots.FirstOrDefault(r => string.Equals(r.Alias, alias, StringComparison.Ordinal))
                ?? throw new InvalidOperationException(UiText.Say($"La radice «@{alias}» non è più nel progetto.", $"The root «@{alias}» is no longer in the project."));
            if (root.Status != CodeRootStatus.Linked)
                throw new InvalidOperationException(UiText.Say($"La radice «@{alias}» non è utilizzabile su questa macchina: {root.Reason}",
                    $"The root «@{alias}» cannot be used on this machine: {root.Reason}"));
            return new Place
            {
                Root = root,
                Real = r => root.IsRemote ? _remoteFiles.OperateAsync(root, r, ct) : Task.FromResult(RemoteCodeRootFiles.OperateLocal(root, r)),
            };
        }

        private static async Task<RemoteResponse> CurrentAsync(Place place, string relative)
        {
            try { return await place.Real(new RemoteRequest { Op = RemoteOps.Read, Path = relative }); }
            catch (RemoteOperationException ex) when (ex.Code == RemoteErrorCodes.NotFound) { return null; }
        }

        private static RootChangeSummary Summarize(RootChange change, RemoteResponse current, IReadOnlyList<ChangeHunk> hunks) => new()
        {
            Alias = change.Alias,
            Relative = change.Relative,
            LogicalPath = CodeRootPaths.Format(change.Alias, change.Relative),
            Status = !change.OriginalExists ? Created : current == null ? Deleted : Modified,
            Added = hunks.Sum(h => h.CurrentLines.Count),
            Removed = hunks.Sum(h => h.OriginalLines.Count),
            Hunks = hunks.Count,
            ChangedOutside = current?.Stamp != change.LastStamp,
            UpdatedAt = change.UpdatedAt,
        };

        /// <summary>
        /// I file da rivedere del progetto. Uno che nel frattempo è tornato com'era (o creato e già sparito) esce dal registro:
        /// non c'è niente da rivedere.
        /// </summary>
        public async Task<IReadOnlyList<RootChangeSummary>> ListAsync(string projectPath, CancellationToken ct)
        {
            var result = new List<RootChangeSummary>();
            foreach (var group in _store.List(projectPath).GroupBy(c => c.Alias))
            {
                Place place;
                try { place = await PlaceAsync(projectPath, group.Key, ct); }
                catch (InvalidOperationException)
                {
                    // radice non raggiungibile adesso: i file restano da rivedere, senza i numeri
                    result.AddRange(group.Select(c => new RootChangeSummary
                    {
                        Alias = c.Alias, Relative = c.Relative, LogicalPath = CodeRootPaths.Format(c.Alias, c.Relative),
                        Status = c.OriginalExists ? Modified : Created, UpdatedAt = c.UpdatedAt,
                    }));
                    continue;
                }
                foreach (var change in group)
                {
                    var current = await CurrentAsync(place, change.Relative);
                    if (Unchanged(change, current)) { _store.Remove(projectPath, change.Alias, change.Relative); continue; }
                    result.Add(Summarize(change, current, RootChangeHunks.Compute(change.OriginalText ?? "", current?.Text ?? "")));
                }
            }
            return result;
        }

        private static bool Unchanged(RootChange change, RemoteResponse current)
            => change.OriginalExists ? current != null && string.Equals(current.Text, change.OriginalText, StringComparison.Ordinal) : current == null;

        public async Task<RootChangeDetail> DetailAsync(string projectPath, string alias, string relative, CancellationToken ct)
        {
            var change = Get(projectPath, alias, relative);
            var place = await PlaceAsync(projectPath, alias, ct);
            var current = await CurrentAsync(place, change.Relative);
            var hunks = RootChangeHunks.Compute(change.OriginalText ?? "", current?.Text ?? "");
            return new RootChangeDetail
            {
                Summary = Summarize(change, current, hunks),
                OriginalText = change.OriginalText,
                CurrentText = current?.Text,
                Encoding = current?.Encoding ?? change.Encoding,
                Hunks = hunks,
            };
        }

        private RootChange Get(string projectPath, string alias, string relative)
            => _store.Get(projectPath, alias, relative) ?? throw new InvalidOperationException(UiText.Say(
                $"@{alias}/{RootChangeStore.Normalize(relative)} non è fra i file da rivedere.", $"@{alias}/{RootChangeStore.Normalize(relative)} is not among the files to review."));

        /// <summary>Il pezzo chiesto, se è ancora quello: altrimenti il file è cambiato e la revisione va ricaricata.</summary>
        private static ChangeHunk Hunk(IReadOnlyList<ChangeHunk> hunks, int index, string hash)
            => index >= 0 && index < hunks.Count && string.Equals(hunks[index].Hash, hash, StringComparison.Ordinal)
                ? hunks[index]
                : throw new RemoteOperationException(RemoteErrorCodes.Changed, UiText.Say(
                    "Il file è cambiato da quando la revisione è stata aperta: ricaricala.", "The file changed since the review was opened: reload it."));

        /// <summary>Tieni un pezzo (<paramref name="hunk"/>) o tutto il file (null): il file non si tocca, cambia l'originale.</summary>
        public async Task KeepAsync(string projectPath, string alias, string relative, int? hunk, string hash, CancellationToken ct)
        {
            var change = Get(projectPath, alias, relative);
            if (hunk == null) { _store.Remove(projectPath, alias, change.Relative); return; }
            var place = await PlaceAsync(projectPath, alias, ct);
            var current = await CurrentAsync(place, change.Relative);
            OnlyWholeFile(change, current);
            var h = Hunk(RootChangeHunks.Compute(change.OriginalText, current.Text), hunk.Value, hash);
            change.OriginalText = RootChangeHunks.Keep(change.OriginalText, h);
            change.UpdatedAt = DateTime.Now;
            if (string.Equals(change.OriginalText, current.Text, StringComparison.Ordinal)) _store.Remove(projectPath, alias, change.Relative);
            else _store.Save(projectPath, change);
        }

        /// <summary>Annulla un pezzo (<paramref name="hunk"/>) o tutto il file (null): il file riprende l'originale.</summary>
        public async Task UndoAsync(string projectPath, string alias, string relative, int? hunk, string hash, CancellationToken ct)
        {
            var change = Get(projectPath, alias, relative);
            var place = await PlaceAsync(projectPath, alias, ct);
            var current = await CurrentAsync(place, change.Relative);
            if (hunk == null)
            {
                await RestoreAsync(place, change, current);
                _store.Remove(projectPath, alias, change.Relative);
                return;
            }
            OnlyWholeFile(change, current);
            var h = Hunk(RootChangeHunks.Compute(change.OriginalText, current.Text), hunk.Value, hash);
            var text = RootChangeHunks.Undo(current.Text, h);
            var written = await place.Real(new RemoteRequest { Op = RemoteOps.Write, Path = change.Relative, Text = text, Stamp = current.Stamp });
            if (string.Equals(text, change.OriginalText, StringComparison.Ordinal)) { _store.Remove(projectPath, alias, change.Relative); return; }
            change.LastStamp = written.Stamp;
            change.UpdatedAt = DateTime.Now;
            _store.Save(projectPath, change);
        }

        private static void OnlyWholeFile(RootChange change, RemoteResponse current)
        {
            if (!change.OriginalExists || current == null)
                throw new InvalidOperationException(UiText.Say(
                    "Un file creato o cancellato si tiene o si annulla intero.", "A created or deleted file is kept or undone as a whole."));
        }

        /// <summary>Il file com'era: riscritto, rimesso (cancellato da MarkAgent) o tolto (creato da MarkAgent).</summary>
        private static async Task RestoreAsync(Place place, RootChange change, RemoteResponse current)
        {
            if (!change.OriginalExists)
            {
                if (current != null) await place.Real(new RemoteRequest { Op = RemoteOps.Delete, Path = change.Relative });
                return;
            }
            if (current != null)
            {
                await place.Real(new RemoteRequest { Op = RemoteOps.Write, Path = change.Relative, Text = change.OriginalText, Stamp = current.Stamp });
                return;
            }
            // Rimesso: create scrive nella codifica che la radice dà all'estensione; se il file era in un'altra, si riscrive in quella.
            var created = await place.Real(new RemoteRequest { Op = RemoteOps.Create, Path = change.Relative, Text = change.OriginalText });
            var now = await place.Real(new RemoteRequest { Op = RemoteOps.Read, Path = change.Relative });
            if (!string.IsNullOrEmpty(change.Encoding) && !string.Equals(now.Encoding, change.Encoding, StringComparison.OrdinalIgnoreCase))
                await place.Real(new RemoteRequest { Op = RemoteOps.Write, Path = change.Relative, Text = change.OriginalText, Stamp = created.Stamp ?? now.Stamp, Encoding = change.Encoding });
        }

        /// <summary>Tieni tutto o Annulla tutto, file per file: un file che non riesce non ferma gli altri, e si dice quale.</summary>
        public async Task<IReadOnlyList<string>> AllAsync(string projectPath, bool keep, CancellationToken ct)
        {
            var problems = new List<string>();
            foreach (var change in _store.List(projectPath))
            {
                try
                {
                    if (keep) await KeepAsync(projectPath, change.Alias, change.Relative, null, null, ct);
                    else await UndoAsync(projectPath, change.Alias, change.Relative, null, null, ct);
                }
                catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
                {
                    problems.Add(CodeRootPaths.Format(change.Alias, change.Relative) + ": " + ex.Message);
                }
            }
            return problems;
        }
    }
}
