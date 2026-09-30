using System.Linq;
using System.Text;

namespace MdExplorer.Features.Slides
{
    /// <summary>
    /// An HTML page of the project opened from a slide deck shows the way that led to it (the
    /// breadcrumb of the decks, slide-navigation.js). The page is the user's file: the breadcrumb
    /// is added when it is served to MdExplorer's view, never in the file on disk. Added as bytes
    /// at the end, so the page's own encoding is not touched; a browser takes what follows
    /// &lt;/html&gt; as part of the body.
    /// </summary>
    public static class HtmlPageTrail
    {
        private const string Tags =
            "\n<link rel=\"stylesheet\" href=\"/javascripts/slides/slide-navigation.css\">" +
            "<script src=\"/javascripts/slides/html-page-trail.js\"></script>\n";

        public static byte[] AddTo(byte[] html)
        {
            return html.Concat(Encoding.ASCII.GetBytes(Tags)).ToArray();
        }
    }
}
