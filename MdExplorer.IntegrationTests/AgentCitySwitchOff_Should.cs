using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// 09/11/2026: in «test del ddd» (nato dal template documenti, città accesa) togliere la spunta della città nelle impostazioni
    /// non la spegneva: la toolbar restava con i comandi della città. Il file è quello del progetto, con il segreto cambiato.
    /// </summary>
    [TestClass]
    public class AgentCitySwitchOff_Should
    {
        private const string Yml = @"# mde-schema: 3
folders: []
compatibility:
  mode: mdexplorer
  gitHubOptions:
    embedImages: false
    stripInteractive: true
    preserveEmoji: true
yamlAutoGeneration:
  enabled: true
  excludePaths:
  - .github
externalBrowser:
  enabled: false
  openAllExternal: false
  urlPatterns: []
project:
  description:
  participants: []
  icon:
agentCity:
  enabled: true
  ownershipDoc: responsabilita.md
  workflowDocs:
  - .mde-template/documenti/workflows/fonte.md
  - .mde-template/documenti/workflows/controllo.md
  roomSecret: segreto-di-prova
  relayUrl:
  maintenance:
  useAgentWorktrees:
harness:
  target: copilot
  model:
templates:
- id: documenti
  url: https://github.com/salaroglio/mdexplorer-template-documenti.git
  kind: foundation
  language: it
  example: true
  theory: true
  version: 2.0.0
  detached:
";

        [TestMethod]
        public async Task Switch_the_city_off_from_the_settings()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("citta-spenta");
            File.WriteAllText(Path.Combine(path, ".development.yml"), Yml);

            // Quello che manda il client togliendo la spunta: enabled, ownershipDoc e workflowDocs.
            var (status, body) = await ctx.PostJson("/api/MdProjects/SetAgentCity?path=" + System.Uri.EscapeDataString(path),
                JsonSerializer.Serialize(new
                {
                    enabled = false, ownershipDoc = "responsabilita.md",
                    workflowDocs = new[] { ".mde-template/documenti/workflows/fonte.md", ".mde-template/documenti/workflows/controllo.md" },
                }));

            Assert.AreEqual(HttpStatusCode.OK, status, body);
            StringAssert.Contains(body, "\"enabled\":false", body);
            StringAssert.Contains(File.ReadAllText(Path.Combine(path, ".development.yml")), "agentCity:\n  enabled: false");
        }
    }
}
