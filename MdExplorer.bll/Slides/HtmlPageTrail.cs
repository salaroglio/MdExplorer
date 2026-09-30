using System.Linq;
using System.Text;

namespace MdExplorer.Features.Slides
{
    /// <summary>
    /// An HTML page of the project shown in MdExplorer's view gets the way that led to it (the breadcrumb of
    /// the decks, slide-navigation.js) and the deck's bar with its tools — annotations and full screen, not the
    /// states (slide-toolbar.js, slide-ink.js). The page is the user's file: the breadcrumb
    /// is added when it is served to MdExplorer's view, never in the file on disk. Added as bytes
    /// at the end, so the page's own encoding is not touched; a browser takes what follows
    /// &lt;/html&gt; as part of the body.
    /// </summary>
    public static class HtmlPageTrail
    {
        private const string Tags =
            // The page is marked as one of the project's HTML pages: the deck's bar shows its tools (annotations, full
            // screen) and none of its states, and the annotations take the whole document as their surface.
            "\n<script>document.documentElement.setAttribute(\"data-mde-html-page\", \"\");</script>" +
            "<link rel=\"stylesheet\" href=\"/javascripts/jqueryForFirstPage/images/image-toolbar.css\">" +
            "<link rel=\"stylesheet\" href=\"/javascripts/slides/slide-toolbar.css\">" +
            "<link rel=\"stylesheet\" href=\"/javascripts/slides/slide-ink.css\">" +
            "<link rel=\"stylesheet\" href=\"/javascripts/slides/slide-navigation.css\">" +
            "<script src=\"/javascripts/jqueryForFirstPage/images/toolbar-shared.js\"></script>" +
            "<script src=\"/javascripts/slides/html-page-trail.js\"></script>" +
            "<script src=\"/javascripts/slides/slide-toolbar.js\"></script>" +
            "<script src=\"/javascripts/slides/slide-ink.js\"></script>\n";

        public static byte[] AddTo(byte[] html)
        {
            return html.Concat(Encoding.ASCII.GetBytes(Tags)).ToArray();
        }
    }
}
