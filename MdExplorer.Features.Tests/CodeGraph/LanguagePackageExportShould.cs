using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// «Esporta linguaggio…» (10/11/2026): il pacchetto del progetto in uno zip che l'import accetta così com'è; uno rotto non
    /// esce, e uno zip con lo stesso nome ma un contenuto diverso non si sovrascrive.
    /// </summary>
    [TestClass]
    public class LanguagePackageExportShould
    {
        private const string Skill = "---\nname: toy-code\ndescription: Come leggere il codice Toy.\n---\n\nUn PART è un blocco.\n";

        private string _root, _project, _other, _out;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-export-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "progetto");
            _other = Path.Combine(_root, "altro-progetto");
            _out = Path.Combine(_root, "Downloads");
            foreach (var d in new[] { _project, _other, _out }) Directory.CreateDirectory(d);
            Write("toy/language.yml", DeclarativeLanguageExtractorShould.ToyDefinition);
            Write("toy/README.md", "# Toy\n");
            Write("toy/prompts/code-doc.md", "Un PART n è un blocco numerato.\n");
            Write("toy/skills/toy-code/SKILL.md", Skill);
            Write("toy/skills/toy-code/parole.md", "Le parole del linguaggio, con le accentate: è, à.\n");
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(_root, true); } catch (IOException) { }
        }

        private string Write(string rel, string text)
        {
            var path = Path.Combine(DeclarativeLanguage.FolderOf(_project), rel.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);
            return path;
        }

        private static string[] FilesOf(string folder) => Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(folder, f).Replace('\\', '/') + "=" + File.ReadAllText(f)).OrderBy(x => x, StringComparer.Ordinal).ToArray();

        [TestMethod]
        public void Export_a_zip_that_the_import_takes_back_identical()
        {
            var result = LanguagePackageExport.Export(_project, "toy", _out);
            Assert.AreEqual(Path.Combine(_out, "mde-language-toy-v1.zip"), result.ZipPath);
            Assert.AreEqual(5, result.Files);
            Assert.IsFalse(result.Unchanged);
            using (var zip = ZipFile.OpenRead(result.ZipPath))
                Assert.IsTrue(zip.Entries.All(e => e.FullName.StartsWith("toy/")), "il pacchetto in una cartella toy/, come lo zip distribuito");

            var imported = LanguagePackageImport.Import(_other, result.ZipPath, replace: false);
            Assert.AreEqual("toy", imported.Id);
            CollectionAssert.AreEqual(FilesOf(Path.Combine(DeclarativeLanguage.FolderOf(_project), "toy")), FilesOf(imported.Folder), "andata e ritorno: gli stessi file, accentate comprese");
        }

        [TestMethod]
        public void Leave_an_identical_zip_and_refuse_to_overwrite_a_different_one()
        {
            var first = LanguagePackageExport.Export(_project, "toy", _out);
            var before = File.ReadAllBytes(first.ZipPath);
            Assert.IsTrue(LanguagePackageExport.Export(_project, "toy", _out).Unchanged, "stesso contenuto, stesso zip: niente da riscrivere");

            Write("toy/prompts/code-doc.md", "Un PART n è un blocco, chiamato con RUN PART n.\n");
            var ex = Assert.ThrowsException<LanguagePackageExportExistsException>(() => LanguagePackageExport.Export(_project, "toy", _out));
            StringAssert.Contains(ex.Message, "version", "dice di alzare la versione");
            CollectionAssert.AreEqual(before, File.ReadAllBytes(first.ZipPath), "lo zip di prima resta com'era");

            Write("toy/language.yml", DeclarativeLanguageExtractorShould.ToyDefinition.Replace("version: 1", "version: 2"));
            Assert.AreEqual(Path.Combine(_out, "mde-language-toy-v2.zip"), LanguagePackageExport.Export(_project, "toy", _out).ZipPath, "con la versione alzata, uno zip nuovo accanto");
            Assert.AreEqual(2, Directory.GetFiles(_out).Length, "nessun file di passaggio rimasto");
        }

        [TestMethod]
        public void Carry_the_findings_an_agent_wrote_next_to_the_rules()
        {
            // quello che un agente rileva sul codice vero viaggia con il pacchetto: uno zip solo, andata e ritorno
            Write("toy/findings/rilevazione-2026-11-11.md", "# Rilevazione\n\n12 file, 3 forme di chiamata: è tutto qui.\n");
            var package = LanguagePackage.Load(Path.Combine(DeclarativeLanguage.FolderOf(_project), "toy"));
            CollectionAssert.AreEqual(new[] { "rilevazione-2026-11-11.md" }, package.Findings.ToArray());
            Assert.AreEqual(1, package.Skills.Count, "una rilevazione non è una skill");

            var result = LanguagePackageExport.Export(_project, "toy", _out);
            Assert.AreEqual(6, result.Files);
            var imported = LanguagePackageImport.Import(_other, result.ZipPath, replace: false);
            CollectionAssert.AreEqual(FilesOf(Path.Combine(DeclarativeLanguage.FolderOf(_project), "toy")), FilesOf(imported.Folder));

            Write("toy/findings/sorgente.txt", "non è una rilevazione");
            StringAssert.Contains(Assert.ThrowsException<LanguageDefinitionException>(() => LanguagePackageExport.Export(_project, "toy", _out)).Message, "findings/*.md");
        }

        [TestMethod]
        public void Refuse_a_broken_or_missing_package()
        {
            Write("toy/skills/toy-code/SKILL.md", "---\nname: toy-code\n---\n");
            Assert.ThrowsException<LanguageDefinitionException>(() => LanguagePackageExport.Export(_project, "toy", _out));
            Assert.AreEqual(0, Directory.GetFiles(_out).Length, "un pacchetto rotto non esce");
            StringAssert.Contains(Assert.ThrowsException<DirectoryNotFoundException>(() => LanguagePackageExport.Export(_project, "zeta", _out)).Message, ".mde-languages/zeta/");
            Assert.ThrowsException<DirectoryNotFoundException>(() => LanguagePackageExport.Export(_project, "toy", Path.Combine(_root, "non-c-e")));
        }
    }
}
