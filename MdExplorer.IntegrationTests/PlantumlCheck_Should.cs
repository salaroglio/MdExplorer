using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Diagrams;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La verifica di un diagramma serve a un modello piccolo: deve dirgli COSA correggere e
    /// DOVE, con una sola azione da compiere. Questi test inchiodano le tre cose che rendono la
    /// risposta usabile invece che generica — la riga giusta, il significato, e la distinzione
    /// fra "diagramma sbagliato" e "verifica non eseguita".
    /// <para>Sprint: docs-internal/Sprints/2026-09-01-Plantuml-Check-Api-Mcp.md.</para>
    /// </summary>
    [TestClass]
    public class PlantumlCheck_Should
    {
        private static PlantumlCheckReport AnalyzeValid(string source)
            => PlantumlCheckAnalyzer.Analyze(source, PlantumlCheckOutcome.FromProcess(0, string.Empty));

        private static PlantumlProblem First(PlantumlCheckReport r, string severity)
            => r.Problems.FirstOrDefault(p => p.Severity == severity);

        [TestMethod]
        public void Catch_the_backtick_that_the_jar_considers_perfectly_valid()
        {
            // Il caso peggiore: per PlantUML il diagramma e' impeccabile, ma MdExplorer chiude il
            // blocco al primo backtick e non mostra niente, senza nessun messaggio.
            var src = "@startuml\nAlice -> Bob : chiama `Run`\n@enduml";

            var r = AnalyzeValid(src);

            Assert.IsFalse(r.Ok, "un diagramma che non verra' mostrato non e' ok");
            var p = First(r, "error");
            Assert.AreEqual(2, p.Line);
            StringAssert.Contains(p.Source, "Run");
            StringAssert.Contains(p.Meaning, "troncato");
        }

        [TestMethod]
        public void Point_at_the_line_that_opens_the_brace_not_at_the_first_line()
        {
            // Il jar su questo caso indica riga 1, cioe' @startuml, che non aiuta a correggere.
            var src = "@startuml\nclass A {\n  +x\n@enduml";

            var r = AnalyzeValid(src);

            var p = First(r, "error");
            Assert.AreEqual(2, p.Line, "la riga utile e' quella dove la graffa si apre");
            StringAssert.Contains(p.Source, "class A");
            StringAssert.Contains(p.Fix, "}");
        }

        [TestMethod]
        public void Catch_an_if_that_is_never_closed()
        {
            var src = "@startuml\nstart\nif (x?) then (si)\n  :fai;\n@enduml";

            var p = First(AnalyzeValid(src), "error");

            Assert.AreEqual(3, p.Line);
            StringAssert.Contains(p.Fix, "endif");
        }

        [TestMethod]
        public void Warn_about_a_pure_white_fill_without_calling_the_diagram_broken()
        {
            var src = "@startuml\nskinparam ClassBackgroundColor #FFFFFF\nclass A\n@enduml";

            var r = AnalyzeValid(src);

            Assert.IsTrue(r.Ok, "si vede ed e' valido: e' un avviso, non un errore");
            var p = First(r, "warning");
            Assert.AreEqual(2, p.Line);
            StringAssert.Contains(p.Meaning, "#1F1F1F");
        }

        [TestMethod]
        public void Warn_when_the_meaning_rides_on_lightness_alone()
        {
            // Due grigi diversi = "questo si, questo no" affidato al chiaro/scuro, che in tema
            // scuro si ribalta e si legge al contrario.
            var src = "@startuml\nrectangle A #EEEEEE\nrectangle B #666666\n@enduml";

            var p = First(AnalyzeValid(src), "warning");

            Assert.IsNotNull(p, "due grigi diversi vanno segnalati");
            StringAssert.Contains(p.Fix, "tinta");
        }

        [TestMethod]
        public void Keep_style_hints_out_of_the_way_when_something_is_actually_broken()
        {
            // D1: un muro di consigli di stile sopra un errore sposta l'attenzione sulla cosa
            // sbagliata. Senza errori l'hint compare, con un errore sparisce.
            var pulito = "@startuml\nAlice -> Bob : ciao\n@enduml";
            Assert.IsTrue(AnalyzeValid(pulito).Problems.Any(p => p.Severity == "hint"),
                "senza tema dichiarato e senza errori, l'hint serve");

            var rotto = "@startuml\nAlice -> Bob : chiama `Run`\n@enduml";
            Assert.IsFalse(AnalyzeValid(rotto).Problems.Any(p => p.Severity == "hint"),
                "con un errore in campo, gli hint tacciono");
        }

        [TestMethod]
        public void Never_report_more_problems_than_a_reader_will_act_on()
        {
            // D2: oltre la soglia non si corregge, si ricomincia da capo.
            var righe = string.Join("\n", Enumerable.Range(0, 40).Select(i => $"rectangle R{i} #FFFFFF"));
            var src = "@startuml\n" + righe + "\n@enduml";

            Assert.AreEqual(PlantumlCheckAnalyzer.MaxProblems, AnalyzeValid(src).Problems.Count);
        }

        [TestMethod]
        public void Say_the_check_did_not_run_instead_of_blaming_the_diagram()
        {
            // Il rischio numero uno: senza java, un "non valido" generico manderebbe un modello
            // piccolo a correggere all'infinito un sorgente che non ha niente che non va.
            var outcome = PlantumlCheckOutcome.Unavailable("java non eseguibile ('java').");

            var r = PlantumlCheckAnalyzer.Analyze("@startuml\nAlice -> Bob\n@enduml", outcome);

            Assert.IsFalse(r.Ok);
            Assert.IsNotNull(r.ToolUnavailable);
            StringAssert.Contains(r.ToolUnavailable, "java");
            Assert.AreEqual(0, r.Problems.Count, "non essendo stato giudicato, non ci sono cose da correggere");
        }

        [TestMethod]
        public void Explain_the_assumed_diagram_type_when_the_jar_rejects_it()
        {
            var outcome = PlantumlCheckOutcome.FromProcess(200, "ERROR\n2\nSyntax Error? (Assumed diagram type: sequence)");

            var r = PlantumlCheckAnalyzer.Analyze("@startuml\nfoobar qwerty\n@enduml", outcome);

            Assert.IsFalse(r.Ok);
            Assert.AreEqual("sequence", r.DiagramType);
            StringAssert.Contains(First(r, "error").Meaning, "sequence");
        }

        // ────────────────────────────────────────────────────────────────
        // Endpoint reale, contro il jar del progetto.
        // ────────────────────────────────────────────────────────────────

        private static async Task<JsonElement> Check(AgentCityContext ctx, string source)
        {
            var res = await ctx.Client.PostAsync("/api/Plantuml/Check",
                new StringContent(JsonSerializer.Serialize(new { source }), Encoding.UTF8, "application/json"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode, await res.Content.ReadAsStringAsync());
            return JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement;
        }

        private static void SkipIfToolMissing(JsonElement body)
        {
            if (body.TryGetProperty("toolUnavailable", out var t) && t.ValueKind == JsonValueKind.String)
            {
                Assert.Inconclusive($"Verifica non eseguibile su questa macchina: {t.GetString()}");
            }
        }

        [TestMethod]
        public async Task Accept_a_diagram_that_is_actually_valid()
        {
            using var ctx = new AgentCityContext();

            var body = await Check(ctx, "@startuml\n!theme plain\nAlice -> Bob : ciao\n@enduml");
            SkipIfToolMissing(body);

            Assert.IsTrue(body.GetProperty("ok").GetBoolean(), body.ToString());
        }

        [TestMethod]
        public async Task Reject_a_diagram_the_jar_cannot_parse()
        {
            using var ctx = new AgentCityContext();

            var body = await Check(ctx, "@startuml\n!theme plain\nAlice -> Bob : ciao\nfoobar qwerty\n@enduml");
            SkipIfToolMissing(body);

            Assert.IsFalse(body.GetProperty("ok").GetBoolean(), body.ToString());
            var problems = body.GetProperty("problems");
            Assert.IsTrue(problems.GetArrayLength() > 0);
            Assert.AreEqual("error", problems[0].GetProperty("severity").GetString());
        }

        [TestMethod]
        public async Task Refuse_an_empty_request_instead_of_answering_ok()
        {
            using var ctx = new AgentCityContext();

            var res = await ctx.Client.PostAsync("/api/Plantuml/Check",
                new StringContent("{\"source\":\"\"}", Encoding.UTF8, "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
        }
    }
}
