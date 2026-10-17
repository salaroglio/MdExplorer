using MdExplorer.Features.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Commands
{
    /// <summary>
    /// The code regions the commands must leave alone. Each case asks about the offset of a
    /// marker, <c>@@</c>, in the text — the same question the commands ask about a regex match.
    /// </summary>
    [TestClass]
    public class MarkdownCodeRegionsShould
    {
        private static bool IsCode(string markdown)
            => MarkdownCodeRegions.Of(markdown).IsCode(markdown.IndexOf("@@", System.StringComparison.Ordinal));

        private static bool IsInTable(string markdown)
            => MarkdownCodeRegions.Of(markdown).IsInTable(markdown.IndexOf("@@", System.StringComparison.Ordinal));

        [TestMethod]
        public void SeeTheContentOfEveryKindOfCodeBlock()
        {
            Assert.IsTrue(IsCode("Testo.\n\n```\n@@\n```\n"), "backtick fence");
            Assert.IsTrue(IsCode("Testo.\n\n~~~\n@@\n~~~\n"), "tilde fence");
            Assert.IsTrue(IsCode("Testo.\n\n```markdown\nuno\n@@\n```\n"), "fence with info string");
            Assert.IsTrue(IsCode("Testo.\n\n    @@ rientrato\n"), "indented code");
            Assert.IsTrue(IsCode("- voce\n\n  ```\n  @@\n  ```\n"), "fence inside a list item");
            Assert.IsTrue(IsCode("> citazione\n>\n> ```\n> @@\n> ```\n"), "fence inside a quote");
            Assert.IsTrue(IsCode("Testo.\r\n\r\n```\r\n@@\r\n```\r\n"), "CRLF");
            Assert.IsTrue(IsCode("```\nnon chiuso\n\n@@\n"), "an unclosed fence runs to the end, as in the page");
        }

        [TestMethod]
        public void SeeAFenceInsideALongerFenceAsCode()
        {
            // The example of a ```plantuml block, written inside a ```` block.
            Assert.IsTrue(IsCode("Testo.\n\n````markdown\n@@```plantuml\nA -> B\n```\n````\n"));
        }

        [TestMethod]
        public void NotSeeTheOpeningLineOfAFencedBlockAsItsCode()
        {
            // The fence commands (```plantuml, ```html, ```plantuml(@json, …)) match there, on
            // their own block, and must go on doing so.
            Assert.IsFalse(IsCode("Testo.\n\n@@```plantuml\nA -> B\n```\n"));
            Assert.IsFalse(IsCode("- voce\n\n  @@```plantuml\n  A -> B\n  ```\n"));
        }

        [TestMethod]
        public void SeeInlineCode()
        {
            Assert.IsTrue(IsCode("Nel testo `![x](@@y.png)` e poi altro.\n"));
            Assert.IsTrue(IsCode("| a | b |\n|---|---|\n| `@@` | 2 |\n"), "inline code in a table cell");
        }

        [TestMethod]
        public void NotSeeTextAsCode()
        {
            Assert.IsFalse(IsCode("Testo con @@ dentro.\n\n```\ncodice\n```\n"), "before a code block");
            Assert.IsFalse(IsCode("```\ncodice\n```\n\nDopo @@.\n"), "after a code block");
            Assert.IsFalse(IsCode("Testo `codice` e @@ fuori.\n"), "next to inline code");
            Assert.IsFalse(IsCode("# Titolo @@\n"));
        }

        [TestMethod]
        public void SeeTables()
        {
            Assert.IsTrue(IsInTable("Prima.\n\n| a | b |\n|---|---|\n| ![x](@@y.png) | 2 |\n\nDopo.\n"));
            Assert.IsTrue(IsInTable("| a | b |\r\n|---|---|\r\n| 1 | @@ |\r\n"), "CRLF");
            Assert.IsFalse(IsInTable("| a | b |\n|---|---|\n| 1 | 2 |\n\nDopo @@.\n"));
            Assert.IsFalse(IsInTable("Una riga con | una barra @@ sola.\n"));
        }
    }
}
