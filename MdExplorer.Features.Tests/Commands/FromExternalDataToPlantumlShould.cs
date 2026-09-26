using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

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
            AssertContains(transformed, "file non trovato");
            Assert.IsFalse(transformed.Contains("@startjson"), "no diagram may be produced from a missing file");
        }

        [TestMethod]
        public void RefuseAPathOutsideTheProject()
        {
            var transformed = Transform("```plantuml(@json, ../../../etc/passwd)\n```");

            AssertContains(transformed, "mde-plantuml-include-error");
            AssertContains(transformed, "esce dalla cartella del progetto");
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
            AssertContains(transformed, "JSON valido");
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
    }
}
