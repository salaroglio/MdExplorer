using System;
using System.IO;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// 16/11/2026, dal cliente: 58 file «con byte NUL», e per sapere che uno era TUTTO a zero (un file rovinato, non una
    /// codifica sconosciuta) bisognava aprirlo a mano. Il messaggio lo dice, con i numeri.
    /// </summary>
    [TestClass]
    public class CodeSourceTextShould
    {
        [TestMethod]
        public void Say_whether_a_file_that_is_not_text_is_all_zeros_or_only_has_some()
        {
            var dir = Path.Combine(Path.GetTempPath(), "mde-sourcetext-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var zeros = Path.Combine(dir, "Vuota.aspx.vb");
                File.WriteAllBytes(zeros, new byte[9051]);
                StringAssert.Contains(Assert.ThrowsException<InvalidDataException>(() => CodeSourceText.Read(zeros)).Message, "9051");
                StringAssert.Contains(Assert.ThrowsException<InvalidDataException>(() => CodeSourceText.Read(zeros)).Message, "entirely zero");

                var some = Path.Combine(dir, "Binario.vb");
                File.WriteAllBytes(some, new byte[] { 65, 0, 0, 66, 67, 0, 68, 69, 70, 71 });
                var message = Assert.ThrowsException<InvalidDataException>(() => CodeSourceText.Read(some)).Message;
                StringAssert.Contains(message, "3 NUL bytes out of 10");
            }
            finally { try { Directory.Delete(dir, true); } catch { } }
        }
    }
}
