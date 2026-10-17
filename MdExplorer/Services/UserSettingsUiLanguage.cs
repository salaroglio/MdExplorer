using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MdExplorer.Services
{
    /// <summary>
    /// La lingua dell'interfaccia letta dal DB utente (impostazione «Language», scritta da AppSettings/SetLanguage).
    /// Letta una volta e poi tenuta: gli errori la chiedono anche dentro una transazione del DB utente, e una seconda
    /// sessione aperta lì dentro rischia «database is locked». SetLanguage la aggiorna con <see cref="Remember"/>.
    /// </summary>
    public sealed class UserSettingsUiLanguage : IUiLanguage
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private volatile string _current;
        public UserSettingsUiLanguage(IServiceScopeFactory scopeFactory) { _scopeFactory = scopeFactory; }

        public string Current => _current ??= Read();

        /// <summary>La lingua appena salvata da SetLanguage.</summary>
        public void Remember(string language) => _current = language == "it" ? "it" : "en";

        private string Read()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            try
            {
                var value = db.GetDal<Setting>().GetList().FirstOrDefault(x => x.Name == "Language")?.ValueString;
                db.Commit();
                return value == "it" ? "it" : "en";
            }
            catch
            {
                db.Rollback();
                throw;
            }
        }
    }
}
