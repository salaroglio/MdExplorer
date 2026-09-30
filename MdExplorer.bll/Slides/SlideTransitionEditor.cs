using HtmlAgilityPack;
using Markdig;
using MdExplorer.Features.Services.SourceMapping;
using MdExplorer.Features.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Slides
{
    /// <summary>Why a transition was not written.</summary>
    public enum SlideTransitionRefusal
    {
        /// <summary>Not one of <see cref="SlideTransitionEditor.Transitions"/>.</summary>
        UnknownTransition,
        /// <summary>The file is not a slide deck (no front matter with document_type: slides).</summary>
        NotADeck,
        /// <summary>The line is not the start of a slide of the file (it changed, or the slide was written by a command).</summary>
        NotASlide,
        /// <summary>The file is written in a way this does not dare to edit (a .slide: comment on several lines, a front matter in flow style).</summary>
        UnsupportedLayout,
        /// <summary>The new file, read again, is not the old one with only the transition changed: nothing was written.</summary>
        ChangesOtherContent,
    }

    /// <summary>The outcome of a transition change: the new text, nothing to do, or why nothing was written.</summary>
    public sealed record SlideTransitionEdit(string NewText, SlideTransitionRefusal? Refusal, string Detail, bool NoChange = false)
    {
        public bool Applied => NewText != null;

        public static SlideTransitionEdit Done(string text) => new(text, null, null);

        public static SlideTransitionEdit Unchanged() => new(null, null, "It is already like that.", true);

        public static SlideTransitionEdit Refused(SlideTransitionRefusal reason, string detail) => new(null, reason, detail);
    }

    /// <summary>
    /// Changes the transition of one slide (<c>&lt;!-- .slide: data-transition="zoom" --&gt;</c>, in the slide's
    /// own lines) or of the whole deck (<c>reveal.config.transition</c> in the front matter), editing the
    /// lines of the file and nothing else.
    ///
    /// <para>
    /// A slide's own value comes first over the deck's (reveal.js's CSS applies the deck's only to a
    /// <c>&lt;section&gt;</c> without <c>data-transition</c>, measured on reveal.js 6.0.2), and it governs how
    /// <i>that slide</i> enters and leaves: there is no "transition between A and B", each of the two slides
    /// animates by its own rule at the same time.
    /// </para>
    ///
    /// <para>
    /// Nothing is written unless the new file, read again, is the old one with only that changed: for a
    /// slide, the deck is rendered before and after and every slide must come out the same but for the
    /// <c>data-transition</c> of that one; for the deck, the front matter is read again and every other
    /// setting must be the same. The same rule as <c>ListItemReorderer</c>: nothing is repaired silently.
    /// </para>
    /// </summary>
    public static class SlideTransitionEditor
    {
        /// <summary>The ones reveal.js documents (its CSS has a few more: linear, default).</summary>
        public static readonly IReadOnlyList<string> Transitions = new[] { "none", "fade", "slide", "convex", "concave", "zoom" };

        private static readonly Regex SlideCommentStart = new(@"<!--\s*\.slide:", RegexOptions.Compiled);
        private static readonly Regex SlideCommentLine = new(@"<!--\s*\.slide:(?<attrs>[^\r\n]*?)-->", RegexOptions.Compiled);
        private static readonly Regex TransitionAttribute = new(@"\s*data-transition\s*=\s*""[^""]*""", RegexOptions.Compiled);

        // ── One slide ───────────────────────────────────────────────────────────────────────

        /// <param name="text">The markdown file as it is on disk.</param>
        /// <param name="slideLine">1-based file line the slide's own lines start on (<c>data-mde-line-start</c> of its section).</param>
        /// <param name="transition">One of <see cref="Transitions"/>, or null: the deck's (the attribute is taken away).</param>
        public static SlideTransitionEdit SetSlide(string text, int slideLine, string transition, MarkdownPipeline pipeline)
        {
            if (transition != null && !Transitions.Contains(transition))
                return SlideTransitionEdit.Refused(SlideTransitionRefusal.UnknownTransition, $"'{transition}' is not a transition: {string.Join(", ", Transitions)}.");

            string body;
            try { body = SlideDeckFrontMatter.Read(text, false).Body; }
            catch (SlideDeckException ex) { return SlideTransitionEdit.Refused(SlideTransitionRefusal.NotADeck, ex.Message); }

            var linesAbove = text.Substring(0, text.Length - body.Length).Count(c => c == '\n');
            var slides = SlideSplitter.Split(body).SelectMany(stack => stack).ToList();
            var index = slides.FindIndex(s => s.FirstLine + linesAbove + 1 == slideLine);
            if (index < 0)
                return SlideTransitionEdit.Refused(SlideTransitionRefusal.NotASlide, $"Line {slideLine} is not the start of a slide of the file.");

            var starts = MarkdownSourceMapService.BuildLineStartOffsets(text);
            var firstLine = slides[index].FirstLine + linesAbove;
            var own = MarkdownSourceMapService.SplitLines(slides[index].Markdown);
            var lineCount = own.Length > 1 && own[own.Length - 1].Length == 0 ? own.Length - 1 : own.Length;
            var endLine = Math.Min(starts.Length, firstLine + Math.Max(1, lineCount));
            var rangeStart = starts[firstLine];
            var rangeEnd = endLine < starts.Length ? starts[endLine] : text.Length;

            var regions = MarkdownCodeRegions.Of(text);
            bool InSlide(Match m) => m.Index >= rangeStart && m.Index < rangeEnd && !regions.IsCode(m.Index);
            var starting = SlideCommentStart.Matches(text).Cast<Match>().Count(InSlide);
            var comments = SlideCommentLine.Matches(text).Cast<Match>().Where(InSlide).ToList();
            if (starting != comments.Count)
                return SlideTransitionEdit.Refused(SlideTransitionRefusal.UnsupportedLayout, "A .slide: comment of this slide is written on several lines: change its data-transition in the file.");

            var eol = text.Contains("\r\n") ? "\r\n" : "\n";
            var target = comments.FirstOrDefault(m => TransitionAttribute.IsMatch(m.Groups["attrs"].Value)) ?? comments.FirstOrDefault();
            string newText;
            if (target != null)
            {
                var attributes = target.Groups["attrs"].Value;
                if (transition != null)
                {
                    var written = $" data-transition=\"{transition}\"";
                    var changed = TransitionAttribute.IsMatch(attributes)
                        ? TransitionAttribute.Replace(attributes, written, 1)
                        : attributes.TrimEnd() + written + " ";
                    var head = target.Value.Substring(0, target.Groups["attrs"].Index - target.Index);
                    newText = text.Substring(0, target.Index) + head + changed + "-->" + text.Substring(target.Index + target.Length);
                }
                else if (!TransitionAttribute.IsMatch(attributes))
                {
                    return SlideTransitionEdit.Unchanged();
                }
                else
                {
                    var left = TransitionAttribute.Replace(attributes, string.Empty, 1);
                    if (left.Trim().Length > 0)
                    {
                        var head = target.Value.Substring(0, target.Groups["attrs"].Index - target.Index);
                        newText = text.Substring(0, target.Index) + head + left + "-->" + text.Substring(target.Index + target.Length);
                    }
                    else
                    {
                        // Nothing else in the comment: it goes away, with its line when it had the line to itself.
                        var line = LineRange(text, starts, target.Index);
                        var alone = text.Substring(line.Start, line.End - line.Start).Trim() == target.Value;
                        newText = alone
                            ? text.Remove(line.Start, line.End - line.Start)
                            : text.Remove(target.Index, target.Length);
                    }
                }
            }
            else if (transition == null)
            {
                return SlideTransitionEdit.Unchanged();
            }
            else
            {
                // A new comment, on the slide's first line with something on it.
                var at = firstLine;
                while (at < endLine - 1 && at < starts.Length && Line(text, starts, at).Trim().Length == 0) at++;
                if (Line(text, starts, at).Trim().Length == 0) at = firstLine;
                newText = text.Insert(starts[at], $"<!-- .slide: data-transition=\"{transition}\" -->{eol}");
            }

            if (newText == text) return SlideTransitionEdit.Unchanged();
            return VerifySlide(text, newText, index, transition, pipeline)
                ? SlideTransitionEdit.Done(newText)
                : SlideTransitionEdit.Refused(SlideTransitionRefusal.ChangesOtherContent, "Writing the transition would change something else in the deck: nothing was written.");
        }

        private static string Line(string text, int[] starts, int line)
            => text.Substring(starts[line], (line + 1 < starts.Length ? starts[line + 1] : text.Length) - starts[line]);

        private static (int Start, int End) LineRange(string text, int[] starts, int offset)
        {
            var i = Array.BinarySearch(starts, offset);
            var line = i >= 0 ? i : ~i - 1;
            return (starts[line], line + 1 < starts.Length ? starts[line + 1] : text.Length);
        }

        /// <summary>The deck rendered before and after: the same slides, each the same but for the target's data-transition.</summary>
        private static bool VerifySlide(string before, string after, int index, string expected, MarkdownPipeline pipeline)
        {
            List<HtmlNode> Leaves(string markdown)
            {
                var page = SlideDeckRenderer.Render(markdown, new SlideDeckRenderOptions { Pipeline = pipeline });
                var document = new HtmlDocument { OptionOutputOriginalCase = true };
                document.LoadHtml(page);
                var root = document.DocumentNode.SelectSingleNode("//div[@class='slides']");
                return root == null ? new List<HtmlNode>() : root.Descendants("section").Where(s => !s.Descendants("section").Any()).ToList();
            }
            // A .slide: comment is consumed by the renderer: what stays is an empty comment (<!-- -->) and a line break. That is the whole trace of the edit, and it is not compared.
            string Others(HtmlNode s) => string.Join("|", s.Attributes.Where(a => a.Name != "data-transition").Select(a => a.Name + "=" + a.Value)) + "\n" + s.InnerHtml.Replace("<!-- -->", string.Empty).Trim();

            var old = Leaves(before);
            var now = Leaves(after);
            if (old.Count != now.Count || index >= now.Count) return false;
            for (var i = 0; i < old.Count; i++)
            {
                if (Others(old[i]) != Others(now[i])) return false;
                var oldValue = old[i].GetAttributeValue("data-transition", null);
                var newValue = now[i].GetAttributeValue("data-transition", null);
                if (i == index ? newValue != expected : newValue != oldValue) return false;
            }
            return true;
        }

        // ── The whole deck ──────────────────────────────────────────────────────────────────

        /// <summary>Sets <c>reveal.config.transition</c> in the front matter. There is no "take it away": the deck's own is what reveal.js uses when a slide does not say.</summary>
        public static SlideTransitionEdit SetDeck(string text, string transition)
        {
            if (transition == null || !Transitions.Contains(transition))
                return SlideTransitionEdit.Refused(SlideTransitionRefusal.UnknownTransition, $"'{transition}' is not a transition: {string.Join(", ", Transitions)}.");

            SlideDeckSettings before;
            try { before = SlideDeckFrontMatter.Read(text, false).Settings; }
            catch (SlideDeckException ex) { return SlideTransitionEdit.Refused(SlideTransitionRefusal.NotADeck, ex.Message); }

            var crlf = text.Contains("\r\n");
            var lines = text.Split('\n').ToList();
            var cr = crlf ? "\r" : string.Empty;
            string Plain(int i) => lines[i].TrimEnd('\r');
            int Indent(string line) => line.Length - line.TrimStart(' ').Length;
            bool Skipped(string line) => line.Trim().Length == 0 || line.TrimStart().StartsWith("#", StringComparison.Ordinal);

            if (Plain(0).TrimStart('﻿').Trim() != "---") return SlideTransitionEdit.Refused(SlideTransitionRefusal.NotADeck, "The file does not start with a front matter.");
            var close = Enumerable.Range(1, lines.Count - 1).FirstOrDefault(i => Plain(i).Trim() == "---");
            if (close == 0) return SlideTransitionEdit.Refused(SlideTransitionRefusal.NotADeck, "The front matter is not closed.");

            // The block of a key at some indentation: the lines after it that are deeper (blank lines and comments go with it).
            int BlockEnd(int keyLine, int keyIndent, int limit)
            {
                var end = keyLine + 1;
                for (var i = keyLine + 1; i < limit; i++)
                {
                    if (Skipped(Plain(i))) continue;
                    if (Indent(Plain(i)) <= keyIndent) break;
                    end = i + 1;
                }
                return end;
            }
            int FindKey(string key, int from, int to, int indent)
            {
                for (var i = from; i < to; i++)
                    if (!Skipped(Plain(i)) && Indent(Plain(i)) == indent && Regex.IsMatch(Plain(i), @"^\s*" + key + @":(\s|$)")) return i;
                return -1;
            }
            string Inline(int line, string key) => Regex.Replace(Plain(line).Trim(), @"^" + key + @":\s*", string.Empty).Split('#')[0].Trim();

            var reveal = FindKey("reveal", 1, close, 0);
            if (reveal >= 0 && Inline(reveal, "reveal").Length > 0)
                return SlideTransitionEdit.Refused(SlideTransitionRefusal.UnsupportedLayout, "'reveal:' is written on one line: change the transition in the file.");

            if (reveal < 0)
            {
                lines.InsertRange(close, new[] { "reveal:" + cr, "  config:" + cr, "    transition: " + transition + cr });
            }
            else
            {
                var revealEnd = BlockEnd(reveal, 0, close);
                var childIndent = Enumerable.Range(reveal + 1, revealEnd - reveal - 1).Where(i => !Skipped(Plain(i))).Select(i => Indent(Plain(i))).DefaultIfEmpty(2).Min();
                var config = FindKey("config", reveal + 1, revealEnd, childIndent);
                if (config >= 0 && Inline(config, "config").Length > 0)
                    return SlideTransitionEdit.Refused(SlideTransitionRefusal.UnsupportedLayout, "'config:' is written on one line: change the transition in the file.");
                if (config < 0)
                {
                    lines.InsertRange(reveal + 1, new[] { new string(' ', childIndent) + "config:" + cr, new string(' ', childIndent + 2) + "transition: " + transition + cr });
                }
                else
                {
                    var configEnd = BlockEnd(config, childIndent, close);
                    var configChild = Enumerable.Range(config + 1, configEnd - config - 1).Where(i => !Skipped(Plain(i))).Select(i => Indent(Plain(i))).DefaultIfEmpty(childIndent + 2).Min();
                    var existing = FindKey("transition", config + 1, configEnd, configChild);
                    if (existing >= 0)
                    {
                        var m = Regex.Match(Plain(existing), @"^(?<head>\s*transition:\s*)(?<value>[^\s#]+)(?<tail>.*)$");
                        if (!m.Success) return SlideTransitionEdit.Refused(SlideTransitionRefusal.UnsupportedLayout, "The 'transition:' line of the front matter is written in a way this does not edit.");
                        lines[existing] = m.Groups["head"].Value + transition + m.Groups["tail"].Value + cr;
                    }
                    else
                    {
                        lines.Insert(config + 1, new string(' ', configChild) + "transition: " + transition + cr);
                    }
                }
            }

            var newText = string.Join("\n", lines);
            if (newText == text) return SlideTransitionEdit.Unchanged();

            // Read again: the transition is the one asked for, and nothing else of the settings moved.
            try
            {
                var (after, newBody) = SlideDeckFrontMatter.Read(newText, false);
                var oldBody = SlideDeckFrontMatter.Read(text, false).Body;
                string Rest(SlideDeckSettings s)
                {
                    var config = s.Config == null ? new JsonObject() : JsonNode.Parse(s.Config.ToJsonString()).AsObject();
                    config.Remove("transition");
                    return $"{s.Title}|{s.Theme}|{s.HighlightTheme}|{config.ToJsonString()}";
                }
                if (after.Config?["transition"]?.GetValue<string>() == transition && Rest(after) == Rest(before) && newBody == oldBody)
                    return SlideTransitionEdit.Done(newText);
            }
            catch (Exception ex) when (ex is SlideDeckException || ex is InvalidOperationException || ex is FormatException)
            {
                // fall through: the file would not read.
            }
            return SlideTransitionEdit.Refused(SlideTransitionRefusal.ChangesOtherContent, "Writing the transition would change something else in the front matter: nothing was written.");
        }
    }
}
