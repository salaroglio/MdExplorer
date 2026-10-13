using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MdExplorer.Services
{
    /// <summary>La lingua dell'interfaccia letta dal DB utente (impostazione «Language», scritta da AppSettings/SetLanguage).</summary>
    public sealed class UserSettingsUiLanguage : IUiLanguage
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public UserSettingsUiLanguage(IServiceScopeFactory scopeFactory) { _scopeFactory = scopeFactory; }

        public string Current
        {
            get
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
}
