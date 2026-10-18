using System.IO;
using System.Linq;
using MdExplorer.Features.Agents.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Il compendio delle dinamiche tra persone e agenti (standard v3): la lettura è a tolleranza zero e raccoglie tutti i
    /// problemi con il loro percorso; l'unione con le schede dice quali puntini non combaciano; la verifica di progetto dice
    /// ciò che non esiste; la regola dei passi trova chi avvia cosa; i rifacimenti hanno un limite solo se l'autore lo
    /// scrive. Il caso di riferimento è la gara del demo (<see cref="GaraV3"/>).
    /// </summary>
    [TestClass]
    public class Workflow_Should
    {
        private static string Issues(WorkflowCheckResult r) => string.Join("\n", r.Issues);

        private static string Gara(string from, string to)
        {
            Assert.IsTrue(GaraV3.Workflow.Contains(from), "la sostituzione del test non trova: " + from);
            return GaraV3.Workflow.Replace(from, to);
        }

        private static WorkflowCheckResult ParseGaraWith(string from, string to) => WorkflowParser.Parse(Gara(from, to));

        private static void AssertError(WorkflowCheckResult r, string path, string fragment)
        {
            Assert.IsTrue(r.Issues.Any(i => i.Severity == WorkflowSeverity.Error && i.Path == path && i.Message.Contains(fragment)),
                $"atteso un errore in '{path}' con «{fragment}». Trovati:\n{Issues(r)}");
            Assert.IsFalse(r.IsValid);
        }

        /// <summary>Il workflow (cambiato o no) unito alle schede della gara, cambiate o no.</summary>
        private static WorkflowCheckResult Bind(string json, System.Func<string, string> cards = null)
        {
            var r = WorkflowParser.Parse(json);
            Assert.IsTrue(r.IsValid, Issues(r));
            var set = GaraV3.CardSet();
            if (cards != null)
            {
                var parser = new MdExplorer.Features.Yaml.YamlAgentCardParser();
                var text = cards(GaraV3.AccountManager);
                var card = parser.GetDescriptor(text);
                Assert.IsTrue(card.IsValid, card.RegistrationError);
                set.Agents["account-manager"] = new WorkflowCard { Path = "x", Skills = card.Card.Skills.ToList(), Replies = card.Card.Replies.ToList() };
            }
            r.Issues.AddRange(WorkflowBinder.Bind(r.Descriptor, set));
            return r;
        }

        // ---- lettura ----

        [TestMethod]
        public void Read_the_gara_workflow_without_issues()
        {
            var r = WorkflowParser.Parse(GaraV3.Workflow);

            Assert.AreEqual(0, r.Issues.Count, Issues(r));
            var wf = r.Descriptor;
            Assert.AreEqual(3, wf.Version);
            Assert.AreEqual(6, wf.Steps.Count);
            Assert.AreEqual("scheda-tecnica", wf.Step("tecnica").Skill);
            Assert.AreEqual("2 — competenza", wf.Step("tecnica").Inputs["criterio"].Text, "un testo con un punto resta un testo");
            CollectionAssert.AreEqual(new[] { "tecnica.scheda", "contratto.scheda", "delivery.scheda" }, wf.Step("sintesi").Inputs["schede"].From);
            var decision = wf.Step("decisione");
            Assert.IsTrue(decision.IsPerson);
            Assert.AreEqual("Account manager", decision.Person);
            CollectionAssert.AreEqual(new[] { "partecipa", "rinuncia" }, decision.Outcomes.Select(o => o.Id).ToList());
            var loop = wf.LoopOf("tecnica", WorkflowLoopKind.UntilApproved);
            Assert.IsNull(loop.Max, "senza max: nessun limite");
            Assert.AreEqual(WorkflowStart.Manual, loop.Restart);
            Assert.AreEqual(0, wf.Step("tecnica").Outputs.Count, "le uscite di un agente arrivano dalla scheda, non dalla lettura");
        }

        [TestMethod]
        public void Refuse_an_unknown_key_with_its_path()
        {
            var r = ParseGaraWith(@"""start"": ""manual""", @"""strat"": ""manual""");
            AssertError(r, "steps[0].strat", "unknown key 'strat'");
            AssertError(r, "steps[0].start", "'start' is missing");
        }

        [DataTestMethod]
        [DataRow(1, "version 1 is no longer supported", "\"after\"")]
        [DataRow(2, "version 2 is no longer supported", "\"skill\"")]
        public void Explain_how_to_convert_an_older_version(int version, string message, string fix)
        {
            var r = ParseGaraWith(@"""mde_workflow"": 3", $@"""mde_workflow"": {version}");
            AssertError(r, "mde_workflow", message);
            StringAssert.Contains(r.Issues.Single().Fix, fix);
        }

        [TestMethod]
        public void Refuse_a_start_that_does_not_fit_the_trigger()
        {
            AssertError(ParseGaraWith(@"""start"": ""manual""", @"""start"": ""auto"""), "steps[0].start", "does not go with trigger launch");
        }

        [TestMethod]
        public void Say_who_does_a_step()
        {
            AssertError(ParseGaraWith(@"""person"": ""Account manager"",", ""), "steps[5]", "does not say who does it");
            AssertError(ParseGaraWith(@"""person"": ""Account manager"",", @"""person"": ""Account manager"", ""agent"": ""account-manager"","),
                "steps[5]", "not both");
            AssertError(ParseGaraWith(@"""skill"": ""cerca-bandi"",", ""), "steps[0].skill", "'skill' is missing");
        }

        [TestMethod]
        public void Send_the_outputs_of_an_agent_to_its_card()
        {
            var r = ParseGaraWith(@"""skill"": ""cerca-bandi"",", @"""skill"": ""cerca-bandi"", ""produces"": [""gara/x.md""],");
            AssertError(r, "steps[0].produces", "declared by its card");
        }

        [TestMethod]
        public void Keep_a_person_step_for_people()
        {
            AssertError(ParseGaraWith(@"""trigger"": { ""after"": [""sintesi""] },", @"""trigger"": { ""launch"": true },"),
                "steps[5].trigger.launch", "is not launched");
            AssertError(ParseGaraWith(@"""task"": ""Leggi", @"""start"": ""auto"", ""task"": ""Leggi"), "steps[5].start", "belongs to an agent's step");
            AssertError(ParseGaraWith(@"""outcomes"": [ { ""id"": ""partecipa"", ""label"": ""Partecipiamo"" }, { ""id"": ""rinuncia"", ""label"": ""Non partecipiamo"" } ]", @"""outcomes"": []"),
                "steps[5].outcomes", "at least one outcome");
            AssertError(ParseGaraWith(@"""until"": ""approved""", @"""until"": ""approved"" }, { ""id"": ""decidi"", ""steps"": [""decisione""], ""times"": 2"),
                "loops[1].steps[0]", "is a person's step");
        }

        [TestMethod]
        public void Start_after_a_person_only_from_one_of_her_outcomes()
        {
            var r = WorkflowParser.Parse(Gara(@"    }
  ],
  ""loops""", @"    },
    { ""id"": ""offerta"", ""agent"": ""account-manager"", ""skill"": ""sintesi-gara"", ""trigger"": { ""reply"": ""forse"", ""to"": ""decisione"" }, ""start"": ""auto"",
      ""inputs"": { ""codice"": ""{codice}"", ""ricerca"": ""{ricerca}"", ""schede"": ""-"" } }
  ],
  ""loops"""));
            AssertError(r, "steps[6].trigger.reply", "'forse' is not an outcome of step 'decisione'");
        }

        [TestMethod]
        public void Refuse_an_input_from_a_step_that_does_not_come_before()
        {
            var r = ParseGaraWith(@"""ricerca"": ""{ricerca}"", ""criterio"": ""2 — competenza""", @"""ricerca"": { ""from"": ""contratto.scheda"" }, ""criterio"": ""2 — competenza""");
            AssertError(r, "steps[1].inputs.ricerca.from", "'contratto' does not come before 'tecnica'");
        }

        [TestMethod]
        public void Refuse_an_undeclared_variable_in_inputs_and_tasks()
        {
            AssertError(ParseGaraWith(@"""criterio"": ""2 — competenza""", @"""criterio"": ""{scadenza}"""), "steps[1].inputs.criterio", "'{scadenza}' is not a declared variable");
            AssertError(ParseGaraWith("sul bando {codice} e decidi", "sul bando {lotto} e decidi"), "steps[5].task", "'{lotto}' is not a declared variable");
        }

        [TestMethod]
        public void Refuse_a_reference_to_a_missing_step_and_steps_that_start_each_other()
        {
            AssertError(ParseGaraWith(@"""after"": [""tecnica"", ""contratto"", ""delivery""]", @"""after"": [""tecnica"", ""contratti"", ""delivery""]"),
                "steps[4].trigger.after", "'contratti' is not the id of any step");

            var cycle = ParseGaraWith(@"""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" },
      ""start"": ""ask-owner"",
      ""inputs"": { ""codice"": ""{codice}"", ""capitolato"": ""{capitolato}"", ""ricerca"": ""{ricerca}"", ""criterio"": ""2 — competenza"" }",
                @"""trigger"": { ""after"": [""sintesi""] },
      ""start"": ""ask-owner"",
      ""inputs"": { ""codice"": ""{codice}"", ""capitolato"": ""{capitolato}"", ""ricerca"": ""{ricerca}"", ""criterio"": ""2 — competenza"" }");
            Assert.IsTrue(cycle.Issues.Any(i => i.Message.Contains("start each other")), Issues(cycle));
        }

        [TestMethod]
        public void Accept_both_loop_kinds_as_the_author_writes_them()
        {
            var forLoop = ParseGaraWith(@"""until"": ""approved"", ""restart"": ""manual""", @"""times"": 3");
            Assert.IsTrue(forLoop.IsValid, Issues(forLoop));
            Assert.AreEqual(3, forLoop.Descriptor.LoopOf("tecnica", WorkflowLoopKind.Times).Times);

            var capped = ParseGaraWith(@"""restart"": ""manual""", @"""max"": 3, ""restart"": ""auto""");
            Assert.IsTrue(capped.IsValid, Issues(capped));
            Assert.AreEqual(3, capped.Descriptor.LoopOf("tecnica", WorkflowLoopKind.UntilApproved).Max);
            Assert.AreEqual(WorkflowStart.Auto, capped.Descriptor.LoopOf("tecnica", WorkflowLoopKind.UntilApproved).Restart);
        }

        [TestMethod]
        public void Refuse_loops_that_mix_kinds_or_miss_their_values()
        {
            AssertError(ParseGaraWith(@"""restart"": ""manual""", @"""restart"": ""manual"", ""times"": 2"), "loops[0]", "not both");
            AssertError(ParseGaraWith(@"""until"": ""approved"", ""restart"": ""manual""", @"""until"": ""approved"""), "loops[0].restart", "'restart' is missing");
            AssertError(ParseGaraWith(@"""until"": ""approved"", ""restart"": ""manual""", @"""times"": 1"), "loops[0].times", "from 2 up");
            AssertError(ParseGaraWith(@"""restart"": ""manual""", @"""restart"": ""manual"", ""max"": 0"), "loops[0].max", "from 1 up");
        }

        [TestMethod]
        public void Refuse_paths_of_a_person_that_leave_the_project_or_use_backslashes()
        {
            const string anchor = @"""inputs"": { ""sintesi"": { ""from"": ""sintesi.sintesi"" } },";
            AssertError(ParseGaraWith(anchor, anchor + @" ""produces"": [ { ""id"": ""verbale"", ""path"": ""gara\\verbale.md"" } ],"), "steps[5].produces[0].path", "slash '/'");
            AssertError(ParseGaraWith(anchor, anchor + @" ""produces"": [ { ""id"": ""verbale"", ""path"": ""../verbale.md"" } ],"), "steps[5].produces[0].path", "goes outside the project");
        }

        [TestMethod]
        public void Collect_every_problem_in_one_pass()
        {
            var r = WorkflowParser.Parse(@"{ ""mde_workflow"": 3, ""steps"": [
                { ""id"": ""Ricerca"", ""agent"": ""x"", ""skill"": ""y"", ""trigger"": { ""launch"": true }, ""start"": ""manual"", ""colore"": ""blu"" } ] }");
            AssertError(r, "title", "'title' is missing");
            AssertError(r, "steps[0].id", "is not kebab-case");
            AssertError(r, "steps[0].colore", "unknown key");
        }

        // ---- unione con le schede ----

        [TestMethod]
        public void Take_outputs_and_reads_from_the_skill_of_each_step()
        {
            var r = Bind(GaraV3.Workflow);

            Assert.IsTrue(r.IsValid, Issues(r));
            var wf = r.Descriptor;
            CollectionAssert.AreEqual(new[] { "gara/schede/tecnica.md" }, wf.Step("tecnica").Produces);
            CollectionAssert.AreEqual(new[] { "ricerca", "capitolato" }, wf.Step("ricerca").Outputs.Select(o => o.Id).ToList());
            Assert.AreEqual("downloads", wf.Step("ricerca").Outputs[1].Action);
            Assert.AreEqual("writes", wf.Step("ricerca").Outputs[0].Action, "senza action vale writes");
            Assert.AreEqual("Criteri di valutazione di un bando", wf.Step("ricerca").Reads[1].Section);
            Assert.IsNotNull(wf.Cards, "il workflow unito porta le schede, che il giro salva");
        }

        [TestMethod]
        public void Want_one_value_for_every_input_of_the_skill_and_no_more()
        {
            AssertError(Bind(Gara(@", ""criterio"": ""2 — competenza""", "")), "steps[1].inputs", "the input 'criterio' of job 'scheda-tecnica' receives nothing");
            AssertError(Bind(Gara(@"""criterio"": ""2 — competenza""", @"""criterio"": ""2 — competenza"", ""lotto"": ""1""")), "steps[1].inputs.lotto", "has no input 'lotto'");
        }

        [TestMethod]
        public void Refuse_a_skill_the_card_does_not_have()
        {
            AssertError(Bind(Gara(@"""skill"": ""scheda-tecnica""", @"""skill"": ""scheda-tecnicha""")), "steps[1].skill", "has no job 'scheda-tecnicha'");
        }

        [TestMethod]
        public void Refuse_an_output_the_step_does_not_deliver()
        {
            AssertError(Bind(Gara(@"""contratto.scheda""", @"""contratto.verbale""")), "steps[4].inputs.schede.from[1]", "step 'contratto' has no output 'verbale'");
        }

        [TestMethod]
        public void Want_the_reply_declared_proposed_by_the_skill_and_its_values_in_the_round()
        {
            AssertError(Bind(GaraV3.Workflow, card => card.Replace("      replies: [avvia-giro]\n", "")), "steps[1].trigger.reply", "does not propose the reply 'avvia-giro'");
            AssertError(Bind(GaraV3.Workflow, card => card.Replace("values: [codice, capitolato, ricerca]", "values: [codice, capitolato, ricerca, lotto]")),
                "steps[1].trigger.reply", "'{lotto}', which is not a workflow variable");
        }

        [TestMethod]
        public void Want_something_to_approve_in_a_loop_or_an_approval()
        {
            var noOutputs = GaraV3.Responsible("responsabile-tecnico", "scheda-tecnica", "Tecnica", "tecnica.md")
                .Replace("      outputs:\n        - { id: scheda, path: gara/schede/tecnica.md }\n", "");
            var parser = new MdExplorer.Features.Yaml.YamlAgentCardParser();
            var card = parser.GetDescriptor(noOutputs);
            Assert.IsTrue(card.IsValid, card.RegistrationError);
            var set = GaraV3.CardSet();
            set.Agents["responsabile-tecnico"] = new WorkflowCard { Path = "x", Skills = card.Card.Skills.ToList(), Replies = new() };
            var r = WorkflowParser.Parse(Gara(@"""start"": ""ask-owner"",
      ""inputs"": { ""codice"": ""{codice}"", ""capitolato"": ""{capitolato}"", ""ricerca"": ""{ricerca}"", ""criterio"": ""2 — competenza"" }",
                @"""start"": ""ask-owner"", ""approval"": { ""by"": ""Account manager"" },
      ""inputs"": { ""codice"": ""{codice}"", ""capitolato"": ""{capitolato}"", ""ricerca"": ""{ricerca}"", ""criterio"": ""2 — competenza"" }")
                .Replace(@"""tecnica.scheda"", ", ""));
            Assert.IsTrue(r.IsValid, Issues(r));
            r.Issues.AddRange(WorkflowBinder.Bind(r.Descriptor, set));
            AssertError(r, "steps[1].approval", "there is nothing to approve");
            AssertError(r, "loops[0].steps[0]", "an approval that would never come");
        }

        // ---- progetto ----

        [TestMethod]
        public void Check_the_gara_in_its_project_without_issues()
        {
            using var project = GaraV3.Project();
            var r = project.Check();
            Assert.IsTrue(r.IsValid, Issues(r));
            Assert.AreEqual(0, r.Issues.Count, Issues(r));
            Assert.AreEqual(".github/agents/responsabile-tecnico.agent.md", r.Descriptor.Cards.Agents["responsabile-tecnico"].Path);
        }

        [TestMethod]
        public void Report_in_the_project_what_does_not_exist()
        {
            using var project = GaraV3.Project(root =>
            {
                File.Delete(Path.Combine(root, ".github", "agents", "responsabile-legale.agent.md"));
                Directory.Delete(Path.Combine(root, "gara", "ricerche"), true);
                File.Delete(Path.Combine(root, "gara", "bandi.md"));
                File.WriteAllText(Path.Combine(root, "gara", "profilo.md"), "# Profilo\n\n## Tecnica\n\n## Delivery\n");
                File.WriteAllText(Path.Combine(root, "responsabilita.md"), GaraV3.Ownership.Replace("| Account manager |", "| Commerciale |"));
            });
            var r = project.Check();

            AssertError(r, "steps[2].agent", "the project has no card 'responsabile-legale.agent.md'");
            AssertError(r, "steps[0].skill", "the folder 'gara/ricerche' of output 'ricerca' does not exist");
            AssertError(r, "steps[0].skill", "reads 'gara/bandi.md', which does not exist");
            AssertError(r, "steps[0].skill", "has no chapter 'Criteri di valutazione di un bando'");
            AssertError(r, "steps[5].person", "'Account manager' is not a role in responsabilita.md");
        }

        [TestMethod]
        public void Ask_for_the_responsibilities_when_a_step_names_a_role()
        {
            using var project = GaraV3.Project();
            var r = WorkflowFile.Check(project.Root, "gara/gara.workflow.json", ownershipDoc: null);
            AssertError(r, "steps[5].person", "which the project does not have");

            File.WriteAllText(Path.Combine(project.Root, "responsabilita.md"), GaraV3.Ownership.Replace("| Ruolo |", "| Ambito |"));
            AssertError(project.Check(), "", "«Ruolo»");
        }

        [TestMethod]
        public void Go_on_with_the_cards_a_round_was_born_with()
        {
            using var project = GaraV3.Project();
            var cards = project.Check().Descriptor.Cards.ToJson();
            File.WriteAllText(Path.Combine(project.Root, ".github", "agents", "responsabile-tecnico.agent.md"),
                GaraV3.Responsible("responsabile-tecnico", "scheda-tecnica", "Tecnica", "tecnica.md").Replace("gara/schede/tecnica.md", "gara/schede/tecnica-v2.md"));

            Assert.AreEqual("gara/schede/tecnica-v2.md", project.Check().Descriptor.Step("tecnica").Produces.Single(), "il workflow di adesso vede la scheda di adesso");
            var round = WorkflowFile.ReadSnapshot(GaraV3.Workflow, cards);
            Assert.IsTrue(round.IsValid, Issues(round));
            Assert.AreEqual("gara/schede/tecnica.md", round.Descriptor.Step("tecnica").Produces.Single(), "il giro va avanti con la scheda con cui è nato");

            AssertError(WorkflowFile.ReadSnapshot(GaraV3.Workflow, null), "", "did not save the cards");
        }

        // ---- regola dei passi ----

        [TestMethod]
        public void Find_the_step_a_message_starts_and_who_starts_it()
        {
            var wf = GaraV3.Bound();

            var sintesi = WorkflowStartPolicy.StepFor(wf, "responsabile-tecnico", "account-manager", isApproval: false);
            Assert.AreEqual("sintesi", sintesi?.Id);
            Assert.AreEqual(WorkflowStart.Auto, sintesi.Start);
            Assert.AreEqual("sintesi", WorkflowStartPolicy.StepFor(wf, "user", "account-manager", isApproval: true)?.Id);
            Assert.IsNull(WorkflowStartPolicy.StepFor(wf, "user", "account-manager", isApproval: false), "la persona ha già deciso");
            Assert.IsNull(WorkflowStartPolicy.StepFor(wf, "responsabile-legale", "responsabile-tecnico", isApproval: false), "non previsto");
        }

        [TestMethod]
        public void Know_the_foreseen_approval_notices_and_assignees()
        {
            var wf = GaraV3.Bound();

            Assert.AreEqual("sintesi", WorkflowStartPolicy.ApprovalStepFor(wf, "responsabile-legale", "account-manager")?.Id);
            Assert.IsNull(WorkflowStartPolicy.ApprovalStepFor(wf, "account-manager", "responsabile-legale"));
            CollectionAssert.AreEqual(new string[0], WorkflowStartPolicy.AssigneesOf(wf, "account-manager").ToList(), "dopo la sintesi c'è una persona, non un agente");
            StringAssert.Contains(WorkflowStartPolicy.NotForeseen(wf, "responsabile-tecnico", "responsabile-legale"), "come: account-manager");
        }

        [TestMethod]
        public void Limit_reworks_only_when_the_author_writes_a_max()
        {
            var wf = GaraV3.Bound();
            Assert.IsNull(WorkflowStartPolicy.ReworksLeft(wf, wf.Step("tecnica"), 5, out _), "ciclo senza max: nessun limite");
            Assert.IsNull(WorkflowStartPolicy.ReworksLeft(wf, wf.Step("sintesi"), 5, out _), "nessun ciclo: nessun limite (W9)");

            var capped = GaraV3.Bound(Gara(@"""restart"": ""manual""", @"""max"": 2, ""restart"": ""manual"""));
            Assert.AreEqual(2, WorkflowStartPolicy.ReworksLeft(capped, capped.Step("tecnica"), 1, out _));
            Assert.AreEqual(1, WorkflowStartPolicy.ReworksLeft(capped, capped.Step("tecnica"), 2, out _));
            Assert.AreEqual(0, WorkflowStartPolicy.ReworksLeft(capped, capped.Step("tecnica"), 3, out var why));
            StringAssert.Contains(why, "has already been redone 2 times");
        }

        [TestMethod]
        public void Read_the_workflow_file_named_by_the_document()
        {
            var root = Path.Combine(Path.GetTempPath(), "wfdoc-" + System.Guid.NewGuid().ToString("N"));
            var folder = Path.Combine(root, "gara");
            Directory.CreateDirectory(folder);
            try
            {
                File.WriteAllText(Path.Combine(folder, "workflow.md"),
                    "---\nmde_type: workflow\ntitle: x\nworkflow: ./gara.workflow.json\n---\n# x\n");
                Assert.AreEqual("gara/gara.workflow.json", WorkflowDocument.JsonPathOf(root, "gara/workflow.md"));

                File.WriteAllText(Path.Combine(folder, "altro.md"), "---\nmde_type: ownership\n---\n");
                var ex = Assert.ThrowsException<System.InvalidOperationException>(() => WorkflowDocument.JsonPathOf(root, "gara/altro.md"));
                StringAssert.Contains(ex.Message, "mde_type: workflow");
            }
            finally { Directory.Delete(root, true); }
        }

        // ---- più workflow per progetto (PW) ----

        private const string Review = @"{
  ""mde_workflow"": 3, ""title"": ""Revisione"",
  ""steps"": [ { ""id"": ""rivedi"", ""agent"": ""responsabile-tecnico"", ""skill"": ""scheda-tecnica"", ""title"": ""Rivedi la scheda"",
                 ""trigger"": { ""launch"": true }, ""start"": ""manual"",
                 ""inputs"": { ""codice"": ""il bando"", ""capitolato"": ""il capitolato"", ""ricerca"": ""la ricerca"", ""criterio"": ""2"" } } ]
}";

        private static void WorkflowDoc(string root, string relative, string json)
        {
            var full = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            File.WriteAllText(full, $"---\nmde_type: workflow\ntitle: x\nworkflow: {json}\n---\n# x\n");
        }

        [TestMethod]
        public void Read_several_workflows_of_a_project()
        {
            using var project = GaraV3.Project(root =>
            {
                WorkflowDoc(root, "gara/workflow.md", "gara.workflow.json");
                WorkflowDoc(root, "revisione/workflow.md", "revisione.workflow.json");
                File.WriteAllText(Path.Combine(root, "revisione", "revisione.workflow.json"), Review);
            });

            var set = WorkflowDocument.LoadAll(project.Root, new[] { "gara/workflow.md", "/revisione/workflow.md" }, "responsabilita.md", out var problem);

            Assert.IsNull(problem, problem);
            CollectionAssert.AreEqual(new[] { "gara/gara.workflow.json", "revisione/revisione.workflow.json" }, set.Select(w => w.JsonPath).ToList());
            CollectionAssert.AreEqual(new[] { "gara/workflow.md", "revisione/workflow.md" }, set.Select(w => w.Document).ToList());
            Assert.AreEqual("Revisione", set[1].Descriptor.Title);
            Assert.AreEqual(0, WorkflowDocument.LoadAll(project.Root, null, "responsabilita.md", out problem).Count, "nessun workflow: elenco vuoto");
            Assert.IsNull(problem);

            // La regola di chi avvia vale su tutti: l'approvazione che sveglia l'account manager è nella gara.
            var found = WorkflowStartPolicy.StepFor(set, "user", "account-manager", isApproval: true);
            Assert.AreEqual("sintesi", found.Step?.Id);
            Assert.AreEqual("gara/gara.workflow.json", found.Workflow.JsonPath);
            StringAssert.Contains(WorkflowStartPolicy.NotForeseen(set, "responsabile-legale", "responsabile-tecnico"),
                "None of the project's workflows («Gara: dal bando alla decisione», «Revisione»)");
        }

        [TestMethod]
        public void Refuse_workflows_that_do_not_go_together()
        {
            using var project = GaraV3.Project(root =>
            {
                WorkflowDoc(root, "gara/workflow.md", "gara.workflow.json");
                WorkflowDoc(root, "gara/copia.md", "gara.workflow.json");
                WorkflowDoc(root, "bis/workflow.md", "bis.workflow.json");
                File.WriteAllText(Path.Combine(root, "bis", "bis.workflow.json"), GaraV3.Workflow.Replace("Gara: dal bando alla decisione", "Gara bis"));
            });
            string Problem(params string[] docs)
            {
                Assert.IsNull(WorkflowDocument.LoadAll(project.Root, docs, "responsabilita.md", out var problem));
                return problem;
            }

            StringAssert.Contains(Problem("gara/workflow.md", "gara/workflow.md"), "is listed twice");
            StringAssert.Contains(Problem("gara/workflow.md", "gara/copia.md"), "name the same workflow 'gara/gara.workflow.json'");
            StringAssert.Contains(Problem("gara/workflow.md", "bis/workflow.md"), "'account-manager' opens a round both in «Gara: dal bando alla decisione» and in «Gara bis»");
            StringAssert.Contains(Problem("gara/workflow.md", "manca/workflow.md"), "does not exist");
        }

        [TestMethod]
        public void Allow_a_for_loop_and_an_until_loop_on_the_same_step_but_not_two_of_a_kind()
        {
            // «Tre giri voluti, ciascuno approvato; se rifiutata si rifà al massimo due volte»: due cicli, uno per tipo.
            var both = ParseGaraWith(@"""restart"": ""manual"" }", @"""restart"": ""manual"", ""max"": 2 },
    { ""id"": ""giri"", ""steps"": [""tecnica""], ""times"": 3 }");
            Assert.IsTrue(both.IsValid, Issues(both));
            Assert.AreEqual(3, both.Descriptor.LoopOf("tecnica", WorkflowLoopKind.Times).Times);
            Assert.AreEqual(2, WorkflowStartPolicy.ReworksLeft(both.Descriptor, both.Descriptor.Step("tecnica"), 1, out _));

            var twice = ParseGaraWith(@"""restart"": ""manual"" }", @"""restart"": ""manual"" },
    { ""id"": ""ancora"", ""steps"": [""tecnica""], ""until"": ""approved"", ""restart"": ""auto"" }");
            Assert.IsTrue(twice.Issues.Any(i => i.Path == "loops[1].steps[0]" && i.Message.Contains("of the same kind")), Issues(twice));
        }
    }
}
