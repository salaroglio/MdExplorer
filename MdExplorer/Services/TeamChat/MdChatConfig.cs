using Microsoft.Extensions.Configuration;

namespace MdExplorer.Services.TeamChat
{
    /// <summary>
    /// Lettura normalizzata della config del relay (<c>MdChat:*</c>), condivisa da chat e
    /// federazione. Il punto chiave è l'<b>API key</b>: <c>appsettings.json</c> versionato porta
    /// il <b>placeholder</b> <see cref="PlaceholderApiKey"/> (uguale a <c>appsettings.example.json</c>),
    /// quindi un checkout pulito NON ha una chiave vera. Trattiamo il placeholder come "assente"
    /// così i client restano <b>dormienti</b> invece di martellare il relay con una chiave finta
    /// (auth fallita in loop). La chiave vera si fornisce SENZA committarla:
    /// <list type="bullet">
    /// <item>variabile d'ambiente <c>MdChat__ApiKey</c> (doppio underscore), oppure</item>
    /// <item><c>appsettings.Development.json</c> (già in <c>.gitignore</c>), oppure</item>
    /// <item>user-secrets in sviluppo.</item>
    /// </list>
    /// La precedenza della config ASP.NET fa vincere queste sul placeholder versionato.
    /// </summary>
    public static class MdChatConfig
    {
        /// <summary>Il valore che <c>appsettings.example.json</c> spedisce: da trattare come non configurato.</summary>
        public const string PlaceholderApiKey = "YOUR_API_KEY_HERE";

        /// <summary>API key effettiva del relay, o <c>null</c> se assente/placeholder (⇒ dormiente).</summary>
        public static string ResolveApiKey(IConfiguration configuration)
        {
            var key = configuration?["MdChat:ApiKey"];
            if (string.IsNullOrWhiteSpace(key) || key == PlaceholderApiKey)
                return null;
            return key;
        }

        /// <summary>True se la chiave configurata è il placeholder versionato (per un warning mirato).</summary>
        public static bool IsPlaceholderApiKey(IConfiguration configuration)
            => configuration?["MdChat:ApiKey"] == PlaceholderApiKey;
    }
}
