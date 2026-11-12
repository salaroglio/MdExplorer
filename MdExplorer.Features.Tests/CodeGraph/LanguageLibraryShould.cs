using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// La libreria dei linguaggi (12/11/2026): i pacchetti si caricano una volta per computer e si attivano nei progetti.
    /// Attivare = copiarli in .mde-languages/ del progetto; disattivare = toglierli, mai sotto una cartella che li usa.
    /// </summary>
    [TestClass]
    public class LanguageLibraryShould
    {
        private string _root, _library, _project, _source;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-library-tests", Guid.NewGuid().ToString("N"));
            _library = Path.Combine(_root, "MdExplorer", LanguageLibrary.FolderName);
            _project = Path.Combine(_root, "progetto");
            _source = Path.Combine(_root, "Downloads", "toy");
            Directory.CreateDirectory(_project);
            Directory.CreateDirectory(Path.Combine(_source, "skills", "toy-code"));
            File.WriteAllText(Path.Combine(_source, LanguagePackage.DefinitionFile), DeclarativeLanguageExtractorShould.ToyDefinition);
            File.WriteAllText(Path.Combine(_source, "skills", "toy-code", "SKILL.md"), "---\nname: toy-code\ndescription: Come leggere il Toy.\n---\n\nUn PART è un blocco.\n");
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch (IOException) { } }

        [TestMethod]
        public void Load_a_package_once_and_activate_it_in_a_project()
        {
            Assert.AreEqual(0, LanguageLibrary.List(_library).Count, "una libreria che non c'è ancora è vuota, non un errore");
            var imported = LanguageLibrary.Import(_library, _source, replace: false);
            Assert.AreEqual(("toy", 1), (imported.Id, imported.Version));
            var entry = LanguageLibrary.List(_library).Single();
            Assert.AreEqual(("Toy", 1, "toy-code"), (entry.DisplayName, entry.Version, entry.Skills.Single()));

            var before = LanguageLibrary.ForProject(_library, _project, _ => Array.Empty<string>()).Single();
            Assert.AreEqual((true, false), (before.InLibrary, before.Active), "in libreria, non ancora nel progetto");

            LanguageLibrary.Activate(_library, _project, "toy", replace: false);
            Assert.IsTrue(File.Exists(Path.Combine(DeclarativeLanguage.FolderOf(_project), "toy", LanguagePackage.DefinitionFile)), "attivo = copiato nel progetto, dove viaggia con Git");
            var after = LanguageLibrary.ForProject(_library, _project, _ => Array.Empty<string>()).Single();
            Assert.AreEqual((true, true, false), (after.InLibrary, after.Active, after.UpdateAvailable));
        }

        [TestMethod]
        public void Offer_the_update_when_the_library_has_a_newer_version()
        {
            LanguageLibrary.Import(_library, _source, replace: false);
            LanguageLibrary.Activate(_library, _project, "toy", replace: false);
            File.WriteAllText(Path.Combine(_source, LanguagePackage.DefinitionFile), DeclarativeLanguageExtractorShould.ToyDefinition.Replace("version: 1", "version: 2"));
            Assert.ThrowsException<LanguagePackageExistsException>(() => LanguageLibrary.Import(_library, _source, replace: false), "c'è già: si sostituisce solo dopo la conferma");
            LanguageLibrary.Import(_library, _source, replace: true);

            var entry = LanguageLibrary.ForProject(_library, _project, _ => Array.Empty<string>()).Single();
            Assert.AreEqual((2, 1, true), (entry.LibraryVersion, entry.ProjectVersion, entry.UpdateAvailable));
            Assert.ThrowsException<LanguagePackageExistsException>(() => LanguageLibrary.Activate(_library, _project, "toy", replace: false));
            LanguageLibrary.Activate(_library, _project, "toy", replace: true);
            Assert.IsFalse(LanguageLibrary.ForProject(_library, _project, _ => Array.Empty<string>()).Single().UpdateAvailable);
        }

        [TestMethod]
        public void Refuse_to_deactivate_a_language_a_folder_still_uses_and_adopt_one_that_came_with_the_project()
        {
            LanguageLibrary.Import(_library, _source, replace: false);
            LanguageLibrary.Activate(_library, _project, "toy", replace: false);
            var ex = Assert.ThrowsException<InvalidOperationException>(() => LanguageLibrary.Deactivate(_project, "toy", new[] { "program/db" }));
            StringAssert.Contains(ex.Message, "program/db");
            Assert.IsTrue(Directory.Exists(Path.Combine(DeclarativeLanguage.FolderOf(_project), "toy")), "rifiutato: non si toglie niente");

            LanguageLibrary.Remove(_library, "toy");
            var orphan = LanguageLibrary.ForProject(_library, _project, _ => new[] { "program/db" }).Single();
            Assert.AreEqual((false, true, "program/db"), (orphan.InLibrary, orphan.Active, orphan.UsedBy.Single()), "arrivato con il progetto (Git), non nella libreria di questo computer");
            LanguageLibrary.Adopt(_library, _project, "toy", replace: false);
            Assert.AreEqual("toy", LanguageLibrary.List(_library).Single().Id);

            LanguageLibrary.Deactivate(_project, "toy", Array.Empty<string>());
            Assert.IsFalse(Directory.Exists(Path.Combine(DeclarativeLanguage.FolderOf(_project), "toy")));
            Assert.ThrowsException<ArgumentException>(() => LanguageLibrary.Remove(_library, "../fuori"), "un id non esce dalla libreria");
        }
    }
}
