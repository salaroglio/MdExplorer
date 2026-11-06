using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MdExplorer.Features.Tests.Slides;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Le parole della pagina del grafo del codice stanno in CG_TEXTS (code-graph-view.js), in italiano e in inglese. Un testo
    /// che manca in una lingua si vedrebbe come la sua chiave: qui si prende prima.
    /// </summary>
    [TestClass]
    public class CodeGraphViewTextsShould
    {
        private static string Script => Path.Combine(SlidePage.RepositoryRoot(), "MdExplorer", "wwwroot", "javascripts", "codegraph", "code-graph-view.js");

        private static readonly Regex Entry = new(@"^\s{8}'?(?<key>[A-Za-z0-9_.]+)'?:\s*'(?<text>(?:[^'\\]|\\.)*)',?\s*$", RegexOptions.Multiline | RegexOptions.Compiled);

        private static Dictionary<string, string> Language(string source, string language)
        {
            var start = source.IndexOf($"    {language}: {{", StringComparison.Ordinal);
            Assert.IsTrue(start >= 0, $"il blocco '{language}' di CG_TEXTS");
            var end = source.IndexOf("\n    }", start, StringComparison.Ordinal);
            return Entry.Matches(source.Substring(start, end - start)).Cast<Match>().ToDictionary(m => m.Groups["key"].Value, m => m.Groups["text"].Value);
        }

        private static (Dictionary<string, string> It, Dictionary<string, string> En, string Source) Texts()
        {
            var source = File.ReadAllText(Script);
            return (Language(source, "it"), Language(source, "en"), source);
        }

        [TestMethod]
        public void Have_the_same_keys_and_placeholders_in_Italian_and_English()
        {
            var (it, en, _) = Texts();
            Assert.IsTrue(en.Count > 50, "le voci lette: " + en.Count);
            CollectionAssert.AreEquivalent(en.Keys.ToList(), it.Keys.ToList(), "chiavi in una lingua sola: " + string.Join(", ", en.Keys.Except(it.Keys).Concat(it.Keys.Except(en.Keys))));
            var placeholder = new Regex(@"\{(\w+)\}");
            foreach (var key in en.Keys)
            {
                var inEn = placeholder.Matches(en[key]).Select(m => m.Value).OrderBy(x => x).ToArray();
                var inIt = placeholder.Matches(it[key]).Select(m => m.Value).OrderBy(x => x).ToArray();
                CollectionAssert.AreEqual(inEn, inIt, $"'{key}': i {{segnaposto}} sono diversi fra le lingue");
                Assert.IsFalse(string.IsNullOrWhiteSpace(en[key]) || string.IsNullOrWhiteSpace(it[key]), $"'{key}' vuoto");
            }
        }

        [TestMethod]
        public void Have_every_key_the_page_asks()
        {
            var (it, _, source) = Texts();
            var body = source.Substring(source.IndexOf("(function () {", StringComparison.Ordinal));
            var asked = Regex.Matches(body, @"\bT\(\s*'(?<key>[A-Za-z0-9_.]+)'").Select(m => m.Groups["key"].Value)
                // chiavi scelte con un ternario, e quelle fatte sul momento ('rel.' + relazione, 'conf.' + confidenza, 'ext.' + relazione)
                .Concat(new[] { "outdatedOne", "outdatedMany", "upgradedOne", "upgradedMany" })
                .Concat(new[] { "calls", "imports", "references", "instantiates", "inherits", "implements" }.SelectMany(r => new[] { "rel." + r, "ext." + r }))
                .Concat(new[] { "extracted", "inferred", "ambiguous" }.Select(c => "conf." + c))
                .Distinct().ToList();
            var missing = asked.Where(k => !k.EndsWith(".", StringComparison.Ordinal) && !it.ContainsKey(k)).ToList();
            Assert.AreEqual(0, missing.Count, "chiavi chieste e non scritte: " + string.Join(", ", missing));
        }
    }
}
