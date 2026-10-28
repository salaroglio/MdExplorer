using System.IO;
using System.Text;
using MdExplorer.Features.Services.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Blinda la codifica con cui i prompt entrano nei CLI di AI. Il difetto che questi test
    /// impediscono di tornare: MarkAgent rispondeva con dei rombi al posto delle lettere
    /// accentate, perche' lo stdin del processo figlio ereditava il code page della console
    /// di Windows e il CLI rileggeva quei byte come UTF-8. Niente spawn di processi: si
    /// verifica la codifica, che e' il punto dove il difetto nasceva.
    /// </summary>
    [TestClass]
    public class AiCliEncoding_Should
    {
        private const string ConAccenti = "perché la città";

        [TestMethod]
        public void Write_accented_text_as_plain_utf8()
        {
            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, AiCliEncoding.PromptStdin, 1024, leaveOpen: true))
            {
                writer.Write(ConAccenti);
            }

            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(ConAccenti), stream.ToArray());
        }

        [TestMethod]
        public void Never_prepend_a_bom()
        {
            // Encoding.UTF8 passerebbe il test qui sopra sui caratteri ma anteporrebbe EF BB BF,
            // che il CLI si troverebbe dentro il prompt: e' l'altra meta' della trappola.
            Assert.AreEqual(0, AiCliEncoding.PromptStdin.GetPreamble().Length,
                "il prompt non deve essere preceduto dal BOM");
        }

        [TestMethod]
        public void Show_why_the_console_code_page_broke_the_answer()
        {
            // Il difetto riprodotto: gli stessi caratteri scritti nel code page della console
            // di Windows diventano byte singoli che non sono UTF-8 valido, e chi legge dall'altra
            // parte della pipe li sostituisce con U+FFFD — il rombo che compariva nelle risposte.
            var comeFacevaWindows = Encoding.Latin1.GetBytes(ConAccenti);
            Assert.IsTrue(Encoding.UTF8.GetString(comeFacevaWindows).Contains('�'));

            var comeFaOra = AiCliEncoding.PromptStdin.GetBytes(ConAccenti);
            Assert.AreEqual(ConAccenti, Encoding.UTF8.GetString(comeFaOra));
        }
    }
}
