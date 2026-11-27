using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La registrazione MCP di opencode va dove opencode la legge (27/11/2026): con XDG_CONFIG_HOME impostata MDE scriveva in
    /// ~/.config/opencode e opencode non vedeva il server di MDE.
    /// </summary>
    [TestClass]
    public class OpenCodeConfigDirectory_Should
    {
        private static readonly string Home = Path.Combine(Path.GetTempPath(), "casa-mario");
        private static readonly string Xdg = Path.Combine(Path.GetTempPath(), "config-mario");

        [TestMethod]
        public void Follow_XDG_CONFIG_HOME_when_it_is_set()
            => Assert.AreEqual(Path.Combine(Xdg, "opencode"), MdExplorer.Service.ProjectsManager.OpenCodeConfigDirectory(Xdg, Home));

        [TestMethod]
        public void Use_the_config_folder_in_the_home_otherwise()
        {
            Assert.AreEqual(Path.Combine(Home, ".config", "opencode"), MdExplorer.Service.ProjectsManager.OpenCodeConfigDirectory(null, Home));
            Assert.AreEqual(Path.Combine(Home, ".config", "opencode"), MdExplorer.Service.ProjectsManager.OpenCodeConfigDirectory("  ", Home));
            Assert.AreEqual(Path.Combine(Home, ".config", "opencode"), MdExplorer.Service.ProjectsManager.OpenCodeConfigDirectory("relativa", Home),
                "un percorso relativo non vale (specifica XDG)");
        }
    }
}
