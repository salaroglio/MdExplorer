using MdExplorer.Features.E2e;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Tests.E2e
{
    [TestClass]
    public class E2eSupportFiles_Should
    {
        private string _root;

        private static readonly IReadOnlyDictionary<string, string> Contents = new Dictionary<string, string>
        {
            ["E2eTests.csproj"] = "<Project><!-- MdExplorer, skill mde-e2e: progetto --></Project>\n",
            ["E2eSupport.cs"] = "// MdExplorer, skill mde-e2e: supporto\n// versione-supporto: 3\n",
            ["e2e.runsettings"] = "<RunSettings><!-- MdExplorer, skill mde-e2e: browser --></RunSettings>\n",
        };

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "e2e-support-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, "test-e2e", "admin"));
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_root, true);

        private string Test(string relative)
        {
            var path = Path.Combine(_root, relative);
            File.WriteAllText(path, "---\n---\n");
            return path;
        }

        private string Read(string relative) => File.ReadAllText(Path.Combine(_root, relative));

        [TestMethod]
        public void Create_the_three_files_next_to_a_test_that_has_none()
        {
            var update = E2eSupportFiles.Ensure(new[] { Test("test-e2e/login.e2e.md") }, _root, Contents, create: true);

            CollectionAssert.AreEquivalent(new[] { "test-e2e/E2eTests.csproj", "test-e2e/E2eSupport.cs", "test-e2e/e2e.runsettings" }, update.Created);
            Assert.AreEqual(Contents["E2eSupport.cs"], Read("test-e2e/E2eSupport.cs"));
        }

        [TestMethod]
        public void Create_nothing_when_asked_only_to_update()
        {
            // A replay without a tests project has no scripts to compile: nothing to write.
            var update = E2eSupportFiles.Ensure(new[] { Test("test-e2e/login.e2e.md") }, _root, Contents, create: false);

            Assert.AreEqual(0, update.Created.Count);
            Assert.IsFalse(File.Exists(Path.Combine(_root, "test-e2e", "E2eTests.csproj")));
        }

        [TestMethod]
        public void Serve_a_nested_test_from_the_project_above_it()
        {
            // Parent and child in one launch, child listed first: one project only, in the parent folder.
            var update = E2eSupportFiles.Ensure(new[] { Test("test-e2e/admin/utenti.e2e.md"), Test("test-e2e/login.e2e.md") }, _root, Contents, create: true);

            Assert.AreEqual(3, update.Created.Count, string.Join(", ", update.Created));
            Assert.IsFalse(File.Exists(Path.Combine(_root, "test-e2e", "admin", "E2eTests.csproj")), "two projects would compile the scripts twice");
        }

        [TestMethod]
        public void Leave_an_up_to_date_file_untouched()
        {
            // A csproj rewritten for nothing looks newer than its packages and asks for a new download (D30).
            var test = Test("test-e2e/login.e2e.md");
            E2eSupportFiles.Ensure(new[] { test }, _root, Contents, create: true);
            var csproj = Path.Combine(_root, "test-e2e", "E2eTests.csproj");
            File.WriteAllText(csproj, Contents["E2eTests.csproj"].Replace("\n", "\r\n"));
            var before = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            File.SetLastWriteTimeUtc(csproj, before);

            var update = E2eSupportFiles.Ensure(new[] { test }, _root, Contents, create: true);

            Assert.AreEqual(0, update.Created.Count + update.Updated.Count);
            Assert.AreEqual(before, File.GetLastWriteTimeUtc(csproj), "CRLF line endings are not a difference");
        }

        [TestMethod]
        public void Update_a_file_an_older_MdExplorer_wrote()
        {
            var test = Test("test-e2e/login.e2e.md");
            E2eSupportFiles.Ensure(new[] { test }, _root, Contents, create: true);
            File.WriteAllText(Path.Combine(_root, "test-e2e", "E2eSupport.cs"), "// MdExplorer, skill mde-e2e: supporto\n// versione-supporto: 2\n");

            var update = E2eSupportFiles.Ensure(new[] { test }, _root, Contents, create: false);

            CollectionAssert.AreEqual(new[] { "test-e2e/E2eSupport.cs" }, update.Updated);
            Assert.AreEqual(Contents["E2eSupport.cs"], Read("test-e2e/E2eSupport.cs"));
        }

        [TestMethod]
        public void Leave_alone_a_file_without_the_marker()
        {
            var test = Test("test-e2e/login.e2e.md");
            E2eSupportFiles.Ensure(new[] { test }, _root, Contents, create: true);
            File.WriteAllText(Path.Combine(_root, "test-e2e", "e2e.runsettings"), "<RunSettings>mio</RunSettings>\n");

            var update = E2eSupportFiles.Ensure(new[] { test }, _root, Contents, create: true);

            CollectionAssert.AreEqual(new[] { "test-e2e/e2e.runsettings" }, update.Customized);
            Assert.AreEqual("<RunSettings>mio</RunSettings>\n", Read("test-e2e/e2e.runsettings"));
        }

        [TestMethod]
        public void Update_the_runsettings_an_agent_copied_from_an_older_skill()
        {
            // Up to mde-e2e v4 the agent wrote e2e.runsettings from the skill, without a marker (seen on 29/09/2026).
            var test = Test("test-e2e/login.e2e.md");
            E2eSupportFiles.Ensure(new[] { test }, _root, Contents, create: true);
            File.WriteAllText(Path.Combine(_root, "test-e2e", "e2e.runsettings"),
                "<RunSettings>\r\n  <Playwright>\r\n    <BrowserName>chromium</BrowserName>\r\n    <LaunchOptions>\r\n      <Channel>msedge</Channel>\r\n" +
                "      <Headless>true</Headless>\r\n    </LaunchOptions>\r\n  </Playwright>\r\n</RunSettings>\r\n");

            var update = E2eSupportFiles.Ensure(new[] { test }, _root, Contents, create: true);

            CollectionAssert.AreEqual(new[] { "test-e2e/e2e.runsettings" }, update.Updated);
            Assert.AreEqual(0, update.Customized.Count);
        }

        [TestMethod]
        public void Refuse_a_skill_without_one_of_the_files()
        {
            Assert.ThrowsException<InvalidOperationException>(() => E2eSupportFiles.FromSkill("# niente esempi\n"));
        }
    }
}
