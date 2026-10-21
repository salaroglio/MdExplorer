using System;
using System.IO;
using System.Linq;
using System.Text;
using MdExplorer.RemoteProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.RemoteAgent
{
    /// <summary>
    /// La codifica riconosciuta dal contenuto, senza configurazioni (l'utente, 21/10: radici con molti file CP-850 e pochi
    /// UTF-8, «in automatico»). E il file si riscrive nella stessa codifica.
    /// </summary>
    [TestClass]
    public class TextDecodingShould
    {
        static TextDecodingShould() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        private const string Italian = "      * Calcolo della quantità già consegnata: perché il saldo è più alto?\r\n      * Verificare città e codice dell'attività.\r\n";
        private const string German = "Größe der Lieferung für Kunden über 100 Stück\n";
        private const string DosScreen = "╔══════════════════╗\n║ ANAGRAFICA CLIENTI ║\n╚══════════════════╝\n  Città: ROMA   Cap: 00100\n";

        private static byte[] In(string encoding, string text) => Encoding.GetEncoding(encoding).GetBytes(text);

        [DataTestMethod]
        [DataRow("ibm850", Italian)]
        [DataRow("windows-1252", Italian)]
        [DataRow("ibm850", German)]
        [DataRow("windows-1252", German)]
        [DataRow("ibm850", DosScreen)]
        public void Recognize_the_8_bit_encoding_from_the_content(string encoding, string text)
        {
            var decoded = TextDecoding.Decode(In(encoding, text));
            Assert.AreEqual(encoding, decoded.Encoding);
            Assert.AreEqual(text, decoded.Text);
        }

        [TestMethod]
        public void Prefer_utf8_and_boms()
        {
            Assert.AreEqual(TextDecoding.Utf8, TextDecoding.Decode(Encoding.UTF8.GetBytes(Italian)).Encoding, "UTF-8 con accentate");
            var bom = TextDecoding.Decode(new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Encoding.UTF8.GetBytes("città")).ToArray());
            Assert.AreEqual(("città", TextDecoding.Utf8, true), (bom.Text, bom.Encoding, bom.Bom));
            var le = TextDecoding.Decode(Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("città")).ToArray());
            Assert.AreEqual(("città", "utf-16", true), (le.Text, le.Encoding, le.Bom), "UTF-16 con BOM non è binario");
            Assert.IsTrue(TextDecoding.LooksLikeText(Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("ab")).ToArray()));
            Assert.IsNull(TextDecoding.Decode(new byte[] { 0x89, 0x50, 0x00, 0x01 }), "binario");
            Assert.AreEqual(TextDecoding.Utf8, TextDecoding.Decode(Encoding.ASCII.GetBytes("solo ascii")).Encoding);
        }

        [TestMethod]
        public void Exclude_1252_when_its_undefined_bytes_appear()
        {
            // 0x81 = «ü» in CP-850, non definito in CP-1252
            Assert.AreEqual(TextDecoding.Ibm850, TextDecoding.Guess8Bit(new byte[] { (byte)'f', 0x81, (byte)'r' }));
        }

        [TestMethod]
        public void Write_back_in_the_same_encoding_keeping_untouched_bytes_and_refuse_what_it_cannot_hold()
        {
            var dir = Path.Combine(Path.GetTempPath(), "mde-enc-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var original = In("ibm850", Italian);
                File.WriteAllBytes(Path.Combine(dir, "prog.cbl"), original);
                var ws = new RemoteWorkspace(dir);
                var (text, stamp, _, encoding) = ws.Read("prog.cbl");
                Assert.AreEqual(TextDecoding.Ibm850, encoding);

                stamp = ws.Write("prog.cbl", text, stamp);
                CollectionAssert.AreEqual(original, File.ReadAllBytes(Path.Combine(dir, "prog.cbl")), "riscritto uguale: byte identici");

                ws.Write("prog.cbl", text.Replace("quantità", "quantità è più"), stamp);
                CollectionAssert.AreEqual(In("ibm850", Italian.Replace("quantità", "quantità è più")), File.ReadAllBytes(Path.Combine(dir, "prog.cbl")), "il testo nuovo in CP-850");

                var (_, stamp2, _, _) = ws.Read("prog.cbl");
                try { ws.Write("prog.cbl", "emoji 😀", stamp2); Assert.Fail("doveva rifiutare"); }
                catch (RemoteFailure f)
                {
                    Assert.AreEqual(RemoteErrorCodes.NotRepresentable, f.Code);
                    StringAssert.Contains(f.Message, "«😀»", "il carattere intero, non «\\0»");
                    StringAssert.Contains(f.Message, "CP-850");
                }

                Assert.AreEqual("prog.cbl", ws.Grep("", "quantità", false, 0, null).Hits.Single().Path, "la ricerca legge il CP-850");
            }
            finally { Directory.Delete(dir, true); }
        }
    }
}
