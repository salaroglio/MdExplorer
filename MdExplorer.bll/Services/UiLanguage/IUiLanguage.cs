namespace MdExplorer.Features.Services
{
    /// <summary>
    /// La lingua dell'interfaccia scelta dall'utente (it, en), per i testi che scrive il backend: il report del grafo del
    /// codice, la pagina del grafo. La fonte è il DB utente (impostazione «Language», la stessa di Angular).
    /// </summary>
    public interface IUiLanguage
    {
        /// <summary>«it» o «en». Se l'utente non ha mai scelto, «en», come il default di Angular.</summary>
        string Current { get; }
    }
}

namespace MdExplorer.Features.Services
{
    /// <summary>Una lingua decisa da chi costruisce: per i test e per chi non ha un utente.</summary>
    public sealed class FixedUiLanguage : IUiLanguage
    {
        public FixedUiLanguage(string language) { Current = language == "it" ? "it" : "en"; }
        public string Current { get; }
    }
}
