using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MdExplorer.Remote.Protocol;

namespace MdExplorer.RemoteAgent
{
    /// <summary>
    /// Le operazioni sui file dentro una radice. Regole (piano, «F1 — il serverino»): nessun percorso esce dalla radice,
    /// i link simbolici non si seguono, una scrittura presenta l'impronta letta, il testo è UTF-8 e il BOM si conserva.
    /// </summary>
    public sealed class RemoteWorkspace
    {
        public const int DefaultMaxHits = 1000;
        private const int SniffBytes = 8192;
        private const int MaxHitText = 1000;
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(2);
        private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        private static readonly byte[] Utf8Bom = { 0xEF, 0xBB, 0xBF };

        /// <summary>La radice assoluta, senza separatore finale.</summary>
        public string Root { get; }

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

        public RemoteEntry Stat(string? path)
        {
            var full = Resolve(path);
            FileSystemInfo info = Directory.Exists(full) ? new DirectoryInfo(full) : new FileInfo(full);
            if (!info.Exists) throw new RemoteFailure(RemoteErrorCodes.NotFound, $"not found: {path}");
            var entry = ToEntry(info);
            if (info is FileInfo file) entry.Stamp = StampOf(file);
            return entry;
        }

        public (string Text, string Stamp, bool Bom) Read(string? path)
        {
            var file = ExistingFile(path);
            var bytes = File.ReadAllBytes(file.FullName);
            if (Array.IndexOf(bytes, (byte)0, 0, Math.Min(bytes.Length, SniffBytes)) >= 0)
                throw new RemoteFailure(RemoteErrorCodes.Binary, $"binary file: {path}");
            var bom = bytes.AsSpan().StartsWith(Utf8Bom);
            string text;
            try { text = StrictUtf8.GetString(bytes, bom ? 3 : 0, bytes.Length - (bom ? 3 : 0)); }
            catch (DecoderFallbackException) { throw new RemoteFailure(RemoteErrorCodes.NotUtf8, $"not UTF-8: {path}"); }
            return (text, StampOf(file), bom);
        }

        /// <summary>Riscrive un file esistente se è ancora quello letto (<paramref name="stamp"/>); BOM e permessi restano.</summary>
        public string Write(string? path, string? text, string? stamp)
        {
            if (text == null) throw new RemoteFailure(RemoteErrorCodes.BadRequest, "text required");
            if (string.IsNullOrEmpty(stamp)) throw new RemoteFailure(RemoteErrorCodes.BadRequest, "stamp required: read the file first");
            var file = ExistingFile(path);
            var current = StampOf(file);
            if (current != stamp) throw new RemoteFailure(RemoteErrorCodes.Changed, $"changed since read: {path}");
            bool bom;
            using (var head = file.OpenRead())
            {
                var first = new byte[3];
                bom = head.Read(first, 0, 3) == 3 && first.AsSpan().SequenceEqual(Utf8Bom);
            }
            var tmp = Path.Combine(file.DirectoryName!, $".{file.Name}.mde-{Guid.NewGuid():N}.tmp");
            try
            {
                File.WriteAllBytes(tmp, Encode(text, bom));
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
            using (var stream = new FileStream(full, FileMode.CreateNew, FileAccess.Write))
            {
                var bytes = Encode(text ?? "", bom: false);
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
                foreach (var line in ReadLinesOrNothing(file))
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
                return Array.IndexOf(buffer, (byte)0, 0, read) < 0;
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

        /// <summary>Le righe di un file da cercare; un file che non si apre non ferma la ricerca.</summary>
        private static IEnumerable<string> ReadLinesOrNothing(string file)
        {
            IEnumerable<string> lines;
            try { lines = File.ReadLines(file); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { yield break; }
            foreach (var line in lines) yield return line;
        }

        private static byte[] Encode(string text, bool bom)
        {
            var body = StrictUtf8.GetBytes(text);
            if (!bom) return body;
            var all = new byte[body.Length + 3];
            Utf8Bom.CopyTo(all, 0);
            body.CopyTo(all, 3);
            return all;
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
