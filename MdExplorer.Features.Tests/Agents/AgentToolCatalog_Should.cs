using System.Linq;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Fase B del piano runner-agnostico: il catalogo dei tool derivato dal manifesto
    /// <c>tools:</c> incrociato col trust. È il punto in cui l'harness smette di <i>sperare</i>
    /// che il modello di autorizzazione a tre ruoli venga rispettato e lo <b>impone</b>.
    /// </summary>
    [TestClass]
    public class AgentToolCatalog_Should
    {
        private static readonly string[] AllOffered =
        {
            "read_markdown_file", "search_documents", "list_agents", "query_agent_memory",
            "create_markdown_file", "update_markdown_file", "assert_learned_fact",
            "send_agent_message", "request_intervention",
        };

        private static ToolAccess AccessOf(string tool, string[] manifest, bool trusted)
            => AgentToolCatalog.Decide(AllOffered, manifest, trusted).First(d => d.Tool == tool).Access;

        [TestMethod]
        public void Always_allow_reading_even_without_trust_or_manifest()
        {
            // La cittadinanza è read-only: leggere non si nega a nessun cittadino.
            foreach (var tool in new[] { "read_markdown_file", "search_documents", "list_agents", "query_agent_memory" })
                Assert.AreEqual(ToolAccess.Allowed, AccessOf(tool, new string[0], trusted: false),
                    $"'{tool}' è sola lettura e va concesso");
        }

        [TestMethod]
        public void Deny_writing_when_the_manifest_does_not_declare_it()
        {
            // Fidato ma senza dichiarazione: la fiducia autorizza ciò che l'agente ha DICHIARATO,
            // non tutto. Senza dichiarazione non c'è nulla da autorizzare.
            Assert.AreEqual(ToolAccess.Denied, AccessOf("create_markdown_file", new[] { "read" }, trusted: true));
            Assert.AreEqual(ToolAccess.Denied, AccessOf("assert_learned_fact", new[] { "read", "search" }, trusted: true));
        }

        [TestMethod]
        public void Allow_writing_only_when_declared_and_trusted()
        {
            Assert.AreEqual(ToolAccess.Allowed, AccessOf("update_markdown_file", new[] { "read", "write" }, trusted: true));
            Assert.AreEqual(ToolAccess.Allowed, AccessOf("update_markdown_file", new[] { "edit" }, trusted: true),
                "'edit' vale quanto 'write' come dichiarazione di scrittura");
        }

        [TestMethod]
        public void Ask_for_approval_when_intent_is_declared_but_trust_is_missing()
        {
            // Intenzione nota, autorizzazione mancante: non sparire in silenzio, chiedere.
            Assert.AreEqual(ToolAccess.ApprovalRequired,
                AccessOf("create_markdown_file", new[] { "write" }, trusted: false));
        }

        [TestMethod]
        public void Gate_outbound_actions_on_trust_alone()
        {
            // Svegliare un collega o consumare budget federato non ha una parola nel manifesto:
            // il cancello è la fiducia. Un agente non fidato non parla per la città.
            Assert.AreEqual(ToolAccess.ApprovalRequired,
                AccessOf("send_agent_message", new[] { "read", "write" }, trusted: false));
            Assert.AreEqual(ToolAccess.ApprovalRequired,
                AccessOf("request_intervention", new string[0], trusted: false));

            Assert.AreEqual(ToolAccess.Allowed, AccessOf("send_agent_message", new string[0], trusted: true));
            Assert.AreEqual(ToolAccess.Allowed, AccessOf("request_intervention", new string[0], trusted: true));
        }

        [TestMethod]
        public void Deny_a_tool_it_does_not_know()
        {
            // Fail-closed. Se domani il server MCP espone un tool nuovo, non deve finire in mano
            // agli agenti solo perché è comparso: qualcuno deve classificarlo prima.
            var d = AgentToolCatalog.Decide(new[] { "delete_everything" }, new[] { "write" }, trusted: true).Single();
            Assert.AreEqual(ToolAccess.Denied, d.Access);
            StringAssert.Contains(d.Reason, "sconosciuto");
        }

        [TestMethod]
        public void Report_a_declared_capability_the_runner_cannot_provide()
        {
            // Un agente istruito a usare la shell, eseguito da un runner che non ce l'ha, non
            // fallisce: fa una cosa diversa e la chiama fatta. Va detto.
            var missing = AgentToolCatalog.UnsupportedCapabilities(
                new[] { "read", "write", "shell" }, runnerHasShell: false);

            Assert.AreEqual(1, missing.Count);
            Assert.AreEqual(AgentToolCatalog.ManifestShell, missing[0].Tool);
            Assert.AreEqual(ToolAccess.Unsupported, missing[0].Access);

            Assert.AreEqual(0, AgentToolCatalog.UnsupportedCapabilities(
                new[] { "read", "write", "shell" }, runnerHasShell: true).Count,
                "su un runner con shell non manca nulla");
        }

        [TestMethod]
        public void Expose_only_the_invocable_names()
        {
            var decisions = AgentToolCatalog.Decide(AllOffered, new[] { "read" }, trusted: false);
            var allowed = AgentToolCatalog.AllowedNames(decisions);

            CollectionAssert.AreEquivalent(
                new[] { "read_markdown_file", "search_documents", "list_agents", "query_agent_memory" },
                allowed.ToArray(),
                "un agente non fidato e senza dichiarazione di scrittura vede solo la lettura");
        }

        // ── Strumenti NATIVI del motore: il manifesto è un limite, non un'intenzione ──

        [TestMethod]
        public void Deny_shell_and_write_when_the_manifest_only_reads()
        {
            // L'agente con `tools: [read, search]` ha eseguito comandi di shell nel collaudo del
            // 03/10/2026: il manifesto arrivava fino al runner e lì veniva ignorato.
            CollectionAssert.AreEquivalent(new[] { "shell", "write" },
                AgentToolCatalog.NativeToolsToDeny(new[] { "read", "search" }).ToArray());
        }

        [TestMethod]
        public void Deny_everything_native_for_a_citizen_with_an_empty_manifest()
        {
            CollectionAssert.AreEquivalent(new[] { "shell", "write" },
                AgentToolCatalog.NativeToolsToDeny(new string[0]).ToArray(),
                "la cittadinanza è read-only: senza dichiarazione non si scrive e non si esegue");
        }

        [TestMethod]
        public void Leave_write_open_when_the_agent_declared_edit_or_write()
        {
            CollectionAssert.AreEquivalent(new[] { "shell" },
                AgentToolCatalog.NativeToolsToDeny(new[] { "read", "edit" }).ToArray(),
                "edit vale come write: modificare un file è scriverlo");
            CollectionAssert.AreEquivalent(new[] { "shell" },
                AgentToolCatalog.NativeToolsToDeny(new[] { "WRITE" }).ToArray(), "senza distinguere maiuscole");
        }

        [TestMethod]
        public void Leave_shell_open_when_the_agent_declared_shell_or_execute()
        {
            CollectionAssert.AreEquivalent(new[] { "write" },
                AgentToolCatalog.NativeToolsToDeny(new[] { "shell" }).ToArray());
            CollectionAssert.AreEquivalent(new[] { "write" },
                AgentToolCatalog.NativeToolsToDeny(new[] { "execute" }).ToArray());
            Assert.AreEqual(0, AgentToolCatalog.NativeToolsToDeny(new[] { "shell", "write" }).Count);
        }

        [TestMethod]
        public void Deny_nothing_when_there_is_no_manifest_at_all()
        {
            // Un agente senza manifesto (schedulato, non cittadino) gira come prima: il limite
            // vale per chi ha una dichiarazione da far rispettare, non retroattivamente per tutti.
            Assert.AreEqual(0, AgentToolCatalog.NativeToolsToDeny(null).Count);
        }
    }
}
