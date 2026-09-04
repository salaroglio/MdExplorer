using System;
using System.Net.Http;
using MdExplorer.Features.Services.Atlassian;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Atlassian
{
    /// <summary>
    /// Il proxy effettivo dipende dalla macchina che esegue i test, quindi qui si verifica
    /// ciò che è deterministico: il messaggio conserva sempre la causa originale, nomina
    /// il servizio e l'indirizzo, e non esplode mai — è un percorso d'errore, se fallisse
    /// nasconderebbe proprio l'informazione che deve dare.
    /// </summary>
    [TestClass]
    public class AtlassianNetworkDiagnosticsShould
    {
        private const string JiraUrl = "https://deda-bit.atlassian.net";

        [TestMethod]
        public void KeepTheOriginalFailureInTheMessage()
        {
            var inner = new HttpRequestException(
                "Impossibile stabilire la connessione. (10.245.65.135:8080)");

            var msg = AtlassianNetworkDiagnostics.DescribeUnreachable("Jira", JiraUrl, inner);

            StringAssert.Contains(msg, "Jira");
            StringAssert.Contains(msg, JiraUrl);
            StringAssert.Contains(msg, "10.245.65.135:8080");
        }

        [TestMethod]
        public void SayWhetherAProxyIsInvolvedEitherWay()
        {
            var msg = AtlassianNetworkDiagnostics.DescribeUnreachable(
                "Jira", JiraUrl, new HttpRequestException("boom"));

            // Su una macchina senza proxy si dichiara l'uscita diretta; con un proxy si
            // nomina. Un caso o l'altro, mai il silenzio che ha disorientato il cliente.
            // Marcatori non ambigui: il ramo "diretta" contiene la frase "no system proxy",
            // quindi cercare "system proxy" da solo matcherebbe entrambi.
            var mentionsProxy = msg.Contains("through the system proxy", StringComparison.Ordinal);
            var mentionsDirect = msg.Contains("The request goes out directly", StringComparison.Ordinal);
            Assert.IsTrue(mentionsProxy ^ mentionsDirect,
                "il messaggio deve dire una cosa sola fra 'via proxy' e 'in diretta': " + msg);

            // Quando l'ambiente HA un proxy, il messaggio deve nominarlo: è tutto il punto
            // della modifica. Eseguendo la suite con HTTPS_PROXY valorizzata si esercita
            // davvero il ramo del cliente, non solo quello della macchina di sviluppo.
            var configured = Environment.GetEnvironmentVariable("HTTPS_PROXY");
            if (!string.IsNullOrWhiteSpace(configured) &&
                Uri.TryCreate(configured, UriKind.Absolute, out var proxyUri))
            {
                Assert.IsTrue(mentionsProxy, "con HTTPS_PROXY impostata il messaggio deve nominare il proxy: " + msg);
                StringAssert.Contains(msg, proxyUri.Host);
            }
        }

        [TestMethod]
        public void NameTheServiceItWasGiven()
        {
            var msg = AtlassianNetworkDiagnostics.DescribeUnreachable(
                "Confluence", "https://deda-bit.atlassian.net/wiki", new HttpRequestException("boom"));

            StringAssert.Contains(msg, "Confluence");
            StringAssert.Contains(msg, "/wiki");
        }

        [TestMethod]
        public void SurviveAMalformedUrlAndANullException()
        {
            var malformed = AtlassianNetworkDiagnostics.DescribeUnreachable(
                "Jira", "not-a-url", new HttpRequestException("boom"));
            StringAssert.Contains(malformed, "not-a-url");

            var noInner = AtlassianNetworkDiagnostics.DescribeUnreachable("Jira", JiraUrl, null);
            StringAssert.Contains(noInner, JiraUrl);
            Assert.IsFalse(string.IsNullOrWhiteSpace(noInner));
        }
    }
}
