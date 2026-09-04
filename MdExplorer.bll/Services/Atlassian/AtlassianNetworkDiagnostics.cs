using System;
using System.Net;
using System.Net.Http;

namespace MdExplorer.Features.Services.Atlassian
{
    /// <summary>
    /// Spiega perché una richiesta verso Atlassian non è nemmeno partita.
    ///
    /// Nasce da una segnalazione reale: il messaggio riportava un IP:porta interno e
    /// basta, e il cliente ha cercato per ore un "gateway MdExplorer" che non esiste.
    /// Quell'indirizzo è il <b>proxy di sistema</b> della sua macchina: .NET lo eredita
    /// da <see cref="HttpClient.DefaultProxy"/> (impostazioni Windows o variabili
    /// HTTP(S)_PROXY), noi non lo configuriamo da nessuna parte. Dirlo dentro l'errore
    /// trasforma una caccia al fantasma in una verifica di un minuto.
    /// </summary>
    public static class AtlassianNetworkDiagnostics
    {
        /// <summary>
        /// Compone il messaggio per un fallimento di connessione verso
        /// <paramref name="baseUrl"/>, nominando il proxy che la richiesta attraversa
        /// (o dichiarando che l'uscita è diretta).
        /// </summary>
        /// <param name="serviceName">"Jira" o "Confluence": compare nel messaggio.</param>
        public static string DescribeUnreachable(string serviceName, string baseUrl, Exception ex)
        {
            var reason = ex?.Message ?? "no further detail";
            var head = $"Could not reach {serviceName} at {baseUrl}: {reason}";

            var proxy = TryResolveProxy(baseUrl);
            if (proxy != null)
            {
                return head +
                    $" The request goes through the system proxy {proxy}, which MdExplorer does " +
                    "not configure: it comes from this machine's Windows/environment proxy settings. " +
                    "Check that the proxy is reachable from here, or add the Atlassian host to the " +
                    "proxy exceptions so the connection bypasses it.";
            }

            return head +
                " The request goes out directly (no system proxy applies to this address), " +
                "so check DNS resolution, the VPN and the firewall.";
        }

        /// <summary>
        /// Il proxy che <see cref="HttpClient.DefaultProxy"/> userebbe per questo indirizzo,
        /// o null per uscita diretta. Non deve mai far fallire la diagnosi: qualunque
        /// problema qui (URL malformato, script PAC che esplode) vale come "non lo so", e il
        /// messaggio resta comunque utile.
        /// </summary>
        private static string TryResolveProxy(string baseUrl)
        {
            try
            {
                if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)) return null;

                IWebProxy proxy = HttpClient.DefaultProxy;
                if (proxy == null || proxy.IsBypassed(uri)) return null;

                // GetProxy restituisce l'URI di destinazione stesso quando non c'è proxy:
                // in quel caso l'uscita è diretta, non "via se stesso".
                var via = proxy.GetProxy(uri);
                if (via == null || via == uri) return null;

                return via.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
