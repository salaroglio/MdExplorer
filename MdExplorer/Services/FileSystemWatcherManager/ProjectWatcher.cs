using System;
using System.IO;

namespace MdExplorer.Services.FileSystemWatcherManager
{
    /// <summary>
    /// Il watcher di un progetto (sprint 2026-10-21-Watcher-Selettivo-E-Gitignore, W3): gli stessi eventi di
    /// <see cref="FileSystemWatcher"/>, ma solo per le cartelle che <c>shouldWatch</c> accetta. Su Linux
    /// <see cref="InotifyProjectWatcher"/> (una sola istanza inotify, un watch per cartella utile); altrove
    /// <see cref="FilteredProjectWatcher"/> (FileSystemWatcher ricorsivo: lì sorvegliare tutto l'albero non costa per cartella).
    /// </summary>
    public interface IProjectWatcher : IDisposable
    {
        string RootPath { get; }
        bool EnableRaisingEvents { get; set; }
        /// <summary>Quante cartelle sono sorvegliate (diagnostica e test).</summary>
        int WatchedDirectories { get; }

        event FileSystemEventHandler Changed;
        event FileSystemEventHandler Created;
        event FileSystemEventHandler Deleted;
        event RenamedEventHandler Renamed;
        event ErrorEventHandler Error;

        /// <summary>
        /// Comincia a sorvegliare. Separato dal costruttore: chi lo crea si iscrive PRIMA agli eventi, così anche un errore
        /// della partenza (limite dei watch pieno) arriva a qualcuno.
        /// </summary>
        void Start();

        /// <summary>Le regole sono cambiate (.gitignore, .mdFoldersIgnore): si rifà l'elenco delle cartelle sorvegliate.</summary>
        void Resync();
    }

    public static class ProjectWatchers
    {
        /// <param name="shouldWatch">Vero se la cartella (percorso assoluto) va sorvegliata; la radice lo è sempre.</param>
        public static IProjectWatcher Create(string rootPath, Func<string, bool> shouldWatch)
            => OperatingSystem.IsLinux()
                ? new InotifyProjectWatcher(rootPath, shouldWatch)
                : new FilteredProjectWatcher(rootPath, shouldWatch);
    }

    /// <summary>
    /// Windows e macOS: un FileSystemWatcher ricorsivo (una sola sorveglianza per l'albero intero), e gli eventi che cadono
    /// in una cartella da non sorvegliare si scartano qui, prima dei gestori.
    /// </summary>
    public sealed class FilteredProjectWatcher : IProjectWatcher
    {
        private readonly System.IO.FileSystemWatcher _watcher;
        private readonly Func<string, bool> _shouldWatch;

        public string RootPath { get; }
        public int WatchedDirectories => -1;

        public event FileSystemEventHandler Changed;
        public event FileSystemEventHandler Created;
        public event FileSystemEventHandler Deleted;
        public event RenamedEventHandler Renamed;
        public event ErrorEventHandler Error;

        public FilteredProjectWatcher(string rootPath, Func<string, bool> shouldWatch)
        {
            RootPath = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            _shouldWatch = shouldWatch;
            _watcher = new System.IO.FileSystemWatcher(RootPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
                IncludeSubdirectories = true,
                InternalBufferSize = 64 * 1024,
            };
            _watcher.Changed += (s, e) => { if (Wanted(e.FullPath)) Changed?.Invoke(this, e); };
            _watcher.Created += (s, e) => { if (Wanted(e.FullPath)) Created?.Invoke(this, e); };
            _watcher.Deleted += (s, e) => { if (Wanted(e.FullPath)) Deleted?.Invoke(this, e); };
            _watcher.Renamed += (s, e) => { if (Wanted(e.FullPath) || Wanted(e.OldFullPath)) Renamed?.Invoke(this, e); };
            _watcher.Error += (s, e) => Error?.Invoke(this, e);
        }

        public bool EnableRaisingEvents { get => _watcher.EnableRaisingEvents; set => _watcher.EnableRaisingEvents = value; }

        public void Start() => _watcher.EnableRaisingEvents = true;

        public void Resync() { /* le regole si leggono a ogni evento */ }

        /// <summary>Ogni cartella fra la radice e il file deve essere da sorvegliare.</summary>
        private bool Wanted(string fullPath)
        {
            for (var dir = Path.GetDirectoryName(fullPath); dir != null && dir.Length > RootPath.Length; dir = Path.GetDirectoryName(dir))
                if (!_shouldWatch(dir)) return false;
            return true;
        }

        public void Dispose() => _watcher.Dispose();
    }
}
