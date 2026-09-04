using System.Linq;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Yaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Blocco <c>runtime:</c> della card: con che motore gira l'agente. Sta fuori da <c>a2a:</c>
    /// e <c>tools:</c> di proposito — quelli formano l'impronta della fiducia, quindi metterci il
    /// modello farebbe decadere il trust a ogni cambio, su ogni macchina.
    /// </summary>
    [TestClass]
    public class AgentRuntimeDeclaration_Should
    {
        private static string Card(string extra) => $@"---
description: ""prova""
tools: [read, write]
{extra}a2a:
  name: wiki-curator
  role: Curatore
---
corpo
";

        [TestMethod]
        public void Read_provider_and_model_from_the_card()
        {
            var parsed = new YamlAgentCardParser().GetDescriptor(Card(
                "runtime:\n  provider: copilot\n  model: gpt-5.6-luna\n"));

            Assert.IsNull(parsed.RegistrationError, parsed.RegistrationError);
            Assert.IsNotNull(parsed.Runtime, "il blocco runtime: deve essere letto");
            Assert.AreEqual("copilot", parsed.Runtime.Provider);
            Assert.AreEqual("gpt-5.6-luna", parsed.Runtime.Model);
        }

        [TestMethod]
        public void Leave_runtime_null_when_the_card_does_not_declare_it()
        {
            // Retrocompatibilità: le card esistenti non hanno il blocco e devono restare valide.
            var parsed = new YamlAgentCardParser().GetDescriptor(Card(string.Empty));

            Assert.IsNull(parsed.RegistrationError);
            Assert.IsTrue(parsed.Runtime == null || parsed.Runtime.IsEmpty,
                "nessuna dichiarazione = si usa il predefinito, non un errore");
        }

        [TestMethod]
        public void Not_break_the_trust_fingerprint()
        {
            // Il punto della scelta di collocazione: cambiare modello NON deve far decadere la
            // fiducia, altrimenti ogni ritocco operativo costringe a rifidarsi su ogni macchina.
            var parser = new YamlAgentCardParser();
            var a = parser.GetDescriptor(Card("runtime:\n  provider: copilot\n  model: modello-uno\n"));
            var b = parser.GetDescriptor(Card("runtime:\n  provider: copilot\n  model: modello-due\n"));

            var hashA = AgentTrustHasher.ComputeHash(a.Card, a.Tools);
            var hashB = AgentTrustHasher.ComputeHash(b.Card, b.Tools);

            Assert.AreEqual(hashA, hashB,
                "il modello sta fuori dall'impronta: cambiarlo non revoca la fiducia");
        }

        [TestMethod]
        public void Still_break_the_fingerprint_when_the_tools_change()
        {
            // Controprova: ciò che DEVE far decadere la fiducia continua a farlo.
            var parser = new YamlAgentCardParser();
            var a = parser.GetDescriptor(Card("runtime:\n  model: m\n"));
            var withShell = Card("runtime:\n  model: m\n").Replace("tools: [read, write]", "tools: [read, write, shell]");
            var b = parser.GetDescriptor(withShell);

            Assert.AreNotEqual(
                AgentTrustHasher.ComputeHash(a.Card, a.Tools),
                AgentTrustHasher.ComputeHash(b.Card, b.Tools),
                "aggiungere 'shell' ai tool deve continuare a far decadere il trust");
        }
    }
}
