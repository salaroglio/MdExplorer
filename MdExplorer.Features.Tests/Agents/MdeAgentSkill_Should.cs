using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Yaml;
using MdExplorer.Features.Yaml.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// La skill <c>mde-agent</c> insegna a scrivere schede di agente: se l'esempio che contiene non passasse dal parser
    /// vero, insegnerebbe a scrivere schede che il registro scarta. Qui si legge l'esempio dalla skill e lo si fa
    /// leggere al parser.
    /// </summary>
    [TestClass]
    public class MdeAgentSkill_Should
    {
        private static string SkillText()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "MdExplorer", "skills", "mde-agent", "SKILL.md");
                if (File.Exists(candidate)) return File.ReadAllText(candidate);
                dir = dir.Parent;
            }
            Assert.Inconclusive("I sorgenti della skill non sono raggiungibili da qui.");
            return null;
        }

        private static string FirstYamlExample(string text)
        {
            var m = Regex.Match(text, "```yaml\\s*\\n(---\\n.*?\\n---)\\s*\\n```", RegexOptions.Singleline);
            Assert.IsTrue(m.Success, "la skill deve contenere un esempio di intestazione completo");
            return m.Groups[1].Value + "\ncorpo\n";
        }

        [TestMethod]
        public void Carry_the_marker_that_makes_it_travel_with_the_other_skills()
        {
            var text = SkillText();
            StringAssert.StartsWith(text, "---\nname: mde-agent\n".Replace("\n", text.Contains("\r\n") ? "\r\n" : "\n"));
            StringAssert.Contains(text, "origin: mdexplorer");
            StringAssert.Contains(text, "version: 1");
        }

        [TestMethod]
        public void Give_an_example_header_that_the_registry_accepts()
        {
            var parsed = new YamlAgentCardParser().GetDescriptor(FirstYamlExample(SkillText()));

            Assert.IsTrue(parsed.IsValid, parsed.RegistrationError);
            Assert.AreEqual("contabile", parsed.Card.Name);
            Assert.IsFalse(string.IsNullOrWhiteSpace(parsed.Card.Summary), "l'esempio deve mostrare il riassunto");
            Assert.IsTrue(parsed.Card.Summary.Length <= AgentCardDescriptor.SummaryMaxLength);
            CollectionAssert.AreEqual(new[] { "account-manager" }, parsed.Card.OnApprovalNotify.ToList());
            Assert.AreEqual("copilot", parsed.Runtime.Provider);
        }

        [TestMethod]
        public void Show_an_example_whose_summary_does_not_contradict_its_tools()
        {
            // La regola che la skill predica: il riassunto non dice «solo legge» se gli strumenti scrivono.
            var parsed = new YamlAgentCardParser().GetDescriptor(FirstYamlExample(SkillText()));
            var effects = AgentEffects.For(parsed.Tools);
            Assert.IsTrue(effects.Single(e => e.Id == AgentEffects.WriteFiles).Granted, "l'esempio usa edit");
            Assert.IsFalse(effects.Single(e => e.Id == AgentEffects.RunCommands).Granted, "e non chiede shell");
            Assert.IsFalse(parsed.Card.Summary.Contains("only reads", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void Keep_the_example_fingerprint_independent_of_runtime()
        {
            // runtime: sta fuori dall'impronta (cambiare motore non deve far decadere la fiducia).
            var text = FirstYamlExample(SkillText());
            var a = new YamlAgentCardParser().GetDescriptor(text);
            var b = new YamlAgentCardParser().GetDescriptor(text.Replace("provider: copilot", "provider: claude"));
            Assert.AreEqual(AgentTrustHasher.ComputeHash(a.Card, a.Tools), AgentTrustHasher.ComputeHash(b.Card, b.Tools));
        }
    }
}
