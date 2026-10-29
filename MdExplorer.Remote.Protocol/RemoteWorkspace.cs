using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.RemoteProtocol
{
    /// <summary>
    /// Le operazioni sui file dentro una radice. Regole (piano, «F1 — MDE Remote»): nessun percorso esce dalla radice,
    /// i link simbolici non si seguono, una scrittura presenta l'impronta letta, il testo si riscrive nella sua codifica (UTF-8,
    /// CP-850, CP-1252, UTF-16, riconosciuta dal contenuto) e il BOM si conserva.
    /// </summary>
    public sealed class RemoteWorkspace
    {
        public const int DefaultMaxHits = 1000;
        private const int SniffBytes = 8192;
        private const int MaxHitText = 1000;
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(2);

        /// <summary>La radice assoluta, senza separatore finale.</summary>
        public string Root { get; }

        /// <summary>Le codifiche per estensione della radice (arrivano con ogni richiesta; vedi <see cref="TextDecoding.ForExtension"/>).</summary>
        public IReadOnlyDictionary<string, string>? ExtensionEncodings { get; set; }

        /// <summary>Il testo di un file: la codifica dell'estensione se assegnata, altrimenti UTF-8 o la stima. Null = binario.</summary>
        private DecodedText? DecodeFile(string fullPath, byte[] bytes)
        {
            var assigned = TextDecoding.ForExtension(fullPath, bytes, ExtensionEncodings);
            if (assigned != null) return bytes.AsSpan().IndexOf((byte)0) >= 0 && !assigned.StartsWith("utf-16", StringComparison.Ordinal) ? null : TextDecoding.DecodeAs(bytes, assigned);
            return TextDecoding.Decode(bytes);
        }

        public RemoteWorkspace(string root)
        {
            var full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
            if (!Directory.Exists(full)) throw new DirectoryNotFoundException($"root not found: {full}");
            Root = full;
        }

        public List<RemoteEntry> List(string? path)
        {
            var full = Resolve(path);
            if (!Directory.Exists(full)) throw NotFoundOrNotFolder(full, path);
            return new DirectoryInfo(full).EnumerateFileSystemInfos()
                .Where(e => !IsLink(e) && !(e is DirectoryInfo && e.Name == ".git"))
                .Select(ToEntry)
                .OrderBy(e => e.IsFolder ? 0 : 1)
                .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Tutti i file sotto <paramref name="path"/>, con dimensione, impronta e «è testo», in ordine (grafo del codice
        /// sulle radici remote, G-D4): il backend vede che cosa è cambiato senza una richiesta per cartella né per file.
        /// Le cartelle in <paramref name="exclude"/> (per nome, maiuscole indifferenti, come lo scanner) e <c>.git</c> si
        /// saltano; i link non si seguono.
        /// </summary>
        public List<RemoteEntry> Tree(string? path, IEnumerable<string>? exclude)
        {
            var full = Resolve(path);
            if (!Directory.Exists(full)) throw NotFoundOrNotFolder(full, path);
            var skip = new HashSet<string>(exclude ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase) { ".git" };
            return FilesUnder(full, skip).Select(f =>
            {
                var file = new FileInfo(f);
                var entry = ToEntry(file);
                entry.Stamp = StampOf(file);
                return entry;
            }).ToList();
        }

        public RemoteEntry Stat(string? path)
        {
            var full = Resolve(path);
            FileSystemInfo info = Directory.Exists(full) ? new DirectoryInfo(full) : new FileInfo(full);
            if (!info.Exists) throw new RemoteFailure(RemoteErrorCodes.NotFound, $"not found: {path}");
            var entry = ToEntry(info);
            if (info is FileInfo file) entry.Stamp = StampOf(file);
            return entry;
        }

        /// <summary>Il testo nella sua codifica, riconosciuta dal contenuto (<see cref="TextDecoding"/>): UTF-8, CP-850, CP-1252, UTF-16.</summary>
        /// <param name="encoding">Rileggere in questa invece di riconoscerla (l'editor, «Rileggi come» / «Ripara»).</param>
        public (string Text, string Stamp, bool Bom, string Encoding) Read(string? path, string? encoding = null)
        {
            var file = ExistingFile(path);
            var bytes = File.ReadAllBytes(file.FullName);
            var decoded = string.IsNullOrEmpty(encoding)
                ? DecodeFile(file.FullName, bytes) ?? throw new RemoteFailure(RemoteErrorCodes.Binary, $"binary file: {path}")
                : TextDecoding.DecodeAs(bytes, encoding);
            return (decoded.Text, StampOf(file), decoded.Bom, decoded.Encoding);
        }

        /// <summary>Riscrive un file esistente se è ancora quello letto (<paramref name="stamp"/>); BOM e permessi restano.</summary>
        /// <param name="encoding">Scrivere in questa (convertire); assente = la codifica del file com'è.</param>
        public string Write(string? path, string? text, string? stamp, string? encoding = null)
        {
            if (text == null) throw new RemoteFailure(RemoteErrorCodes.BadRequest, "text required");
            if (string.IsNullOrEmpty(stamp)) throw new RemoteFailure(RemoteErrorCodes.BadRequest, "stamp required: read the file first");
            var file = ExistingFile(path);
            var current = StampOf(file);
            if (current != stamp) throw new RemoteFailure(RemoteErrorCodes.Changed, $"changed since read: {path}");
            // la stessa codifica (e lo stesso BOM) del file com'è: con l'impronta uguale sono i byte che si sono letti
            var existing = DecodeFile(file.FullName, File.ReadAllBytes(file.FullName))
                ?? throw new RemoteFailure(RemoteErrorCodes.Binary, $"binary file: {path}");
            // convertire: il BOM resta solo se la codifica non cambia (UTF-16 lo vuole sempre)
            var target = string.IsNullOrEmpty(encoding) ? existing.Encoding : encoding;
            var bom = target.StartsWith("utf-16", StringComparison.Ordinal) || (target == existing.Encoding && existing.Bom);
            var bytes = TextDecoding.Encode(text, target, bom);
            var tmp = Path.Combine(file.DirectoryName!, $".{file.Name}.mde-{Guid.NewGuid():N}.tmp");
            try
            {
                File.WriteAllBytes(tmp, bytes);
                if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(tmp, File.GetUnixFileMode(file.FullName));
                File.Move(tmp, file.FullName, overwrite: true);
            }
            finally
            {
                if (File.Exists(tmp)) File.Delete(tmp);
            }
            return StampOf(new FileInfo(file.FullName));
        }

        /// <summary>Un file nuovo (UTF-8 senza BOM); le cartelle mancanti si creano. Rifiuta se esiste già.</summary>
        public string Create(string? path, string? text)
        {
            var full = ResolveNew(path);
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            // un file nuovo nella codifica della sua estensione, se la radice ne ha assegnata una; altrimenti UTF-8
            var encoding = TextDecoding.ForExtension(full, ReadOnlySpan<byte>.Empty, ExtensionEncodings) ?? TextDecoding.Utf8;
            using (var stream = new FileStream(full, FileMode.CreateNew, FileAccess.Write))
            {
                var bytes = TextDecoding.Encode(text ?? "", encoding, bom: encoding.StartsWith("utf-16", StringComparison.Ordinal));
                stream.Write(bytes, 0, bytes.Length);
            }
            return StampOf(new FileInfo(full));
        }

        /// <summary>Una cartella nuova, con quelle sopra se mancano. Rifiuta se esiste già.</summary>
        public void Mkdir(string? path)
        {
            Directory.CreateDirectory(ResolveNew(path));
        }

        public void Delete(string? path, bool recursive)
        {
            var full = ResolveNotRoot(path);
            if (File.Exists(full)) { File.Delete(full); return; }
            if (!Directory.Exists(full)) throw new RemoteFailure(RemoteErrorCodes.NotFound, $"not found: {path}");
            if (!recursive && Directory.EnumerateFileSystemEntries(full).Any())
                throw new RemoteFailure(RemoteErrorCodes.NotEmpty, $"folder not empty: {path}");
            // Directory.Delete non segue i link dentro la cartella: toglie il link, non ciò a cui punta.
            Directory.Delete(full, recursive);
        }

        /// <summary>Rinomina o sposta un file o una cartella; la destinazione non deve esistere, le cartelle sopra si creano.</summary>
        public void Move(string? path, string? to)
        {
            var source = ResolveNotRoot(path);
            var target = ResolveNew(to);
            var isFile = File.Exists(source);
            if (!isFile && !Directory.Exists(source)) throw new RemoteFailure(RemoteErrorCodes.NotFound, $"not found: {path}");
            if (!isFile && (target + Path.DirectorySeparatorChar).StartsWith(source + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                throw new RemoteFailure(RemoteErrorCodes.BadRequest, $"cannot move a folder into itself: {path} -> {to}");
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            if (isFile) File.Move(source, target); else Directory.Move(source, target);
        }

        public (List<RemoteHit> Hits, bool Truncated) Grep(string? path, string? pattern, bool ignoreCase, int maxHits, IEnumerable<string>? exclude)
        {
            if (string.IsNullOrEmpty(pattern)) throw new RemoteFailure(RemoteErrorCodes.BadRequest, "pattern required");
            var start = Resolve(path);
            Regex regex;
            try
            {
                regex = new Regex(pattern, RegexOptions.CultureInvariant | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None), RegexTimeout);
            }
            catch (ArgumentException ex) { throw new RemoteFailure(RemoteErrorCodes.BadRequest, $"invalid pattern: {ex.Message}"); }
            var limit = maxHits > 0 ? maxHits : DefaultMaxHits;
            var skip = new HashSet<string>(exclude ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
            var hits = new List<RemoteHit>();
            var files = File.Exists(start) ? new[] { start } : Directory.Exists(start) ? FilesUnder(start, skip) : throw NotFoundOrNotFolder(start, path);
            foreach (var file in files)
            {
                if (!LooksLikeText(file)) continue;
                var number = 0;
                foreach (var line in DecodedLinesOrNothing(file, DecodeFile))
                {
                    number++;
                    bool match;
                    try { match = regex.IsMatch(line); }
                    catch (RegexMatchTimeoutException) { throw new RemoteFailure(RemoteErrorCodes.Timeout, $"pattern too slow on {Relative(file)}:{number}"); }
                    if (!match) continue;
                    if (hits.Count == limit) return (hits, true);
                    hits.Add(new RemoteHit { Path = Relative(file), Line = number, Text = line.Length > MaxHitText ? line.Substring(0, MaxHitText) : line });
                }
            }
            return (hits, false);
        }

        /// <summary>Il repository che contiene la radice, leggendo i file di <c>.git</c>; <c>null</c> se non c'è.</summary>
        public RemoteGitInfo? GitInfo()
        {
            for (var dir = new DirectoryInfo(Root); dir != null; dir = dir.Parent)
            {
                var dotGit = Path.Combine(dir.FullName, ".git");
                string? gitDir = null;
                if (Directory.Exists(dotGit)) gitDir = dotGit;
                else if (File.Exists(dotGit)) gitDir = GitDirFromFile(dotGit);
                if (gitDir == null) continue;
                var config = Path.Combine(CommonGitDir(gitDir), "config");
                return new RemoteGitInfo
                {
                    Top = dir.FullName,
                    Remotes = File.Exists(config) ? GitConfig.Remotes(File.ReadAllLines(config)) : new List<RemoteGitRemote>(),
                };
            }
            return null;
        }

        // ---- percorsi ----

        /// <summary>
        /// Il percorso assoluto di <paramref name="path"/> (relativo alla radice, <c>/</c>), se sta dentro la radice e non
        /// passa da un link simbolico. Il controllo per prefisso vuole il separatore: <c>/srv/proj2</c> non è dentro <c>/srv/proj</c>.
        /// </summary>
        public string Resolve(string? path)
        {
            if (path == null) throw new RemoteFailure(RemoteErrorCodes.BadRequest, "path required");
            if (Path.IsPathRooted(path)) throw new RemoteFailure(RemoteErrorCodes.BadRequest, $"path must be relative to the root: {path}");
            var full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.Combine(Root, path)));
            if (full != Root && !full.StartsWith(Root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                throw new RemoteFailure(RemoteErrorCodes.OutsideRoot, $"path outside root: {path}");
            var current = Root;
            foreach (var part in full.Substring(Root.Length).Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                current = Path.Combine(current, part);
                var info = new FileInfo(current);
                if (!info.Exists && !Directory.Exists(current)) break;   // da qui in giù non esiste: niente link possibili
                if (IsLink(info)) throw new RemoteFailure(RemoteErrorCodes.Link, $"symbolic links are not followed: {path}");
            }
            return full;
        }

        private string ResolveNotRoot(string? path)
        {
            var full = Resolve(path);
            if (full == Root) throw new RemoteFailure(RemoteErrorCodes.BadRequest, "the root itself cannot be changed");
            return full;
        }

        private string ResolveNew(string? path)
        {
            var full = ResolveNotRoot(path);
            if (File.Exists(full) || Directory.Exists(full)) throw new RemoteFailure(RemoteErrorCodes.Exists, $"already exists: {path}");
            return full;
        }

        private FileInfo ExistingFile(string? path)
        {
            var file = new FileInfo(Resolve(path));
            if (!file.Exists)
                throw Directory.Exists(file.FullName)
                    ? new RemoteFailure(RemoteErrorCodes.BadRequest, $"is a folder: {path}")
                    : new RemoteFailure(RemoteErrorCodes.NotFound, $"not found: {path}");
            return file;
        }

        private static RemoteFailure NotFoundOrNotFolder(string full, string? path) =>
            File.Exists(full)
                ? new RemoteFailure(RemoteErrorCodes.BadRequest, $"is a file: {path}")
                : new RemoteFailure(RemoteErrorCodes.NotFound, $"not found: {path}");

        private string Relative(string full) => full == Root ? "" : full.Substring(Root.Length + 1).Replace(Path.DirectorySeparatorChar, '/');

        // ---- file ----

        /// <summary>Dimensione + data di modifica: basta a vedere che il file è cambiato, senza hash crittografici (D5).</summary>
        public static string StampOf(FileInfo file) => $"{file.Length}-{file.LastWriteTimeUtc.Ticks}";

        private static bool IsLink(FileSystemInfo info) => info.LinkTarget != null;

        private RemoteEntry ToEntry(FileSystemInfo info)
        {
            var entry = new RemoteEntry { Name = info.Name, Path = Relative(info.FullName), IsFolder = info is DirectoryInfo };
            if (info is FileInfo file)
            {
                entry.Size = file.Length;
                try { entry.IsText = LooksLikeText(file.FullName, rethrow: true); }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { entry.Unreadable = ex.Message; }
            }
            return entry;
        }

        private static bool LooksLikeText(string file, bool rethrow = false)
        {
            try
            {
                using var stream = File.OpenRead(file);
                var buffer = new byte[SniffBytes];
                var read = stream.Read(buffer, 0, buffer.Length);
                return TextDecoding.LooksLikeText(buffer.AsSpan(0, read));
            }
            catch (Exception) when (!rethrow) { return false; }
        }

        private static IEnumerable<string> FilesUnder(string dir, HashSet<string> skip)
        {
            IEnumerable<FileSystemInfo> children;
            try { children = new DirectoryInfo(dir).EnumerateFileSystemInfos().OrderBy(c => c.Name, StringComparer.Ordinal).ToList(); }
            catch (UnauthorizedAccessException) { yield break; }
            foreach (var child in children)
            {
                if (IsLink(child)) continue;
                if (child is DirectoryInfo)
                {
                    if (skip.Contains(child.Name)) continue;
                    foreach (var f in FilesUnder(child.FullName, skip)) yield return f;
                }
                else yield return child.FullName;
            }
        }

        /// <summary>
        /// Le righe di un file da cercare, decodificato nella sua codifica (un file CP-850 si trova cercando «città»); un
        /// file che non si apre non ferma la ricerca.
        /// </summary>
        private static IEnumerable<string> DecodedLinesOrNothing(string file, Func<string, byte[], DecodedText?> decode)
        {
            DecodedText? decoded;
            try { decoded = decode(file, File.ReadAllBytes(file)); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { yield break; }
            if (decoded == null) yield break;
            using var reader = new StringReader(decoded.Text);
            string? line;
            while ((line = reader.ReadLine()) != null) yield return line;
        }

        // ---- git ----

        /// <summary><c>.git</c> come file (worktree, submodule): «gitdir: percorso».</summary>
        private static string? GitDirFromFile(string dotGitFile)
        {
            var line = File.ReadLines(dotGitFile).FirstOrDefault(l => l.StartsWith("gitdir:", StringComparison.Ordinal));
            if (line == null) return null;
            var target = line.Substring("gitdir:".Length).Trim();
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(dotGitFile)!, target));
        }

        /// <summary>Un worktree tiene la config nella cartella comune indicata da <c>commondir</c>.</summary>
        private static string CommonGitDir(string gitDir)
        {
            var commonDir = Path.Combine(gitDir, "commondir");
            return File.Exists(commonDir) ? Path.GetFullPath(Path.Combine(gitDir, File.ReadAllText(commonDir).Trim())) : gitDir;
        }
    }
}
