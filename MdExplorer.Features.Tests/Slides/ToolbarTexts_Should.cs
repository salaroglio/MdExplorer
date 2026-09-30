using MdExplorer.Features.Services.SourceMapping;
using MdExplorer.Features.Slides;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Tests.Slides
{
    /// <summary>
    /// The words of the slide bar, the transitions panel, the corrections and the paste menu live in one place,
    /// toolbar-shared.js, in Italian and English. A word missing in one language shows as its key on the page:
    /// here it is caught before.
    /// </summary>
    [TestClass]
    public class ToolbarTexts_Should
    {
        private static string Scripts => Path.Combine(SlidePage.RepositoryRoot(), "MdExplorer", "wwwroot", "javascripts");

        private static readonly Regex Entry = new(@"^\s{8}""?(?<key>[A-Za-z0-9_.]+)""?:\s*(?<q>['""])(?<text>.*)\k<q>,?\s*$", RegexOptions.Multiline | RegexOptions.Compiled);

        private static Dictionary<string, string> Language(string source, string language)
        {
            var start = source.IndexOf($"    {language}: {{", StringComparison.Ordinal);
            Assert.IsTrue(start >= 0, $"the '{language}' block of TOOLBAR_TEXTS");
            var end = source.IndexOf("\n    }", start, StringComparison.Ordinal);
            return Entry.Matches(source.Substring(start, end - start)).Cast<Match>().ToDictionary(m => m.Groups["key"].Value, m => m.Groups["text"].Value);
        }

        private static (Dictionary<string, string> It, Dictionary<string, string> En) Texts()
        {
            var source = File.ReadAllText(Path.Combine(Scripts, "jqueryForFirstPage", "images", "toolbar-shared.js"));
            return (Language(source, "it"), Language(source, "en"));
        }

        [TestMethod]
        public void HaveTheSameKeysInItalianAndEnglish()
        {
            var (it, en) = Texts();

            CollectionAssert.AreEquivalent(en.Keys.ToList(), it.Keys.ToList(), "the keys only in one language: "
                + string.Join(", ", en.Keys.Except(it.Keys).Concat(it.Keys.Except(en.Keys))));
        }

        [TestMethod]
        public void FillTheSamePlaceholdersInBothLanguages()
        {
            var (it, en) = Texts();
            var placeholder = new Regex(@"\{(\w+)\}");

            foreach (var key in en.Keys.Where(it.ContainsKey))
            {
                var inEn = placeholder.Matches(en[key]).Cast<Match>().Select(m => m.Value).OrderBy(x => x).ToArray();
                var inIt = placeholder.Matches(it[key]).Cast<Match>().Select(m => m.Value).OrderBy(x => x).ToArray();
                CollectionAssert.AreEqual(inEn, inIt, $"'{key}': the {{placeholders}} differ between the languages");
            }
        }

        [TestMethod]
        public void NotLeaveAnEmptyText()
        {
            var (it, en) = Texts();

            Assert.IsFalse(it.Concat(en).Any(pair => pair.Value.Trim().Length == 0), "an empty text: " + string.Join(", ", it.Concat(en).Where(p => p.Value.Trim().Length == 0).Select(p => p.Key)));
        }

        [TestMethod]
        public void HaveEveryKeyTheScriptsAsk()
        {
            var (it, _) = Texts();
            var used = new Regex(@"(?:_toolbarText|\bT)\(\s*'(?<key>[A-Za-z0-9_.]+)'");
            var files = new[]
            {
                Path.Combine("slides", "slide-toolbar.js"), Path.Combine("slides", "slide-transitions.js"), Path.Combine("slides", "slide-list-drag.js"),
                Path.Combine("slides", "slide-edit.js"), Path.Combine("slides", "slide-navigation.js"),
                Path.Combine("jqueryForFirstPage", "inline-edit", "inline-edit.js"), Path.Combine("jqueryForFirstPage", "clipboard", "clipboard-paste.js"),
            };

            var missing = files
                .SelectMany(f => used.Matches(File.ReadAllText(Path.Combine(Scripts, f))).Cast<Match>().Select(m => (File: f, Key: m.Groups["key"].Value)))
                // 'tr.' + value is a key made on the spot: the six names are HaveTheSixTransitionNames.
                .Where(k => !k.Key.EndsWith(".", StringComparison.Ordinal) && !it.ContainsKey(k.Key))
                .Select(k => $"{k.File}: {k.Key}")
                .ToList();

            Assert.AreEqual(0, missing.Count, "keys asked and not written: " + string.Join("; ", missing));
        }

        [TestMethod]
        public void HaveTheSixTransitionNames()
        {
            var (it, en) = Texts();

            foreach (var value in SlideTransitionEditor.Transitions)
            {
                Assert.IsTrue(it.ContainsKey("tr." + value) && en.ContainsKey("tr." + value), $"the name of the transition '{value}'");
            }
        }

        [TestMethod]
        public void SayEveryReasonTheServerCanGive()
        {
            var (it, en) = Texts();
            var reasons = Enum.GetNames<ListItemMoveRefusal>().Select(n => "refusal.MoveListItem." + n)
                .Concat(Enum.GetNames<SlideTransitionRefusal>().Select(n => "refusal.SlideTransition." + n))
                .Concat(Enum.GetNames<RenderedTextRefusal>().Where(n => n != nameof(RenderedTextRefusal.None)).Select(n => "refusal.EditRenderedText." + n))
                .ToList();

            var missing = reasons.Where(r => !it.ContainsKey(r) || !en.ContainsKey(r)).ToList();

            Assert.AreEqual(0, missing.Count, "a refusal the page cannot say in both languages: " + string.Join(", ", missing));
        }
    }
}
