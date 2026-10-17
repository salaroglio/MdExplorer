using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>Punto 3b del 14/10/2026: i ponti fra programmi li dice il build, non i nomi delle classi.</summary>
    [TestClass]
    public class CodeBuildBridgesShould
    {
        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-bridges-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, ".git"));   // la risalita per settings.gradle si ferma qui
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private void Put(string rel, string text)
        {
            var abs = Path.Combine(_root, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(abs));
            File.WriteAllText(abs, text);
        }

        private List<CodeProgramLocation> Programs(params string[] keys) => keys.Select(k => new CodeProgramLocation(k, Path.Combine(_root, k))).ToList();

        private const string PomNs = "xmlns=\"http://maven.apache.org/POM/4.0.0\"";

        [TestMethod]
        public void Read_maven_dependencies_between_program_folders_with_the_inherited_group()
        {
            Put("core/pom.xml", $"<project {PomNs}><parent><groupId>com.acme</groupId><artifactId>parent</artifactId></parent><artifactId>core</artifactId></project>");
            Put("web/pom.xml", $"<project {PomNs}><groupId>com.acme</groupId><artifactId>web</artifactId><dependencies>" +
                "<dependency><groupId>com.acme</groupId><artifactId>core</artifactId></dependency>" +
                "<dependency><groupId>org.slf4j</groupId><artifactId>slf4j-api</artifactId></dependency></dependencies>" +
                "<build><plugins><plugin><dependencies><dependency><groupId>com.acme</groupId><artifactId>web</artifactId></dependency></dependencies></plugin></plugins></build></project>");
            var warnings = new List<string>();

            var bridges = CodeBuildBridges.Find(Programs("core", "web"), warnings);

            Assert.AreEqual(1, bridges.Count, string.Join("; ", bridges));
            Assert.AreEqual(("web", "core", CodeLanguages.Java), (bridges[0].From, bridges[0].To, bridges[0].Language));
            StringAssert.Contains(bridges[0].Evidence, "com.acme:core");
            Assert.AreEqual(0, warnings.Count, string.Join("; ", warnings));
        }

        [TestMethod]
        public void Read_gradle_project_dependencies_with_the_settings_above_the_program_folders()
        {
            Put("settings.gradle", "rootProject.name = 'shop'\ninclude ':core', ':web'\ninclude 'core-utils'\nproject(':legacy').projectDir = file('old/legacy')\ninclude ':legacy'\n");
            Put("core/build.gradle", "dependencies { implementation projects.coreUtils }\n");
            Put("core-utils/build.gradle", "plugins { id 'java' }\n");
            Put("old/legacy/build.gradle", "plugins { id 'java' }\n");
            Put("web/build.gradle.kts", "dependencies {\n  implementation(project(\":core\"))\n  implementation(project(path = \":legacy\"))\n  testImplementation(project(\":missing\"))\n}\n");
            var warnings = new List<string>();

            var bridges = CodeBuildBridges.Find(Programs("core", "web", "core-utils", "old/legacy"), warnings);

            CollectionAssert.AreEquivalent(new[] { "core→core-utils", "web→core", "web→old/legacy" }, bridges.Select(b => b.From + "→" + b.To).ToArray());
            Assert.IsTrue(warnings.Any(w => w.Contains(":missing")), "un project() non incluso si dice: " + string.Join("; ", warnings));
        }

        [TestMethod]
        public void Read_dotnet_project_references()
        {
            Put("src/Acme.Core/Acme.Core.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");
            Put("src/Acme.Web/Acme.Web.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\"><ItemGroup><ProjectReference Include=\"..\\Acme.Core\\Acme.Core.csproj\" /></ItemGroup></Project>");
            var warnings = new List<string>();

            var bridges = CodeBuildBridges.Find(Programs("src/Acme.Core", "src/Acme.Web"), warnings);

            Assert.AreEqual(1, bridges.Count);
            Assert.AreEqual(("src/Acme.Web", "src/Acme.Core", CodeLanguages.CSharp), (bridges[0].From, bridges[0].To, bridges[0].Language));
        }

        [TestMethod]
        public void Make_no_bridge_inside_one_program_nor_towards_folders_that_are_not_programs()
        {
            // un multi-modulo dentro UNA cartella program è un programma solo; un modulo fuori dai program non fa ponte
            Put("app/settings.gradle", "include ':a', ':b'\n");
            Put("app/a/build.gradle", "dependencies { implementation project(':b') }\n");
            Put("app/b/build.gradle", "\n");
            Put("lib/pom.xml", $"<project {PomNs}><groupId>x</groupId><artifactId>lib</artifactId></project>");
            Put("tool/pom.xml", $"<project {PomNs}><groupId>x</groupId><artifactId>tool</artifactId><dependencies><dependency><groupId>x</groupId><artifactId>lib</artifactId></dependency></dependencies></project>");
            var warnings = new List<string>();

            var bridges = CodeBuildBridges.Find(Programs("app", "tool"), warnings);

            Assert.AreEqual(0, bridges.Count, string.Join("; ", bridges));
        }

        [TestMethod]
        public void Say_when_a_build_file_cannot_be_read()
        {
            Put("broken/pom.xml", "<project><artifactId>");
            var warnings = new List<string>();
            CodeBuildBridges.Find(Programs("broken"), warnings);
            Assert.IsTrue(warnings.Any(w => w.Contains("non si legge come XML")), string.Join("; ", warnings));
        }
    }
}
