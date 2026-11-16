using MdExplorer.Features.Services.Ocr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Ocr
{
    [TestClass]
    public class OcrMarkdown_Should
    {
        [TestMethod]
        public void TurnAnOtslTableIntoAPipeTable()
        {
            var page = "## 2. Esito\n\n" +
                       "<fcel>Prova<fcel>Soglia<nl><fcel>Pressione<fcel>10,0 bar<nl><fcel>Rumore<ecel><nl>\n\n" +
                       "Fine.";

            var markdown = OcrMarkdown.FromPage(page);

            Assert.AreEqual(
                "## 2. Esito\n\n" +
                "| Prova | Soglia |\n| --- | --- |\n| Pressione | 10,0 bar |\n| Rumore |  |\n\n" +
                "Fine.", markdown);
        }

        [TestMethod]
        public void KeepMergedCellsByWritingHtml()
        {
            // «Totale» copre due colonne, «Nord» due righe: una tabella a barre non lo sa dire.
            var otsl = "<fcel>Zona<fcel>Totale<lcel><nl><fcel>Nord<fcel>1<fcel>2<nl><ucel><fcel>3<fcel>4<nl>";

            var html = OcrMarkdown.TableFromOtsl(otsl);

            Assert.AreEqual(
                "<table>\n" +
                "<tr><th>Zona</th><th colspan=\"2\">Totale</th></tr>\n" +
                "<tr><td rowspan=\"2\">Nord</td><td>1</td><td>2</td></tr>\n" +
                "<tr><td>3</td><td>4</td></tr>\n" +
                "</table>", html);
        }

        [TestMethod]
        public void EscapeABarInsideACell()
        {
            Assert.AreEqual("| a\\|b |\n| --- |", OcrMarkdown.TableFromOtsl("<fcel>a|b<nl>"));
        }

        [TestMethod]
        public void PadARowShorterThanTheOthers()
        {
            Assert.AreEqual("| a | b |\n| --- | --- |\n| c |  |", OcrMarkdown.TableFromOtsl("<fcel>a<fcel>b<nl><fcel>c<nl>"));
        }

        [TestMethod]
        public void DropTheFenceTheModelWrapsThePageIn()
        {
            Assert.AreEqual("# Titolo\n\nTesto.", OcrMarkdown.FromPage("```markdown\n# Titolo\n\nTesto.\n```\n"));
        }

        [TestMethod]
        public void LeaveATextThatOnlyMentionsAMarkerAlone()
        {
            // Il marcatore non apre la riga: è testo che parla di OTSL, non una tabella.
            Assert.AreEqual("Il marcatore <nl> chiude la riga.", OcrMarkdown.FromPage("Il marcatore <nl> chiude la riga."));
        }

        [TestMethod]
        public void DropWhatTheModelThoughtBeforeAnswering()
        {
            Assert.AreEqual("", OcrMarkdown.FromPage("</think>\n\n"));
            Assert.AreEqual("# Titolo", OcrMarkdown.FromPage("<think>\nè un titolo\n</think>\n\n# Titolo"));
        }

        [TestMethod]
        public void JoinPagesSkippingTheEmptyOnes()
        {
            Assert.AreEqual("uno\n\ntre\n", OcrMarkdown.JoinPages(new[] { "uno\n", "  ", "tre" }));
        }
    }
}
