using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Slides
{
    /// <summary>
    /// Applies the attribute comments of reveal.js's markdown plugin to a rendered slide, with the
    /// plugin's own rules (reveal.js 6.0.2, <c>plugin/markdown</c>, functions <c>addAttributes</c>
    /// and <c>addAttributeInElement</c>):
    /// <list type="bullet">
    /// <item><c>&lt;!-- .element: class="fragment" --&gt;</c> goes to the nearest element before the
    /// comment among its siblings (a <c>&lt;br&gt;</c> does not count), or to its parent when there is
    /// none: written at the end of a list item, it goes to the <c>&lt;li&gt;</c>;</item>
    /// <item>right after a list, it goes to the list's last item when no blank line separates them,
    /// to the whole list otherwise;</item>
    /// <item><c>&lt;!-- .slide: data-background-color="#000" --&gt;</c> goes to the slide's
    /// <c>&lt;section&gt;</c>.</item>
    /// </list>
    /// An attribute written this way replaces the element's own (a <c>class</c> replaces the class),
    /// as <c>setAttribute</c> does in the plugin.
    /// </summary>
    internal static class SlideAttributes
    {
        /// <summary>
        /// Set by <see cref="SlideDeckRenderer"/> at the start of an attribute comment written on the
        /// line after a list: the HTML no longer says whether a blank line separated them.
        /// </summary>
        internal const string AfterListItemMarker = "mde-after-list-item";

        private static readonly Regex ElementAttributes = new(@"\.element\s*?(.+?)$", RegexOptions.Multiline);
        private static readonly Regex SlideAttributesComment = new(@"\.slide:\s*?(\S.+?)$", RegexOptions.Multiline);
        private static readonly Regex Attribute = new("([^\"= ]+?)=\"([^\"]+?)\"|(data-[^\"= ]+?)(?=[\" ])");

        /// <summary>Whether <paramref name="html"/> has comments to apply at all: most slides do not, and are left untouched.</summary>
        public static bool AnyIn(string html)
            => html.Contains("<!--") && (html.Contains(".element") || html.Contains(".slide:"));

        /// <summary>Applies the comments found in <paramref name="section"/>'s content to its elements and to the section itself.</summary>
        public static void Apply(HtmlNode section) => Apply(section, section, null);

        private static void Apply(HtmlNode section, HtmlNode node, HtmlNode previousElement)
        {
            var children = node.ChildNodes;
            var nearest = node;
            for (var i = 0; i < children.Count; i++)
            {
                for (var j = i - 1; j >= 0; j--)
                {
                    if (children[j].NodeType == HtmlNodeType.Element && children[j].Name != "br")
                    {
                        nearest = children[j];
                        break;
                    }
                }
                var child = children[i];
                if (child.NodeType is HtmlNodeType.Element or HtmlNodeType.Comment)
                {
                    Apply(section, child, nearest);
                }
            }

            if (node is HtmlCommentNode comment)
            {
                var target = previousElement;
                if (target is { Name: "ul" or "ol" } && comment.Comment.Contains(AfterListItemMarker))
                {
                    target = target.LastChild(HtmlNodeType.Element) ?? target;
                }
                if (!ApplyIn(comment, target, ElementAttributes))
                {
                    ApplyIn(comment, section, SlideAttributesComment);
                }
            }
        }

        private static bool ApplyIn(HtmlCommentNode comment, HtmlNode target, Regex separator)
        {
            // The plugin reads the comment's nodeValue, without "<!--" and "-->": HtmlAgilityPack's
            // Comment has them, and "(.+?)$" would swallow the closing "-->".
            var whole = comment.Comment;
            if (!whole.StartsWith("<!--") || !whole.EndsWith("-->"))
            {
                return false;
            }
            var text = whole.Substring(4, whole.Length - 7);
            var match = separator.Match(text);
            if (!match.Success || target == null)
            {
                return false;
            }
            text = text.Remove(match.Index, match.Length).Replace(AfterListItemMarker, "");
            comment.Comment = "<!--" + text + "-->";
            foreach (Match attribute in Attribute.Matches(match.Groups[1].Value))
            {
                if (attribute.Groups[2].Success)
                {
                    target.SetAttributeValue(attribute.Groups[1].Value, attribute.Groups[2].Value);
                }
                else
                {
                    target.SetAttributeValue(attribute.Groups[3].Value, "");
                }
            }
            return true;
        }

        private static HtmlNode LastChild(this HtmlNode node, HtmlNodeType type)
        {
            for (var child = node.LastChild; child != null; child = child.PreviousSibling)
            {
                if (child.NodeType == type)
                {
                    return child;
                }
            }
            return null;
        }
    }
}
