using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.AI.SkillLearning;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.AI
{
    /// <summary>
    /// La risposta a «Impara da questa attività» (sprint skill che migliorano, F2): il blocco si legge, ogni proposta si
    /// controlla, e una proposta che non passa resta con i suoi motivi. Il primo test usa una risposta vera di F0.
    /// </summary>
    [TestClass]
    public class SkillLearningBlock_Should
    {
        private static readonly SkillLearningContext Project = new()
        {
            ExistingSkills = new Dictionary<string, bool> { ["mde-doc"] = true, ["regole-del-gruppo"] = false },
            From = 1,
            To = 104,
        };

        private static string Answer(string proposalsJson) => "Ho imparato una cosa.\n\n<<<mde-learn\n{\"proposals\": [" + proposalsJson + "]}\nmde-learn>>>";

        private static string Proposal(string action = "create", string skill = "prova-isolata", string why = "evita errori",
            string evidence = "[12]", string content = null)
        {
            content ??= $"---\\nname: {skill}\\ndescription: Prova. Use when: prova.\\n---\\n\\n# Corpo\\n";
            return $"{{\"action\": \"{action}\", \"skill\": \"{skill}\", \"why\": \"{why}\", \"evidence\": {evidence}, \"content\": \"{content}\"}}";
        }

        /// <summary>I messaggi seguono la lingua dell'interfaccia (nei test l'inglese): si controlla la parola in tutte e due.</summary>
        private static void Says(string text, string it, string en)
            => Assert.IsTrue(text != null && (text.Contains(it) || text.Contains(en)), $"atteso «{it}» o «{en}» in: {text}");

        [TestMethod]
        public void Read_a_real_answer_with_its_two_valid_proposals()
        {
            var text = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "AI", "Fixtures", "learn-answer-stop-button.txt"));

            var answer = SkillLearningBlock.Read(text, Project);

            Assert.IsNull(answer.BlockProblem);
            StringAssert.StartsWith(answer.Prose, "Ho imparato");
            Assert.IsFalse(answer.Prose.Contains("<<<mde-learn"), "la prosa è senza il blocco");
            CollectionAssert.AreEqual(new[] { "signalr-hub-metodi-lunghi", "prova-isolata-backend-mdexplorer" }, answer.Proposals.Select(p => p.Skill).ToArray());
            Assert.IsTrue(answer.Proposals.All(p => p.IsValid), string.Join("; ", answer.Proposals.SelectMany(p => p.Problems)));
            Assert.IsTrue(answer.Proposals[0].Evidence.Contains(23));
            StringAssert.StartsWith(answer.Proposals[1].Content, "---\nname: prova-isolata-backend-mdexplorer");
        }

        [TestMethod]
        public void Say_what_is_wrong_with_the_block_itself()
        {
            Assert.IsNotNull(SkillLearningBlock.Read("Solo prosa, niente blocco.", Project).BlockProblem);
            Says(SkillLearningBlock.Read("x\n<<<mde-learn\n{\"proposals\": [", Project).BlockProblem, "chiuso", "closed");
            Assert.IsNotNull(SkillLearningBlock.Read("x\n<<<mde-learn\nnon json\nmde-learn>>>", Project).BlockProblem);
            Assert.IsNotNull(SkillLearningBlock.Read("x\n<<<mde-learn\n{\"altro\": 1}\nmde-learn>>>", Project).BlockProblem);

            var none = SkillLearningBlock.Read("Niente di durevole.\n<<<mde-learn\n{\"proposals\": []}\nmde-learn>>>", Project);
            Assert.IsNull(none.BlockProblem, "nessuna proposta è una risposta valida");
            Assert.AreEqual(0, none.Proposals.Count);
        }

        [TestMethod]
        public void Keep_an_invalid_proposal_with_its_reasons()
        {
            var answer = SkillLearningBlock.Read(Answer(string.Join(",",
                Proposal(skill: "mde-mia"),                                            // 0: nome riservato
                Proposal(skill: "Con Spazi"),                                          // 1: nome non valido
                Proposal(action: "create", skill: "regole-del-gruppo"),                // 2: c'è già
                Proposal(action: "update", skill: "non-esiste"),                       // 3: non c'è
                Proposal(action: "update", skill: "mde-doc"),                          // 4: di MdExplorer
                Proposal(skill: "senza-prove", evidence: "[]"),                        // 5: niente prove
                Proposal(skill: "prove-fuori", evidence: "[3, 500]"),                  // 6: fuori intervallo
                Proposal(skill: "senza-frontmatter", content: "# solo corpo"),         // 7
                Proposal(skill: "nome-diverso", content: "---\\nname: altro\\ndescription: x\\n---\\n"), // 8
                Proposal(skill: "con-mde", content: "---\\nname: con-mde\\ndescription: x\\nmde:\\n  origin: mdexplorer\\n---\\n"), // 9
                Proposal(action: "delete", skill: "azione-strana"),                    // 10
                Proposal(skill: "senza-perche", why: ""),                              // 11
                Proposal(skill: "valida-davvero"))),                                   // 12
                Project);

            Assert.AreEqual(13, answer.Proposals.Count, "nessuna proposta sparisce");
            string Why(int i) => string.Join(" | ", answer.Proposals[i].Problems);
            StringAssert.Contains(Why(0), "mde-");
            Says(Why(1), "minuscole", "lowercase");
            StringAssert.Contains(Why(2), "update");
            Says(Why(3), "non c'è", "not in the project");
            StringAssert.Contains(Why(4), "MdExplorer");
            Says(Why(5), "prove", "evidence");
            StringAssert.Contains(Why(6), "500");
            StringAssert.Contains(Why(7), "frontmatter");
            StringAssert.Contains(Why(8), "altro");
            StringAssert.Contains(Why(9), "mde:");
            StringAssert.Contains(Why(10), "delete");
            StringAssert.Contains(Why(11), "why");
            Assert.IsTrue(answer.Proposals[12].IsValid, Why(12));
            Assert.AreEqual(12, answer.Proposals.Count(p => !p.IsValid));
        }

        [TestMethod]
        public void Leave_the_chat_thread_block_out_of_the_prose()
        {
            var answer = SkillLearningBlock.Read("Niente da imparare.\n\n<<<mde-thread\n{}\nmde-thread>>>\n<<<mde-learn\n{\"proposals\": []}\nmde-learn>>>", Project);

            Assert.AreEqual("Niente da imparare.", answer.Prose);
            Assert.IsNull(answer.BlockProblem);
        }

        [TestMethod]
        public void Mark_two_proposals_on_the_same_skill()
        {
            var answer = SkillLearningBlock.Read(Answer(Proposal(skill: "doppia") + "," + Proposal(skill: "doppia")), Project);

            Assert.IsTrue(answer.Proposals.All(p => p.Problems.Any(x => x.Contains("stessa skill") || x.Contains("same skill"))));
        }

        [TestMethod]
        public void Refuse_a_skill_too_long_to_be_useful()
        {
            var body = string.Concat(Enumerable.Repeat("riga\\n", SkillLearningBlock.MaxContentLines + 10));
            var answer = SkillLearningBlock.Read(Answer(Proposal(skill: "lunga", content: "---\\nname: lunga\\ndescription: x\\n---\\n" + body)), Project);

            Says(answer.Proposals.Single().Problems.Single(), "divisa", "split");
        }
    }
}
