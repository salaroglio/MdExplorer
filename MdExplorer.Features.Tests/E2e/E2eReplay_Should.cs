using MdExplorer.Features.E2e;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Tests.E2e
{
    [TestClass]
    public class E2eReplay_Should
    {
        private const string Test =
            "---\ne2e:\n  baseUrl: https://example.org\n---\n## T1 — Uno\n1. Apri `/a`\n\n## Artefatti\n\n## Esiti\n\n" +
            "| Data | Test | Esito | Dettagli |\n|---|---|---|---|\n| 2026-09-27 10:00 | T1 | ✅ superato | [report](x) |\n";

        [TestMethod]
        public void Put_the_new_rows_on_top_of_the_results_table_and_touch_nothing_else()
        {
            var written = E2eResultsTable.AddRows(Test, new[]
            {
                new E2eResultRow("2026-09-27 11:00", 2, "❌ fallito (script)", "atteso \"a|b\" — [screenshot](y/)"),
                new E2eResultRow("2026-09-27 11:00", 1, "✅ superato (script)", "[screenshot](y/)"),
            });

            Assert.AreEqual(Test.Replace("|---|---|---|---|\n",
                "|---|---|---|---|\n| 2026-09-27 11:00 | T2 | ❌ fallito (script) | atteso \"a\\|b\" — [screenshot](y/) |\n| 2026-09-27 11:00 | T1 | ✅ superato (script) | [screenshot](y/) |\n"), written);
        }

        [TestMethod]
        public void Create_the_table_when_the_section_is_still_empty()
        {
            var written = E2eResultsTable.AddRows("## T1 — Uno\n1. a\n\n## Esiti\n", new[] { new E2eResultRow("d", 1, "✅", "x") });
            StringAssert.Contains(written, "## Esiti\n\n| Data | Test | Esito | Dettagli |\n|---|---|---|---|\n| d | T1 | ✅ | x |");
        }

        [TestMethod]
        public void Read_the_outcome_of_each_script_from_the_trx()
        {
            var trx = Path.GetTempFileName();
            File.WriteAllText(trx, """
                <?xml version="1.0" encoding="utf-8"?>
                <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
                  <Results>
                    <UnitTestResult testId="a" testName="T1_Uno" outcome="Passed" />
                    <UnitTestResult testId="b" testName="T2_Due" outcome="Failed">
                      <Output><ErrorInfo><Message>Locator expected to be visible
                dettagli</Message></ErrorInfo></Output>
                    </UnitTestResult>
                    <UnitTestResult testId="c" testName="Altro" outcome="Passed" />
                  </Results>
                  <TestDefinitions>
                    <UnitTest id="a"><TestMethod className="MdeE2e.Login.Test_T1" name="T1_Uno" /></UnitTest>
                    <UnitTest id="b"><TestMethod className="MdeE2e.Login.Test_T2" name="T2_Due" /></UnitTest>
                    <UnitTest id="c"><TestMethod className="Altro.Progetto.Test" name="Altro" /></UnitTest>
                  </TestDefinitions>
                </TestRun>
                """);
            var classes = new Dictionary<string, E2eScript>
            {
                ["MdeE2e.Login.Test_T1"] = new E2eScript(1, "/x/login.T1.spec.cs", "valido", "f", "g", "f"),
                ["MdeE2e.Login.Test_T2"] = new E2eScript(2, "/x/login.T2.spec.cs", "valido", "f", "g", "f"),
            };

            var outcomes = E2eReplay.ReadTrx(trx, classes).OrderBy(o => o.Test).ToList();
            File.Delete(trx);

            Assert.AreEqual(2, outcomes.Count, "a test of another project is not ours");
            Assert.IsTrue(outcomes[0].Passed);
            Assert.IsFalse(outcomes[1].Passed);
            StringAssert.StartsWith(outcomes[1].Message, "Locator expected to be visible");
            Assert.AreEqual("login.T2.spec.cs", outcomes[1].Script);
        }

        [TestMethod]
        public void Refuse_to_replay_before_the_user_agrees_to_download_the_packages()
        {
            // D3/D7: `dotnet test` would download ~200 MB from NuGet on its own. Without a restore for the current
            // csproj the replay stops and says so; `dotnet` is never started (the path given here does not exist).
            var root = Path.Combine(Path.GetTempPath(), "e2e-replay-" + Guid.NewGuid().ToString("N"));
            try
            {
                var folder = Path.Combine(root, "test-e2e");
                Directory.CreateDirectory(Path.Combine(folder, "login.e2e", "scripts"));
                var test = Path.Combine(folder, "login.e2e.md");
                File.WriteAllText(test, Test);
                var project = Path.Combine(folder, "E2eTests.csproj");
                File.WriteAllText(project, "<Project Sdk=\"Microsoft.NET.Sdk\" />\n");
                var item = E2eRunPlanner.Plan(test, root, new DateTime(2026, 9, 27, 10, 30, 0)).Items.Single();

                Assert.AreEqual(project, E2eReplay.FindProject(item, root));
                Assert.IsFalse(E2eReplay.PackagesRestored(project));
                var result = E2eReplay.RunAsync(item, root, "chrome", Path.Combine(root, "no-dotnet"), DateTime.Now,
                    new Dictionary<string, string>(), null, default).GetAwaiter().GetResult();
                Assert.IsTrue(result.NeedsRestore);
                Assert.AreEqual(E2eReplay.PackagesMissing, result.Problem);

                var assets = Path.Combine(folder, "obj", "project.assets.json");
                Directory.CreateDirectory(Path.GetDirectoryName(assets)!);
                File.WriteAllText(assets, "{}");
                Assert.IsTrue(E2eReplay.PackagesRestored(project));

                // A failed restore (no network, unreachable source) writes the file anyway, with the error in its logs.
                File.WriteAllText(assets, "{\"version\":3,\"libraries\":{},\"logs\":[{\"code\":\"NU1301\",\"level\":\"Error\",\"message\":\"Unable to load the service index\"}]}");
                Assert.IsFalse(E2eReplay.PackagesRestored(project), "a failed restore is not a restore");
                File.WriteAllText(assets, "{\"version\":3,\"libraries\":{},\"logs\":[{\"code\":\"NU1803\",\"level\":\"Warning\"}]}");
                Assert.IsTrue(E2eReplay.PackagesRestored(project), "a warning does not stop the replay");

                // The agent changes the csproj (a package added): a new restore is needed, so a new consent.
                File.SetLastWriteTimeUtc(project, File.GetLastWriteTimeUtc(assets).AddMinutes(1));
                Assert.IsFalse(E2eReplay.PackagesRestored(project));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }
    }
}
