using System.Linq;
using MdExplorer.Features.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Templates
{
    /// <summary>Il CHANGELOG di un template, letto con tolleranza zero (P0/P3 del piano dei template).</summary>
    [TestClass]
    public class TemplateChangelog_Should
    {
        private const string Good = "# CHANGELOG\n\n## 2.0.0 — 2026-12-01\n" +
                                    "⚠️ incompatibile: lo stato «in valutazione» si chiama «aperta». Cosa fare: rinomina le righe del registro.\n" +
                                    "- Nuovo modello del verbale.\n\n## 1.2.0 — 2026-11-03\n- La sintesi cita la sezione.\n\n## 1.0.0 — 2026-10-18\n- Prima.\n";

        [TestMethod]
        public void Read_the_entries_and_give_those_after_the_projects_version()
        {
            var entries = TemplateChangelog.Read(Good, "2.0.0", out var problems);

            Assert.AreEqual(0, problems.Count, string.Join("\n", problems));
            CollectionAssert.AreEqual(new[] { "2.0.0", "1.2.0", "1.0.0" }, entries.Select(e => e.Version).ToList());
            Assert.AreEqual(1, entries[0].Incompatible.Count);
            Assert.AreEqual("2026-11-03", entries[1].Date);
            CollectionAssert.AreEqual(new[] { "2.0.0", "1.2.0" }, TemplateChangelog.Between(entries, "1.0.0", "2.0.0").Select(e => e.Version).ToList());
            Assert.AreEqual(3, TemplateChangelog.Between(entries, "0.0.0", "2.0.0").Count);
        }

        [TestMethod]
        public void Say_every_way_a_changelog_can_be_wrong()
        {
            string Problems(string text, string version) { TemplateChangelog.Read(text, version, out var p); return string.Join("\n", p); }

            StringAssert.Contains(Problems(Good, "2.1.0"), "the first entry of the CHANGELOG.md is 2.0.0, but the manifest says 2.1.0".Replace("the first", "The first"));
            StringAssert.Contains(Problems(Good.Replace("⚠️ incompatibile: lo stato «in valutazione» si chiama «aperta». Cosa fare: rinomina le righe del registro.\n", ""), "2.0.0"),
                "2.0.0 is a major version");
            StringAssert.Contains(Problems(Good.Replace("- La sintesi cita la sezione.", "⚠️ incompatible: x. What to do: y."), "2.0.0"),
                "1.2.0 has an «⚠️ incompatible» line but is not a major version");
            StringAssert.Contains(Problems(Good.Replace(" Cosa fare: rinomina le righe del registro.", ""), "2.0.0"), "does not say what to do");
            StringAssert.Contains(Problems(Good.Replace("## 1.2.0 — 2026-11-03", "## 1.2 - 3 novembre"), "2.0.0"), "Invalid entry title");
            StringAssert.Contains(Problems("# CHANGELOG\n\n## 1.0.0 — 2026-10-18\n\n## 1.1.0 — 2026-10-19\n", "1.0.0"), "go from the most recent");
            StringAssert.Contains(Problems("", "1.0.0"), "is empty");
        }
    }
}
