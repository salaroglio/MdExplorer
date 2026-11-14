using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MdExplorer.Services.FileSystemWatcherManager
{
    /// <summary>
    /// Linux: UNA istanza inotify e un watch solo per le cartelle utili (sprint 2026-10-21-Watcher-Selettivo-E-Gitignore,
    /// W3). FileSystemWatcher ricorsivo mette un watch su OGNI sottocartella (.git, node_modules…): su questo repository
    /// 20.805 contro 974; e ogni FileSystemWatcher è un'istanza in più (limite 128 per utente), quindi non si possono usare
    /// tanti watcher non ricorsivi. Gli eventi sono quelli di FileSystemWatcher (Changed = IN_MODIFY, Created, Deleted,
    /// Renamed con le coppie MOVED_FROM/MOVED_TO, Error su IN_Q_OVERFLOW).
    /// </summary>
    public sealed class InotifyProjectWatcher : IProjectWatcher
    {
        private const uint IN_MODIFY = 0x2, IN_MOVED_FROM = 0x40, IN_MOVED_TO = 0x80, IN_CREATE = 0x100, IN_DELETE = 0x200,
            IN_DELETE_SELF = 0x400, IN_MOVE_SELF = 0x800, IN_Q_OVERFLOW = 0x4000, IN_IGNORED = 0x8000, IN_ONLYDIR = 0x01000000,
            IN_DONT_FOLLOW = 0x02000000, IN_EXCL_UNLINK = 0x04000000, IN_ISDIR = 0x40000000;
        private const uint Mask = IN_MODIFY | IN_MOVED_FROM | IN_MOVED_TO | IN_CREATE | IN_DELETE | IN_DELETE_SELF | IN_MOVE_SELF
                                  | IN_ONLYDIR | IN_DONT_FOLLOW | IN_EXCL_UNLINK;
        private const int IN_CLOEXEC = 0x80000, IN_NONBLOCK = 0x800, ENOSPC = 28, POLLIN = 1;

        [DllImport("libc", SetLastError = true)] private static extern int inotify_init1(int flags);
        [DllImport("libc", SetLastError = true)] private static extern int inotify_add_watch(int fd, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, uint mask);
        [DllImport("libc", SetLastError = true)] private static extern int inotify_rm_watch(int fd, int wd);
        [DllImport("libc", SetLastError = true)] private static extern nint read(int fd, byte[] buffer, nint count);
        [DllImport("libc", SetLastError = true)] private static extern int close(int fd);
        [DllImport("libc", SetLastError = true)] private static extern int poll(ref PollFd fds, nuint nfds, int timeout);

        [StructLayout(LayoutKind.Sequential)]
        private struct PollFd { public int fd; public short events; public short revents; }

        private readonly Func<string, bool> _shouldWatch;
        private readonly object _lock = new();
        private readonly Dictionary<int, string> _pathByWd = new();
        private readonly Dictionary<string, int> _wdByPath = new(StringComparer.Ordinal);
        private Thread _reader;
        private int _fd;
        private volatile bool _disposed;
        private bool _limitReported;

        public string RootPath { get; }
        public bool EnableRaisingEvents { get; set; } = true;
        public int WatchedDirectories { get { lock (_lock) return _wdByPath.Count; } }

        public event FileSystemEventHandler Changed;
        public event FileSystemEventHandler Created;
        public event FileSystemEventHandler Deleted;
        public event RenamedEventHandler Renamed;
        public event ErrorEventHandler Error;

        public InotifyProjectWatcher(string rootPath, Func<string, bool> shouldWatch)
        {
            RootPath = Path.GetFullPath(rootPath).TrimEnd('/');
            _shouldWatch = shouldWatch;
            _fd = inotify_init1(IN_CLOEXEC | IN_NONBLOCK);
            if (_fd < 0)
                throw new IOException($"inotify_init1 failed (errno {Marshal.GetLastWin32Error()}): too many inotify instances? (fs.inotify.max_user_instances)");
        }

        public void Start()
        {
            if (_reader != null) return;
            AddTree(RootPath, isRoot: true);
            _reader = new Thread(ReadLoop) { IsBackground = true, Name = "inotify " + Path.GetFileName(RootPath) };
            _reader.Start();
        }

        public void Resync()
        {
            lock (_lock)
            {
                foreach (var (path, wd) in _wdByPath.ToList())
                    if (path != RootPath && !WatchAllowed(path)) Remove(path, wd);
            }
            AddTree(RootPath, isRoot: true);
        }

        // ---- watch ----

        /// <summary>La cartella e le sue sottocartelle da sorvegliare; i link simbolici non si seguono.</summary>
        private void AddTree(string dir, bool isRoot = false)
        {
            var stack = new Stack<string>();
            stack.Push(dir);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!isRoot || current != RootPath)
                    if (!WatchAllowed(current)) continue;
                if (!AddWatch(current)) continue;
                IEnumerable<DirectoryInfo> children;
                try { children = new DirectoryInfo(current).EnumerateDirectories().Where(d => d.LinkTarget == null).ToList(); }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { continue; }
                foreach (var child in children) stack.Push(child.FullName);
            }
        }

        private bool WatchAllowed(string dir)
        {
            try { return _shouldWatch(dir); }
            catch (Exception ex)
            {
                Raise(Error, new ErrorEventArgs(new IOException($"rule for {dir} failed: {ex.Message}", ex)));
                return false;
            }
        }

        private bool AddWatch(string dir)
        {
            lock (_lock)
            {
                if (_disposed) return false;
                if (_wdByPath.ContainsKey(dir)) return true;
                var wd = inotify_add_watch(_fd, dir, Mask);
                if (wd < 0)
                {
                    var errno = Marshal.GetLastWin32Error();
                    if (errno == ENOSPC && !_limitReported)
                    {
                        // UNA volta per watcher: un errore per cartella era il ciclo di ricariche del 21/10
                        _limitReported = true;
                        Raise(Error, new ErrorEventArgs(new IOException(
                            $"inotify watch limit reached (fs.inotify.max_user_watches) while watching {dir}: some folders are not watched")));
                    }
                    return false;
                }
                if (_pathByWd.TryGetValue(wd, out var old) && old != dir) _wdByPath.Remove(old);   // stessa cartella, nome nuovo
                _pathByWd[wd] = dir;
                _wdByPath[dir] = wd;
                return true;
            }
        }

        private void Remove(string path, int wd)
        {
            inotify_rm_watch(_fd, wd);
            _pathByWd.Remove(wd);
            _wdByPath.Remove(path);
        }

        /// <summary>Una cartella sorvegliata è stata rinominata: i suoi watch restano, cambiano i percorsi del sottoalbero.</summary>
        private void MoveSubtree(string from, string to)
        {
            lock (_lock)
            {
                foreach (var (path, wd) in _wdByPath.Where(p => p.Key == from || p.Key.StartsWith(from + "/", StringComparison.Ordinal)).ToList())
                {
                    var moved = to + path.Substring(from.Length);
                    _wdByPath.Remove(path);
                    _wdByPath[moved] = wd;
                    _pathByWd[wd] = moved;
                }
            }
        }

        private void ForgetSubtree(string dir)
        {
            lock (_lock)
                foreach (var (path, wd) in _wdByPath.Where(p => p.Key == dir || p.Key.StartsWith(dir + "/", StringComparison.Ordinal)).ToList())
                    Remove(path, wd);
        }

        // ---- lettura ----

        private void ReadLoop()
        {
            var buffer = new byte[64 * 1024];
            (uint Cookie, string Path, bool IsDir)? pendingMove = null;
            while (!_disposed)
            {
                var pfd = new PollFd { fd = _fd, events = POLLIN };
                var ready = poll(ref pfd, 1, 200);
                if (_disposed) break;
                if (ready <= 0)
                {
                    // un MOVED_FROM rimasto solo: il file è uscito dalle cartelle sorvegliate → cancellato
                    if (pendingMove is { } lone) { pendingMove = null; MovedAway(lone.Path, lone.IsDir); }
                    continue;
                }
                var n = (int)read(_fd, buffer, buffer.Length);
                if (n <= 0) continue;
                var offset = 0;
                while (offset + 16 <= n)
                {
                    var wd = BitConverter.ToInt32(buffer, offset);
                    var mask = BitConverter.ToUInt32(buffer, offset + 4);
                    var cookie = BitConverter.ToUInt32(buffer, offset + 8);
                    var len = (int)BitConverter.ToUInt32(buffer, offset + 12);
                    var name = len > 0 ? Encoding.UTF8.GetString(buffer, offset + 16, len).TrimEnd('\0') : string.Empty;
                    offset += 16 + len;

                    if ((mask & IN_Q_OVERFLOW) != 0)
                    {
                        Raise(Error, new ErrorEventArgs(new InternalBufferOverflowException("inotify queue overflow: events were lost")));
                        continue;
                    }
                    string dir;
                    lock (_lock) { if (!_pathByWd.TryGetValue(wd, out dir)) continue; }
                    if ((mask & IN_IGNORED) != 0) { lock (_lock) { _pathByWd.Remove(wd); if (_wdByPath.TryGetValue(dir, out var w) && w == wd) _wdByPath.Remove(dir); } continue; }
                    if ((mask & (IN_DELETE_SELF | IN_MOVE_SELF)) != 0) continue;   // lo dice già il genitore
                    var isDir = (mask & IN_ISDIR) != 0;
                    var full = name.Length == 0 ? dir : dir + "/" + name;

                    if (pendingMove is { } from && ((mask & IN_MOVED_TO) == 0 || cookie != from.Cookie))
                    {
                        pendingMove = null;
                        MovedAway(from.Path, from.IsDir);
                    }
                    if ((mask & IN_MOVED_FROM) != 0) { pendingMove = (cookie, full, isDir); continue; }
                    if ((mask & IN_MOVED_TO) != 0)
                    {
                        if (pendingMove is { } source && source.Cookie == cookie)
                        {
                            pendingMove = null;
                            if (isDir)
                            {
                                if (WatchAllowed(full)) { MoveSubtree(source.Path, full); AddTree(full); }
                                else ForgetSubtree(source.Path);
                            }
                            RaiseRenamed(full, source.Path);
                        }
                        else
                        {
                            if (isDir) AddTree(full);
                            RaiseFs(Created, WatcherChangeTypes.Created, full);
                        }
                        continue;
                    }
                    if ((mask & IN_CREATE) != 0)
                    {
                        if (isDir) AddTree(full);
                        RaiseFs(Created, WatcherChangeTypes.Created, full);
                    }
                    else if ((mask & IN_DELETE) != 0)
                    {
                        if (isDir) ForgetSubtree(full);
                        RaiseFs(Deleted, WatcherChangeTypes.Deleted, full);
                    }
                    else if ((mask & IN_MODIFY) != 0 && !isDir)
                    {
                        RaiseFs(Changed, WatcherChangeTypes.Changed, full);
                    }
                }
            }
        }

        private void MovedAway(string path, bool isDir)
        {
            if (isDir) ForgetSubtree(path);
            RaiseFs(Deleted, WatcherChangeTypes.Deleted, path);
        }

        private void RaiseFs(FileSystemEventHandler handler, WatcherChangeTypes type, string fullPath)
        {
            if (!EnableRaisingEvents || handler == null) return;
            var parent = Path.GetDirectoryName(fullPath);
            Raise(handler, new FileSystemEventArgs(type, parent, Path.GetFileName(fullPath)));
        }

        private void RaiseRenamed(string fullPath, string oldFullPath)
        {
            if (!EnableRaisingEvents || Renamed == null) return;
            var args = new RenamedEventArgs(WatcherChangeTypes.Renamed, Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath),
                Path.GetRelativePath(Path.GetDirectoryName(fullPath), oldFullPath));
            try { Renamed(this, args); } catch { /* un gestore che fallisce non ferma il watcher */ }
        }


        private void Raise(FileSystemEventHandler handler, FileSystemEventArgs args)
        {
            try { handler?.Invoke(this, args); } catch { /* un gestore che fallisce non ferma il watcher */ }
        }

        private void Raise(ErrorEventHandler handler, ErrorEventArgs args)
        {
            try { handler?.Invoke(this, args); } catch { }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _reader?.Join(1000);
            lock (_lock)
            {
                close(_fd);
                _fd = -1;
                _pathByWd.Clear();
                _wdByPath.Clear();
            }
        }
    }
}
