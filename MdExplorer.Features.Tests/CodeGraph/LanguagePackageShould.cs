using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Il linguaggio come pacchetto modulare (B6a): una cartella <c>.mde-languages/&lt;id&gt;/</c> con le regole, il prompt
    /// dedicato a Mark e le skill per gli agenti, letta in modo rigido; il file piatto di prima si sposta una volta sola.
    /// </summary>
    [TestClass]
    public class LanguagePackageShould
    {
        private const string Skill = "---\nname: toy-code\ndescription: Come leggere il codice Toy con i code_*.\n---\n\n# Toy\n\nUn PART è un blocco numerato.\n";

        private string _project, _languages;

        [TestInitialize]
        public void Setup()
        {
            _project = Path.Combine(Path.GetTempPath(), "mde-pkg-tests", Guid.NewGuid().ToString("N"));
            _languages = DeclarativeLanguage.FolderOf(_project);
            Directory.CreateDirectory(_languages);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(Path.GetDirectoryName(_project), true); } catch (IOException) { }
        }

        private string Write(string rel, string text)
        {
            var path = Path.Combine(_languages, rel.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);
            return path;
        }

        private string FullPackage()
        {
            Write("toy/language.yml", DeclarativeLanguageExtractorShould.ToyDefinition);
            Write("toy/README.md", "# Toy\n");
            Write("toy/prompts/code-doc.md", "Un PART n è un blocco chiamato con RUN PART n.\n");
            Write("toy/skills/toy-code/SKILL.md", Skill);
            return Write("toy/skills/toy-code/riferimento.md", "Le parole del linguaggio.\n");
        }

        [TestMethod]
        public void Read_rules_prompt_and_skills_of_a_package()
        {
            FullPackage();
            var catalog = CodeLanguageCatalog.ForProject(_project);
            Assert.AreEqual(0, catalog.Errors.Count, string.Join("\n", catalog.Errors.Values.Select(e => e.Message)));
            var package = catalog.ById("toy").Package;
            Assert.AreEqual("toy", package.Language.Id);
            Assert.AreEqual("Un PART n è un blocco chiamato con RUN PART n.", package.CodeDocPrompt);
            Assert.AreEqual(8, package.CodeDocPromptHash.Length);
            var skill = package.Skills.Single();
            Assert.AreEqual("toy-code", skill.Name);
            Assert.AreEqual("Come leggere il codice Toy con i code_*.", skill.Description);
            CollectionAssert.AreEquivalent(new[] { "SKILL.md", "riferimento.md" }, skill.Files.Keys.ToList(), "i file della skill viaggiano con lei");
            Assert.AreSame(package.Language, catalog.ById("toy").Definition);
        }

        [TestMethod]
        public void Change_the_prompt_fingerprint_when_the_prompt_changes()
        {
            FullPackage();
            var before = LanguagePackage.Load(Path.Combine(_languages, "toy")).CodeDocPromptHash;
            Write("toy/prompts/code-doc.md", "Un PART n è un blocco numerato.\n");
            Assert.AreNotEqual(before, LanguagePackage.Load(Path.Combine(_languages, "toy")).CodeDocPromptHash);
            File.Delete(Path.Combine(_languages, "toy", "prompts", "code-doc.md"));
            var bare = LanguagePackage.Load(Path.Combine(_languages, "toy"));
            Assert.IsNull(bare.CodeDocPrompt, "prompt e skill sono facoltativi");
            Assert.IsNull(bare.CodeDocPromptHash);
        }

        [DataTestMethod]
        [DataRow("toy/script.ps1", "x", "script.ps1", "README.md, prompts/code-doc.md")]
        [DataRow("toy/prompts/altro.md", "x", "altro.md", "README.md, prompts/code-doc.md")]
        [DataRow("toy/prompts/code-doc.md", "  \n", "code-doc.md", "prompt")]
        [DataRow("toy/skills/code/SKILL.md", "---\nname: code\ndescription: x\n---\n", "SKILL.md", "'toy-'")]
        [DataRow("toy/skills/toy-code/SKILL.md", "---\nname: toy-altro\ndescription: x\n---\n", "SKILL.md", "name:")]
        [DataRow("toy/skills/toy-code/SKILL.md", "---\nname: toy-code\n---\n", "SKILL.md", "description:")]
        [DataRow("toy/skills/toy-code/SKILL.md", "---\nname: toy-code\ndescription: x\nmde:\n  origin: mdexplorer\n---\n", "SKILL.md", "mde")]
        [DataRow("toy/skills/toy-code/SKILL.md", "# senza intestazione\n", "SKILL.md", "---")]
        [DataRow("toy/skills/toy-code/note.md", "x", "SKILL.md", "SKILL.md")]
        public void Refuse_a_package_that_does_not_follow_the_format(string rel, string text, string file, string why)
        {
            Write("toy/language.yml", DeclarativeLanguageExtractorShould.ToyDefinition);
            Write(rel, text);
            var catalog = CodeLanguageCatalog.ForProject(_project);
            Assert.IsNull(catalog.ById("toy"), "un pacchetto rotto non entra, nemmeno con le regole buone");
            var error = catalog.Errors["toy"];
            StringAssert.EndsWith(error.DefinitionPath, file, "l'errore nomina il file");
            StringAssert.Contains(error.Reason, why);
        }

        [TestMethod]
        public void Refuse_a_package_without_rules()
        {
            Write("toy/prompts/code-doc.md", "x");
            var error = CodeLanguageCatalog.ForProject(_project).Errors["toy"];
            StringAssert.EndsWith(error.DefinitionPath, "language.yml");
        }

        [TestMethod]
        public void Move_the_old_flat_file_into_its_package_once_and_say_so()
        {
            Write("toy.lang.yml", DeclarativeLanguageExtractorShould.ToyDefinition);
            var catalog = CodeLanguageCatalog.ForProject(_project);
            Assert.IsNotNull(catalog.ById("toy"), "spostato e letto nello stesso giro");
            StringAssert.Contains(catalog.Notices.Single(), "toy/language.yml");
            Assert.IsFalse(File.Exists(Path.Combine(_languages, "toy.lang.yml")));
            Assert.IsTrue(File.Exists(Path.Combine(_languages, "toy", "language.yml")));
            Assert.AreEqual(0, CodeLanguageCatalog.ForProject(_project).Notices.Count, "una volta sola");
        }

        [TestMethod]
        public void Not_merge_a_flat_file_into_an_existing_package()
        {
            FullPackage();
            Write("toy.lang.yml", DeclarativeLanguageExtractorShould.ToyDefinition.Replace("version: 1", "version: 2"));
            var catalog = CodeLanguageCatalog.ForProject(_project);
            Assert.IsNull(catalog.ById("toy"), "due definizioni dello stesso linguaggio: non se ne sceglie una a caso");
            StringAssert.EndsWith(catalog.Errors["toy"].DefinitionPath, "toy.lang.yml");
            Assert.IsTrue(File.Exists(Path.Combine(_languages, "toy.lang.yml")), "niente si sposta né si cancella");
        }
    }
}
