using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.Ocr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Ocr
{
    [TestClass]
    public class MetafileToPng_Should
    {
        [TestMethod]
        public void RefuseBytesThatAreNotAMetafile_WithoutWritingAnything()
        {
            var png = Path.Combine(Path.GetTempPath(), "mde-metafile-test-" + Guid.NewGuid().ToString("N") + ".png");
            try
            {
                var whyNot = MetafileToPng.Convert(new byte[] { 1, 2, 3, 4 }, ".emf", png);

                Assert.IsNotNull(whyNot, "quattro byte a caso non sono un disegno");
                Assert.IsFalse(File.Exists(png), "se non ci riesce non lascia file");
            }
            finally
            {
                File.Delete(png);
            }
        }
    }
}
