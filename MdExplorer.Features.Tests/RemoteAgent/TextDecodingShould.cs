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

        // ---- raddrizzare (l'utente, 21/10: «che succede se l'llm modifica in maniera sbagliata un file?») ----

        private static string Dir()
        {
            var dir = Path.Combine(Path.GetTempPath(), "mde-fix-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        [TestMethod]
        public void Repair_a_cp850_file_where_an_llm_added_utf8_lines()
        {
            var dir = Dir();
            try
            {
                // le righe originali in CP-850, la riga aggiunta (sed/perl con un testo UTF-8) in UTF-8
                var mixed = In("ibm850", "      * Quantità già consegnata\n").Concat(Encoding.UTF8.GetBytes("      * Uscita: è più pulito\n")).Concat(In("ibm850", "           STOP RUN.\n")).ToArray();
                File.WriteAllBytes(Path.Combine(dir, "p.cbl"), mixed);
                var ws = new RemoteWorkspace(dir);
                var auto = ws.Read("p.cbl");
                Assert.AreEqual(TextDecoding.Ibm850, auto.Encoding, "riconosciuto CP-850 (la maggioranza)");
                Assert.IsFalse(auto.Text.Contains("è più"), "letta in automatico, la riga UTF-8 si vede storta: " + auto.Text);

                var repaired = ws.Read("p.cbl", "mixed-ibm850");
                Assert.AreEqual("      * Quantità già consegnata\n      * Uscita: è più pulito\n           STOP RUN.\n", repaired.Text);
                ws.Write("p.cbl", repaired.Text, repaired.Stamp, TextDecoding.Ibm850);
                CollectionAssert.AreEqual(In("ibm850", repaired.Text), File.ReadAllBytes(Path.Combine(dir, "p.cbl")), "tutto in CP-850");
                Assert.AreEqual(TextDecoding.Ibm850, ws.Read("p.cbl").Encoding);
            }
            finally { Directory.Delete(dir, true); }
        }

        [TestMethod]
        public void Keep_dos_boxes_as_cp850_when_repairing_even_where_their_bytes_look_like_utf8()
        {
            // «═╗» in CP-850 = CD BB, UTF-8 valido per caso (U+037B «ͻ»): visto dal vivo il 21/10 su una maschera COBOL
            var original = In("ibm850", DosScreen);
            var mixed = original.Concat(Encoding.UTF8.GetBytes("  Nota: è più semplice – € 10\n")).ToArray();
            Assert.AreEqual(DosScreen + "  Nota: è più semplice – € 10\n", TextDecoding.DecodeMixed(mixed, TextDecoding.Ibm850));
        }

        [TestMethod]
        public void Convert_a_file_an_llm_turned_into_utf8_back_to_cp850_and_read_as_another_encoding()
        {
            var dir = Dir();
            try
            {
                File.WriteAllBytes(Path.Combine(dir, "p.cbl"), new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Encoding.UTF8.GetBytes(Italian)).ToArray());
                var ws = new RemoteWorkspace(dir);
                var read = ws.Read("p.cbl");
                Assert.AreEqual((TextDecoding.Utf8, true), (read.Encoding, read.Bom));
                ws.Write("p.cbl", read.Text, read.Stamp, TextDecoding.Ibm850);
                CollectionAssert.AreEqual(In("ibm850", Italian), File.ReadAllBytes(Path.Combine(dir, "p.cbl")), "in CP-850, senza il BOM UTF-8");

                // stima sbagliata: «rileggi come» cambia solo la lettura; salvato così com'è, i byte non cambiano
                var asWin = ws.Read("p.cbl", TextDecoding.Windows1252);
                Assert.AreEqual(TextDecoding.Windows1252, asWin.Encoding);
                ws.Write("p.cbl", asWin.Text, asWin.Stamp, TextDecoding.Windows1252);
                Assert.IsTrue(In("ibm850", Italian).AsSpan().StartsWith(File.ReadAllBytes(Path.Combine(dir, "p.cbl")).AsSpan(0, 10)));
            }
            finally { Directory.Delete(dir, true); }
        }

        [TestMethod]
        public void Refuse_to_read_as_utf8_what_is_not_and_to_write_in_two_encodings()
        {
            var dir = Dir();
            try
            {
                File.WriteAllBytes(Path.Combine(dir, "p.cbl"), In("ibm850", Italian));
                var ws = new RemoteWorkspace(dir);
                try { ws.Read("p.cbl", TextDecoding.Utf8); Assert.Fail("non è UTF-8"); }
                catch (RemoteFailure f) { Assert.AreEqual(RemoteErrorCodes.NotUtf8, f.Code); }
                var r = ws.Read("p.cbl");
                try { ws.Write("p.cbl", r.Text, r.Stamp, "mixed-ibm850"); Assert.Fail("si scrive in UNA codifica"); }
                catch (RemoteFailure f) { Assert.AreEqual(RemoteErrorCodes.BadRequest, f.Code); }
            }
            finally { Directory.Delete(dir, true); }
        }

        // ---- le codifiche per estensione della radice (l'utente, 21/10: «in base all'estensione viene scelto il formato») ----

        [TestMethod]
        public void Use_the_encoding_assigned_to_the_extension_before_any_guess_and_let_a_bom_win()
        {
            var rules = new System.Collections.Generic.Dictionary<string, string> { [".cbl"] = TextDecoding.Ibm850, [".md"] = TextDecoding.Utf8 };
            Assert.AreEqual(TextDecoding.Ibm850, TextDecoding.ForExtension("/srv/p/PROG.CBL", In("windows-1252", Italian), rules), "maiuscole indifferenti");
            Assert.IsNull(TextDecoding.ForExtension("/srv/p/a.java", In("ibm850", Italian), rules), "non elencata: UTF-8 o stima");
            Assert.IsNull(TextDecoding.ForExtension("/srv/p/a.md", In("ibm850", Italian), rules), "UTF-8 assegnato = il comportamento normale");
            Assert.IsNull(TextDecoding.ForExtension("/srv/p/a.cbl", new byte[] { 0xEF, 0xBB, 0xBF, 0x41 }, rules), "il BOM lo dichiara il file: vince");
            Assert.IsNull(TextDecoding.ForExtension("/srv/p/a.cbl", In("ibm850", Italian), null));
        }

        [TestMethod]
        public void Read_write_create_and_search_with_the_rules_of_the_root()
        {
            var dir = Dir();
            try
            {
                // un CP-1252 che la stima riconoscerebbe come tale: la regola dice CP-850, e vince la regola
                File.WriteAllBytes(Path.Combine(dir, "a.cbl"), In("windows-1252", "perché\n"));
                var ws = new RemoteWorkspace(dir) { ExtensionEncodings = new System.Collections.Generic.Dictionary<string, string> { [".cbl"] = TextDecoding.Ibm850 } };
                var read = ws.Read("a.cbl");
                Assert.AreEqual(TextDecoding.Ibm850, read.Encoding);
                Assert.AreEqual(Encoding.GetEncoding("ibm850").GetString(In("windows-1252", "perché\n")), read.Text);

                ws.Create("nuovo.cbl", "città\n");
                CollectionAssert.AreEqual(In("ibm850", "città\n"), File.ReadAllBytes(Path.Combine(dir, "nuovo.cbl")), "un file nuovo nasce nella codifica della sua estensione");
                ws.Create("nuovo.md", "città\n");
                CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("città\n"), File.ReadAllBytes(Path.Combine(dir, "nuovo.md")));

                Assert.AreEqual("nuovo.cbl", ws.Grep("", "città", false, 0, null).Hits.Select(h => h.Path).OrderBy(p => p).First(), "la ricerca legge con la regola");
                var stamp = ws.Read("nuovo.cbl").Stamp;
                ws.Write("nuovo.cbl", "più\n", stamp);
                CollectionAssert.AreEqual(In("ibm850", "più\n"), File.ReadAllBytes(Path.Combine(dir, "nuovo.cbl")));
            }
            finally { Directory.Delete(dir, true); }
        }
    }
}
