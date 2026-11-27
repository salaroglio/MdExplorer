using System.IO;
using MdExplorer.Features.Services.AI.OpenCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.AI
{
    /// <summary>
    /// Il permesso <c>external_directory</c> di opencode nella chat di MarkAgent: «once» dentro le radici di codice esterne
    /// collegate, «reject» fuori. Senza risposta il turno resta fermo per sempre (misurato il 29/10/2026).
    /// </summary>
    [TestClass]
    public class OpenCodeExternalDirectory_Should
    {
        private static readonly string Root = Path.Combine(Path.GetTempPath(), "mde-oc-roots", "globo");

        [TestMethod]
        public void Allow_only_the_folders_inside_a_linked_root()
        {
            var roots = new[] { Root };
            Assert.IsTrue(OpenCodeSession.InsideRoots(new[] { Path.Combine(Root, "core") + "/*", Path.Combine(Root, "core", "A.java") }, roots));
            Assert.IsTrue(OpenCodeSession.InsideRoots(new[] { Root + "/*" }, roots), "la radice stessa");
            Assert.IsFalse(OpenCodeSession.InsideRoots(new[] { Root + "2/*" }, roots), "una cartella che comincia allo stesso modo non è dentro");
            Assert.IsFalse(OpenCodeSession.InsideRoots(new[] { Path.Combine(Root, "core") + "/*", Path.Combine(Root, "..", "altro", "x.txt") }, roots), "basta un percorso fuori");
            Assert.IsFalse(OpenCodeSession.InsideRoots(new[] { Path.Combine(Root, "core") + "/*" }, new string[0]), "nessuna radice collegata");
            Assert.IsFalse(OpenCodeSession.InsideRoots(new string[0], roots), "nessun percorso chiesto");
        }

        [TestMethod]
        public void Answer_every_other_ask_with_a_refusal_that_says_why()
        {
            var roots = new[] { Root };
            Assert.AreEqual(("once", (string)null), OpenCodeSession.ChatReply("external_directory", new[] { Root + "/*" }, roots));
            foreach (var permission in new[] { "external_directory", "doom_loop", "bash", "webfetch" })
            {
                var (response, note) = OpenCodeSession.ChatReply(permission, new[] { "/altrove/*" }, roots);
                Assert.AreEqual("reject", response, permission + ": mai senza risposta, che fermerebbe il turno");
                StringAssert.StartsWith(note, permission + " · ", "la chat dice che cosa ha negato");
            }
        }

        [TestMethod]
        public void Refuse_edits_inside_a_code_root_with_what_to_use_and_allow_the_others()
        {
            var roots = new[] { Root };
            var (response, note) = OpenCodeSession.ChatReply("edit", new[] { "tmp/x" }, roots, filePath: Path.Combine(Root, "core", "A.java"));
            Assert.AreEqual("reject", response);
            StringAssert.Contains(note, "remote_edit", "il messaggio dice al modello che cosa usare");
            Assert.AreEqual(("once", (string)null), OpenCodeSession.ChatReply("edit", new[] { "doc.md" }, roots, filePath: "/progetto/doc.md"), "fuori dalle radici: come il default di opencode");
            Assert.AreEqual(("once", (string)null), OpenCodeSession.ChatReply("edit", new string[0], roots), "senza percorso: non è una radice");
        }

        [TestMethod]
        public void Answer_the_web_with_the_choice_of_the_project()
        {
            foreach (var permission in OpenCodeSession.WebPermissions)
            {
                Assert.AreEqual(("once", (string)null), OpenCodeSession.ChatReply(permission, new[] { "https://example.com" }, null, allowWeb: true), permission);
                var (response, note) = OpenCodeSession.ChatReply(permission, new[] { "https://example.com" }, null, allowWeb: false);
                Assert.AreEqual("reject", response, permission);
                StringAssert.StartsWith(note, permission + " · ");
            }
            Assert.AreEqual("reject", OpenCodeSession.ChatReply("bash", new[] { "*" }, null, allowWeb: true).Response, "il web non apre altro");
        }
    }
}
