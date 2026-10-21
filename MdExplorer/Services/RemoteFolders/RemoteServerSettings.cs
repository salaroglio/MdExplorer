using System;
using System.Collections.Concurrent;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MdExplorer.Services.RemoteFolders
{
    /// <summary>
    /// Le scelte dell'utente per un server (sprint 2026-10-21-Cartella-Remota-Via-SSH, F3), per destinatario ssh: valgono
    /// per tutte le cartelle di quel server, perché <c>noexec</c> e le regole sui programmi nuovi sono del server.
    /// <list type="bullet">
    /// <item>P13: il consenso a caricare il serverino. Senza, MDE non carica niente e lo chiede.</item>
    /// <item>P11: la cartella del serverino al posto di <c>~/.cache/mdexplorer</c> (home <c>noexec</c>).</item>
    /// </list>
    /// </summary>
    public interface IRemoteServerSettings
    {
        bool UploadAllowed(string destination);
        void AllowUpload(string destination);

        /// <summary>La cartella base del serverino (assoluta, o relativa alla home); null = <see cref="DefaultAgentFolder"/>.</summary>
        string AgentFolder(string destination);
        void SetAgentFolder(string destination, string folder);
    }

    public static class RemoteAgentFolders
    {
        public const string DefaultAgentFolder = ".cache/mdexplorer";

        /// <summary>
        /// Una cartella del serverino scritta dall'utente: assoluta (<c>/opt/dev/mde</c>) o relativa alla home
        /// (<c>bin/mde</c>); niente <c>..</c>, niente <c>~</c>, niente a capo. Null o vuota = il default.
        /// </summary>
        public static string Normalize(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder)) return null;
            var f = folder.Trim().TrimEnd('/');
            if (f.Length == 0 || f.StartsWith("~", StringComparison.Ordinal) || f.IndexOfAny(new[] { '\n', '\r', '\0' }) >= 0
                || f.Split('/').Any(p => p == ".."))
                throw new InvalidOperationException(UiText.Say(
                    $"«{folder}» non è una cartella valida per il serverino: scrivi un percorso assoluto (/opt/…) o relativo alla home, senza ~ e senza «..».",
                    $"«{folder}» is not a valid folder for the agent: write an absolute path (/opt/…) or one relative to the home, without ~ and without «..»."));
            return f;
        }
    }

    /// <summary>
    /// Righe <see cref="Setting"/> nello UserDB, una per server e scelta. ⚠️ <c>Setting.Name</c> è UNIQUE (migrazione
    /// della tabella): il destinatario sta nel nome (<c>RemoteAgentFolder:build-server</c>), non solo in Description —
    /// trovato il 21/10 con il secondo server, la cui riga non entrava.
    /// </summary>
    public sealed class UserDbRemoteServerSettings : IRemoteServerSettings
    {
        private const string UploadSetting = "RemoteAgentUploadAllowed";
        private const string FolderSetting = "RemoteAgentFolder";
        private readonly IServiceScopeFactory _scopeFactory;

        public UserDbRemoteServerSettings(IServiceScopeFactory scopeFactory) { _scopeFactory = scopeFactory; }

        public bool UploadAllowed(string destination) => WithDb(db => Find(db, UploadSetting, destination) != null);

        public void AllowUpload(string destination) => WithDb(db =>
        {
            if (Find(db, UploadSetting, destination) == null)
                db.GetDal<Setting>().Save(new Setting { Name = NameOf(UploadSetting, destination), Description = destination, ValueDateTime = DateTime.UtcNow });
            return true;
        });

        public string AgentFolder(string destination) => WithDb(db => Find(db, FolderSetting, destination)?.ValueString);

        public void SetAgentFolder(string destination, string folder) => WithDb(db =>
        {
            var normalized = RemoteAgentFolders.Normalize(folder);
            var row = Find(db, FolderSetting, destination);
            if (normalized == null) { if (row != null) db.GetDal<Setting>().Delete(row); return true; }
            row ??= new Setting { Name = NameOf(FolderSetting, destination), Description = destination };
            row.ValueString = normalized;
            db.GetDal<Setting>().Save(row);
            return true;
        });

        private static string NameOf(string setting, string destination) => setting + ":" + destination;

        private static Setting Find(IUserSettingsDB db, string setting, string destination)
        {
            var name = NameOf(setting, destination);
            return db.GetDal<Setting>().GetList().Where(s => s.Name == name).ToList().FirstOrDefault();
        }

        private T WithDb<T>(Func<IUserSettingsDB, T> work)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            try
            {
                var result = work(db);
                db.Commit();
                return result;
            }
            catch
            {
                db.Rollback();
                throw;
            }
        }
    }

    /// <summary>In memoria: per i test.</summary>
    public sealed class MemoryRemoteServerSettings : IRemoteServerSettings
    {
        private readonly ConcurrentDictionary<string, bool> _allowed = new();
        private readonly ConcurrentDictionary<string, string> _folders = new();

        public MemoryRemoteServerSettings(bool allowAll = false) { AllowAll = allowAll; }
        public bool AllowAll { get; }

        public bool UploadAllowed(string destination) => AllowAll || _allowed.ContainsKey(destination);
        public void AllowUpload(string destination) => _allowed[destination] = true;
        public string AgentFolder(string destination) => _folders.TryGetValue(destination, out var f) ? f : null;
        public void SetAgentFolder(string destination, string folder)
        {
            var normalized = RemoteAgentFolders.Normalize(folder);
            if (normalized == null) _folders.TryRemove(destination, out _); else _folders[destination] = normalized;
        }
    }
}
