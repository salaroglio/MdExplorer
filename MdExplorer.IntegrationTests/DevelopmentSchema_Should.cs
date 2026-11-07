using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Configuration;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// F1 dello sprint «Schema di .development.yml»: lo schema dentro MDE e il controllo gruppo per gruppo. I gruppi sono
    /// facoltativi, le voci di un gruppo obbligatorie dallo schema 2 (D6); un gruppo che non combacia si spegne, il resto va.
    /// </summary>
    [TestClass]
    public class DevelopmentSchema_Should
    {
        private static string Say(string it, string en) => it;

        /// <summary>Il file come lo scrive MDE oggi (schema 1: le voci vuote non ci sono).</summary>
        private const string Today =
            "folders:\n- path: docs\n  tags:\n  - analysis\ncompatibility:\n  mode: mdexplorer\n  gitHubOptions:\n    embedImages: false\n    stripInteractive: true\n    preserveEmoji: true\n" +
            "yamlAutoGeneration:\n  enabled: true\n  excludePaths:\n  - .github\nexternalBrowser:\n  enabled: false\n  openAllExternal: false\n  urlPatterns: []\n" +
            "project:\n  participants:\n  - gitEmail: a@example.com\n    gitName: A\n    manual: false\n" +
            "harness:\n  target: claude\n" +
            "codeRoots:\n- alias: legacy\n  repository: https://example.com/legacy.git\n  programs:\n  - path: batch\n- alias: web\n  repository: https://example.com/web.git\n";

        private static SchemaSnapshot WithHarnessEffort(int version, bool missingAllowed)
        {
            var root = DevelopmentSchema.FromModel();
            root.Keys["harness"].Keys["effort"] = new SchemaNode { Kind = SchemaKind.Text };
            return new SchemaSnapshot { Version = version, MissingKeysAllowed = missingAllowed, Root = root };
        }

        [TestMethod]
        public void Ship_a_snapshot_of_the_current_number_equal_to_the_model()
        {
            var expected = DevelopmentSchemaSnapshots.ToJson(DevelopmentSchema.CurrentFromModel(missingKeysAllowed: DevelopmentSchema.Current == 1));
            var shipped = DevelopmentSchemaSnapshots.ToJson(DevelopmentSchemaSnapshots.All.Last());
            if (expected != shipped)
            {
                var file = Path.Combine(Path.GetTempPath(), $"development-schema-{DevelopmentSchema.Current}.json");
                File.WriteAllText(file, expected);
                Assert.Fail($"Il modello di .development.yml è cambiato: alza DevelopmentSchema.Current e aggiungi MdExplorer.bll/Configuration/Schemas/development-schema-N.json " +
                            $"(quella del modello di oggi è in {file}). Un'istantanea già pubblicata non si cambia.");
            }
            CollectionAssert.AreEqual(Enumerable.Range(1, DevelopmentSchema.Current).ToList(), DevelopmentSchemaSnapshots.All.Select(s => s.Version).ToList());
        }

        [TestMethod]
        public void Accept_the_file_mde_writes_today_and_one_without_groups()
        {
            foreach (var text in new[] { Today, "", "folders: []\n", "redmine:\nharness:\n  target: none\n" })
            {
                var check = DevelopmentSchemaCheck.Run(text);
                Assert.IsTrue(check.AllGood, text + " → " + check.Describe(Say));
                Assert.AreEqual(1, check.FileSchema);
                Assert.AreEqual(string.Empty, check.Describe(Say));
            }
        }

        [TestMethod]
        public void Switch_off_only_the_group_with_an_unknown_entry()
        {
            var check = DevelopmentSchemaCheck.Run(Today.Replace("harness:\n  target: claude\n", "harness:\n  target: claude\n  effort: high\n"));

            Assert.AreEqual("harness", check.SwitchedOff.Single().Path);
            var m = check.SwitchedOff.Single().Mismatches.Single();
            Assert.AreEqual(("harness.effort", SchemaMismatchKind.UnknownKey), (m.Path, m.Kind));
            Assert.AreEqual(Today.Split('\n').ToList().IndexOf("harness:") + 3, m.Line, "la riga della voce, contata da 1 (harness, target, effort)");
            Assert.IsTrue(check.IsOff("harness"));
            Assert.IsFalse(check.IsOff("compatibility"));
            Assert.IsFalse(check.IsOff("codeRoots[0]"));
            StringAssert.Contains(check.Describe(Say), "harness");
            StringAssert.Contains(check.Describe(Say), "harness.effort");
        }

        [TestMethod]
        public void Switch_off_a_value_this_mde_does_not_know_and_accept_the_known_ones_in_any_case()
        {
            var codex = DevelopmentSchemaCheck.Run("harness:\n  target: codex\n");
            Assert.AreEqual(SchemaMismatchKind.ValueNotAllowed, codex.SwitchedOff.Single().Mismatches.Single().Kind);
            StringAssert.Contains(codex.Describe(Say), "codex");

            foreach (var ok in new[] { "Claude", "none", "copilot", "opencode" })
                Assert.IsTrue(DevelopmentSchemaCheck.Run($"harness:\n  target: {ok}\n").AllGood, ok);
            Assert.IsFalse(DevelopmentSchemaCheck.Run("folders:\n- path: a\n  codeGraph:\n    describe: sometimes\n").AllGood);
            Assert.IsFalse(DevelopmentSchemaCheck.Run("redmine:\n  baseUrl: https://r\n  textFormatting: html\n").AllGood);
        }

        [TestMethod]
        public void Switch_off_a_wrong_type()
        {
            foreach (var text in new[] { "agentCity:\n  enabled: maybe\n", "harness:\n  target:\n    name: claude\n", "agentCity: true\n", "folders:\n  path: docs\n" })
            {
                var check = DevelopmentSchemaCheck.Run(text);
                Assert.AreEqual(SchemaMismatchKind.WrongType, check.SwitchedOff.Single().Mismatches.First().Kind, text);
            }
        }

        [TestMethod]
        public void Keep_an_unknown_group_off_and_the_rest_on()
        {
            var check = DevelopmentSchemaCheck.Run(Today + "jenkins:\n  url: https://ci\n");
            var off = check.SwitchedOff.Single();
            Assert.AreEqual(("jenkins", SchemaMismatchKind.UnknownGroup), (off.Path, off.Mismatches.Single().Kind));
            Assert.IsFalse(check.IsOff("harness"));
        }

        [TestMethod]
        public void Switch_off_everything_when_the_file_cannot_be_read()
        {
            var check = DevelopmentSchemaCheck.Run("harness:\n<<<<<<< HEAD\n  target: claude\n=======\n  target: copilot\n>>>>>>> origin\n");
            Assert.IsNotNull(check.Unreadable);
            Assert.IsTrue(check.IsOff("harness") && check.IsOff("folders"));
            StringAssert.Contains(check.Describe(Say), "non si legge");
            Assert.IsNotNull(DevelopmentSchemaCheck.Run("- a\n- b\n").Unreadable, "una lista non è un file di voci");
        }

        [TestMethod]
        public void Switch_off_only_the_list_item_that_does_not_match()
        {
            var check = DevelopmentSchemaCheck.Run(Today.Replace("- alias: web\n", "- alias: web\n  mirror: true\n"));

            var off = check.SwitchedOff.Single();
            Assert.AreEqual(("codeRoots[1]", "web"), (off.Path, off.ItemKey));
            Assert.IsFalse(check.IsOff("codeRoots[0]"));
            Assert.IsFalse(check.IsOff("codeRoots[0].programs[0]"));
            Assert.IsTrue(check.IsOff("codeRoots[1].programs[0]"), "dentro un elemento spento è spento tutto");
            StringAssert.Contains(check.Describe(Say), "la radice di codice «web» (codeRoots[1])");

            var inner = DevelopmentSchemaCheck.Run(Today.Replace("  - path: batch\n", "  - path: batch\n    owner: x\n"));
            Assert.AreEqual("codeRoots[0].programs[0]", inner.SwitchedOff.Single().Path, "anche un elemento di una lista dentro un elemento");
            Assert.IsFalse(inner.IsOff("codeRoots[0]"));

            var people = DevelopmentSchemaCheck.Run(Today.Replace("    manual: false\n", "    manual: false\n    phone: 1\n"));
            Assert.AreEqual(("project.participants[0]", "a@example.com"), (people.SwitchedOff.Single().Path, people.SwitchedOff.Single().ItemKey));
        }

        [TestMethod]
        public void Switch_off_the_whole_group_when_a_subgroup_does_not_match()
        {
            var check = DevelopmentSchemaCheck.Run(Today.Replace("    preserveEmoji: true\n", "    preserveEmoji: true\n    mermaid: true\n"));
            var off = check.SwitchedOff.Single();
            Assert.AreEqual(("compatibility", "compatibility.gitHubOptions.mermaid"), (off.Path, off.Mismatches.Single().Path));
        }

        [TestMethod]
        public void Not_count_the_retired_entries_that_the_cleanup_removes()
        {
            var text = "agentCity:\n  enabled: true\n  workflowDoc: flusso.md\n  autoMergeAgentDeliverables: false\natlassian:\n  jiraBaseUrl: https://j\n  planningFolder: specs/da-jira\n";
            Assert.IsTrue(DevelopmentSchemaCheck.Run(text).AllGood, DevelopmentSchemaCheck.Run(text).Describe(Say));
        }

        [TestMethod]
        public void Tell_to_update_when_the_file_comes_from_a_newer_schema()
        {
            var check = DevelopmentSchemaCheck.Run("# mde-schema: 9\n" + Today.Replace("  target: claude\n", "  target: claude\n  effort: high\n"));
            Assert.AreEqual(9, check.FileSchema);
            Assert.IsTrue(check.WrittenByNewerMde);
            StringAssert.Contains(check.Describe(Say), "MDE più nuovo (schema 9");
            StringAssert.Contains(check.Describe(Say), "aggiorni MDE");

            Assert.IsFalse(DevelopmentSchemaCheck.Run("# mde-schema: 1\n" + Today).WrittenByNewerMde);
            Assert.AreEqual(1, DevelopmentSchemaCheck.Run("# un commento\n" + Today).FileSchema);
        }

        [TestMethod]
        public void Require_every_entry_from_schema_two_and_recognize_a_group_of_schema_one()
        {
            var one = new SchemaSnapshot { Version = 1, MissingKeysAllowed = true, Root = DevelopmentSchema.FromModel() };
            var two = WithHarnessEffort(2, missingAllowed: false);
            const string full = "harness:\n  target: claude\n  model:\n  effort:\n";

            Assert.IsTrue(DevelopmentSchemaCheck.Run(full, new[] { two }).AllGood, "le voci non compilate ci sono, vuote");
            var onlyTwo = DevelopmentSchemaCheck.Run("harness:\n  target: claude\n", new[] { two });
            CollectionAssert.AreEquivalent(new[] { "harness.model", "harness.effort" }, onlyTwo.SwitchedOff.Single().Mismatches.Select(m => m.Path).ToList());
            Assert.IsTrue(onlyTwo.SwitchedOff.Single().Mismatches.All(m => m.Kind == SchemaMismatchKind.MissingKey));

            // Il gruppo l'ha scritto un MDE con lo schema 1: quello con lo schema 2 lo riconosce e non lo spegne (L2).
            Assert.IsTrue(DevelopmentSchemaCheck.Run("harness:\n  target: claude\n", new[] { one, two }).AllGood);
            // E quello con lo schema 1 spegne il gruppo dello schema 2 (D5: il vecchio deperisce).
            var old = DevelopmentSchemaCheck.Run("# mde-schema: 2\n" + full, new[] { one });
            Assert.AreEqual(("harness", "harness.effort"), (old.SwitchedOff.Single().Path, old.SwitchedOff.Single().Mismatches.Single().Path));
        }

        /// <summary>
        /// Prova sui file veri (F0), solo se <c>MDE_SCHEMA_PROBE_DIR</c> indica una cartella di <c>.yml</c>: nessuno di quei
        /// progetti deve avere una funzione spenta il giorno dell'aggiornamento.
        /// </summary>
        [TestMethod]
        public void Accept_every_real_file_of_the_probe_folder()
        {
            var dir = Environment.GetEnvironmentVariable("MDE_SCHEMA_PROBE_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Inconclusive("MDE_SCHEMA_PROBE_DIR non impostata."); return; }
            var files = File.ReadAllLines(Path.Combine(dir, "files.txt")).Where(l => l.Trim().Length > 0).ToList();
            var bad = new List<string>();
            foreach (var f in files)
            {
                var check = DevelopmentSchemaCheck.Run(File.ReadAllText(f));
                if (!check.AllGood) bad.Add(f + ": " + check.Describe(Say));
            }
            Console.WriteLine($"{files.Count} file controllati");
            Assert.AreEqual(0, bad.Count, string.Join("\n", bad));
        }

        // ---- F5: la regola di crescita ----

        /// <summary>
        /// Le impronte delle istantanee già pubblicate: un'istantanea uscita in un pacchetto non si cambia mai (gli MDE installati la
        /// usano per riconoscere i gruppi scritti con quel numero). Quando ne aggiungi una nuova, aggiungi qui la sua impronta.
        /// </summary>
        private static readonly Dictionary<int, string> Published = new()
        {
            [1] = "217dfd9b9fddc939fe7d95703603cdd044af3b12df05e2dee9d078001e7fc3a3",
            [2] = "a7cd33ba715b2bd78dec6cb45b9589edec7ec8425eaba40e087737adee8e90ad",
        };

        private static string SnapshotText(int version)
        {
            using var stream = typeof(DevelopmentSchemaSnapshots).Assembly.GetManifestResourceStream(DevelopmentSchemaSnapshots.ResourceName(version));
            using var reader = new StreamReader(stream!);
            return reader.ReadToEnd().Replace("\r\n", "\n");
        }

        [TestMethod]
        public void Never_change_a_published_snapshot()
        {
            foreach (var (version, hash) in Published)
            {
                var actual = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(SnapshotText(version)))).ToLowerInvariant();
                Assert.AreEqual(hash, actual,
                    $"L'istantanea {version} di .development.yml è cambiata: gli MDE già installati riconoscono i gruppi con quella vecchia. " +
                    "Rimettila com'era e porta la modifica in un numero nuovo.");
            }
            Assert.IsTrue(Published.ContainsKey(DevelopmentSchemaSnapshots.Current),
                $"Manca l'impronta dell'istantanea {DevelopmentSchemaSnapshots.Current} in Published: aggiungila quando la pubblichi.");
        }

        /// <summary>
        /// Le voci di <paramref name="before"/> che <paramref name="after"/> perde o cambia: sparite, tipo diverso, valori ammessi tolti.
        /// Una voce ritirata in <c>RetiredConfigKeys</c> può sparire: la pulizia la toglie dai file prima del controllo.
        /// </summary>
        private static List<string> Broken(SchemaNode before, SchemaNode after, string path)
        {
            var found = new List<string>();
            if (after == null)
            {
                if (!Retired.Contains(path)) found.Add($"«{path}» sparita: ritirala in RetiredConfigKeys o lasciala");
                return found;
            }
            if (before.Kind != after.Kind) { found.Add($"«{path}» ha cambiato tipo ({before.Kind} → {after.Kind}): fai una voce nuova e ritira questa"); return found; }
            if (before.Choices != null)
            {
                var lost = before.Choices.Except(after.Choices ?? new List<string>(), StringComparer.OrdinalIgnoreCase).ToList();
                if (after.Choices == null) { }   // da elenco chiuso a testo libero: si accetta di più, nessun file si spegne
                else if (lost.Count > 0) found.Add($"«{path}» non ammette più {string.Join(", ", lost)}: un file con quei valori si spegnerebbe");
            }
            else if (after.Choices != null) found.Add($"«{path}» è diventata a elenco chiuso: i valori scritti prima si spegnerebbero");
            if (before.Kind == SchemaKind.Group)
                foreach (var (key, child) in before.Keys ?? new SortedDictionary<string, SchemaNode>())
                    found.AddRange(Broken(child, after.Keys != null && after.Keys.TryGetValue(key, out var a) ? a : null, path.Length == 0 ? key : path + "." + key));
            if (before.Kind == SchemaKind.List)
                found.AddRange(Broken(before.Item, after.Item, path + "[]"));
            return found;
        }

        private static readonly HashSet<string> Retired = new(
            RetiredConfigKeys.All.Select(r => r.Section + "." + r.Key).Concat(RetiredConfigKeys.BecameList.Select(r => r.Section + "." + r.Key)), StringComparer.Ordinal);

        [TestMethod]
        public void Grow_only_by_adding_from_one_number_to_the_next()
        {
            var all = DevelopmentSchemaSnapshots.All;
            for (var i = 1; i < all.Count; i++)
            {
                var broken = Broken(all[i - 1].Root, all[i].Root, "");
                Assert.AreEqual(0, broken.Count, $"Dallo schema {all[i - 1].Version} al {all[i].Version}:\n" + string.Join("\n", broken));
            }
        }

        [TestMethod]
        public void Catch_a_removed_a_retyped_and_a_narrowed_entry()
        {
            var before = DevelopmentSchema.FromModel();
            var after = DevelopmentSchema.FromModel();
            after.Keys["harness"].Keys.Remove("model");
            after.Keys["redmine"].Keys["projects"] = new SchemaNode { Kind = SchemaKind.Text };
            after.Keys["harness"].Keys["target"].Choices.Remove("opencode");
            after.Keys["atlassian"].Keys.Remove("jiraBaseUrl");

            var broken = Broken(before, after, "");

            Assert.AreEqual(4, broken.Count, string.Join("\n", broken));
            Assert.IsTrue(broken.Any(b => b.Contains("«harness.model» sparita")));
            Assert.IsTrue(broken.Any(b => b.Contains("«redmine.projects» ha cambiato tipo")));
            Assert.IsTrue(broken.Any(b => b.Contains("opencode")));

            // Una voce ritirata in RetiredConfigKeys può sparire (il caso di planningFolder, 04/06).
            before.Keys["atlassian"].Keys["planningFolder"] = new SchemaNode { Kind = SchemaKind.Text };
            Assert.IsFalse(Broken(before, DevelopmentSchema.FromModel(), "").Any(b => b.Contains("planningFolder")));
        }
    }
}
