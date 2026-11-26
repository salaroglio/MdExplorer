using System;
using System.IO;
using MdExplorer.Features.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Utilities
{
    /// <summary>I nomi liberi al posto della data attaccata al nome (26/11/2026).</summary>
    [TestClass]
    public class FreeFileName_Should
    {
        private string _dir;

        [TestInitialize]
        public void Init() => Directory.CreateDirectory(_dir = Path.Combine(Path.GetTempPath(), "mde-free-" + Guid.NewGuid().ToString("N")));

        [TestCleanup]
        public void Done() => Directory.Delete(_dir, true);

        private void Touch(string relative)
        {
            var full = Path.Combine(_dir, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            File.WriteAllText(full, "x");
        }

        [TestMethod]
        public void Keep_the_name_when_it_is_free_and_count_from_two_when_it_is_not()
        {
            Assert.AreEqual(Path.Combine(_dir, "Contratto.docx"), FreeFileName.For(_dir, "Contratto", ".docx"));
            Touch("Contratto.docx");
            Assert.AreEqual(Path.Combine(_dir, "Contratto-2.docx"), FreeFileName.For(_dir, "Contratto", ".docx"));
            Touch("Contratto-2.docx");
            Assert.AreEqual(Path.Combine(_dir, "Contratto-3.docx"), FreeFileName.For(_dir, "Contratto", ".docx"));
        }

        [TestMethod]
        public void Keep_a_converted_document_and_its_attachments_and_images_together()
        {
            Assert.AreEqual("Contratto", FreeFileName.ForConvertedDocument(_dir, "Contratto"));

            Touch("Contratto.attach1.md");          // un allegato di una conversione di prima
            Assert.AreEqual("Contratto-2", FreeFileName.ForConvertedDocument(_dir, "Contratto"));

            Directory.Delete(_dir, true); Directory.CreateDirectory(_dir);
            Touch("assets/Contratto_img/img1.png");  // le sue immagini
            Assert.AreEqual("Contratto-2", FreeFileName.ForConvertedDocument(_dir, "Contratto"));

            Directory.Delete(_dir, true); Directory.CreateDirectory(_dir);
            Touch("Contratto.md");
            Touch("Contratto-2.md");
            Touch("Contratto.docx");                 // il Word stesso non c'entra
            Assert.AreEqual("Contratto-3", FreeFileName.ForConvertedDocument(_dir, "Contratto"));
        }
    }
}
