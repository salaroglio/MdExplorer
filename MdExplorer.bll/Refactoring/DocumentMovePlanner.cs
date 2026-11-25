using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services;

namespace MdExplorer.Features.Refactoring
{
    /// <summary>Un documento o una cartella da spostare: percorsi completi di partenza e di arrivo.</summary>
    public sealed record DocumentMove(string From, string To, bool IsFolder);

    /// <summary>Che cosa farà uno spostamento di documenti, deciso prima di toccare il disco.</summary>
    public sealed class DocumentMovePlan
    {
        /// <summary>Da spostare, in ordine: percorsi completi di partenza e di arrivo.</summary>
        public IReadOnlyList<DocumentMove> Moves { get; init; } = Array.Empty<DocumentMove>();
        /// <summary>Già nella cartella di arrivo: non c'è niente da fare (percorsi completi).</summary>
        public IReadOnlyList<string> Skipped { get; init; } = Array.Empty<string>();
        /// <summary>Perché non si sposta niente; null se si può.</summary>
        public string Refusal { get; init; }
    }

    /// <summary>
    /// Il controllo di uno spostamento di più documenti (sprint 2026-11-25-Spostamento-File-E-Link): o tutto l'elenco si
    /// può spostare, o non si sposta niente e si dice quale documento lo impedisce. Un nome già presente nella cartella di
    /// arrivo, o ripetuto fra i documenti scelti, è un rifiuto: spostare cancellerebbe un documento.
    /// </summary>
    public static class DocumentMovePlanner
    {
        public static DocumentMovePlan Plan(string projectRoot, IEnumerable<string> sourceRelativePaths, string destination)
        {
            if (string.IsNullOrWhiteSpace(projectRoot) || !Directory.Exists(projectRoot))
                return Refuse(UiText.Say("Nessun progetto aperto.", "No project is open."));
            var root = Path.GetFullPath(projectRoot);
            var cmp = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            var dest = Path.GetFullPath(Path.IsPathRooted(destination ?? string.Empty)
                ? destination
                : Path.Combine(root, Clean(destination)));
            if (!Inside(root, dest, cmp))
                return Refuse(UiText.Say($"La cartella «{destination}» è fuori dal progetto.", $"The folder «{destination}» is outside the project."));
            if (!Directory.Exists(dest))
                return Refuse(UiText.Say($"La cartella «{Rel(root, dest)}» non c'è.", $"The folder «{Rel(root, dest)}» does not exist."));

            var moves = new List<DocumentMove>();
            var inputs = (sourceRelativePaths ?? Enumerable.Empty<string>()).Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => (Raw: s, Full: Path.GetFullPath(Path.Combine(root, Clean(s))))).ToList();
            // Le cartelle scelte: quello che sta dentro di loro viaggia con loro, non da solo.
            var chosenFolders = inputs.Where(i => Directory.Exists(i.Full)).Select(i => i.Full.TrimEnd(Path.DirectorySeparatorChar)).ToList();
            var skipped = new List<string>();
            var names = new Dictionary<string, string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            foreach (var (raw, fullFrom) in inputs)
            {
                var from = fullFrom.TrimEnd(Path.DirectorySeparatorChar);
                if (moves.Any(m => string.Equals(m.From, from, cmp)) || skipped.Any(s => string.Equals(s, from, cmp))) continue;
                if (chosenFolders.Any(f => !string.Equals(f, from, cmp) && from.StartsWith(f + Path.DirectorySeparatorChar, cmp))) continue;
                if (!Inside(root, from, cmp))
                    return Refuse(UiText.Say($"«{raw}» è fuori dal progetto.", $"«{raw}» is outside the project."));
                var isFolder = Directory.Exists(from);
                if (isFolder)
                {
                    if (string.Equals(from, root.TrimEnd(Path.DirectorySeparatorChar), cmp))
                        return Refuse(UiText.Say("La cartella del progetto non si sposta.", "The project folder cannot be moved."));
                    if (string.Equals(dest, from, cmp) || dest.StartsWith(from + Path.DirectorySeparatorChar, cmp))
                        return Refuse(UiText.Say($"«{Rel(root, from)}» non può andare dentro se stessa.", $"«{Rel(root, from)}» cannot go inside itself."));
                }
                else if (!File.Exists(from))
                    return Refuse(UiText.Say($"«{Rel(root, from)}» non c'è più: forse è stato spostato o cancellato.",
                        $"«{Rel(root, from)}» is no longer there: maybe it was moved or deleted."));
                if (!isFolder && !from.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                    return Refuse(UiText.Say($"«{Rel(root, from)}» non è un documento markdown: per ora si spostano solo quelli.",
                        $"«{Rel(root, from)}» is not a markdown document: for now only those can be moved."));
                if (string.Equals(Path.GetDirectoryName(from), dest, cmp)) { skipped.Add(from); continue; }

                var name = Path.GetFileName(from);
                var to = Path.Combine(dest, name);
                if (names.TryGetValue(name, out var other))
                    return Refuse(UiText.Say($"«{Rel(root, from)}» e «{Rel(root, other)}» hanno lo stesso nome: nella stessa cartella non ci stanno tutti e due.",
                        $"«{Rel(root, from)}» and «{Rel(root, other)}» have the same name: they cannot both go in the same folder."));
                if (File.Exists(to) || Directory.Exists(to))
                    return Refuse(UiText.Say($"In «{Rel(root, dest)}» c'è già «{name}»: non sposto niente, per non cancellarlo.",
                        $"«{Rel(root, dest)}» already has «{name}»: nothing is moved, so as not to delete it."));
                names[name] = from;
                moves.Add(new DocumentMove(from, to, isFolder));
            }
            if (moves.Count == 0 && skipped.Count == 0)
                return Refuse(UiText.Say("Niente da spostare.", "Nothing to move."));
            return new DocumentMovePlan { Moves = moves, Skipped = skipped };
        }

