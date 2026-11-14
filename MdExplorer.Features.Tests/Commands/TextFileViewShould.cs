using System.IO;
using System.Text;
using MdExplorer.Features.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Commands
{
    /// <summary>
    /// Which files a click in the md-tree shows as text: decided by the content, not the name.
    /// </summary>
    [TestClass]
    public class TextFileViewShould
    {
        private static bool IsText(byte[] content)
        {
            var path = Path.Combine(Path.GetTempPath(), "mde-textview-" + System.Guid.NewGuid().ToString("N"));
            File.WriteAllBytes(path, content);
            try { return TextFileView.IsText(path); }
            finally { File.Delete(path); }
        }

        [TestMethod]
        public void SeeTextWhateverTheName()
        {
            Assert.IsTrue(IsText(Encoding.UTF8.GetBytes("{ \"a\": 1 }\n")), "UTF-8");
            Assert.IsTrue(IsText(Encoding.UTF8.GetBytes("città, perché, €\n")), "UTF-8 with accents");
            Assert.IsTrue(IsText(new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes("con BOM"))), "UTF-8 with BOM");
            Assert.IsTrue(IsText(Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("PowerShell > file.txt"))), "UTF-16 LE with BOM");
            Assert.IsTrue(IsText(new byte[0]), "an empty file");
        }

        [TestMethod]
        public void NotSeeABinaryAsText()
        {
            Assert.IsFalse(IsText(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00 }), "PNG header");
            Assert.IsFalse(IsText(new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00 }), "zip header");
            Assert.IsFalse(IsText(new byte[] { 0x61, 0x62, 0xFF, 0xFE, 0xFD, 0x63 }), "invalid UTF-8");
            Assert.IsFalse(IsText(Encoding.Unicode.GetBytes("UTF-16 senza BOM")), "UTF-16 without BOM has NUL bytes");
        }

        [TestMethod]
        public void NotMistakeACharacterCutAtTheEndOfTheSampleForBinary()
        {
            // 8191 ASCII bytes, then "è" (2 bytes): the first 8 KB end in the middle of it.
            var bytes = new byte[8191];
            for (var i = 0; i < bytes.Length; i++) bytes[i] = (byte)'a';
            Assert.IsTrue(IsText(bytes.Concat(Encoding.UTF8.GetBytes("è e poi altro"))));
        }

        [TestMethod]
        public void ReadTheTextAsItWasRecognised()
        {
            var path = Path.Combine(Path.GetTempPath(), "mde-textview-read.txt");
            File.WriteAllBytes(path, Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("perché")));
            try { Assert.AreEqual("perché", TextFileView.ReadText(path)); }
            finally { File.Delete(path); }
        }

        [TestMethod]
        public void ColorByExtensionAndLeaveTheRestPlain()
        {
            Assert.AreEqual("json", TextFileView.LanguageFor("/p/a.json"));
            Assert.AreEqual("csharp", TextFileView.LanguageFor("/p/a.CS"));
            Assert.AreEqual("", TextFileView.LanguageFor("/p/server.log"));
            Assert.AreEqual("", TextFileView.LanguageFor("/p/Dockerfile"));
        }
    }

    internal static class ByteArrayExtensions
    {
        public static byte[] Concat(this byte[] first, byte[] second)
        {
            var result = new byte[first.Length + second.Length];
            first.CopyTo(result, 0);
            second.CopyTo(result, first.Length);
            return result;
        }
    }
}
