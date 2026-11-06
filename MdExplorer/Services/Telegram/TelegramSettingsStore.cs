using System;
using System.Collections.Generic;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using Microsoft.Extensions.DependencyInjection;

namespace MdExplorer.Services.Telegram
{
    /// <summary>
    /// Il collegamento a Telegram di questa installazione (docs-internal/Sprints/2026-10-31-Telecomando-Telegram.md, F2):
    /// righe <c>Telegram.*</c> della tabella Setting del DB utente. Il token del bot personale sta solo qui, cifrato con
    /// <c>IPasswordProtector</c> (DPAPI su Windows, Data Protection altrove).
    /// </summary>
    public sealed class TelegramSettingsStore
    {
        public const string TokenProtected = "Telegram.BotTokenProtected";
        public const string BotId = "Telegram.BotId";
        public const string BotUsername = "Telegram.BotUsername";
        public const string OwnerId = "Telegram.OwnerId";
        public const string OwnerName = "Telegram.OwnerName";
        public const string Enabled = "Telegram.Enabled";
        /// <summary>«listening» o «standby» (D3): chi ha perso il bot per un 409 resta in attesa anche dopo un riavvio.</summary>
        public const string Role = "Telegram.Role";
        public const string InstallationName = "Telegram.InstallationName";

        private const string Prefix = "Telegram.";
        private readonly IServiceScopeFactory _scopeFactory;

        public TelegramSettingsStore(IServiceScopeFactory scopeFactory) { _scopeFactory = scopeFactory; }

        public IReadOnlyDictionary<string, string> ReadAll()
            => InTransaction(db => db.GetDal<Setting>().GetList()
                .Where(x => x.Name.StartsWith(Prefix))
                .ToDictionary(x => x.Name, x => x.ValueString));

        /// <summary>Scrive (o con <c>null</c> cancella) le righe date, tutte nella stessa transazione.</summary>
        public void Write(IReadOnlyDictionary<string, string> values)
            => InTransaction<object>(db =>
            {
                var dal = db.GetDal<Setting>();
                var existing = dal.GetList().Where(x => x.Name.StartsWith(Prefix)).ToList();
                foreach (var (name, value) in values)
                {
                    var row = existing.FirstOrDefault(x => x.Name == name);
                    if (row != null)
                    {
                        // SQL per nome, come AppSettingsController: un Id scritto da una migrazione ha un formato di GUID diverso.
                        db.Evict(row);
                        var sql = value == null
                            ? "DELETE FROM Setting WHERE Name = :name"
                            : "UPDATE Setting SET ValueString = :val WHERE Name = :name";
                        var q = db.CreateSQLQuery(sql).SetParameter("name", name);
                        if (value != null) q.SetParameter("val", value);
                        q.ExecuteUpdate();
                    }
                    else if (value != null)
                    {
                        dal.Save(new Setting { Name = name, Description = "Telecomando Telegram", ValueString = value });
                    }
                }
                return null;
            });

        public void Write(string name, string value) => Write(new Dictionary<string, string> { [name] = value });

        private T InTransaction<T>(Func<IUserSettingsDB, T> work)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
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
}
