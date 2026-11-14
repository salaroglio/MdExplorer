using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MdExplorer.Features.Agents.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// La skill <c>mde-workflow</c> insegna a scrivere workflow: se il suo esempio completo non passasse dal parser vero,
    /// insegnerebbe a scrivere workflow che MDE rifiuta. Qui si legge l'esempio dalla skill e lo si fa leggere al parser.
    /// </summary>
    [TestClass]
    public class MdeWorkflowSkill_Should
    {
        private static string SkillText()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "MdExplorer", "skills", "mde-workflow", "SKILL.md");
                if (File.Exists(candidate)) return File.ReadAllText(candidate);
                dir = dir.Parent;
            }
            Assert.Inconclusive("I sorgenti della skill non sono raggiungibili da qui.");
            return null;
        }

        [TestMethod]
        public void Give_a_complete_example_the_parser_accepts()
        {
            var text = SkillText();
            var examples = Regex.Matches(text, "```json\\s*\\n(\\{\\s*\\n\\s*\"mde_workflow\".*?)\\n```", RegexOptions.Singleline)
                .Select(m => m.Groups[1].Value).Where(j => j.Contains("\"steps\": [\n")).ToList();
            Assert.AreEqual(1, examples.Count, "la skill ha un esempio completo");

            var parsed = WorkflowParser.Parse(examples[0]);
            Assert.IsTrue(parsed.IsValid, string.Join("\n", parsed.Issues));
            Assert.AreEqual(3, parsed.Descriptor.Version);
            Assert.IsTrue(parsed.Descriptor.Steps.Any(s => s.IsPerson), "l'esempio mostra il passo di una persona");
            Assert.IsTrue(parsed.Descriptor.Steps.Any(s => s.ApprovalBy != null), "e un'approvazione per ruolo");
            Assert.IsTrue(parsed.Descriptor.Steps.Any(s => s.Inputs.Values.Any(i => i.From.Count > 1)), "e un ingresso da più uscite");
        }
    }
}
