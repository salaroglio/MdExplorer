using MdExplorer.Features.Services.AI.CopilotSdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// The Copilot SDK session does not discover a project's .github/skills by itself (measured on
    /// 1.0.11: only Copilot's built-in skills were listed), so MarkAgent never saw MdExplorer's
    /// skills. The session is told where they are.
    /// </summary>
    [TestClass]
    public class CopilotSdkSkillDirectories_Should
    {
        private string _project;

        [TestInitialize]
        public void Init()
        {
            _project = Path.Combine(Path.GetTempPath(), "mde-skilldirs-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_project);
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_project, recursive: true);

        [TestMethod]
        public void Point_the_session_at_the_projects_github_skills()
        {
            var skills = Path.Combine(_project, ".github", "skills");
            Directory.CreateDirectory(skills);

            CollectionAssert.AreEqual(new[] { skills }, CopilotSdkSkillDirectories());

            string[] CopilotSdkSkillDirectories() => CopilotSdkSession.SkillDirectoriesFor(_project).ToArray();
        }

        [TestMethod]
        public void Name_no_folder_when_the_project_has_no_skills()
        {
            Assert.AreEqual(0, CopilotSdkSession.SkillDirectoriesFor(_project).Count);
            Assert.AreEqual(0, CopilotSdkSession.SkillDirectoriesFor(null).Count);
        }
    }
}
