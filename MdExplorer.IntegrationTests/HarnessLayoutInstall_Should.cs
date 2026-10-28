using System;
using System.IO;
using System.Linq;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// I percorsi dove MdExplorer deposita skill, agent e prompt erano stringhe <c>.github</c>
    /// sparse nel codice; ora vivono in <see cref="HarnessLayout"/>. Questa classe è la rete che
    /// tiene fermo il comportamento storico mentre l'astrazione si insinua sotto: se il refactoring
    /// spostasse anche un solo file, o cambiasse un suffisso, il confronto sull'albero prodotto lo
    /// direbbe subito.
    /// <para>Sprint: docs-internal/Sprints/2026-08-31-Opencode-Harness-Support.md, fase F0.</para>
    /// </summary>
    [TestClass]
    public class HarnessLayoutInstall_Should
    {
        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-harness-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
            catch (IOException) { /* la pulizia non è il soggetto del test */ }
        }

        /// <summary>File prodotti, relativi alla radice del progetto, con separatore '/'.</summary>
        private string[] ProducedFiles()
            => Directory.GetFiles(_root, "*", SearchOption.AllDirectories)
                .Select(f => f.Substring(_root.Length + 1).Replace(Path.DirectorySeparatorChar, '/'))
                .OrderBy(f => f, StringComparer.Ordinal)
                .ToArray();

        [TestMethod]
        public void Install_the_copilot_catalogs_exactly_where_they_have_always_been()
        {
            MdeSkillUpdater.EnsureAllSkillsInstalled(_root, fusekiEnabled: false, agentCityEnabled: true);

            CollectionAssert.AreEqual(
                new[]
                {
                    ".github/agents/mde-skillcreator.agent.md",
                    ".github/prompts/mde-code-doc.prompt.md",
                    ".github/prompts/mde-codegen-graph.prompt.md",
                    ".github/prompts/mde-mark-folder-synthesis.prompt.md",
                    ".github/prompts/mde-mark-summarize.prompt.md",
                    ".github/skills/mde-agent/SKILL.md",
                    ".github/skills/mde-code/SKILL.md",
                    ".github/skills/mde-doc/SKILL.md",
                    ".github/skills/mde-e2e-signals/SKILL.md",
                    ".github/skills/mde-e2e/SKILL.md",
                    ".github/skills/mde-e2e/esecuzione.md",
                    ".github/skills/mde-e2e/mappa.md",
                    ".github/skills/mde-e2e/preparare.md",
                    ".github/skills/mde-e2e/rigioco-fallito.md",
                    ".github/skills/mde-features/SKILL.md",
                    ".github/skills/mde-ownership/SKILL.md",
                    ".github/skills/mde-plantuml/SKILL.md",
                    ".github/skills/mde-prompt-for-agents/SKILL.md",
                    ".github/skills/mde-readme/SKILL.md",
                    ".github/skills/mde-slide/SKILL.md",
                    ".github/skills/mde-template/SKILL.md",
                    ".github/skills/mde-workflow/SKILL.md",
                },
                ProducedFiles(),
                "il layout Copilot deve restare bit per bit quello di sempre");
        }

        [TestMethod]
        public void Add_the_fuseki_skills_only_when_fuseki_is_configured()
        {
            MdeSkillUpdater.EnsureAllSkillsInstalled(_root, fusekiEnabled: true, agentCityEnabled: true);

            var produced = ProducedFiles();
            foreach (var name in new[] { "mde-tbox", "mde-abox", "mde-shacl" })
            {
                CollectionAssert.Contains(produced, $".github/skills/{name}/SKILL.md",
                    $"la skill Fuseki '{name}' deve comparire quando Fuseki è configurato");
            }
            Assert.AreEqual(25, produced.Length, "16 skill (mde-template dal 18/10, mde-ownership dal 20/10 con la città) + 4 file delle fasi di mde-e2e (28/10) + 1 agent + 4 prompt (mde-code-doc dal 10/10)");
        }

        [TestMethod]
        public void Write_the_mde_version_marker_in_every_file_it_installs()
        {
            MdeSkillUpdater.EnsureAllSkillsInstalled(_root, fusekiEnabled: true, agentCityEnabled: true);

            // i file accanto a uno SKILL.md seguono il suo marker, non ne hanno uno loro
            foreach (var relative in ProducedFiles().Where(f => !f.Contains("/skills/") || f.EndsWith("/SKILL.md")))
            {
                var marker = MdeSkillUpdater.ExtractMdeMarker(
                    File.ReadAllText(Path.Combine(_root, relative.Replace('/', Path.DirectorySeparatorChar))));
                Assert.AreEqual("mdexplorer", marker.Origin, $"{relative} deve portare il marker mde:");
                Assert.IsTrue(marker.Version > 0, $"{relative} deve dichiarare una versione");
            }
        }

        [TestMethod]
        public void Leave_alone_a_file_the_user_has_taken_ownership_of()
        {
            var skill = Path.Combine(_root, ".github", "skills", "mde-doc", "SKILL.md");
            Directory.CreateDirectory(Path.GetDirectoryName(skill));
            // Nessun blocco mde: → il file è dell'utente, MdExplorer non lo tocca.
            var mine = "---\nname: mde-doc\ndescription: la mia versione\n---\n\nRegole mie.\n";
            File.WriteAllText(skill, mine);

            MdeSkillUpdater.EnsureAllSkillsInstalled(_root, fusekiEnabled: false, agentCityEnabled: true);

            Assert.AreEqual(mine, File.ReadAllText(skill),
                "un file senza marker mde: appartiene all'utente e non va sovrascritto");
        }

        [TestMethod]
        public void Install_the_opencode_catalogs_where_opencode_reads_them()
        {
            MdeSkillUpdater.EnsureCatalogsInstalled(_root, HarnessLayout.OpenCode, fusekiEnabled: false, agentCityEnabled: true);

            CollectionAssert.AreEqual(
                new[]
                {
                    ".opencode/agents/mde-skillcreator.md",
                    ".opencode/commands/mde-code-doc.md",
                    ".opencode/commands/mde-codegen-graph.md",
                    ".opencode/commands/mde-mark-folder-synthesis.md",
                    ".opencode/commands/mde-mark-summarize.md",
                    ".opencode/skills/mde-agent/SKILL.md",
                    ".opencode/skills/mde-code/SKILL.md",
                    ".opencode/skills/mde-doc/SKILL.md",
                    ".opencode/skills/mde-e2e-signals/SKILL.md",
                    ".opencode/skills/mde-e2e/SKILL.md",
                    ".opencode/skills/mde-e2e/esecuzione.md",
                    ".opencode/skills/mde-e2e/mappa.md",
                    ".opencode/skills/mde-e2e/preparare.md",
                    ".opencode/skills/mde-e2e/rigioco-fallito.md",
                    ".opencode/skills/mde-features/SKILL.md",
                    ".opencode/skills/mde-ownership/SKILL.md",
                    ".opencode/skills/mde-plantuml/SKILL.md",
                    ".opencode/skills/mde-prompt-for-agents/SKILL.md",
                    ".opencode/skills/mde-readme/SKILL.md",
                    ".opencode/skills/mde-slide/SKILL.md",
                    ".opencode/skills/mde-template/SKILL.md",
                    ".opencode/skills/mde-workflow/SKILL.md",
                },
                ProducedFiles(),
                "agent e comandi sono file piatti <nome>.md, le skill restano in cartella");
        }

        [TestMethod]
        public void Give_the_opencode_agent_the_frontmatter_opencode_understands()
        {
            MdeSkillUpdater.EnsureCatalogsInstalled(_root, HarnessLayout.OpenCode, fusekiEnabled: false, agentCityEnabled: true);

            var agent = File.ReadAllText(HarnessLayout.OpenCode.AgentFullPath(_root, "mde-skillcreator"));

            StringAssert.Contains(agent, "mode: subagent");
            StringAssert.Contains(agent, "permission:");
            Assert.IsFalse(agent.Contains("tools: [read"),
                "in opencode la chiave tools: e' deprecata: al suo posto vanno mode: e permission:");
            Assert.AreEqual("mdexplorer", MdeSkillUpdater.ExtractMdeMarker(agent).Origin,
                "il marker mde: sopravvive anche in opencode, che ignora le chiavi che non conosce");
        }

        [TestMethod]
        public void Give_both_agent_variants_the_very_same_body()
        {
            // Il corpo dell'agent ha una sola casa: se qualcuno lo duplicasse per layout, questo
            // test e' il posto dove la divergenza si vede prima di arrivare all'utente.
            var copilotRoot = Path.Combine(_root, "cp");
            var opencodeRoot = Path.Combine(_root, "oc");
            Directory.CreateDirectory(copilotRoot);
            Directory.CreateDirectory(opencodeRoot);

            MdeSkillUpdater.EnsureCatalogsInstalled(copilotRoot, HarnessLayout.Copilot, fusekiEnabled: false, agentCityEnabled: true);
            MdeSkillUpdater.EnsureCatalogsInstalled(opencodeRoot, HarnessLayout.OpenCode, fusekiEnabled: false, agentCityEnabled: true);

            var copilotAgent = File.ReadAllText(HarnessLayout.Copilot.AgentFullPath(copilotRoot, "mde-skillcreator"));
            var opencodeAgent = File.ReadAllText(HarnessLayout.OpenCode.AgentFullPath(opencodeRoot, "mde-skillcreator"));

            Assert.AreEqual(BodyAfterFrontmatter(copilotAgent), BodyAfterFrontmatter(opencodeAgent),
                "i due agent devono differire SOLO nel frontmatter");
            Assert.AreEqual(
                MdeSkillUpdater.ExtractMdeMarker(copilotAgent).Version,
                MdeSkillUpdater.ExtractMdeMarker(opencodeAgent).Version,
                "la versione sta in due frontmatter distinti: devono restare allineate");
        }

        /// <summary>Testo dopo la seconda riga '---', cioe' il corpo senza frontmatter.</summary>
        private static string BodyAfterFrontmatter(string content)
        {
            var first = content.IndexOf("---", StringComparison.Ordinal);
            var second = content.IndexOf("---", first + 3, StringComparison.Ordinal);
            return content.Substring(second + 3);
        }

        [TestMethod]
        public void Describe_the_opencode_layout_with_the_paths_opencode_actually_reads()
        {
            var oc = HarnessLayout.OpenCode;

            // Percorsi verificati su opencode.ai/docs il 2026-08-31: skill in cartella con
            // SKILL.md, agent e comandi come file piatti <nome>.md, istruzioni in AGENTS.md.
            Assert.AreEqual(".opencode/skills/mde-doc/SKILL.md", oc.SkillRelativePath("mde-doc"));
            Assert.AreEqual(".opencode/agents/mde-skillcreator.md", oc.AgentRelativePath("mde-skillcreator"));
            Assert.AreEqual(".opencode/commands/mde-mark-summarize.md", oc.PromptRelativePath("mde-mark-summarize"));
            Assert.AreEqual("AGENTS.md", oc.InstructionsFile);
        }

        [TestMethod]
        public void Describe_the_copilot_layout_with_the_paths_the_backend_already_reads()
        {
            var cp = HarnessLayout.Copilot;

            // Questi tre percorsi sono citati oggi, come stringhe, da AgentPromptsController e
            // MarkFolderJobService: la fase F4 li farà passare di qui, e devono coincidere.
            Assert.AreEqual(".github/skills/mde-prompt-for-agents/SKILL.md", cp.SkillRelativePath("mde-prompt-for-agents"));
            Assert.AreEqual(".github/prompts/mde-mark-summarize.prompt.md", cp.PromptRelativePath("mde-mark-summarize"));
            Assert.AreEqual(".github/copilot-instructions.md", cp.InstructionsFile);
        }

        [TestMethod]
        public void Refuse_to_hand_out_a_layout_for_a_project_with_no_harness()
        {
            // "Nessun harness" si gestisce non installando niente, non chiedendo un layout.
            Assert.ThrowsException<InvalidOperationException>(() => HarnessLayout.For(HarnessTarget.None));
        }

        [TestMethod]
        public void Reject_an_unknown_harness_id_instead_of_defaulting()
        {
            Assert.IsFalse(HarnessLayout.TryParseId("cursor", out _),
                "un valore sconosciuto va segnalato, non ricondotto a un default silenzioso");
            Assert.IsFalse(HarnessLayout.TryParseId(null, out _));

            Assert.IsTrue(HarnessLayout.TryParseId("copilot", out var copilot));
            Assert.AreEqual(HarnessTarget.Copilot, copilot);
            Assert.IsTrue(HarnessLayout.TryParseId(" OpenCode ", out var oc));
            Assert.AreEqual(HarnessTarget.OpenCode, oc);
            Assert.IsTrue(HarnessLayout.TryParseId("none", out var none));
            Assert.AreEqual(HarnessTarget.None, none);
        }

        [TestMethod]
        public void Install_the_skills_of_the_agents_only_with_the_agent_city()
        {
            MdeSkillUpdater.EnsureAllSkillsInstalled(_root, fusekiEnabled: false);
            var withoutCity = ProducedFiles();
            foreach (var name in new[] { "mde-agent", "mde-workflow", "mde-ownership", "mde-prompt-for-agents" })
                CollectionAssert.DoesNotContain(withoutCity, $".github/skills/{name}/SKILL.md", $"'{name}' serve solo agli agenti: senza la città non c'è");
            CollectionAssert.Contains(withoutCity, ".github/skills/mde-doc/SKILL.md");

            // La città si legge da .development.yml, come la scrive MDE quando la accende.
            File.WriteAllText(Path.Combine(_root, ".development.yml"), "agentCity:\n  enabled: true\n");
            MdeSkillUpdater.EnsureAllSkillsInstalled(_root, fusekiEnabled: false);
            var withCity = ProducedFiles();
            foreach (var name in new[] { "mde-agent", "mde-workflow", "mde-ownership", "mde-prompt-for-agents" })
                CollectionAssert.Contains(withCity, $".github/skills/{name}/SKILL.md", $"'{name}' arriva con la città");
        }

        [TestMethod]
        public void Keep_the_files_of_a_skill_with_their_SKILL_md()
        {
            var folder = Path.Combine(_root, ".github", "skills", "mde-e2e");
            var skill = Path.Combine(folder, "SKILL.md");
            var phase = Path.Combine(folder, "esecuzione.md");
            MdeSkillUpdater.EnsureAllSkillsInstalled(_root, fusekiEnabled: false, agentCityEnabled: true);
            var shipped = File.ReadAllText(phase);

            File.Delete(phase);
            MdeSkillUpdater.EnsureAllSkillsInstalled(_root, fusekiEnabled: false, agentCityEnabled: true);
            Assert.AreEqual(shipped, File.ReadAllText(phase), "un file mancante torna anche con lo SKILL.md aggiornato");

            File.WriteAllText(phase, "vecchio");
            File.WriteAllText(skill, File.ReadAllText(skill).Replace("  version: ", "  version: 0"));
            MdeSkillUpdater.EnsureAllSkillsInstalled(_root, fusekiEnabled: false, agentCityEnabled: true);
            Assert.AreEqual("vecchio", File.ReadAllText(phase), "SKILL.md più nuovo di quello distribuito: niente si tocca");

            File.WriteAllText(skill, "---\nname: mde-e2e\nmde:\n  origin: mdexplorer\n  version: 1\n---\n");
            MdeSkillUpdater.EnsureAllSkillsInstalled(_root, fusekiEnabled: false, agentCityEnabled: true);
            Assert.AreEqual(shipped, File.ReadAllText(phase), "lo SKILL.md aggiornato riscrive i suoi file");

            File.WriteAllText(skill, "---\nname: mde-e2e\n---\nmia\n");
            File.WriteAllText(phase, "mio");
            MdeSkillUpdater.EnsureAllSkillsInstalled(_root, fusekiEnabled: false, agentCityEnabled: true);
            Assert.AreEqual("mio", File.ReadAllText(phase), "la skill dell'utente: anche i suoi file sono dell'utente");
        }
    }
}
