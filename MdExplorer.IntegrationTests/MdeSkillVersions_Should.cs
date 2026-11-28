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
        /// <summary>
        /// Ciò che il pacchetto distribuisce: tipo/nome → versione e impronta (di tutti gli ambienti agentici).
        /// 28/11/2026: impronte cambiate SENZA alzare le versioni perché si è aggiunto l'ambiente Codex; calcolate senza Codex
        /// coincidevano tutte con le precedenti, cioè i file degli altri tre ambienti sono identici (sprint 2026-11-28-Motore-Codex).
        /// </summary>
        private static readonly Dictionary<string, (int Version, string Hash)> Known = new()
        {
            ["agent/mde-skillcreator"] = (4, "d6ff9cc4732bcc5c"),
            ["prompt/mde-code-doc"] = (12, "cd4ca488bd2264fb"),
            ["prompt/mde-codegen-graph"] = (1, "4321e161ac595062"),
            ["prompt/mde-doc-tidy"] = (2, "3991d6b084912880"),
            ["prompt/mde-doc-tidy-redraw"] = (1, "199b3b7ae0e68bea"),
            ["prompt/mde-mark-folder-synthesis"] = (2, "1bc0f5b264683526"),
            ["prompt/mde-mark-summarize"] = (2, "39bb16f575d46cbe"),
            ["skill/mde-abox"] = (5, "edc8d8d16eff9edc"),
            ["skill/mde-agent"] = (10, "80a06f232bbd8d9c"),
            ["skill/mde-code"] = (45, "0d11bfca963e5b47"),
            ["skill/mde-code-check"] = (8, "296e450c3dd75967"),
            ["skill/mde-doc"] = (9, "e9b11ecab1facc82"),
            ["skill/mde-e2e"] = (9, "54dd8a6ffd27fe9b"),
            ["skill/mde-e2e-signals"] = (1, "f5c70a65468fa8f0"),
            ["skill/mde-features"] = (5, "bb0bcbf5f4e3e9dd"),
            ["skill/mde-ownership"] = (3, "1c6f881d8651d723"),
            ["skill/mde-plantuml"] = (6, "b5cbae63bbbad8cc"),
            ["skill/mde-prompt-for-agents"] = (2, "a30d10eaa3f6c0d1"),
            ["skill/mde-readme"] = (8, "b95569ec12c3e57f"),
            ["skill/mde-shacl"] = (5, "2e75cb22a95d46cb"),
            ["skill/mde-slide"] = (18, "0a61c39e60b99d58"),
            ["skill/mde-tbox"] = (5, "c440fdf168323680"),
            ["skill/mde-template"] = (7, "7d947b0d4c90862b"),
            ["skill/mde-workflow"] = (8, "1517a76bb093dbf4"),
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