        /// <summary>
        /// La rinomina di un documento o di una cartella: uno spostamento nello stesso posto con un altro nome. Il nome
        /// nuovo non ha barre né caratteri non validi e non è di un altro file o cartella; un documento resta un <c>.md</c>
        /// (si aggiunge se manca); su Windows cambiare solo maiuscole e minuscole è ammesso. <paramref name="source"/> è
        /// relativo al progetto o completo.
        /// </summary>
        public static DocumentMovePlan PlanRename(string projectRoot, string source, string newName)
        {
            if (string.IsNullOrWhiteSpace(projectRoot) || !Directory.Exists(projectRoot))
                return Refuse(UiText.Say("Nessun progetto aperto.", "No project is open."));
            var root = Path.GetFullPath(projectRoot);
            var cmp = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var from = SourcePath(root, source);
            from = from.TrimEnd(Path.DirectorySeparatorChar);
            if (!Inside(root, from, cmp))
                return Refuse(UiText.Say($"«{source}» è fuori dal progetto.", $"«{source}» is outside the project."));
            var isFolder = Directory.Exists(from);
            if (isFolder && string.Equals(from, root.TrimEnd(Path.DirectorySeparatorChar), cmp))
                return Refuse(UiText.Say("La cartella del progetto non si rinomina da qui.", "The project folder cannot be renamed from here."));
            if (!isFolder && !File.Exists(from))
                return Refuse(UiText.Say($"«{Rel(root, from)}» non c'è più: forse è stato spostato o cancellato.",
                    $"«{Rel(root, from)}» is no longer there: maybe it was moved or deleted."));
            if (!isFolder && !from.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                return Refuse(UiText.Say($"«{Rel(root, from)}» non è un documento markdown.", $"«{Rel(root, from)}» is not a markdown document."));

            var name = (newName ?? string.Empty).Trim();
            if (name.Length == 0) return Refuse(UiText.Say("Manca il nome nuovo.", "The new name is missing."));
            if (name.IndexOfAny(new[] { '/', '\\' }) >= 0 || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name == "." || name == "..")
                return Refuse(UiText.Say($"«{name}» non è un nome di file valido (niente barre né caratteri speciali).",
                    $"«{name}» is not a valid file name (no slashes or special characters)."));
            if (!isFolder && !name.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) name += ".md";
            if (!isFolder && name.Length == 3) return Refuse(UiText.Say("Manca il nome nuovo.", "The new name is missing."));

            var to = Path.Combine(Path.GetDirectoryName(from), name);
            if (string.Equals(to, from, StringComparison.Ordinal))
                return Refuse(UiText.Say("Il nome è uguale a quello di adesso.", "The name is the same as the current one."));
            // Su Windows «A.md» e «a.md» sono lo stesso file: cambiare solo le maiuscole è una rinomina, non un conflitto.
            var sameFile = string.Equals(to, from, StringComparison.OrdinalIgnoreCase) && OperatingSystem.IsWindows();
            if (!sameFile && (File.Exists(to) || Directory.Exists(to)))
                return Refuse(UiText.Say($"In «{Rel(root, Path.GetDirectoryName(from))}» c'è già «{name}»: scegli un altro nome.",
                    $"«{Rel(root, Path.GetDirectoryName(from))}» already has «{name}»: choose another name."));
            return new DocumentMovePlan { Moves = new[] { new DocumentMove(from, to, isFolder) } };
        }

        /// <summary>Un percorso del client: completo se comincia con la radice del progetto, altrimenti relativo all'albero.</summary>
        private static string SourcePath(string root, string raw)
        {
            var r = (raw ?? string.Empty).Trim();
            if (Path.IsPathRooted(r) && Path.GetFullPath(r).StartsWith(root, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                return Path.GetFullPath(r);
            return Path.GetFullPath(Path.Combine(root, Clean(r)));
        }

        private static DocumentMovePlan Refuse(string why) => new() { Refusal = why };

        /// <summary>Un percorso dell'albero: senza la barra iniziale e con il separatore del sistema.</summary>
        private static string Clean(string path)
            => (path ?? string.Empty).Replace('\\', '/').TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

        private static bool Inside(string root, string path, StringComparison cmp)
        {
            var r = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return string.Equals(path.TrimEnd(Path.DirectorySeparatorChar), root.TrimEnd(Path.DirectorySeparatorChar), cmp)
                   || path.StartsWith(r, cmp);
        }

        private static string Rel(string root, string path) => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
    }
}
