using System;
using System.IO;
using MdExplorer.Features.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Utilities
{
    /// <summary>La sonda di Pandoc della pagina «Che cosa installare» (19/11/2026): ricerca nel PATH e lettura della versione.</summary>
    [TestClass]
    public class PandocProbe_Should
    {
        [TestMethod]
        public void Read_the_version_from_the_first_line_of_pandoc_version()
        {
            Assert.AreEqual("3.1.11.1", PandocProbe.ParseVersion("pandoc 3.1.11.1\nFeatures: +server +lua\nScripting engine: Lua 5.4\n"));
            Assert.AreEqual("3.6", PandocProbe.ParseVersion("pandoc.exe 3.6\r\nUser data directory: C:\\Users\\mario.rossi\\AppData\\Roaming\\pandoc\r\n"));
            Assert.IsNull(PandocProbe.ParseVersion("command not found"));
            Assert.IsNull(PandocProbe.ParseVersion(null));
        }

        [TestMethod]
        public void Read_the_version_of_git_on_every_system()
        {
            Assert.AreEqual("2.43.0", GitProbe.ParseVersion("git version 2.43.0\n"));
            Assert.AreEqual("2.45.1", GitProbe.ParseVersion("git version 2.45.1.windows.1\r\n"));
            Assert.AreEqual("2.39.3", GitProbe.ParseVersion("git version 2.39.3 (Apple Git-145)\n"));
            Assert.IsNull(GitProbe.ParseVersion("git: command not found"));
            Assert.IsNull(GitProbe.ParseVersion(null));
        }

        [TestMethod]
        public void Find_the_executable_in_the_first_folder_of_the_path_that_has_it()
        {
            var root = Path.Combine(Path.GetTempPath(), "pandoc-probe-" + Guid.NewGuid().ToString("N"));
            var empty = Directory.CreateDirectory(Path.Combine(root, "empty")).FullName;
            var first = Directory.CreateDirectory(Path.Combine(root, "first")).FullName;
            var second = Directory.CreateDirectory(Path.Combine(root, "second")).FullName;
            try
            {
                File.WriteAllText(Path.Combine(first, "pandoc"), "");
                File.WriteAllText(Path.Combine(second, "pandoc"), "");
                var path = string.Join(Path.PathSeparator, "", "  ", "\"" + empty + "\"", first, second);

                Assert.AreEqual(Path.Combine(first, "pandoc"), PandocProbe.FindOnPath(path, "pandoc"));
                Assert.IsNull(PandocProbe.FindOnPath(empty, "pandoc"));
                Assert.IsNull(PandocProbe.FindOnPath(null, "pandoc"));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
