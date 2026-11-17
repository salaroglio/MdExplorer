using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MdExplorer.Features.Services.Ocr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Ocr
{
    [TestClass]
    public class WordTableCards_Should
    {
        private static readonly string Long = string.Join(" ", Enumerable.Repeat("parola", WordTableCards.HeavyWords + 1));

        private static string Row(params string[] cells) => "<tr>" + string.Concat(cells.Select(c => c.StartsWith("<t") ? c : "<td>" + c + "</td>")) + "</tr>";
        private static string Table(params string[] rows) => "<table>" + string.Concat(rows) + "</table>";
        private static string Flat(string html) => Regex.Replace(html, @">\s+<", "><");

        private static string Apply(string html, List<string> warnings = null) => Flat(WordTableCards.Apply(html, warnings ?? new List<string>()));

        [TestMethod]
        public void LeaveALightTableAlone()
        {
            var html = Table(Row("<p>Sigla</p>", "<p>Significato</p>"), Row("<p>MDE</p>", "<p>Uno</p><p>Due</p>"));

            Assert.AreEqual(html, Apply(html));
        }

        [TestMethod]
        public void TurnEachRowIntoACard_WhenACellCarriesAListAnImageOrTooManyWords()
        {
            var html = "<h2>Funzionali</h2>" + Table(
                Row("<td colspan=\"4\"><p>Gruppo accesso</p></td>"),
                Row("<p>Codice</p>", "<p>Descrizione</p>", "<p>Priorita</p>", "<p>Note</p>"),
                Row("<p>R-001</p>", "<p>Chiede le credenziali.</p><ul><li>nome</li></ul><p><img src=\"a.png\" /></p>", "<p>Alta</p>", ""),
                Row("<p>R-002</p>", "<p>" + Long + "</p>", "<p>Media</p>", "<p>Da rivedere</p>"));

            var result = Apply(html);

            Assert.AreEqual(
                "<h2>Funzionali</h2><p><strong>Gruppo accesso</strong></p>"
                + "<h3>R-001</h3><p><strong>Descrizione</strong></p><p>Chiede le credenziali.</p><ul><li>nome</li></ul><p><img src=\"a.png\"></p>"
                + "<ul><li><strong>Priorita:</strong> Alta</li></ul>"
                + "<h3>R-002</h3><p><strong>Descrizione</strong></p><p>" + Long + "</p>"
                + "<ul><li><strong>Priorita:</strong> Media</li><li><strong>Note:</strong> Da rivedere</li></ul>",
                result.Replace(" />", ">"));
        }

        [TestMethod]
        public void KeepEveryWordOfTheTable_RepeatingOnlyTheHeaders()
        {
            var html = Table(
                Row("<p>Codice</p>", "<p>Testo</p>", "<p>Stato</p>"),
                Row("<p>A1</p>", "<p>primo</p><ul><li>secondo</li></ul>", "<p>terzo</p>"),
                Row("<p>A2</p>", "<p>quarto</p><ol><li>quinto</li></ol>", "<p>sesto</p>"));

            var words = Regex.Matches(Regex.Replace(Apply(html), "<[^>]+>", " "), @"\w+").Select(m => m.Value).ToList();

            CollectionAssert.AreEqual(
                new[] { "A1", "Testo", "primo", "secondo", "Stato", "terzo", "A2", "Testo", "quarto", "quinto", "Stato", "sesto" },
                words);
        }

        [TestMethod]
        public void TurnAOneColumnTableIntoABox()
        {
            var html = Table(Row("<p>Attenzione.</p><ul><li>primo</li></ul>"), Row("<p>Breve.</p>"));

            Assert.AreEqual("<blockquote><p>Attenzione.</p><ul><li>primo</li></ul></blockquote><blockquote><p>Breve.</p></blockquote>", Apply(html));
        }

        [TestMethod]
        public void TreatATwoColumnTableWithoutHeaderAsLabelAndValuePairs()
        {
            var html = "<h1>Scheda</h1>" + Table(
                Row("<p>Scopo</p>", "<p>Descrivere.</p><ul><li>uno</li></ul>"),
                Row("<p>Autore</p>", "<p>Mario Rossi</p>"));

            Assert.AreEqual(
                "<h1>Scheda</h1><h2>Scopo</h2><p>Descrivere.</p><ul><li>uno</li></ul><h2>Autore</h2><p>Mario Rossi</p>",
                Apply(html));
        }

        [TestMethod]
        public void SeparateCardsWithARule_WhenTheFirstColumnIsNotATitle()
        {
            var html = Table(
                Row("<th><p>Rischio</p></th>", "<th><p>Esito</p></th>"),
                Row("<p>" + Long + "</p>", "<p>Basso</p>"),
                Row("<p>" + Long + "</p>", "<p>Alto</p>"));

            Assert.AreEqual(
                "<p><strong>Rischio</strong></p><p>" + Long + "</p><ul><li><strong>Esito:</strong> Basso</li></ul>"
                + "<hr><p><strong>Rischio</strong></p><p>" + Long + "</p><ul><li><strong>Esito:</strong> Alto</li></ul>",
                Apply(html).Replace("<hr />", "<hr>"));
        }

        [TestMethod]
        public void MeltATableInsideACell_OneLevelBelowItsCard()
        {
            var inner = Table(Row("<p>Passo</p>", "<p>Fai.</p><ul><li>x</li></ul>"));
            var html = "<h2>Capitolo</h2>" + Table(
                Row("<p>Codice</p>", "<p>Testo</p>", "<p>Stato</p>"),
                Row("<p>A1</p>", "<p>Prima.</p>" + inner, "<p>Ok</p>"));

            Assert.AreEqual(
                "<h2>Capitolo</h2><h3>A1</h3><p><strong>Testo</strong></p><p>Prima.</p><h4>Passo</h4><p>Fai.</p><ul><li>x</li></ul>"
                + "<ul><li><strong>Stato:</strong> Ok</li></ul>",
                Apply(html));
        }

        [TestMethod]
        public void GiveTheCardsOfTwoTablesTheSameLevel()
        {
            var table = Table(Row("<p>A</p>", "<p>B</p>", "<p>C</p>"), Row("<p>K1</p>", "<p>x</p><ul><li>y</li></ul>", "<p>z</p>"));

            var result = Apply("<h3>Sezione</h3>" + table + "<p>poi</p>" + table);

            Assert.AreEqual(2, Regex.Matches(result, "<h4>K1</h4>").Count, result);
        }

        [TestMethod]
        public void LeaveATableWithVerticallyMergedCells_AndWarn()
        {
            var html = Table(Row("<td rowspan=\"2\"><p>A</p></td>", "<p>" + Long + "</p>"), Row("<p>B</p>"));
            var warnings = new List<string>();

            Assert.AreEqual(html, Apply(html, warnings));
            Assert.AreEqual(1, warnings.Count);
        }
    }
}
