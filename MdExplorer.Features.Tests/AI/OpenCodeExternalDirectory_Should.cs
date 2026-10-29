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
    }
}
