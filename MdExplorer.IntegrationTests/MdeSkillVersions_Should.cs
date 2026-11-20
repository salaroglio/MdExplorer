using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// N5 della valutazione del 07/11. Le skill, gli agenti e i prompt <c>mde-*</c> sono di MDE: un progetto che li ha già li riceve di
    /// nuovo solo se sale il numero <c>mde: version</c> (un numero, non il contenuto, perché un MDE vecchio non riporti indietro la
    /// skill di uno nuovo). Un SKILL.md o un file di accompagnamento cambiato senza alzare il numero non arriva mai; una skill tolta
    /// dal catalogo resta nei progetti. Qui si ferma lo sviluppatore in entrambi i casi.
    /// </summary>
    [TestClass]
    public class MdeSkillVersions_Should
    {
        /// <summary>Ciò che il pacchetto distribuisce: tipo/nome → versione e impronta (di tutti gli ambienti agentici).</summary>
        private static readonly Dictionary<string, (int Version, string Hash)> Known = new()
        {
            ["agent/mde-skillcreator"] = (4, "f9346eaf140f8548"),
            ["prompt/mde-code-doc"] = (12, "b093e8a244d73267"),
            ["prompt/mde-codegen-graph"] = (1, "b8d892587efab736"),
            ["prompt/mde-doc-tidy"] = (2, "8e0082344957edb7"),
            ["prompt/mde-doc-tidy-redraw"] = (1, "c399c9958bd7cd5e"),
            ["prompt/mde-mark-folder-synthesis"] = (2, "1526a5a6329b9195"),
            ["prompt/mde-mark-summarize"] = (2, "c69e11c1bca86639"),
            ["skill/mde-abox"] = (5, "fcbe1b9b473c9c16"),
            ["skill/mde-agent"] = (10, "8348d262de9c1a1d"),
            ["skill/mde-code"] = (44, "cee1905190f2ef50"),
            ["skill/mde-code-check"] = (7, "ff08a39e19df66e3"),
            ["skill/mde-doc"] = (9, "89503e394ca2dbf7"),
            ["skill/mde-e2e"] = (9, "83edd59bbc973ad3"),
            ["skill/mde-e2e-signals"] = (1, "5284bec8166f03f6"),
            ["skill/mde-features"] = (5, "ece7e9e39a175525"),
            ["skill/mde-ownership"] = (3, "b2a90adacaf25045"),
            ["skill/mde-plantuml"] = (6, "6f250184b3b7ac66"),
            ["skill/mde-prompt-for-agents"] = (2, "e29712a329f1eb83"),
            ["skill/mde-readme"] = (8, "e05171345ea9ceaa"),
            ["skill/mde-shacl"] = (5, "8dcd1b6f2b894105"),
            ["skill/mde-slide"] = (18, "27ffa0ab64a1128e"),
            ["skill/mde-tbox"] = (5, "7d00e1aa4dac8778"),
            ["skill/mde-template"] = (7, "7feca705c2e8cb23"),
            ["skill/mde-workflow"] = (6, "09380831b1ba9886"),
        };

        private static Dictionary<string, (int Version, string Hash)> Current()
            => MdeSkillUpdater.Shipped()
                .GroupBy(a => a.Kind + "/" + a.Name)
                .ToDictionary(g => g.Key, g => (g.Max(a => a.Version),
                    Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
                        string.Join("|", g.OrderBy(a => a.Layout).Select(a => a.Layout + ":" + a.ContentHash)))))[..16].ToLowerInvariant()));

        private static string Line(string key, (int Version, string Hash) v) => $"            [\"{key}\"] = ({v.Version}, \"{v.Hash}\"),";

        [TestMethod]
        public void Raise_the_version_whenever_a_shipped_file_changes()
        {
            var now = Current();
            var problems = new List<string>();
            foreach (var (key, v) in now)
            {
                if (!Known.TryGetValue(key, out var was)) { problems.Add($"{key}: nuovo, aggiungi a Known:\n{Line(key, v)}"); continue; }
                if (v.Hash == was.Hash && v.Version == was.Version) continue;
                if (v.Version <= was.Version)
                    problems.Add($"{key}: il contenuto è cambiato ma «mde: version» è ancora {v.Version}: alzala nel SKILL.md (o nel file dell'agente/prompt), " +
                                 "altrimenti i progetti che l'hanno già non lo ricevono. Poi aggiorna Known.");
                else
                    problems.Add($"{key}: versione {was.Version} → {v.Version}, aggiorna Known:\n{Line(key, v)}");
            }
            foreach (var key in Known.Keys.Where(k => !now.ContainsKey(k)))
                problems.Add($"{key}: tolto dal catalogo. MDE oggi non toglie dai progetti ciò che non distribuisce più: prima di ritirarlo " +
                             "aggiungi la rimozione (solo dei file con «origin: mdexplorer»), poi togli la riga da Known.");
            Assert.AreEqual(0, problems.Count, string.Join("\n", problems));
        }
    }
}
