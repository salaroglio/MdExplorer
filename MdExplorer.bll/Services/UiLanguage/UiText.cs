namespace MdExplorer.Features.Services
{
    /// <summary>
    /// Un testo per la persona nella lingua dell'interfaccia (17/10/2026), per il codice che non riceve
    /// <see cref="IUiLanguage"/>: gli errori della città che il servizio restituisce all'interfaccia, anche quelli sollevati
    /// nel bll e passati come <c>ex.Message</c>. All'avvio il servizio gli dà la sua lingua con <see cref="Use"/>; senza
    /// (test, strumenti) vale l'inglese, il default di <see cref="IUiLanguage"/>.
    /// </summary>
    public static class UiText
    {
        private static IUiLanguage _language;

        public static void Use(IUiLanguage language) => _language = language;

        /// <summary>«it» o «en».</summary>
        public static string Current => _language?.Current ?? "en";

        public static string Say(string it, string en) => Current == "it" ? it : en;
    }
}
