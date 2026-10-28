using System;
using System.Linq;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Quanti agenti insieme: una domanda sola, una manopola sola.
    /// <para>
    /// Il tetto dei run concorrenti e il numero di posti di lavoro rispondevano entrambi a
    /// «quanti agenti possono lavorare insieme su questa macchina», ed erano indipendenti. Con
    /// quattro posti e un tetto di due, due agenti restavano parcheggiati davanti a due
    /// scrivanie libere. Nel verso opposto era peggio: ammessi più agenti dei posti, l'ultimo
    /// non trovava dove sedersi e il run falliva invece di aspettare il turno.
    /// </para>
    /// <para>
    /// Il cancello si costruisce a mano: il contenitore dei test lo sostituisce con un doppio
    /// che ammette sempre, per poter esercitare la coda differita senza dipendere dalle risorse.
    /// </para>
    /// </summary>
    [TestClass]
    public class AgentRunGateSlots_Should
    {
        [TestMethod]
        public void Admit_exactly_as_many_agents_as_there_are_workplaces()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("gate-posti");
            var pref = ctx.Factory.Services.GetRequiredService<IAgentWorktreePreference>();
            var gate = new CopilotResourceGate(pref);

            pref.Set(path, true);
            pref.SetSlots(path, 3);

            var admitted = Enumerable.Range(0, 3).Select(i => gate.TryEnter(path, "a" + i)).ToList();
            Assert.IsTrue(admitted.All(d => d.Admitted), "tre posti, tre agenti");

            // Il quarto non fallisce: aspetta il turno, ed è la differenza tra una coda e un errore.
            var fourth = gate.TryEnter(path, "a3");
            Assert.IsFalse(fourth.Admitted);

            admitted[0].Slot.Dispose();
            Assert.IsTrue(gate.TryEnter(path, "a3").Admitted, "liberato un posto, il quarto entra");
        }

        [TestMethod]
        public void Keep_the_machine_default_when_there_are_no_workplaces_at_all()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("gate-senza-isolamento");
            var pref = ctx.Factory.Services.GetRequiredService<IAgentWorktreePreference>();
            var gate = new CopilotResourceGate(pref);

            // Senza isolamento gli agenti girano nel progetto: di scrivanie non ce ne sono, e
            // il numero configurato non descrive più niente.
            pref.Set(path, false);
            pref.SetSlots(path, 8);

            var admitted = Enumerable.Range(0, CopilotResourceGate.DefaultMaxConcurrent)
                .Select(i => gate.TryEnter(path, "a" + i)).ToList();
            Assert.IsTrue(admitted.All(d => d.Admitted));
            Assert.IsFalse(gate.TryEnter(path, "extra").Admitted,
                "resta il tetto della macchina, non gli otto posti che nessuno userà");
        }

        [TestMethod]
        public void Notice_that_the_number_changed_in_the_settings()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("gate-cambio");
            var pref = ctx.Factory.Services.GetRequiredService<IAgentWorktreePreference>();
            var gate = new CopilotResourceGate(pref);

            pref.Set(path, true);
            pref.SetSlots(path, 1);
            Assert.IsTrue(gate.TryEnter(path, "a0").Admitted);
            Assert.IsFalse(gate.TryEnter(path, "a1").Admitted, "un posto solo");

            // Alzato il numero nelle impostazioni, il cancello deve adeguarsi subito: un tetto
            // che resta quello di ieri finché non si riavvia è indistinguibile da un bug.
            pref.SetSlots(path, 3);
            Assert.IsTrue(gate.TryEnter(path, "a1").Admitted);
        }

        [TestMethod]
        public void Count_each_project_on_its_own()
        {
            using var ctx = new AgentCityContext();
            var (_, uno) = ctx.SeedProject("gate-uno");
            var (_, due) = ctx.SeedProject("gate-due");
            var pref = ctx.Factory.Services.GetRequiredService<IAgentWorktreePreference>();
            var gate = new CopilotResourceGate(pref);

            pref.Set(uno, true); pref.SetSlots(uno, 1);
            pref.Set(due, true); pref.SetSlots(due, 1);

            Assert.IsTrue(gate.TryEnter(uno, "a").Admitted);
            Assert.IsFalse(gate.TryEnter(uno, "b").Admitted);

            // I posti sono del progetto: quelli pieni di uno non chiudono la porta all'altro.
            Assert.IsTrue(gate.TryEnter(due, "a").Admitted);
        }
    }
}
