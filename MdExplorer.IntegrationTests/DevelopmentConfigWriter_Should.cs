using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MdExplorer.Features.Configuration;
using MdExplorer.Service.Models;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// F3 dello sprint «Schema di .development.yml»: la scrittura per gruppo. Si riscrive solo il gruppo che cambia, con tutte le sue
    /// voci (D6); i gruppi uguali e quelli spenti restano byte per byte; il nuovo vince sul vecchio (D5).
    /// </summary>
    [TestClass]
    public class DevelopmentConfigWriter_Should
    {
        /// <summary>Un file scritto con lo schema 1 (voci vuote omesse), come quelli di oggi.</summary>
        private const string Old =
            "folders:\n- path: docs\n  tags:\n  - analysis\n- path: specs\n  tags: []\n" +
            "project:\n  description: Il progetto\n  participants: []\n" +
            "harness:\n  target: copilot\n";

        private static DevelopmentConfig Read(string text) => DevelopmentConfigFile.Read(Write(text)).Config;

        private static string Write(string text)
        {
            var file = Path.Combine(Path.GetTempPath(), "writer-" + Guid.NewGuid().ToString("N") + ".yml");
            File.WriteAllText(file, text);
            return file;
        }

        private static string Block(string text, string key)
        {
            var lines = text.Split('\n');
            var start = Array.FindIndex(lines, l => l.StartsWith(key + ":"));
            if (start < 0) return null;
            var end = Array.FindIndex(lines, start + 1, l => l.Length > 0 && !l.StartsWith(" ") && !l.StartsWith("-") && !l.StartsWith("#"));
            return string.Join("\n", lines.Skip(start).Take((end < 0 ? lines.Length : end) - start)).TrimEnd('\n');
        }

        [TestMethod]
        public void Write_a_new_file_with_the_number_and_every_entry_of_the_groups_present()
        {
            var config = new DevelopmentConfig { Harness = new HarnessConfig { Target = "claude" } };
            config.Folders.Add(new DevelopmentFolder { Path = "docs" });

            var text = DevelopmentConfigWriter.Compose(null, config);

            StringAssert.StartsWith(text, $"# mde-schema: {MdExplorer.Features.Configuration.DevelopmentSchemaSnapshots.Current}\n");
            StringAssert.Contains(text, "harness:\n  target: claude\n  model:\n", "voce non compilata: scritta, senza valore");
            StringAssert.Contains(text, "  description:\n");
            StringAssert.Contains(text, "  knowledgeGraph:\n");
            Assert.IsFalse(text.Contains("redmine:"), "i gruppi assenti non si scrivono");
            Assert.IsFalse(text.Contains("atlassian:"));
            Assert.IsFalse(text.Contains("''"), text);

            var check = DevelopmentSchemaCheck.Run(text);
            Assert.IsTrue(check.AllGood, check.Describe((it, en) => it));
            var back = DevelopmentConfigFile.Read(Write(text)).Config;
            Assert.AreEqual("claude", back.Harness.Target);
            Assert.IsNull(back.Harness.Model);
            Assert.IsNotNull(back.Folders.Single().Tags, "la lista non compilata torna lista vuota");
            Assert.AreEqual(text, DevelopmentConfigWriter.Compose(text, back), "riletto e riscritto: identico");
        }

        [TestMethod]
        public void Rewrite_only_the_group_that_changes()
        {
            var config = Read(Old);
            config.Harness.Target = "claude";

            var text = DevelopmentConfigWriter.Compose(Old, config);

            Assert.AreEqual(Block(Old, "folders"), Block(text, "folders"), "uguale: byte per byte, anche senza le voci vuote dello schema 1");
            Assert.AreEqual(Block(Old, "project"), Block(text, "project"));
            Assert.AreEqual("harness:\n  target: claude\n  model:", Block(text, "harness"), "il gruppo che cambia si completa (D6)");
            StringAssert.StartsWith(text, $"# mde-schema: {MdExplorer.Features.Configuration.DevelopmentSchemaSnapshots.Current}\n");
        }

        [TestMethod]
        public void Leave_the_file_alone_when_nothing_changes()
        {
            Assert.AreSame(Old, DevelopmentConfigWriter.Compose(Old, Read(Old)), "nemmeno il numero in testa");
            var withNumber = "# mde-schema: 2\n" + Old;
            Assert.AreSame(withNumber, DevelopmentConfigWriter.Compose(withNumber, Read(withNumber)));
        }

        [TestMethod]
        public void Keep_a_switched_off_group_and_an_unknown_one_byte_for_byte()
        {
            var off = "agentCity:\n  enabled: true\n  roomSecret: S\n  effort: high\n";
            var unknown = "jenkins:\n  url: https://ci\n";
            var original = Old + off + unknown;
            var config = Read(original);
            config.Harness.Target = "claude";

            var text = DevelopmentConfigWriter.Compose(original, config);

            StringAssert.Contains(text, off);
            StringAssert.Contains(text, unknown);
            StringAssert.Contains(text, "target: claude");
        }

        [TestMethod]
        public void Put_a_switched_off_item_back_in_its_place()
        {
            var original = Old.Replace("- path: specs\n  tags: []\n", "- path: specs\n  tags: []\n  mirror: true\n") + "codeRoots:\n- alias: a\n  repository: r\n- alias: b\n  repository: r\n  owner: x\n- alias: c\n  repository: r\n";
            var config = Read(original);
            Assert.AreEqual(1, config.Folders.Count);
            config.Folders.Single().Tags.Add("program");
            config.CodeRoots.Add(new CodeRootConfig { Alias = "d", Repository = "r" });

            var text = DevelopmentConfigWriter.Compose(original, config);
            var back = DevelopmentSchemaCheck.Run(text);

            Assert.AreEqual("folders[1]", back.SwitchedOff.First().Path, "l'elemento spento è ancora secondo");
            StringAssert.Contains(text, "mirror: true");
            StringAssert.Contains(text, "- program");
            CollectionAssert.AreEqual(new[] { "folders[1]", "codeRoots[1]" }, back.SwitchedOff.Select(o => o.Path).ToList());
            var aliases = DevelopmentConfigFile.Read(Write(text)).Config.CodeRoots.Select(r => r.Alias).ToList();
            CollectionAssert.AreEqual(new[] { "a", "c", "d" }, aliases, "b resta nel file, spento, al suo posto");
            StringAssert.Contains(Block(text, "codeRoots"), "- alias: b\n  repository: r\n  owner: x\n- alias: c");
        }

        [TestMethod]
        public void Refuse_to_write_into_a_switched_off_group()
        {
            var original = Old.Replace("  target: copilot\n", "  target: codex\n");
            var config = Read(original);
            Assert.IsNull(config.Harness);
            config.Harness = new HarnessConfig { Target = "claude" };

            var ex = Assert.ThrowsException<DevelopmentConfigOffException>(() => DevelopmentConfigWriter.Compose(original, config));
            StringAssert.Contains(ex.Message, "harness");
        }

        [TestMethod]
        public void Let_an_older_mde_change_values_but_not_create_in_a_newer_file()
        {
            var newer = "# mde-schema: 9\n" + Old;
            var config = Read(newer);
            config.Project.Description = "altra";
            var text = DevelopmentConfigWriter.Compose(newer, config);
            StringAssert.StartsWith(text, "# mde-schema: 9\n", "il numero non si abbassa");
            StringAssert.Contains(text, "description: altra");

            var group = Read(newer);
            group.Redmine = new RedmineConfig { BaseUrl = "https://r" };
            StringAssert.Contains(Assert.ThrowsException<DevelopmentConfigOffException>(() => DevelopmentConfigWriter.Compose(newer, group)).Message, "aggiorna MDE");

            var item = Read(newer);
            item.Folders.Add(new DevelopmentFolder { Path = "nuova" });
            Assert.ThrowsException<DevelopmentConfigOffException>(() => DevelopmentConfigWriter.Compose(newer, item));

            Assert.IsTrue(DevelopmentConfigWriter.Compose("# mde-schema: 1\n" + Old, config).StartsWith($"# mde-schema: {MdExplorer.Features.Configuration.DevelopmentSchemaSnapshots.Current}\n"), "un numero più basso sale");
        }

        [TestMethod]
        public void Refuse_a_file_that_cannot_be_read_and_remove_a_group_taken_away()
        {
            var broken = "harness:\n<<<<<<< HEAD\n  target: a\n=======\n  target: b\n>>>>>>> x\n";
            Assert.ThrowsException<DevelopmentConfigOffException>(() => DevelopmentConfigWriter.Compose(broken, new DevelopmentConfig()));

            var config = Read(Old);
            config.Harness = null;
            Assert.IsNull(Block(DevelopmentConfigWriter.Compose(Old, config), "harness"));
        }

        [TestMethod]
        public void Drop_the_retired_entries_when_it_writes()
        {
            var original = Old + "agentCity:\n  enabled: true\n  autoMergeAgentDeliverables: false\n";
            var config = Read(original);
            config.Harness.Target = "claude";
            var text = DevelopmentConfigWriter.Compose(original, config);
            Assert.IsFalse(text.Contains("autoMergeAgentDeliverables"), text);
            StringAssert.Contains(text, "agentCity:\n  enabled: true\n");
        }

        [TestMethod]
        public void Keep_bom_and_windows_line_endings_through_the_writer()
        {
            var file = Path.Combine(Path.GetTempPath(), "writer-" + Guid.NewGuid().ToString("N") + ".yml");
            File.WriteAllText(file, Old.Replace("\n", "\r\n"), new UTF8Encoding(true));
            var config = DevelopmentConfigFile.Read(file).Config;
            config.Harness.Target = "claude";

            Assert.IsTrue(MdeRepoWriter.WriteDevelopmentConfig(file, config));

            var bytes = File.ReadAllBytes(file);
            Assert.AreEqual(0xEF, bytes[0]);
            var after = File.ReadAllText(file);
            StringAssert.Contains(after, "harness:\r\n  target: claude\r\n");
            Assert.IsFalse(after.Replace("\r\n", "").Contains('\n'), "tutto CRLF");
            Assert.IsFalse(MdeRepoWriter.WriteDevelopmentConfig(file, DevelopmentConfigFile.Read(file).Config), "riscritto uguale: niente");
        }

        /// <summary>
        /// Sui file veri di F0 (<c>MDE_SCHEMA_PROBE_DIR</c>): riletti e riscritti senza cambiare niente restano identici, e cambiando un
        /// gruppo gli altri restano byte per byte.
        /// </summary>
        [TestMethod]
        public void Leave_every_real_file_as_it_was_and_touch_only_the_changed_group()
        {
            var dir = Environment.GetEnvironmentVariable("MDE_SCHEMA_PROBE_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Inconclusive("MDE_SCHEMA_PROBE_DIR non impostata."); return; }
            var bad = new List<string>();
            foreach (var f in File.ReadAllLines(Path.Combine(dir, "files.txt")).Where(l => l.Trim().Length > 0))
            {
                var original = File.ReadAllText(f).Replace("\r\n", "\n");
                var config = DevelopmentConfigFile.Read(f).Config;
                var cleaned = RetiredConfigKeys.Remove(original, out var retired);
                var same = DevelopmentConfigWriter.Compose(original, config);
                if (retired.Count == 0 && !ReferenceEquals(same, original)) { bad.Add(f + ": riscritto senza cambiare niente"); continue; }

                config.ExternalBrowser ??= new ExternalBrowserConfig();
                config.ExternalBrowser.OpenAllExternal = !config.ExternalBrowser.OpenAllExternal;
                var changed = DevelopmentConfigWriter.Compose(original, config);
                foreach (var key in new[] { "folders", "compatibility", "project", "harness", "agentCity", "atlassian", "templates", "codeRoots", "redmine", "yamlAutoGeneration" })
                    if (Block(cleaned, key) != Block(changed, key)) bad.Add($"{f}: «{key}» cambiato");
            }
            Assert.AreEqual(0, bad.Count, string.Join("\n", bad));
        }
    }
}
