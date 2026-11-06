using MdExplorer.Features.Commands.pdf;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Commands
{
    /// <summary>
    /// I percorsi delle immagini che vanno a Pandoc.
    /// <para>
    /// ⚠️ Difetto vero, misurato il 21/09/2026: per un documento nella <b>radice</b> del progetto
    /// il percorso veniva composto come <c>"" + "/" + nome</c>, cioè <c>/nome.png</c>. Nel viewer
    /// funziona (un'altra trasformazione ci mette davanti <c>/api/mdexplorer</c>), ma Pandoc lo
    /// legge come radice del filesystem — <c>Could not fetch resource '/diagramma.png'</c> — e
    /// nel .docx l'immagine non c'era proprio.
    /// </para>
    /// </summary>
    [TestClass]
    public class ImagePathsForPandocShould
    {
        [TestMethod]
        public void Drop_the_leading_slash_of_a_document_in_the_project_root()
        {
            var markdown = "Testo.\n\n![](/diagramma.png)\n";

            var risultato = ManageLinkAsImagesPdf.MakeRelativeToProjectRoot(markdown);

            // Pandoc gira con la cartella di lavoro sulla radice del progetto: così lo trova.
            StringAssert.Contains(risultato, "![](diagramma.png)");
        }

        [TestMethod]
        public void Leave_a_path_that_already_points_inside_a_folder()
        {
            var markdown = "![](docs/diagramma.png)\n\n![alt](assets/immagini/foto.jpg)\n";

            var risultato = ManageLinkAsImagesPdf.MakeRelativeToProjectRoot(markdown);

            Assert.AreEqual(markdown, risultato);
        }

        [TestMethod]
        public void Leave_the_urls_alone()
        {
            // Questi Pandoc li scarica: accorciarli li romperebbe.
            var markdown = "![](https://esempio.it/foto.png)\n\n![](//host/foto.png)\n\n![](data:image/png;base64,AAA)\n";

            var risultato = ManageLinkAsImagesPdf.MakeRelativeToProjectRoot(markdown);

            Assert.AreEqual(markdown, risultato);
        }

        [TestMethod]
        public void Leave_an_image_written_inside_a_code_block()
        {
            // Un'immagine dentro il codice è testo: si mostra come l'autore l'ha scritta.
            var markdown = "Esempio:\n\n```\n![](/diagramma.png)\n```\n\n![](/vera.png)\n";

            var risultato = ManageLinkAsImagesPdf.MakeRelativeToProjectRoot(markdown);

            StringAssert.Contains(risultato, "```\n![](/diagramma.png)\n```");
            StringAssert.Contains(risultato, "![](vera.png)");
        }

        [TestMethod]
        public void Handle_several_images_in_the_same_document()
        {
            var markdown = "![](/uno.png)\n\ntesto\n\n![due](/due.png)\n\n![](tre/tre.png)\n";

            var risultato = ManageLinkAsImagesPdf.MakeRelativeToProjectRoot(markdown);

            StringAssert.Contains(risultato, "![](uno.png)");
            StringAssert.Contains(risultato, "![due](due.png)");
            StringAssert.Contains(risultato, "![](tre/tre.png)");
        }
    }
}
