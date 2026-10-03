using MdExplorer.Features.E2e;
using MdExplorer.Features.Services;
using MdExplorer.Features.Yaml.Interfaces;
using MdExplorer.Features.Yaml.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MdExplorer.Features.Tests.E2e
{
    [TestClass]
    public class E2eRunSettingsResolver_Should
    {
        private string _root;
        private string _sub;
        private string _test;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "e2e-settings-" + Guid.NewGuid().ToString("N"));
            _sub = Path.Combine(_root, "test-e2e", "login");
            Directory.CreateDirectory(_sub);
            _test = Path.Combine(_sub, "login.e2e.md");
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_root, true);

        private static void Write(string path, string run) =>
            File.WriteAllText(path, "---\ntitle: x\ne2e:\n  run:\n" + run + "---\n# x\n");

        [TestMethod]
        public void Use_the_defaults_when_nobody_says_anything()
        {
            File.WriteAllText(_test, "---\ne2e:\n  baseUrl: https://example.org\n---\n");
            var settings = E2eRunSettingsResolver.Resolve(_test, _root);

            Assert.AreEqual(new E2eTextSetting(null, null), settings.Engine, "the project's engine (D10)");
            Assert.AreEqual(new E2eTextSetting(null, null), settings.Model);
            Assert.AreEqual(new E2eSetting(false, null), settings.CommitAfterRun);
            Assert.AreEqual(new E2eSetting(true, null), settings.Headless);
        }

        [TestMethod]
        public void Let_the_particular_win_key_by_key_up_to_the_project_root()
        {
            var rootSettings = E2eRunSettingsResolver.FolderSettingsPath(_root);
            var middleSettings = E2eRunSettingsResolver.FolderSettingsPath(Path.Combine(_root, "test-e2e"));
            Write(rootSettings, "    engine: copilot\n    model: gpt-5\n    commitAfterRun: true\n    headless: false\n");
            Write(middleSettings, "    commitAfterRun: false\n");
            Write(_test, "    engine: claude\n    headless: true\n");

            var settings = E2eRunSettingsResolver.Resolve(_test, _root);

            Assert.AreEqual(new E2eTextSetting("claude", _test), settings.Engine, "the test wins over every folder");
            Assert.AreEqual(new E2eTextSetting(null, _test), settings.Model, "engine and model travel together: gpt-5 is Copilot's, not Claude's");
            Assert.AreEqual(new E2eSetting(false, middleSettings), settings.CommitAfterRun, "the nearer folder wins over the root");
            Assert.AreEqual(new E2eSetting(true, _test), settings.Headless, "the test wins over every folder");
        }

        [TestMethod]
        public void Resolve_a_folder_from_its_own_settings_upwards()
        {
            var middle = Path.Combine(_root, "test-e2e");
            Write(E2eRunSettingsResolver.FolderSettingsPath(_root), "    headless: false\n");
            Write(E2eRunSettingsResolver.FolderSettingsPath(_sub), "    headless: true\n");

            Assert.AreEqual(false, E2eRunSettingsResolver.Resolve(middle, _root).Headless.Value, "the folder below does not count");
            Assert.AreEqual(true, E2eRunSettingsResolver.Resolve(_sub, _root).Headless.Value);
        }

        [TestMethod]
        public void Not_look_above_the_project_root()
        {
            var project = Path.Combine(_root, "test-e2e");
            Write(E2eRunSettingsResolver.FolderSettingsPath(_root), "    headless: false\n");
            File.WriteAllText(_test, "---\ne2e:\n  baseUrl: https://example.org\n---\n");

            Assert.AreEqual(new E2eSetting(true, null), E2eRunSettingsResolver.Resolve(_test, project).Headless);
            Assert.ThrowsException<ArgumentException>(() => E2eRunSettingsResolver.Resolve(_test, Path.Combine(_root, "altro")));
        }

        [TestMethod]
        public async Task Keep_the_folder_settings_when_the_folder_summary_is_regenerated()
        {
            var folder = Path.Combine(_root, "test-e2e");
            var toc = E2eRunSettingsResolver.FolderSettingsPath(folder);
            var service = new TocGenerationService(NullLogger<TocGenerationService>.Instance, new FixedYaml(), null);

            Assert.IsTrue(await service.GenerateTocAsync(folder, toc));
            File.WriteAllText(toc, E2eFrontMatter.WriteRunSettings(File.ReadAllText(toc), new E2eRunSettings(null, null, true, null), "toc"));

            Assert.IsTrue(await service.GenerateTocAsync(folder, toc));
            Assert.AreEqual(new E2eSetting(true, toc), E2eRunSettingsResolver.Resolve(folder, _root).CommitAfterRun);
            StringAssert.Contains(File.ReadAllText(toc), "generated: fresh");
        }

        private sealed class FixedYaml : IYamlDefaultGenerator
        {
            public string GenerateDefaultYaml(string projectPath = null) => "---\r\ngenerated: fresh\r\n---\r\n";
            public MdExplorerDocumentDescriptor GenerateDefaultDescriptor(string projectPath = null) => new();
        }
    }
}
