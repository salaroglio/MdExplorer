using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Agents.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Il compendio delle dinamiche tra agenti (standard v1): la lettura è a tolleranza zero, raccoglie tutti i problemi con
    /// il loro percorso, e la verifica di progetto distingue ciò che non esiste (errore) da ciò in cui le schede dicono
    /// altro (avviso). Il caso di riferimento è la gara del demo.
    /// </summary>
    [TestClass]
    public class Workflow_Should
    {
        private const string Gara = @"{
  ""mde_workflow"": 1,
  ""title"": ""Gara: dal bando alla sintesi"",
  ""steps"": [
    { ""id"": ""ricerca"", ""agent"": ""account-manager"", ""title"": ""Cerca i bandi"",
      ""trigger"": { ""launch"": true }, ""start"": ""manual"",
      ""produces"": [""citta-degli-agenti/gara/ricerche/ricerca-*.md""] },
    { ""id"": ""avvio"", ""agent"": ""account-manager"",
      ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" }, ""start"": ""auto"" },
    { ""id"": ""tecnica"", ""agent"": ""responsabile-tecnico"",
      ""trigger"": { ""assignment"": ""avvio"" }, ""start"": ""ask-owner"",
      ""produces"": [""citta-degli-agenti/gara/schede/tecnica.md""] },
    { ""id"": ""contratto"", ""agent"": ""responsabile-legale"",
      ""trigger"": { ""assignment"": ""avvio"" }, ""start"": ""ask-owner"",
      ""produces"": [""citta-degli-agenti/gara/schede/contrattuale.md""] },
    { ""id"": ""delivery"", ""agent"": ""responsabile-delivery"",
      ""trigger"": { ""assignment"": ""avvio"" }, ""start"": ""ask-owner"",
      ""produces"": [""citta-degli-agenti/gara/schede/delivery.md""] },
    { ""id"": ""sintesi"", ""agent"": ""account-manager"",
      ""trigger"": { ""approval"": [""tecnica"", ""contratto"", ""delivery""], ""wait"": ""all"" }, ""start"": ""auto"",
      ""produces"": [""citta-degli-agenti/gara/schede/sintesi.md""] }
  ],
  ""loops"": [
    { ""id"": ""rifacimento"", ""steps"": [""tecnica"", ""contratto"", ""delivery""],
      ""on"": ""rejected"", ""restart"": ""manual"", ""max"": 2, ""then"": ""stop"" }
  ]
}";

        private static List<AgentRegistryEntry> GaraAgents() => new()
        {
            new AgentRegistryEntry
            {
                Name = "account-manager", AcceptsMessagesFrom = new List<string> { "user" },
                Replies = new List<AgentRegistryReply> { new() { Id = "avvia-giro", Label = "Avvia il giro su {codice}" } },
            },
            Lead("responsabile-tecnico"), Lead("responsabile-legale"), Lead("responsabile-delivery"),
        };

        private static AgentRegistryEntry Lead(string name) => new()
        {
            Name = name,
            AcceptsMessagesFrom = new List<string> { "account-manager", "user" },
            OnApprovalNotify = new List<string> { "account-manager" },
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

        [TestMethod]
        public void Read_the_gara_workflow_without_issues()
        {
            var r = WorkflowParser.Parse(Gara);

            Assert.IsTrue(r.IsValid, Issues(r));
            Assert.AreEqual(0, r.Issues.Count, Issues(r));
            var wf = r.Descriptor;
            Assert.AreEqual(6, wf.Steps.Count);
            Assert.AreEqual(WorkflowStart.AskOwner, wf.Step("tecnica").Start);
            Assert.AreEqual("avvio", wf.Step("tecnica").Trigger.FromStep);
            CollectionAssert.AreEqual(new[] { "tecnica", "contratto", "delivery" }, wf.Step("sintesi").Trigger.ApprovalOf);
            Assert.AreEqual(2, wf.Loops.Single().Max);
            Assert.AreEqual("Cerca i bandi", wf.Step("ricerca").Label);
            Assert.AreEqual("avvio", wf.Step("avvio").Label, "senza title, l'etichetta è l'id");

            var project = WorkflowProjectCheck.Check(wf, GaraAgents(), AllFolders);
            Assert.AreEqual(0, project.Count, string.Join("\n", project));
        }

        [TestMethod]
        public void Refuse_an_unknown_key_with_its_path()
        {
            var r = ParseGaraWith(@"""start"": ""ask-owner"",
      ""produces"": [""citta-degli-agenti/gara/schede/tecnica.md""]",
                @"""strat"": ""ask-owner"",
      ""produces"": [""citta-degli-agenti/gara/schede/tecnica.md""]");

            AssertError(r, "steps[2].strat", "chiave sconosciuta 'strat'");
            AssertError(r, "steps[2].start", "manca 'start'");
        }

        [TestMethod]
        public void Refuse_a_trigger_with_two_forms()
        {
            var r = ParseGaraWith(@"{ ""assignment"": ""avvio"" }, ""start"": ""ask-owner"",
      ""produces"": [""citta-degli-agenti/gara/schede/tecnica.md""]",
                @"{ ""assignment"": ""avvio"", ""launch"": true }, ""start"": ""ask-owner"",
      ""produces"": [""citta-degli-agenti/gara/schede/tecnica.md""]");

            AssertError(r, "steps[2].trigger", "più forme insieme");
        }

        [TestMethod]
        public void Refuse_a_start_that_does_not_fit_the_trigger()
        {
            var launchAuto = ParseGaraWith(@"""trigger"": { ""launch"": true }, ""start"": ""manual""", @"""trigger"": { ""launch"": true }, ""start"": ""auto""");
            AssertError(launchAuto, "steps[0].start", "non va con un trigger launch");

            var replyAsk = ParseGaraWith(@"""to"": ""ricerca"" }, ""start"": ""auto""", @"""to"": ""ricerca"" }, ""start"": ""ask-owner""");
            AssertError(replyAsk, "steps[1].start", "ha già scelto");

            var unknown = ParseGaraWith(@"""trigger"": { ""launch"": true }, ""start"": ""manual""", @"""trigger"": { ""launch"": true }, ""start"": ""subito""");
            AssertError(unknown, "steps[0].start", "non è un modo di avvio");
        }

        [TestMethod]
        public void Refuse_a_reference_to_a_missing_step()
        {
            var r = ParseGaraWith(@"""approval"": [""tecnica"", ""contratto"", ""delivery""]", @"""approval"": [""tecnica"", ""contratti"", ""delivery""]");

            AssertError(r, "steps[5].trigger.approval", "'contratti' non è l'id di nessun passo");
            StringAssert.Contains(r.Issues.First(i => i.Path == "steps[5].trigger.approval").Fix, "contratto");
        }

        [TestMethod]
        public void Refuse_steps_that_start_each_other()
        {
            // avvio parte dall'incarico di tecnica, tecnica dall'incarico di avvio: un giro senza fine non dichiarato.
            var r = ParseGaraWith(@"""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" }", @"""trigger"": { ""assignment"": ""tecnica"" }");

            Assert.IsTrue(r.Issues.Any(i => i.Message.Contains("si fanno partire a vicenda") && i.Message.Contains("→")), Issues(r));
        }

        [TestMethod]
        public void Refuse_a_step_that_never_starts_and_a_workflow_with_no_launch()
        {
            var noLaunch = ParseGaraWith(@"""trigger"": { ""launch"": true }, ""start"": ""manual""", @"""trigger"": { ""assignment"": ""avvio"" }, ""start"": ""auto""");
            Assert.IsTrue(noLaunch.Issues.Any(i => i.Message.Contains("nessun passo parte da un lancio") || i.Message.Contains("a vicenda")), Issues(noLaunch));

            var orphan = WorkflowParser.Parse(@"{ ""mde_workflow"": 1, ""title"": ""t"", ""steps"": [
                { ""id"": ""a"", ""agent"": ""x"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" },
                { ""id"": ""b"", ""agent"": ""y"", ""trigger"": { ""assignment"": ""c"" }, ""start"": ""auto"" },
                { ""id"": ""c"", ""agent"": ""z"", ""trigger"": { ""assignment"": ""b"" }, ""start"": ""auto"" } ] }");
            Assert.IsFalse(orphan.IsValid);
            Assert.IsTrue(orphan.Issues.Any(i => i.Message.Contains("non parte mai")), Issues(orphan));
        }

        [TestMethod]
        public void Accept_only_the_rework_loop_in_v1()
        {
            var r = ParseGaraWith(@"""on"": ""rejected"", ""restart"": ""manual"", ""max"": 2", @"""on"": ""approved"", ""restart"": ""auto"", ""max"": 0");

            AssertError(r, "loops[0].on", "rifacimento");
            AssertError(r, "loops[0].restart", "niente riparte da solo");
            AssertError(r, "loops[0].max", "da 1 in su");
        }

        [TestMethod]
        public void Refuse_paths_that_leave_the_project_or_use_backslashes()
        {
            var back = ParseGaraWith("citta-degli-agenti/gara/schede/tecnica.md", @"citta-degli-agenti\\gara\\schede\\tecnica.md");
            AssertError(back, "steps[2].produces[0]", "barra '/'");

            var up = ParseGaraWith("citta-degli-agenti/gara/schede/tecnica.md", "../fuori/tecnica.md");
            AssertError(up, "steps[2].produces[0]", "esce dal progetto");

            var star = ParseGaraWith("citta-degli-agenti/gara/schede/tecnica.md", "citta-degli-agenti/*/tecnica.md");
            AssertError(star, "steps[2].produces[0]", "solo nel nome del file");
        }

        [TestMethod]
        public void Refuse_an_unsupported_version_or_something_that_is_not_json()
        {
            AssertError(ParseGaraWith(@"""mde_workflow"": 1", @"""mde_workflow"": 2"), "mde_workflow", "versione 2 non supportata");
            AssertError(WorkflowParser.Parse("{ non json"), "", "non è JSON valido");
            Assert.IsNull(WorkflowParser.Parse("[1,2]").Descriptor);
        }

        [TestMethod]
        public void Collect_every_problem_in_one_pass()
        {
            var r = WorkflowParser.Parse(@"{ ""mde_workflow"": 1, ""steps"": [
                { ""id"": ""Ricerca"", ""agent"": ""x"", ""trigger"": { ""launch"": true }, ""start"": ""manual"", ""colore"": ""blu"" } ] }");

            AssertError(r, "title", "manca 'title'");
            AssertError(r, "steps[0].id", "non è kebab-case");
            AssertError(r, "steps[0].colore", "chiave sconosciuta");
        }

        [TestMethod]
        public void Report_in_the_project_what_does_not_exist_as_errors()
        {
            var wf = WorkflowParser.Parse(Gara).Descriptor;
            var agents = GaraAgents().Where(a => a.Name != "responsabile-legale").ToList();
            agents[0].Replies = new List<AgentRegistryReply>();

            var issues = WorkflowProjectCheck.Check(wf, agents, folder => folder != "citta-degli-agenti/gara/ricerche");

            string Msg(string path) => string.Join(" | ", issues.Where(i => i.Path == path).Select(i => i.Message));
            StringAssert.Contains(Msg("steps[3].agent"), "non c'è un agente 'responsabile-legale'");
            StringAssert.Contains(Msg("steps[1].trigger.reply"), "non dichiara la risposta 'avvia-giro'");
            StringAssert.Contains(Msg("steps[0].produces[0]"), "la cartella 'citta-degli-agenti/gara/ricerche' non esiste");
            Assert.IsTrue(issues.Where(i => i.Path != null).All(i => i.Severity == WorkflowSeverity.Error), string.Join("\n", issues));
        }

        [TestMethod]
        public void Warn_where_the_cards_route_differently()
        {
            var wf = WorkflowParser.Parse(Gara).Descriptor;
            var agents = GaraAgents();
            agents.Single(a => a.Name == "responsabile-tecnico").AcceptsMessagesFrom = new List<string> { "user" };
            agents.Single(a => a.Name == "responsabile-delivery").OnApprovalNotify = new List<string>();

            var issues = WorkflowProjectCheck.Check(wf, agents, AllFolders);

            Assert.AreEqual(2, issues.Count, string.Join("\n", issues));
            Assert.IsTrue(issues.All(i => i.Severity == WorkflowSeverity.Warning));
            StringAssert.Contains(issues.Single(i => i.Path == "steps[2].trigger.assignment").Message, "accepts_messages_from");
            StringAssert.Contains(issues.Single(i => i.Path == "steps[5].trigger.approval").Message, "'responsabile-delivery'");
        }

        [TestMethod]
        public void Refuse_a_reply_step_given_to_another_agent()
        {
            var wf = ParseGaraWith(@"{ ""id"": ""avvio"", ""agent"": ""account-manager""", @"{ ""id"": ""avvio"", ""agent"": ""responsabile-tecnico""").Descriptor;

            var issues = WorkflowProjectCheck.Check(wf, GaraAgents(), AllFolders);

            Assert.IsTrue(issues.Any(i => i.Path == "steps[1].agent" && i.Severity == WorkflowSeverity.Error && i.Message.Contains("arriva a 'account-manager'")),
                string.Join("\n", issues));
        }

        [TestMethod]
        public void Find_the_step_an_assignment_starts_and_who_starts_it()
        {
            var wf = WorkflowParser.Parse(Gara).Descriptor;

            var tecnica = WorkflowStartPolicy.StepFor(wf, "account-manager", "responsabile-tecnico", isApproval: false);
            Assert.AreEqual("tecnica", tecnica?.Id);
            Assert.AreEqual(WorkflowStart.AskOwner, tecnica.Start);

            var sintesi = WorkflowStartPolicy.StepFor(wf, "user", "account-manager", isApproval: true);
            Assert.AreEqual("sintesi", sintesi?.Id, "l'avviso di approvazione arriva da «user» ma è un'approvazione");
            Assert.AreEqual(WorkflowStart.Auto, sintesi.Start);
        }

        [TestMethod]
        public void Leave_the_person_and_unknown_routes_to_the_caller()
        {
            var wf = WorkflowParser.Parse(Gara).Descriptor;

            Assert.IsNull(WorkflowStartPolicy.StepFor(wf, "user", "account-manager", isApproval: false), "la persona ha già deciso");
            Assert.IsNull(WorkflowStartPolicy.StepFor(wf, "responsabile-legale", "responsabile-tecnico", isApproval: false),
                "un incarico che il workflow non descrive non ha passo: lo decide chi chiama");
        }

        [TestMethod]
        public void Prefer_asking_the_owner_when_two_steps_match()
        {
            var wf = ParseGaraWith(@"""trigger"": { ""assignment"": ""avvio"" }, ""start"": ""ask-owner"",
      ""produces"": [""citta-degli-agenti/gara/schede/delivery.md""]",
                @"""trigger"": { ""assignment"": ""avvio"" }, ""start"": ""auto"",
      ""produces"": [""citta-degli-agenti/gara/schede/delivery.md""]").Descriptor;
            wf.Step("delivery").Agent = "responsabile-tecnico";

            Assert.AreEqual(WorkflowStart.AskOwner, WorkflowStartPolicy.StepFor(wf, "account-manager", "responsabile-tecnico", false).Start);
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

                ex = Assert.ThrowsException<System.InvalidOperationException>(() => WorkflowDocument.JsonPathOf(root, "gara/manca.md"));
                StringAssert.Contains(ex.Message, "non esiste");
            }
            finally { System.IO.Directory.Delete(root, true); }
        }
    }
}
