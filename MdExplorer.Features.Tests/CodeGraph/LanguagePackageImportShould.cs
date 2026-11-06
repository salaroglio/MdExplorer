using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// «Importa linguaggio…» (B6d): da una cartella o da uno zip in <c>.mde-languages/&lt;id&gt;/</c>; tutto letto prima di
    /// copiare, niente di rotto o pericoloso entra, uno che c'è già si sostituisce solo con la conferma.
    /// </summary>
    [TestClass]
    public class LanguagePackageImportShould
    {
        private const string Skill = "---\nname: toy-code\ndescription: Come leggere il codice Toy.\n---\n\nUn PART è un blocco.\n";

        private string _root, _project, _outside;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-import-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "progetto");
            _outside = Path.Combine(_root, "fuori");
            Directory.CreateDirectory(_project);
            Directory.CreateDirectory(_outside);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(_root, true); } catch (IOException) { }
        }

        private static string Rules(int version = 1) => DeclarativeLanguageExtractorShould.ToyDefinition.Replace("version: 1", "version: " + version);

        private static void Write(string folder, string rel, string text)
        {
            var path = Path.Combine(folder, rel.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);
        }

        /// <summary>Un repository come quello vero: README, .git, e il pacchetto nella sottocartella toy/.</summary>
        private string Repository(int version = 1, bool withSkill = true)
        {
            var repo = Path.Combine(_outside, "mde-language-toy-v" + version);
            Write(repo, "README.md", "# repo\n");
            Write(repo, ".git/HEAD", "ref: refs/heads/main\n");
            Write(repo, "toy/language.yml", Rules(version));
            Write(repo, "toy/prompts/code-doc.md", "Un PART n è un blocco numerato.\n");
            if (withSkill) Write(repo, "toy/skills/toy-code/SKILL.md", Skill);
            return repo;
        }

        private string Zip(Dictionary<string, string> entries)
        {
            var path = Path.Combine(_outside, Guid.NewGuid().ToString("N") + ".zip");
            using var zip = ZipFile.Open(path, ZipArchiveMode.Create);
            foreach (var (name, text) in entries)
            {
                using var w = new StreamWriter(zip.CreateEntry(name).Open());
                w.Write(text);
            }
            return path;
        }

        private string Installed(string rel) => Path.Combine(DeclarativeLanguage.FolderOf(_project), rel.Replace('/', Path.DirectorySeparatorChar));

        [TestMethod]
        public void Import_a_package_from_a_repository_folder()
        {
            var result = LanguagePackageImport.Import(_project, Repository(), replace: false);
            Assert.AreEqual("toy", result.Id);
            Assert.AreEqual(1, result.Version);
            Assert.IsFalse(result.Replaced);
            CollectionAssert.AreEqual(new[] { "toy-code" }, result.Skills.ToList());
            Assert.IsTrue(result.HasPrompt);
            Assert.IsTrue(File.Exists(Installed("toy/language.yml")));
            Assert.IsTrue(File.Exists(Installed("toy/skills/toy-code/SKILL.md")));
            Assert.IsFalse(Directory.Exists(Installed("toy/.git")), "la .git del repository non è del pacchetto");
            Assert.IsNotNull(CodeLanguageCatalog.ForProject(_project).ById("toy"), "subito nel catalogo");
            Assert.IsFalse(Directory.EnumerateDirectories(DeclarativeLanguage.FolderOf(_project), ".import-*").Any(), "nessuna cartella di passaggio rimasta");
        }

        [TestMethod]
        public void Import_a_package_from_a_zip_at_the_root_or_in_one_folder()
        {
            var flat = Zip(new() { ["language.yml"] = Rules(), ["skills/toy-code/SKILL.md"] = Skill });
            Assert.AreEqual("toy", LanguagePackageImport.Import(_project, flat, replace: false).Id, "alla radice: l'id dalle regole");
            Directory.Delete(Installed("toy"), true);
            var nested = Zip(new() { ["README.md"] = "x", ["toy/language.yml"] = Rules(), ["toy/prompts/code-doc.md"] = "Toy.\n" });
            Assert.IsTrue(LanguagePackageImport.Import(_project, nested, replace: false).HasPrompt);
        }

        [DataTestMethod]
        [DataRow("toy/../../evil.md", "..")]
        [DataRow("toy/run.ps1", "run.ps1")]
        [DataRow("altro/language.yml", "altro")]
        public void Refuse_a_zip_that_is_not_a_clean_package(string extra, string why)
        {
            var zip = Zip(new() { ["toy/language.yml"] = Rules(), [extra] = "x" });
            var ex = Assert.ThrowsException<LanguageDefinitionException>(() => LanguagePackageImport.Import(_project, zip, replace: false));
            StringAssert.Contains(ex.Message, why);
            Assert.IsFalse(Directory.Exists(Installed("toy")), "niente copiato");
        }

        [TestMethod]
        public void Refuse_a_broken_package_or_one_that_takes_another_language_extension()
        {
            var repo = Repository();
            Write(repo, "toy/skills/toy-code/SKILL.md", "---\nname: toy-code\n---\n");
            var ex = Assert.ThrowsException<LanguageDefinitionException>(() => LanguagePackageImport.Import(_project, repo, replace: false));
            StringAssert.EndsWith(ex.DefinitionPath, "SKILL.md", "l'errore nomina il file del pacchetto da importare");
            Assert.IsFalse(Directory.Exists(DeclarativeLanguage.FolderOf(_project)), "un pacchetto rotto non entra");

            var cs = Repository(2);
            Write(cs, "toy/language.yml", Rules().Replace("extensions: [.toy, .toh, .tor, .tos]", "extensions: [.toy, .cs]"));
            StringAssert.Contains(Assert.ThrowsException<LanguageDefinitionException>(() => LanguagePackageImport.Import(_project, cs, replace: false)).Message, ".cs");

            var two = Path.Combine(_outside, "due");
            Write(two, "toy/language.yml", Rules());
            Write(two, "zeta/language.yml", Rules().Replace("id: toy", "id: zeta"));
            StringAssert.Contains(Assert.ThrowsException<LanguageDefinitionException>(() => LanguagePackageImport.Import(_project, two, replace: false)).Message, "toy, zeta",
                "due pacchetti nella stessa cartella: si sceglie quello giusto, non uno a caso");
            StringAssert.Contains(Assert.ThrowsException<LanguageDefinitionException>(() => LanguagePackageImport.Import(_project, _outside, replace: false)).Message, "language.yml",
                "una cartella senza pacchetto lo dice");
        }

        [TestMethod]
        public void Replace_an_existing_package_only_when_asked()
        {
            LanguagePackageImport.Import(_project, Repository(1), replace: false);
            var newer = Repository(2, withSkill: false);
            var ex = Assert.ThrowsException<LanguagePackageExistsException>(() => LanguagePackageImport.Import(_project, newer, replace: false));
            Assert.AreEqual((1, 2), (ex.ExistingVersion, ex.ImportedVersion));
            StringAssert.Contains(File.ReadAllText(Installed("toy/language.yml")), "version: 1", "senza conferma niente cambia");

            var result = LanguagePackageImport.Import(_project, newer, replace: true);
            Assert.IsTrue(result.Replaced);
            StringAssert.Contains(File.ReadAllText(Installed("toy/language.yml")), "version: 2");
            Assert.IsFalse(Directory.Exists(Installed("toy/skills")), "si sostituisce il pacchetto intero: la skill tolta nella versione nuova sparisce");
        }
    }
}
