using System.IO;
using MdExplorer.Features.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests
{
    /// <summary>Il controllo «è dentro?» unico (sprint Radici-Di-Codice-Esterne, F0): niente cartelle sorelle, niente «..».</summary>
    [TestClass]
    public class PathContainmentShould
    {
        private static readonly string Root = Path.Combine(Path.GetTempPath(), "mde-contain", "proj");

        [TestMethod]
        public void Accept_the_root_and_what_is_under_it()
        {
            Assert.IsTrue(PathContainment.IsInside(Root, Root));
            Assert.IsTrue(PathContainment.IsInside(Root + Path.DirectorySeparatorChar, Root));
            Assert.IsTrue(PathContainment.IsInside(Root, Path.Combine(Root, "a", "b.md")));
            Assert.IsTrue(PathContainment.IsInside(Root, "a/b.md"), "relativo alla radice");
            Assert.AreEqual("a/b.md", PathContainment.RelativeOrNull(Root, Path.Combine(Root, "a", "b.md")));
        }

        [TestMethod]
        public void Refuse_a_sibling_folder_with_the_same_prefix()
        {
            Assert.IsFalse(PathContainment.IsInside(Root, Root + "2"), "/proj2 non è dentro /proj");
            Assert.IsFalse(PathContainment.IsInside(Root, Path.Combine(Root + "2", "x.md")));
            Assert.IsNull(PathContainment.RelativeOrNull(Root, Root + "2"));
        }

        [TestMethod]
        public void Refuse_escapes_with_dot_dot_and_empty_roots()
        {
            Assert.IsFalse(PathContainment.IsInside(Root, "../fuori.md"));
            Assert.IsFalse(PathContainment.IsInside(Root, Path.Combine(Root, "a", "..", "..", "fuori.md")));
            Assert.IsFalse(PathContainment.IsInside("", Root));
            Assert.IsFalse(PathContainment.IsInside(Root, ""));
        }
    }
}
