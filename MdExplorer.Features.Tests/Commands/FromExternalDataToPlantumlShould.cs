using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Tests.Commands
{
    /// <summary>
    /// ```plantuml(@json, path) must expand into a plain ```plantuml block holding the content
    /// of the external file, because everything downstream (SVG, cache, interactivity, export)
    /// only knows about ordinary PlantUML blocks.
    /// </summary>
    [TestClass]
    public class FromExternalDataToPlantumlShould
    {
        private string _projectRoot;

        private const string SampleJson = "{\n  \"fruit\": \"Apple\",\n  \"nested\": { \"a\": 1 }\n}";

        [TestInitialize]
        public void CreateProjectOnDisk()
        {
            _projectRoot = Path.Combine(Path.GetTempPath(), "mde-plantuml-include-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_projectRoot, "docs", "data"));

            File.WriteAllText(Path.Combine(_projectRoot, "docs", "servizi.json"), SampleJson);
            File.WriteAllText(Path.Combine(_projectRoot, "docs", "data", "nested.json"), SampleJson);
            File.WriteAllText(Path.Combine(_projectRoot, "root.json"), SampleJson);
            File.WriteAllText(Path.Combine(_projectRoot, "docs", "values.yaml"), "app: MdExplorer\nservizi:\n  db: localhost");
        }

        [TestCleanup]
        public void RemoveProjectFromDisk()
        {
            try { Directory.Delete(_projectRoot, recursive: true); } catch { /* best effort */ }
        }

        private static FromExternalDataToPlantuml BuildCommand()
            => new FromExternalDataToPlantuml(NullLogger<FromExternalDataToPlantuml>.Instance);

        /// <summary>The document being rendered is docs/doc.md inside the temp project.</summary>
        private RequestInfo RequestForDocsFolder() => new RequestInfo
        {
            CurrentRoot = _projectRoot,
            CurrentQueryRequest = Path.Combine("docs", "doc.md"),
        };

        /// <summary>
        /// StringAssert.Contains runs the haystack through string.Format, and the braces of a
        /// JSON payload blow it up before the assertion is even evaluated. Assert on the plain
        /// bool instead, and keep the payload out of the failure message.
        /// </summary>
        private static void AssertContains(string haystack, string needle)
            => Assert.IsTrue(haystack != null && haystack.Contains(needle),
                             "the transformed markdown was expected to contain: " + needle);

        private string Transform(string markdown)
            => BuildCommand().TransformInNewMDFromMD(markdown, RequestForDocsFolder());

        [TestMethod]
        public void WrapTheExternalJsonInAStartjsonBlock()
        {
            var transformed = Transform("prima\n\n```plantuml(@json, ./servizi.json)\n```\n\ndopo");

            AssertContains(transformed, "```plantuml");
            AssertContains(transformed, "@startjson");
            AssertContains(transformed, "\"fruit\": \"Apple\"");
            AssertContains(transformed, "@endjson");
            Assert.IsFalse(transformed.Contains("plantuml(@json"),
                "the declaration must be gone: left in place, the PlantUML command would render it as diagram source");
            AssertContains(transformed, "prima");
            AssertContains(transformed, "dopo");
        }

        [TestMethod]
        public void PutTheBodyDirectivesBeforeTheData()
        {
            var transformed = Transform(
                "```plantuml(@json, ./servizi.json)\n#highlight \"nested\" / \"a\"\n```");

            var highlightAt = transformed.IndexOf("#highlight", StringComparison.Ordinal);
            var dataAt = transformed.IndexOf("\"fruit\"", StringComparison.Ordinal);
            var openAt = transformed.IndexOf("@startjson", StringComparison.Ordinal);

            Assert.IsTrue(highlightAt > openAt, "directives go after @startjson");
            Assert.IsTrue(highlightAt < dataAt, "directives must come BEFORE the data, or PlantUML ignores them");
        }

        [TestMethod]
        public void UseStartyamlForTheYamlDirective()
        {
            var transformed = Transform("```plantuml(@yaml, ./values.yaml)\n```");

            AssertContains(transformed, "@startyaml");
            AssertContains(transformed, "app: MdExplorer");
            AssertContains(transformed, "@endyaml");
        }

        [TestMethod]
        public void ResolveAPathInASubfolderOfTheDocument()
        {
            // Regression guard for path resolution: it must work on Linux too, where joining
            // segments back with a backslash (IHelper.NormalizePath) would not find the file.
            var transformed = Transform("```plantuml(@json, ./data/nested.json)\n```");

            AssertContains(transformed, "@startjson");
            AssertContains(transformed, "\"fruit\": \"Apple\"");
        }

        [TestMethod]
        public void ResolveALeadingSlashAgainstTheProjectRoot()
        {
            var transformed = Transform("```plantuml(@json, /root.json)\n```");

            AssertContains(transformed, "@startjson");
            AssertContains(transformed, "\"fruit\": \"Apple\"");
        }

        [TestMethod]
        public void ExpandEveryBlockOfTheDocument()
        {
            var transformed = Transform(
                "# titolo\n\n```plantuml(@json, ./servizi.json)\n```\n\ntesto in mezzo\n\n```plantuml(@yaml, ./values.yaml)\n```\n");

            AssertContains(transformed, "@startjson");
            AssertContains(transformed, "@startyaml");
            AssertContains(transformed, "testo in mezzo");
            Assert.IsFalse(transformed.Contains("plantuml(@"), "no declaration may survive the transform");
        }

        [TestMethod]
        public void ProduceABlockThePlantumlCommandCanRead()
        {
            // The contract between the two commands: FromPlantumlToSvg reads ```plantuml up to
            // the first backtick. If our expansion did not match THAT regex, the diagram would
            // simply never be generated.
            var transformed = Transform("```plantuml(@json, ./servizi.json)\n#highlight \"nested\"\n```");

            var plantumlRegex = new System.Text.RegularExpressions.Regex(
                @"```plantuml([^```]*)`{3}(?:(?={){([^{]*)}|)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                System.Text.RegularExpressions.RegexOptions.Singleline);

            var matches = plantumlRegex.Matches(transformed);

            Assert.AreEqual(1, matches.Count, "the expansion must be visible to the PlantUML command");
            var diagramSource = matches[0].Groups[1].Value;
            AssertContains(diagramSource, "@startjson");
            AssertContains(diagramSource, "#highlight");
            AssertContains(diagramSource, "\"fruit\": \"Apple\"");
            AssertContains(diagramSource, "@endjson");
        }

        [TestMethod]
        public void LeaveAnOrdinaryPlantumlBlockAlone()
        {
            const string markdown = "```plantuml\n@startuml\nAlice -> Bob\n@enduml\n```";

            Assert.AreEqual(markdown, Transform(markdown),
                "a plain PlantUML block has nothing to do with this command");
        }

        [TestMethod]
        public void ExplainThatTheFileIsMissing()
        {
            var transformed = Transform("```plantuml(@json, ./manca.json)\n```");

            AssertContains(transformed, "mde-plantuml-include-error");
            AssertContains(transformed, "file not found");
            Assert.IsFalse(transformed.Contains("@startjson"), "no diagram may be produced from a missing file");
        }

        [TestMethod]
        public void RefuseAPathOutsideTheProject()
        {
            var transformed = Transform("```plantuml(@json, ../../../etc/passwd)\n```");

            AssertContains(transformed, "mde-plantuml-include-error");
            AssertContains(transformed, "goes outside the project folder");
        }

        [TestMethod]
        public void ExplainAnUnknownDirective()
        {
            var transformed = Transform("```plantuml(@xml, ./servizi.json)\n```");

            AssertContains(transformed, "mde-plantuml-include-error");
            AssertContains(transformed, "@xml");
            AssertContains(transformed, "@json");
        }

        [TestMethod]
        public void ExplainThatTheJsonIsBroken()
        {
            File.WriteAllText(Path.Combine(_projectRoot, "docs", "rotto.json"), "{ \"a\": }");

            var transformed = Transform("```plantuml(@json, ./rotto.json)\n```");

            AssertContains(transformed, "mde-plantuml-include-error");
            // HttpUtility.HtmlEncode turns the accented "è" into &#232;, so match the ASCII part.
            AssertContains(transformed, "not valid JSON");
        }

        [TestMethod]
        public void RefuseContentHoldingABacktick()
        {
            // A backtick would close the generated fence early and the diagram would come out
            // truncated with no explanation — say it instead.
            File.WriteAllText(Path.Combine(_projectRoot, "docs", "backtick.json"), "{ \"cmd\": \"`ls`\" }");

            var transformed = Transform("```plantuml(@json, ./backtick.json)\n```");

            AssertContains(transformed, "mde-plantuml-include-error");
            AssertContains(transformed, "backtick");
        }

        [TestMethod]
        public void KeepTheCssClassSuffixWorking()
        {
            // The PlantUML command reads {...} right after the closing fence: our expansion must
            // not get between the two, otherwise sizing classes would stop working.
            var transformed = Transform("```plantuml(@json, ./servizi.json)\n```{ .fullWidth }");

            var fenceEnd = transformed.LastIndexOf("```", StringComparison.Ordinal);
            var classAt = transformed.IndexOf("{ .fullWidth }", StringComparison.Ordinal);

            Assert.IsTrue(classAt > fenceEnd, "the class suffix must stay right after the closing fence");
        }
        private const string SmallWorkflow = @"{
  ""mde_workflow"": 3, ""title"": ""Prova"",
  ""variables"": { ""codice"": ""il bando"" },
  ""steps"": [
    { ""id"": ""ricerca"", ""agent"": ""cercatore"", ""skill"": ""cerca"", ""title"": ""Cerca i bandi"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" },
    { ""id"": ""tecnica"", ""agent"": ""tecnico"", ""skill"": ""scheda"", ""trigger"": { ""after"": [""ricerca""] }, ""start"": ""ask-owner"",
      ""inputs"": { ""codice"": ""{codice}"" } },
    { ""id"": ""legale"", ""agent"": ""legale"", ""skill"": ""clausole"", ""trigger"": { ""after"": [""ricerca""] }, ""start"": ""ask-owner"",
      ""inputs"": { ""codice"": ""{codice}"" } },
    { ""id"": ""sintesi"", ""agent"": ""cercatore"", ""skill"": ""sintesi"", ""trigger"": { ""after"": [""tecnica"", ""legale""] }, ""start"": ""auto"",
      ""brief"": ""Scrivi la sintesi."" }
  ],
  ""loops"": [ { ""id"": ""rifacimento"", ""steps"": [""tecnica""], ""until"": ""approved"", ""max"": 2, ""restart"": ""manual"" } ]
}";

        /// <summary>Le schede dei tre agenti del workflow piccolo: due nel posto solito, una altrove nel progetto.</summary>
        private void WriteSmallCards()
        {
            void Card(string relative, string name, string skills)
            {
                var full = Path.Combine(_projectRoot, relative.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                File.WriteAllText(full, $"---\na2a:\n  name: {name}\n  skills:\n{skills}---\n");
            }
            Card(".github/agents/cercatore.agent.md", "cercatore",
                "    - id: cerca\n      outputs:\n        - { id: ricerca, path: docs/ricerche/ricerca-*.md }\n    - id: sintesi\n");
            Card(".github/agents/tecnico.agent.md", "tecnico",
                "    - id: scheda\n      inputs:\n        - { id: codice, description: il bando }\n      outputs:\n        - { id: scheda, path: docs/schede/tecnica.md }\n");
            Card("altrove/legale.agent.md", "legale",
                "    - id: clausole\n      inputs:\n        - { id: codice, description: il bando }\n");
        }

        [TestMethod]
        public void DrawTheAgentsWorkflowWithLinksToCardsAndArtifacts()
        {
            File.WriteAllText(Path.Combine(_projectRoot, "docs", "gara.workflow.json"), SmallWorkflow);
            WriteSmallCards();

            var transformed = Transform("```plantuml(@workflow, ./gara.workflow.json)\ntitle Il giro\n```");

            AssertContains(transformed, "```plantuml\n@startuml\ntitle Il giro\n");
            AssertContains(transformed, "**Cerca i bandi**");
            AssertContains(transformed, "[[mde:.github/agents/tecnico.agent.md]]");
            AssertContains(transformed, "actor \"responsabile di tecnico\" as p_tecnica");
            AssertContains(transformed, "[[mde:altrove/legale.agent.md]]");
            Assert.IsFalse(transformed.Contains("[[mde:docs/schede/tecnica.md]]"), "un artefatto non ancora scritto non ha link");
            AssertContains(transformed, "file \"schede/tecnica.md\" as f_tecnica_0 #FFFFFF;line.dashed");
            AssertContains(transformed, "documento non ancora scritto");

            Directory.CreateDirectory(Path.Combine(_projectRoot, "docs", "schede"));
            File.WriteAllText(Path.Combine(_projectRoot, "docs", "schede", "tecnica.md"), "# scheda");
            var written = Transform("```plantuml(@workflow, ./gara.workflow.json)\n```");
            AssertContains(written, "file \"schede/tecnica.md\" as f_tecnica_0 [[mde:docs/schede/tecnica.md]]");
            Assert.IsFalse(written.Contains("non ancora scritto"), "scritto, il file si apre e la legenda non ne parla");
            AssertContains(transformed, "hexagon \"attende tutti\"");
            AssertContains(transformed, "s_tecnica -[#D93025,dashed]-> s_tecnica");
            AssertContains(transformed, "al massimo 2 volte");
            AssertContains(transformed, "s_legale --> j_sintesi : finito");
            AssertContains(transformed, "s_tecnica --> j_sintesi : approvato");
            Assert.IsFalse(transformed.Contains("[[mde:docs/ricerche/ricerca-*.md]]"), "una famiglia di file non ha un file solo da aprire");
            Assert.IsFalse(transformed.Contains("\r"), "il diagramma è uguale su ogni sistema");
            Assert.IsFalse(transformed.Contains("plantuml(@workflow"));
        }

        [TestMethod]
        public void ShowTheErrorsInsteadOfDrawingABrokenWorkflow()
        {
            WriteSmallCards();
            File.WriteAllText(Path.Combine(_projectRoot, "docs", "rotto.workflow.json"),
                SmallWorkflow.Replace("\"start\": \"auto\"", "\"strat\": \"auto\""));

            var transformed = Transform("```plantuml(@workflow, ./rotto.workflow.json)\n```");

            AssertContains(transformed, "mde-plantuml-include-error");
            AssertContains(transformed, "steps[3].strat");
            Assert.IsFalse(transformed.Contains("@startuml"), "un workflow sbagliato non si disegna: sembrerebbe giusto");
        }

        [TestMethod]
        public void ProduceAWorkflowBlockThePlantumlCommandCanRead()
        {
            WriteSmallCards();
            File.WriteAllText(Path.Combine(_projectRoot, "docs", "gara.workflow.json"), SmallWorkflow);
            var transformed = Transform("```plantuml(@workflow, ./gara.workflow.json)\n```");

            var plantumlRegex = new System.Text.RegularExpressions.Regex(
                @"```plantuml([^```]*)`{3}(?:(?={){([^{]*)}|)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            var matches = plantumlRegex.Matches(transformed);

            Assert.AreEqual(1, matches.Count);
            AssertContains(matches[0].Groups[1].Value, "@enduml");
        }

        /// <summary>Il documento delle responsabilità della gara: i ruoli degli omini.</summary>
        private sealed class GaraOwnership : MdExplorer.Features.Agents.IOwnershipSource
        {
            public System.Collections.Generic.IReadOnlyList<MdExplorer.Features.Agents.OwnershipEntry> GetActiveOwnership(string projectPath) =>
                throw new AssertFailedException("il diagramma legge tutte le righe dei ruoli, anche quelle non prese");

            public System.Collections.Generic.IReadOnlyList<MdExplorer.Features.Agents.OwnershipEntry> GetRoleRows(string projectPath) => new[]
            {
                new MdExplorer.Features.Agents.OwnershipEntry { Role = "Account manager", GitEmail = "anna@x.it", Agents = new[] { "account-manager" } },
                new MdExplorer.Features.Agents.OwnershipEntry { Role = "Responsabile tecnico", GitEmail = "marco@x.it", Agents = new[] { "responsabile-tecnico" } },
                // Un ruolo che nessuno ha ancora preso: l'omino ha comunque il suo nome.
                new MdExplorer.Features.Agents.OwnershipEntry { Role = "Responsabile legale", Agents = new[] { "responsabile-legale" } },
                new MdExplorer.Features.Agents.OwnershipEntry { Role = "Direzione", GitEmail = "dir@x.it" },
            };
        }

        /// <summary>
        /// La gara convertita alla v3 (sprint workflow v3, F0), ridotta a due schede di responsabili: ciò che il diagramma a
        /// mano mostrava (persone, documenti letti e scritti, pulsante, attesa, decisione) ora viene dal workflow e dalle schede.
        /// </summary>
        private const string GaraWorkflow = @"{
  ""mde_workflow"": 3, ""title"": ""Gara"",
  ""variables"": { ""codice"": ""il bando"", ""capitolato"": ""il documento di gara"", ""ricerca"": ""la ricerca"" },
  ""steps"": [
    { ""id"": ""ricerca"", ""agent"": ""account-manager"", ""skill"": ""cerca-bandi"", ""title"": ""Cerca i bandi"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" },
    { ""id"": ""tecnica"", ""agent"": ""responsabile-tecnico"", ""skill"": ""scheda-tecnica"", ""title"": ""Scheda tecnica"",
      ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" }, ""start"": ""ask-owner"",
      ""inputs"": { ""codice"": ""{codice}"", ""capitolato"": ""{capitolato}"", ""criterio"": ""2 — competenza"" } },
    { ""id"": ""contratto"", ""agent"": ""responsabile-legale"", ""skill"": ""scheda-contrattuale"", ""title"": ""Scheda contrattuale"",
      ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" }, ""start"": ""ask-owner"", ""approval"": { ""by"": ""Direzione"" },
      ""inputs"": { ""codice"": ""{codice}"", ""capitolato"": ""{capitolato}"", ""criterio"": ""4 — rischio contrattuale"" } },
    { ""id"": ""sintesi"", ""agent"": ""account-manager"", ""skill"": ""sintesi-gara"", ""title"": ""Sintesi per decidere"",
      ""trigger"": { ""after"": [""tecnica"", ""contratto""], ""wait"": ""all"" }, ""start"": ""auto"",
      ""inputs"": { ""codice"": ""{codice}"", ""schede"": { ""from"": [""tecnica.scheda"", ""contratto.scheda""] } } },
    { ""id"": ""decisione"", ""person"": ""Account manager"", ""title"": ""Decidi se partecipare"", ""trigger"": { ""after"": [""sintesi""] },
      ""task"": ""Leggi la sintesi sul bando {codice} e decidi."", ""inputs"": { ""sintesi"": { ""from"": ""sintesi.sintesi"" } },
      ""outcomes"": [ { ""id"": ""partecipa"", ""label"": ""Partecipiamo"" }, { ""id"": ""rinuncia"", ""label"": ""Non partecipiamo"" } ] },
    { ""id"": ""offerta"", ""agent"": ""responsabile-tecnico"", ""skill"": ""offerta"", ""title"": ""Offerta tecnica"",
      ""trigger"": { ""reply"": ""partecipa"", ""to"": ""decisione"" }, ""start"": ""auto"",
      ""inputs"": { ""codice"": ""{codice}"", ""ricerca"": { ""from"": ""ricerca.ricerca"" } } }
  ],
  ""loops"": [ { ""id"": ""rifacimento"", ""steps"": [""tecnica"", ""contratto""], ""until"": ""approved"", ""restart"": ""manual"" } ]
}";

        private void WriteGara()
        {
            void Write(string relative, string text)
            {
                var full = Path.Combine(_projectRoot, relative.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                File.WriteAllText(full, text);
            }
            Write("docs/gara.workflow.json", GaraWorkflow);
            Write("docs/registro-bandi.md", "# Registro\n");
            Write(".github/agents/account-manager.agent.md", @"---
a2a:
  name: account-manager
  skills:
    - id: cerca-bandi
      reads:
        - { path: docs/portale/bandi.md, note: i bandi del committente }
        - { path: docs/registro-bandi.md, note: i bandi già visti }
        - { path: docs/profilo.md, section: Criteri, note: i cinque criteri }
      outputs:
        - { id: ricerca, path: docs/ricerche/ricerca-*.md, note: con una domanda per ciascuno }
        - { id: capitolato, path: docs/documenti/*-capitolato.md, action: downloads, note: scaricato col programma }
      replies: [avvia-giro]
    - id: sintesi-gara
      inputs:
        - { id: codice, description: il bando }
        - { id: schede, description: le schede approvate }
      outputs:
        - { id: sintesi, path: docs/schede/sintesi.md, note: ""i cinque criteri, con la fonte"" }
        - { id: registro, path: docs/registro-bandi.md, action: updates, note: la riga del bando }
  replies:
    - id: avvia-giro
      label: Avvia il giro su {codice}
      description: Parte la valutazione del bando {codice}.
      message: avvia {codice}
      values: [codice, capitolato, ricerca]
---
");
            string Responsible(string name, string skill, string section, string file, string extra = "") => $@"---
a2a:
  name: {name}
  skills:
    - id: {skill}
      inputs:
        - {{ id: codice, description: il bando }}
        - {{ id: capitolato, description: il documento di gara }}
        - {{ id: criterio, description: il tuo criterio }}
      reads:
        - {{ path: docs/profilo.md, section: {section} }}
      outputs:
        - {{ id: scheda, path: docs/schede/{file}.md }}
{extra}---
";
            Write(".github/agents/responsabile-tecnico.agent.md", Responsible("responsabile-tecnico", "scheda-tecnica", "Tecnica", "tecnica",
                "    - id: offerta\n      inputs:\n        - { id: codice, description: il bando }\n        - { id: ricerca, description: la ricerca }\n      outputs:\n        - { id: offerta, path: docs/schede/offerta.md }\n"));
            Write(".github/agents/responsabile-legale.agent.md", Responsible("responsabile-legale", "scheda-contrattuale", "Contratti", "contrattuale"));
        }

        [TestMethod]
        public void DrawTheRolesTheDocumentsTheButtonsAndThePersonStepsOfTheGara()
        {
            WriteGara();
            var command = new FromExternalDataToPlantuml(NullLogger<FromExternalDataToPlantuml>.Instance, new GaraOwnership());
            var transformed = command.TransformInNewMDFromMD("```plantuml(@workflow, ./gara.workflow.json)\n```", RequestForDocsFolder());
            Console.WriteLine(transformed);
            Assert.IsFalse(transformed.Contains("mde-plantuml-include-error"), transformed);

            // Gli omini sono i ruoli di responsabilita.md, con che cosa fanno sul passo.
            AssertContains(transformed, "actor \"Account manager\" as p_ricerca");
            AssertContains(transformed, "p_ricerca --> s_ricerca : lancia\\ne approva");
            AssertContains(transformed, "p_tecnica --> s_tecnica : avvia\\ne approva");
            AssertContains(transformed, "p_contratto --> s_contratto : avvia\n");
            AssertContains(transformed, "actor \"Direzione\" as pa_contratto");
            AssertContains(transformed, "pa_contratto --> s_contratto : approva");
            AssertContains(transformed, "p_sintesi --> s_sintesi : approva\n");

            // In verde i documenti letti, dalla cartella del workflow, con il capitolo e la nota; in lilla quelli consegnati, col verbo.
            AssertContains(transformed, "file \"portale/bandi.md\\n<size:10><i>i bandi del committente</i></size>\" <<input>> as r_ricerca_0 [[mde:docs/portale/bandi.md]]");
            AssertContains(transformed, "file \"profilo.md · Tecnica\" <<input>> as r_tecnica_0");
            AssertContains(transformed, "r_ricerca_2 ..> s_ricerca : legge");
            AssertContains(transformed, "s_ricerca ..> f_ricerca_1 : scarica");
            AssertContains(transformed, "s_sintesi ..> f_sintesi_1 : aggiorna");
            AssertContains(transformed, "file \"registro-bandi.md\\n<size:10><i>la riga del bando</i></size>\" as f_sintesi_1 [[mde:docs/registro-bandi.md]]");

            // Il pulsante con la sua etichetta e chi lo preme; l'incarico sulla freccia.
            AssertContains(transformed, "hexagon \"pulsante «Avvia il giro su {codice}»\\n<size:10>lo preme Account manager</size>\" as b_ricerca_avvia_giro");
            AssertContains(transformed, "s_ricerca --> b_ricerca_avvia_giro");
            AssertContains(transformed, "b_ricerca_avvia_giro --> s_tecnica : incarico: codice, capitolato, criterio «2 — competenza»");

            // Le schede ricevute con «from» portano la freccia dal file approvato.
            AssertContains(transformed, "f_tecnica_0 --> j_sintesi : approvato");
            AssertContains(transformed, "j_sintesi --> s_sintesi : incarico: codice");
            AssertContains(transformed, "f_sintesi_0 --> s_decisione : approvato");

            // Il passo della persona, i suoi esiti, e ciò che parte da un esito.
            AssertContains(transformed, "rectangle \"**Decidi se partecipare**\\n<size:11><&person> Account manager</size>");
            AssertContains(transformed, "hexagon \"esito «Non partecipiamo»\" as o_decisione_rinuncia");
            AssertContains(transformed, "o_decisione_partecipa --> s_offerta : incarico: codice");
            AssertContains(transformed, "f_ricerca_0 ..> s_offerta : riceve");
            AssertContains(transformed, "tocca a una persona: lo chiude con un esito");
        }

        [TestMethod]
        public void TakeTheRolesInEveryVariantTheFactoryBuilds()
        {
            // La fabbrica dei comandi passa i servizi per NOME di parametro del primo costruttore: una variante (pagina, PDF,
            // markdown) senza «ownership» disegnerebbe gli omini senza ruolo, in silenzio (visto nella prova di F8).
            var variants = typeof(FromExternalDataToPlantuml).Assembly.GetTypes()
                .Where(t => typeof(FromExternalDataToPlantuml).IsAssignableFrom(t) && !t.IsAbstract).ToList();
            Assert.IsTrue(variants.Count >= 3, "pagina, PDF e markdown");
            foreach (var t in variants)
                Assert.IsTrue(t.GetConstructors()[0].GetParameters().Any(p => p.Name == "ownership"), t.Name + " non riceve i ruoli");
        }

        [TestMethod]
        public void DrawAForLoopAsAThickBlueArrowWithItsRounds()
        {
            var forLoop = SmallWorkflow.Replace("\"until\": \"approved\", \"max\": 2, \"restart\": \"manual\"", "\"times\": 3");
            Assert.AreNotEqual(SmallWorkflow, forLoop, "la sostituzione del test non trova il ciclo");
            File.WriteAllText(Path.Combine(_projectRoot, "docs", "for.workflow.json"), forLoop);
            WriteSmallCards();

            var transformed = Transform("```plantuml(@workflow, ./for.workflow.json)\n```");

            AssertContains(transformed, "s_tecnica -[#1A73E8,bold]-> s_tecnica : ×3");
            AssertContains(transformed, "3 giri, ciascuno approvato");
        }
    }
}
