using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Agents.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Il compendio delle dinamiche tra agenti (standard v2, il vocabolario dello schedulatore): la lettura è a tolleranza
    /// zero e raccoglie tutti i problemi con il loro percorso; la verifica di progetto dice ciò che non esiste o che non
    /// arriverebbe; la regola dei passi trova chi avvia cosa; i rifacimenti hanno un limite solo se l'autore lo scrive.
    /// Il caso di riferimento è la gara del demo.
    /// </summary>
    [TestClass]
    public class Workflow_Should
    {
        private const string Gara = @"{
  ""mde_workflow"": 2,
  ""title"": ""Gara: dal bando alla sintesi"",
  ""description"": ""Dal bando trovato sul portale alla sintesi che l'account manager usa per decidere se partecipare."",
  ""variables"": {
    ""codice"": ""il codice del bando su cui si fa il giro, scelto con «Avvia il giro»""
  },
  ""steps"": [
    {
      ""id"": ""ricerca"",
      ""agent"": ""account-manager"",
      ""title"": ""Cerca i bandi"",
      ""trigger"": {
        ""launch"": true
      },
      ""start"": ""manual"",
      ""produces"": [
        ""citta-degli-agenti/gara/ricerche/ricerca-*.md""
      ]
    },
    {
      ""id"": ""avvio"",
      ""agent"": ""account-manager"",
      ""title"": ""Avvia il giro"",
      ""trigger"": {
        ""reply"": ""avvia-giro"",
        ""to"": ""ricerca""
      },
      ""start"": ""auto"",
      ""brief"": ""La persona ha scelto il bando {codice}: avvia il giro dei tre responsabili.""
    },
    {
      ""id"": ""tecnica"",
      ""agent"": ""responsabile-tecnico"",
      ""title"": ""Scheda tecnica"",
      ""trigger"": {
        ""after"": [
          ""avvio""
        ]
      },
      ""start"": ""ask-owner"",
      ""brief"": ""Scrivi la scheda di fattibilità tecnica sul bando {codice}."",
      ""produces"": [
        ""citta-degli-agenti/gara/schede/tecnica.md""
      ]
    },
    {
      ""id"": ""contratto"",
      ""agent"": ""responsabile-legale"",
      ""title"": ""Scheda contrattuale"",
      ""trigger"": {
        ""after"": [
          ""avvio""
        ]
      },
      ""start"": ""ask-owner"",
      ""brief"": ""Scrivi la scheda contrattuale sul bando {codice}."",
      ""produces"": [
        ""citta-degli-agenti/gara/schede/contrattuale.md""
      ]
    },
    {
      ""id"": ""delivery"",
      ""agent"": ""responsabile-delivery"",
      ""title"": ""Scheda di delivery"",
      ""trigger"": {
        ""after"": [
          ""avvio""
        ]
      },
      ""start"": ""ask-owner"",
      ""brief"": ""Scrivi la scheda di delivery (team e tempi) sul bando {codice}."",
      ""produces"": [
        ""citta-degli-agenti/gara/schede/delivery.md""
      ]
    },
    {
      ""id"": ""sintesi"",
      ""agent"": ""account-manager"",
      ""title"": ""Sintesi per decidere"",
      ""trigger"": {
        ""after"": [
          ""tecnica"",
          ""contratto"",
          ""delivery""
        ],
        ""wait"": ""all""
      },
      ""start"": ""auto"",
      ""brief"": ""Le tre schede sul bando {codice} sono approvate: scrivi la sintesi per decidere se partecipare."",
      ""produces"": [
        ""citta-degli-agenti/gara/schede/sintesi.md""
      ]
    }
  ],
  ""loops"": [
    {
      ""id"": ""rifacimento"",
      ""steps"": [
        ""tecnica"",
        ""contratto"",
        ""delivery""
      ],
      ""until"": ""approved"",
      ""restart"": ""manual""
    }
  ]
}";

        private static List<AgentRegistryEntry> GaraAgents() => new()
        {
            new AgentRegistryEntry
            {
                Name = "account-manager",
                Replies = new List<AgentRegistryReply> { new() { Id = "avvia-giro", Label = "Avvia il giro su {codice}", Message = "avvia {codice}" } },
            },
            new AgentRegistryEntry { Name = "responsabile-tecnico" },
            new AgentRegistryEntry { Name = "responsabile-legale" },
            new AgentRegistryEntry { Name = "responsabile-delivery" },
        };

        private static bool AllFolders(string _) => true;
        private static string Issues(WorkflowCheckResult r) => string.Join("\n", r.Issues);

        private static WorkflowCheckResult ParseGaraWith(string from, string to)
        {
            Assert.IsTrue(Gara.Contains(from), "la sostituzione del test non trova: " + from);
            return WorkflowParser.Parse(Gara.Replace(from, to));
        }

        private static void AssertError(WorkflowCheckResult r, string path, string fragment)
        {
            Assert.IsTrue(r.Issues.Any(i => i.Severity == WorkflowSeverity.Error && i.Path == path && i.Message.Contains(fragment)),
                $"atteso un errore in '{path}' con «{fragment}». Trovati:\n{Issues(r)}");
            Assert.IsFalse(r.IsValid);
        }

        // ---- lettura ----

        [TestMethod]
        public void Read_the_gara_workflow_without_issues()
        {
            var r = WorkflowParser.Parse(Gara);

            Assert.AreEqual(0, r.Issues.Count, Issues(r));
            var wf = r.Descriptor;
            Assert.AreEqual(2, wf.Version);
            Assert.AreEqual(6, wf.Steps.Count);
            Assert.IsTrue(wf.Variables.ContainsKey("codice"));
            Assert.AreEqual(WorkflowTriggerKind.After, wf.Step("tecnica").Trigger.Kind);
            CollectionAssert.AreEqual(new[] { "tecnica", "contratto", "delivery" }, wf.Step("sintesi").Trigger.After);
            StringAssert.Contains(wf.Step("tecnica").Brief, "{codice}");
            var loop = wf.LoopOf("tecnica", WorkflowLoopKind.UntilApproved);
            Assert.AreEqual(WorkflowLoopKind.UntilApproved, loop.Kind);
            Assert.IsNull(loop.Max, "senza max: nessun limite");
            Assert.AreEqual(WorkflowStart.Manual, loop.Restart);

            Assert.AreEqual(0, WorkflowProjectCheck.Check(wf, GaraAgents(), AllFolders).Count);
        }

        [TestMethod]
        public void Refuse_an_unknown_key_with_its_path()
        {
            var r = ParseGaraWith(@"""start"": ""manual"",", @"""strat"": ""manual"",");
            AssertError(r, "steps[0].strat", "chiave sconosciuta 'strat'");
            AssertError(r, "steps[0].start", "manca 'start'");
        }

        [TestMethod]
        public void Explain_how_to_convert_a_version_1_file()
        {
            var r = ParseGaraWith(@"""mde_workflow"": 2", @"""mde_workflow"": 1");
            AssertError(r, "mde_workflow", "versione 1 non più supportata");
            StringAssert.Contains(r.Issues.Single().Fix, "\"after\"");
        }

        [TestMethod]
        public void Name_the_version_1_trigger_forms()
        {
            var r = ParseGaraWith(@"""after"": [
          ""avvio""
        ]
      },
      ""start"": ""ask-owner"",
      ""brief"": ""Scrivi la scheda di fattibilità tecnica", @"""assignment"": ""avvio""
      },
      ""start"": ""ask-owner"",
      ""brief"": ""Scrivi la scheda di fattibilità tecnica");
            AssertError(r, "steps[2].trigger.assignment", "versione 1");
        }

        [TestMethod]
        public void Refuse_a_start_that_does_not_fit_the_trigger()
        {
            AssertError(ParseGaraWith(@"""start"": ""manual"",", @"""start"": ""auto"","), "steps[0].start", "non va con un trigger launch");
            AssertError(ParseGaraWith(@"""start"": ""ask-owner"",
      ""brief"": ""Scrivi la scheda di fattibilità tecnica", @"""start"": ""manual"",
      ""brief"": ""Scrivi la scheda di fattibilità tecnica"), "steps[2].start", "non va con un trigger after");
        }

        [TestMethod]
        public void Ask_for_the_brief_of_every_step_the_person_does_not_launch()
        {
            var r = ParseGaraWith(@"""brief"": ""Scrivi la scheda di fattibilità tecnica sul bando {codice}."",", "");
            AssertError(r, "steps[2].brief", "manca il testo dell'incarico");
        }

        [TestMethod]
        public void Refuse_a_brief_that_uses_an_undeclared_variable()
        {
            var r = ParseGaraWith("sul bando {codice}.\"", "sul bando {codice} entro {scadenza}.\"");
            AssertError(r, "steps[2].brief", "'{scadenza}' non è una variabile dichiarata");
        }

        [TestMethod]
        public void Refuse_a_reference_to_a_missing_step_and_steps_that_start_each_other()
        {
            var missing = ParseGaraWith(@"""contratto"",
          ""delivery""", @"""contratti"",
          ""delivery""");
            AssertError(missing, "steps[5].trigger.after", "'contratti' non è l'id di nessun passo");

            var cycle = ParseGaraWith(@"""reply"": ""avvia-giro"",
        ""to"": ""ricerca""", @"""after"": [""tecnica""]");
            Assert.IsTrue(cycle.Issues.Any(i => i.Message.Contains("si fanno partire a vicenda")), Issues(cycle));
        }

        [TestMethod]
        public void Accept_both_loop_kinds_as_the_author_writes_them()
        {
            var forLoop = ParseGaraWith(@"""until"": ""approved"",
      ""restart"": ""manual""", @"""times"": 3");
            Assert.IsTrue(forLoop.IsValid, Issues(forLoop));
            Assert.AreEqual(WorkflowLoopKind.Times, forLoop.Descriptor.LoopOf("tecnica", WorkflowLoopKind.Times).Kind);
            Assert.AreEqual(3, forLoop.Descriptor.LoopOf("tecnica", WorkflowLoopKind.Times).Times);

            var capped = ParseGaraWith(@"""restart"": ""manual""", @"""max"": 3,
      ""restart"": ""auto""");
            Assert.IsTrue(capped.IsValid, Issues(capped));
            Assert.AreEqual(3, capped.Descriptor.LoopOf("tecnica", WorkflowLoopKind.UntilApproved).Max);
            Assert.AreEqual(WorkflowStart.Auto, capped.Descriptor.LoopOf("tecnica", WorkflowLoopKind.UntilApproved).Restart);
        }

        [TestMethod]
        public void Refuse_loops_that_mix_kinds_or_miss_their_values()
        {
            AssertError(ParseGaraWith(@"""restart"": ""manual""", @"""restart"": ""manual"", ""times"": 2"), "loops[0]", "non tutti e due");
            AssertError(ParseGaraWith(@"""until"": ""approved"",
      ""restart"": ""manual""", @"""until"": ""approved"""), "loops[0].restart", "manca 'restart'");
            AssertError(ParseGaraWith(@"""until"": ""approved"",
      ""restart"": ""manual""", @"""times"": 1"), "loops[0].times", "da 2 in su");
            AssertError(ParseGaraWith(@"""restart"": ""manual""", @"""restart"": ""manual"", ""max"": 0"), "loops[0].max", "da 1 in su");
        }

        [TestMethod]
        public void Refuse_paths_that_leave_the_project_or_use_backslashes()
        {
            AssertError(ParseGaraWith("citta-degli-agenti/gara/schede/tecnica.md", @"citta-degli-agenti\\gara\\schede\\tecnica.md"), "steps[2].produces[0]", "barra '/'");
            AssertError(ParseGaraWith("citta-degli-agenti/gara/schede/tecnica.md", "../fuori/tecnica.md"), "steps[2].produces[0]", "esce dal progetto");
        }

        [TestMethod]
        public void Collect_every_problem_in_one_pass()
        {
            var r = WorkflowParser.Parse(@"{ ""mde_workflow"": 2, ""steps"": [
                { ""id"": ""Ricerca"", ""agent"": ""x"", ""trigger"": { ""launch"": true }, ""start"": ""manual"", ""colore"": ""blu"" } ] }");
            AssertError(r, "title", "manca 'title'");
            AssertError(r, "steps[0].id", "non è kebab-case");
            AssertError(r, "steps[0].colore", "chiave sconosciuta");
        }

        // ---- progetto ----

        [TestMethod]
        public void Report_in_the_project_what_does_not_exist_or_would_not_arrive()
        {
            var wf = WorkflowParser.Parse(Gara).Descriptor;
            var agents = GaraAgents().Where(a => a.Name != "responsabile-legale").ToList();
            agents[0].Replies = new List<AgentRegistryReply> { new() { Id = "avvia-giro", Label = "Avvia {codice} entro {scadenza}", Message = "avvia {codice}" } };

            var issues = WorkflowProjectCheck.Check(wf, agents, folder => folder != "citta-degli-agenti/gara/ricerche");

            string Msg(string path) => string.Join(" | ", issues.Where(i => i.Path == path).Select(i => i.Message));
            StringAssert.Contains(Msg("steps[3].agent"), "non c'è un agente 'responsabile-legale'");
            StringAssert.Contains(Msg("steps[0].produces[0]"), "'citta-degli-agenti/gara/ricerche' non esiste");
            StringAssert.Contains(Msg("steps[1].trigger.reply"), "'{scadenza}', che non è una variabile del workflow");

            agents[0].Replies = new List<AgentRegistryReply>();
            StringAssert.Contains(string.Join(" | ", WorkflowProjectCheck.Check(wf, agents, AllFolders).Select(i => i.Message)),
                "non dichiara la risposta 'avvia-giro'");
        }

        // ---- regola dei passi ----

        [TestMethod]
        public void Find_the_step_a_message_starts_and_who_starts_it()
        {
            var wf = WorkflowParser.Parse(Gara).Descriptor;

            var tecnica = WorkflowStartPolicy.StepFor(wf, "account-manager", "responsabile-tecnico", isApproval: false);
            Assert.AreEqual("tecnica", tecnica?.Id);
            Assert.AreEqual(WorkflowStart.AskOwner, tecnica.Start);
            Assert.AreEqual("sintesi", WorkflowStartPolicy.StepFor(wf, "user", "account-manager", isApproval: true)?.Id);
            Assert.IsNull(WorkflowStartPolicy.StepFor(wf, "user", "account-manager", isApproval: false), "la persona ha già deciso");
            Assert.IsNull(WorkflowStartPolicy.StepFor(wf, "responsabile-legale", "responsabile-tecnico", isApproval: false), "non previsto");
        }

        [TestMethod]
        public void Know_the_foreseen_approval_notices_and_assignees()
        {
            var wf = WorkflowParser.Parse(Gara).Descriptor;

            Assert.AreEqual("sintesi", WorkflowStartPolicy.ApprovalStepFor(wf, "responsabile-legale", "account-manager")?.Id);
            Assert.IsNull(WorkflowStartPolicy.ApprovalStepFor(wf, "account-manager", "responsabile-legale"));
            StringAssert.Contains(WorkflowStartPolicy.NotForeseen(wf, "account-manager", "responsabile-qualita"),
                "responsabile-tecnico, responsabile-legale, responsabile-delivery");
            StringAssert.Contains(WorkflowStartPolicy.NotForeseen(wf, "responsabile-tecnico", "responsabile-legale"), "vengono: account-manager");

        }

        [TestMethod]
        public void Limit_reworks_only_when_the_author_writes_a_max()
        {
            var wf = WorkflowParser.Parse(Gara).Descriptor;
            Assert.IsNull(WorkflowStartPolicy.ReworksLeft(wf, wf.Step("tecnica"), 5, out _), "ciclo senza max: nessun limite");
            Assert.IsNull(WorkflowStartPolicy.ReworksLeft(wf, wf.Step("sintesi"), 5, out _), "nessun ciclo: nessun limite (W9)");

            var capped = ParseGaraWith(@"""restart"": ""manual""", @"""max"": 2,
      ""restart"": ""manual""").Descriptor;
            Assert.AreEqual(2, WorkflowStartPolicy.ReworksLeft(capped, capped.Step("tecnica"), 1, out _));
            Assert.AreEqual(1, WorkflowStartPolicy.ReworksLeft(capped, capped.Step("tecnica"), 2, out _));
            Assert.AreEqual(0, WorkflowStartPolicy.ReworksLeft(capped, capped.Step("tecnica"), 3, out var why));
            StringAssert.Contains(why, "già stato rifatto 2 volte");
        }

        [TestMethod]
        public void Read_the_workflow_file_named_by_the_document()
        {
            var root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "wfdoc-" + System.Guid.NewGuid().ToString("N"));
            var folder = System.IO.Path.Combine(root, "gara");
            System.IO.Directory.CreateDirectory(folder);
            try
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(folder, "workflow.md"),
                    "---\nmde_type: workflow\ntitle: x\nworkflow: ./gara.workflow.json\n---\n# x\n");
                Assert.AreEqual("gara/gara.workflow.json", WorkflowDocument.JsonPathOf(root, "gara/workflow.md"));

                System.IO.File.WriteAllText(System.IO.Path.Combine(folder, "altro.md"), "---\nmde_type: ownership\n---\n");
                var ex = Assert.ThrowsException<System.InvalidOperationException>(() => WorkflowDocument.JsonPathOf(root, "gara/altro.md"));
                StringAssert.Contains(ex.Message, "mde_type: workflow");
            }
            finally { System.IO.Directory.Delete(root, true); }
        }

        [TestMethod]
        public void Allow_a_for_loop_and_an_until_loop_on_the_same_step_but_not_two_of_a_kind()
        {
            // «Tre giri voluti, ciascuno approvato; se rifiutata si rifà al massimo due volte»: due cicli, uno per tipo.
            var both = ParseGaraWith(@"""restart"": ""manual""
    }", @"""restart"": ""manual"", ""max"": 2
    },
    { ""id"": ""giri"", ""steps"": [""tecnica""], ""times"": 3 }");
            Assert.IsTrue(both.IsValid, Issues(both));
            Assert.AreEqual(3, both.Descriptor.LoopOf("tecnica", WorkflowLoopKind.Times).Times);
            Assert.AreEqual(2, WorkflowStartPolicy.ReworksLeft(both.Descriptor, both.Descriptor.Step("tecnica"), 1, out _));

            var twice = ParseGaraWith(@"""restart"": ""manual""
    }", @"""restart"": ""manual""
    },
    { ""id"": ""ancora"", ""steps"": [""tecnica""], ""until"": ""approved"", ""restart"": ""auto"" }");
            Assert.IsTrue(twice.Issues.Any(i => i.Path == "loops[1].steps[0]" && i.Message.Contains("dello stesso tipo")), Issues(twice));
        }
    }
}
