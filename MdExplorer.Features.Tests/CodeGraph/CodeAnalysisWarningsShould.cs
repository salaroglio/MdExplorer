using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>Gli avvisi dell'ultima estrazione per cartella (12/11/2026): MarkAgent li legge da code_inventory, il report li elenca tutti.</summary>
    [TestClass]
    public class CodeAnalysisWarningsShould
    {
        [TestMethod]
        public void Keep_the_warnings_of_each_folder_and_tell_none_recorded_from_no_warnings()
        {
            var project = Path.Combine(Path.GetTempPath(), "mde-warnings-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(project);
            try
            {
                Assert.IsNull(CodeAnalysisWarnings.Read(project, "app"), "nessuna estrazione registrata: null, non «nessun avviso»");
                CodeAnalysisWarnings.Write(project, "app", 3, 10, new[] { "Dl.vbproj: manca X", "Dl.vbproj: manca X", " " });
                CodeAnalysisWarnings.Write(project, "db", 5, 5, Array.Empty<string>());
                var app = CodeAnalysisWarnings.Read(project, "app");
                CollectionAssert.AreEqual(new[] { "Dl.vbproj: manca X" }, app.Warnings, "doppioni e righe vuote via");
                Assert.AreEqual((3, 10), (app.Reparsed, app.Files));
                Assert.AreEqual(0, CodeAnalysisWarnings.Read(project, "db").Warnings.Count);
                CollectionAssert.AreEqual(new[] { "app", "db" }, CodeAnalysisWarnings.ReadAll(project).Select(e => e.Folder).ToArray(), "una cartella non cancella l'altra");
            }
            finally { try { Directory.Delete(project, true); } catch (IOException) { } }
        }
    }
}
