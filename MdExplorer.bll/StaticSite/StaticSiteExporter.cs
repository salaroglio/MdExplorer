using HtmlAgilityPack;
using MdExplorer.Features.Slides;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.StaticSite
{
    /// <summary>A markdown file turned into its page by MdExplorer (deck or document), for the export.</summary>
    public sealed class RenderedMarkdown
    {
        public string Html { get; init; }

        /// <summary>A reveal.js deck (<c>document_type: slides</c>); otherwise a document.</summary>
        public bool IsDeck { get; init; }
    }

    public sealed class StaticSiteRequest
    {
        /// <summary>The project's folder on disk.</summary>
        public string ProjectRoot { get; init; }

        /// <summary>The service's wwwroot on disk: MdExplorer's files the pages load.</summary>
        public string WebRoot { get; init; }

        /// <summary>The file the export starts from (project-relative, with <c>/</c>): the start page opens it.</summary>
        public string Entry { get; init; }

        /// <summary>
        /// Renders a markdown file of the project (project-relative path) for the export; the second
        /// argument is where KaTeX is from that page (<c>SlideDeckRenderOptions.KatexBase</c>).
        /// </summary>
        public Func<string, string, RenderedMarkdown> Render { get; init; }
    }

    public sealed class StaticSiteExport
    {
        /// <summary>Zip path → content.</summary>
        public IReadOnlyDictionary<string, byte[]> Files { get; init; }

        public StaticSiteReport Report { get; init; }

        public void WriteZip(Stream stream)
        {
            using var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true);
            foreach (var (path, content) in Files.OrderBy(f => f.Key, StringComparer.Ordinal))
            {
                var entry = zip.CreateEntry(path, CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                entryStream.Write(content, 0, content.Length);
            }
        }
    }

    /// <summary>
    /// The static HTML export (sprint docs-internal/Sprints/2026-09-30-Slide-Export-HTML.md): from a deck,
    /// everything its links reach, recursively — decks, documents, HTML pages of the project and the files
    /// they load (images, backgrounds, videos, styles, scripts, anything) — as a local web site with no
    /// engine: a zip that opens with a double click. Each address a page carries is rewritten from the
    /// service's (<c>/api/mdexplorer/…?connectionId=…</c>, <c>/reveal/…</c>) to a relative path in the zip.
    /// What cannot be carried goes in the report (<see cref="StaticSiteReport"/>), never left out silently.
    /// </summary>
    public static class StaticSiteExporter
    {
        /// <summary>Attributes holding one address. <c>href</c> of an <c>&lt;a&gt;</c> is a link, the others load something.</summary>
        private static readonly string[] UrlAttributes =
        {
            "href", "src", "xlink:href", "poster", "data", "data-src",
            "data-background-image", "data-background-iframe",
        };

        /// <summary>reveal.js: several formats of the same video, comma-separated.</summary>
        private const string VideoAttribute = "data-background-video";

        /// <summary>
        /// Attributes of MdExplorer's view that name files of this disk: they mean nothing in a zip sent to
        /// someone else, and would tell them where the project is.
        /// </summary>
        private static readonly string[] LocalAttributes =
        {
            "DocumentPath", "ProjectPath", "ConnectionId", "data-mde-source-hash", "mdeFullPathDocument",
        };

        private static readonly Regex Scheme = new(@"^[a-zA-Z][a-zA-Z0-9+.\-]*:", RegexOptions.Compiled);
        private static readonly Regex CssUrl = new(@"url\(\s*(['""]?)([^'"")]+?)\1\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex CssImport = new(@"@import\s+(['""])([^'""]+)\1", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const string ServicePages = "/api/mdexplorer/";

        public static StaticSiteExport Export(StaticSiteRequest request)
        {
            if (request?.Render == null || string.IsNullOrEmpty(request.ProjectRoot) || string.IsNullOrEmpty(request.WebRoot) || string.IsNullOrEmpty(request.Entry))
            {
                throw new ArgumentException("The export needs the project's folder, the service's wwwroot, the file to start from and a renderer.", nameof(request));
            }
            return new Run(request).Export();
        }

        /// <summary>Where an address leads.</summary>
        private enum TargetKind { None, External, Service, Project, Asset, Missing, Outside }

        private readonly struct Target
        {
            public TargetKind Kind { get; init; }

            /// <summary>Project-relative (<see cref="TargetKind.Project"/>) or wwwroot-relative (<see cref="TargetKind.Asset"/>).</summary>
            public string Path { get; init; }

            /// <summary><c>?pages=</c> of a link to a deck, kept for the page to read.</summary>
            public string Pages { get; init; }

            /// <summary><c>#…</c>, as written.</summary>
            public string Fragment { get; init; }
        }

        /// <summary>Where the addresses of a text are read from: a file of the project, or one of MdExplorer's.</summary>
        private sealed class Context
        {
            public bool IsAsset { get; init; }

            /// <summary>The folder relative addresses are resolved against (project- or wwwroot-relative).</summary>
            public string Folder { get; init; }

            /// <summary>The zip path of the file being written: addresses become relative to it.</summary>
            public string Output { get; init; }
        }

        private sealed class Run
        {
            private readonly StaticSiteRequest _request;
            private readonly Dictionary<string, byte[]> _files = new(StringComparer.Ordinal);
            private readonly Queue<string> _projectQueue = new();
            private readonly HashSet<string> _projectSeen = new(StringComparer.Ordinal);
            private readonly Queue<(string Path, string Referer)> _assetQueue = new();
            private readonly HashSet<string> _assetSeen = new(StringComparer.Ordinal);
            private readonly StaticSiteReport _report = new();
            private readonly string _projectRootFull;

            public Run(StaticSiteRequest request)
            {
                _request = request;
                _projectRootFull = Path.GetFullPath(request.ProjectRoot).TrimEnd(Path.DirectorySeparatorChar, '/');
            }

            public StaticSiteExport Export()
            {
                var entry = Normalize(_request.Entry);
                if (!ProjectHas(entry))
                {
                    throw new StaticSiteException($"The file to export does not exist in the project: {_request.Entry}");
                }
                var startTarget = EnqueueProject(entry);

                while (_projectQueue.Count > 0 || _assetQueue.Count > 0)
                {
                    while (_projectQueue.Count > 0)
                    {
                        ExportProjectFile(_projectQueue.Dequeue());
                    }
                    while (_assetQueue.Count > 0)
                    {
                        var (asset, referer) = _assetQueue.Dequeue();
                        ExportAsset(asset, referer);
                    }
                }

                var start = "index.html";
                if (_files.ContainsKey(start))
                {
                    start = "avvio-mdexplorer.html";
                    _report.Add(StaticSiteIssueKind.StartPageRenamed, start, "index.html");
                }
                _files[start] = Encoding.UTF8.GetBytes(StartPage(StaticSitePaths.Link(start, startTarget)));

                _report.StartPage = start;
                _report.Entry = startTarget;
                _report.Files = _files.Count + 1;
                _report.Bytes = _files.Values.Sum(f => (long)f.Length);
                var reportPath = StaticSitePaths.OutputOfAsset("resoconto.html");
                _files[reportPath] = Encoding.UTF8.GetBytes(_report.ToHtml(Path.GetFileName(entry)));
                return new StaticSiteExport { Files = _files, Report = _report };
            }

            private static string Normalize(string path)
                => StaticSitePaths.Combine(string.Empty, path.Replace('\\', '/')) ?? path;

            // ---- the project's files ----

            private bool ProjectHas(string path)
                => !string.IsNullOrEmpty(path) && File.Exists(StaticSitePaths.OnDisk(_request.ProjectRoot, path));

            private bool WebRootHas(string path)
                => !string.IsNullOrEmpty(path) && File.Exists(StaticSitePaths.OnDisk(_request.WebRoot, path));

            private string OutputOf(string projectPath) => StaticSitePaths.OutputOf(projectPath, ProjectHas);

            /// <summary>Queues a file of the project, once; its zip path.</summary>
            private string EnqueueProject(string projectPath)
            {
                var output = OutputOf(projectPath);
                if (output.StartsWith(StaticSitePaths.AssetFolder + "/", StringComparison.OrdinalIgnoreCase))
                {
                    throw new StaticSiteException(
                        $"The project has a folder named '{StaticSitePaths.AssetFolder}', which the export keeps for MdExplorer's files: rename it to export ({projectPath}).");
                }
                if (_projectSeen.Add(projectPath))
                {
                    _projectQueue.Enqueue(projectPath);
                }
                return output;
            }

            private string EnqueueAsset(string assetPath, string referer)
            {
                if (_assetSeen.Add(assetPath))
                {
                    _assetQueue.Enqueue((assetPath, referer));
                }
                return StaticSitePaths.OutputOfAsset(assetPath);
            }

            private void ExportProjectFile(string projectPath)
            {
                var output = OutputOf(projectPath);
                if (StaticSitePaths.IsMarkdown(projectPath))
                {
                    RenderedMarkdown rendered;
                    try
                    {
                        rendered = _request.Render(projectPath, StaticSitePaths.Link(output, StaticSitePaths.OutputOfAsset("katex")));
                    }
                    catch (Exception ex)
                    {
                        _report.Add(StaticSiteIssueKind.RenderFailed, output, projectPath, ex.Message);
                        return;
                    }
                    if (rendered.IsDeck)
                    {
                        // The math plugin loads KaTeX by itself: not an address on the page.
                        EnqueueAssetFolder("katex", output);
                    }
                    var html = ExportPage(rendered.Html, projectPath, output, htmlPage: false);
                    _files[output] = Encoding.UTF8.GetBytes(html);
                    _report.Pages++;
                    return;
                }

                var bytes = File.ReadAllBytes(StaticSitePaths.OnDisk(_request.ProjectRoot, projectPath));
                var context = new Context { Folder = StaticSitePaths.FolderOf(projectPath), Output = output };
                if (StaticSitePaths.IsHtml(projectPath))
                {
                    _files[output] = ExportProjectHtmlPage(bytes, projectPath, output);
                    _report.Pages++;
                }
                else if (StaticSitePaths.IsCss(projectPath))
                {
                    _files[output] = Encoding.UTF8.GetBytes(RewriteCss(Encoding.UTF8.GetString(bytes), context));
                }
                else
                {
                    _files[output] = bytes;
                }
            }

            private void EnqueueAssetFolder(string folder, string referer)
            {
                var disk = StaticSitePaths.OnDisk(_request.WebRoot, folder);
                if (!Directory.Exists(disk))
                {
                    _report.Add(StaticSiteIssueKind.Missing, referer, "/" + folder, "Cartella di MdExplorer mancante nel servizio.");
                    return;
                }
                foreach (var file in Directory.EnumerateFiles(disk, "*", SearchOption.AllDirectories))
                {
                    var relative = Path.GetRelativePath(_request.WebRoot, file).Replace(Path.DirectorySeparatorChar, '/');
                    EnqueueAsset(relative, referer);
                }
            }

            private void ExportAsset(string assetPath, string referer)
            {
                if (!WebRootHas(assetPath))
                {
                    _report.Add(StaticSiteIssueKind.Missing, referer, "/" + assetPath, "File di MdExplorer mancante nel servizio.");
                    return;
                }
                var output = StaticSitePaths.OutputOfAsset(assetPath);
                var bytes = File.ReadAllBytes(StaticSitePaths.OnDisk(_request.WebRoot, assetPath));
                if (StaticSitePaths.IsCss(assetPath))
                {
                    // Fonts and images of a style sheet (reveal.js themes, KaTeX) sit next to it.
                    var context = new Context { IsAsset = true, Folder = StaticSitePaths.FolderOf(assetPath), Output = output };
                    bytes = Encoding.UTF8.GetBytes(RewriteCss(Encoding.UTF8.GetString(bytes), context));
                }
                _files[output] = bytes;
            }

            // ---- pages ----

            /// <summary>An HTML page of the project: its addresses rewritten, the deck's bar and breadcrumb added.</summary>
            private byte[] ExportProjectHtmlPage(byte[] bytes, string projectPath, string output)
            {
                var document = new HtmlDocument { OptionOutputOriginalCase = true, OptionReadEncoding = true };
                using (var stream = new MemoryStream(bytes))
                {
                    document.Load(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                }
                var encoding = document.DeclaredEncoding ?? document.StreamEncoding ?? Encoding.UTF8;

                // What MdExplorer's view adds to it (HtmlPageTrail): the way that led here, annotations, full screen.
                var host = document.DocumentNode.SelectSingleNode("//body") ?? document.DocumentNode;
                var trail = HtmlNode.CreateNode("<div></div>");
                trail.InnerHtml = HtmlPageTrail.Tags;
                foreach (var node in trail.ChildNodes.ToList())
                {
                    host.AppendChild(node);
                }
                var html = ExportDocument(document, projectPath, output, htmlPage: true);
                return encoding.GetBytes(html);
            }

            private string ExportPage(string html, string projectPath, string output, bool htmlPage)
            {
                var document = new HtmlDocument { OptionOutputOriginalCase = true };
                document.LoadHtml(html);
                return ExportDocument(document, projectPath, output, htmlPage);
            }

            private string ExportDocument(HtmlDocument document, string projectPath, string output, bool htmlPage)
            {
                var context = new Context { Folder = StaticSitePaths.FolderOf(projectPath), Output = output };

                foreach (var node in document.DocumentNode.Descendants().Where(n => n.NodeType == HtmlNodeType.Element).ToList())
                {
                    foreach (var name in UrlAttributes)
                    {
                        var attribute = node.Attributes[name];
                        if (attribute == null) continue;
                        var isLink = name == "href" && node.Name == "a";
                        attribute.Value = RewriteAddress(attribute.Value, context, isLink ? node : null);
                    }
                    var videos = node.Attributes[VideoAttribute];
                    if (videos != null)
                    {
                        videos.Value = string.Join(",", videos.Value.Split(',').Select(v => RewriteAddress(v.Trim(), context, null)));
                    }
                    var srcset = node.Attributes["srcset"];
                    if (srcset != null)
                    {
                        srcset.Value = string.Join(", ", srcset.Value.Split(',').Select(candidate =>
                        {
                            var parts = candidate.Trim().Split(' ', 2);
                            return RewriteAddress(parts[0], context, null) + (parts.Length > 1 ? " " + parts[1] : string.Empty);
                        }));
                    }
                    var style = node.Attributes["style"];
                    if (style != null && style.Value.Contains("url(", StringComparison.OrdinalIgnoreCase))
                    {
                        style.Value = RewriteCss(style.Value, context);
                    }
                    if (node.Name == "style")
                    {
                        node.InnerHtml = RewriteCss(node.InnerHtml, context);
                    }
                    foreach (var local in LocalAttributes)
                    {
                        node.Attributes.Remove(local);
                    }
                    if (node.Attributes["data-mde-missing-diagram"] != null)
                    {
                        _report.Add(StaticSiteIssueKind.DiagramNotGenerated, output, projectPath);
                    }
                    if (node.GetClasses().Contains("mermaid") || node.GetClasses().Contains("language-mermaid"))
                    {
                        _report.Add(StaticSiteIssueKind.Mermaid, output, projectPath);
                    }
                }

                // Who the page is in the export: the scripts find their way (breadcrumb, links, bar) from here.
                var root = document.DocumentNode.SelectSingleNode("//html");
                if (root != null)
                {
                    root.SetAttributeValue("data-mde-export", "");
                    root.SetAttributeValue("data-mde-export-root", StaticSitePaths.ToRoot(output));
                    root.SetAttributeValue("data-mde-export-path", output);
                    if (htmlPage) root.SetAttributeValue("data-mde-html-page", "");
                }

                var result = document.DocumentNode.OuterHtml;
                if (result.Contains(_projectRootFull, StringComparison.OrdinalIgnoreCase))
                {
                    // A path of this disk left in the page (an attribute not in LocalAttributes, a text).
                    _report.Add(StaticSiteIssueKind.LocalPath, output, _projectRootFull);
                }
                return result;
            }

            // ---- addresses ----

            /// <summary>
            /// An address as it must be written in the zip: relative to <see cref="Context.Output"/>, the file it
            /// points at queued. <paramref name="link"/> is the <c>&lt;a&gt;</c> it belongs to, if it is a link.
            /// </summary>
            private string RewriteAddress(string value, Context context, HtmlNode link)
            {
                var target = Resolve(value, context);
                switch (target.Kind)
                {
                    case TargetKind.Project:
                    {
                        var output = EnqueueProject(target.Path);
                        if (link != null)
                        {
                            // Tells slide-navigation.js what kind of page the link opens (breadcrumb).
                            link.SetAttributeValue("data-mde-export-kind",
                                StaticSitePaths.IsMarkdown(target.Path) ? "markdown" : StaticSitePaths.IsHtml(target.Path) ? "page" : "file");
                        }
                        var pages = target.Pages != null && StaticSitePaths.IsMarkdown(target.Path)
                            ? "?pages=" + Uri.EscapeDataString(target.Pages)
                            : string.Empty;
                        return StaticSitePaths.Link(context.Output, output) + pages + target.Fragment;
                    }
                    case TargetKind.Asset:
                        return StaticSitePaths.Link(context.Output, EnqueueAsset(target.Path, context.Output)) + target.Fragment;
                    case TargetKind.External:
                        if (link == null)
                        {
                            _report.Add(StaticSiteIssueKind.NeedsNetwork, context.Output, value.Trim());
                        }
                        return value;
                    case TargetKind.Service:
                        _report.Add(StaticSiteIssueKind.NeedsService, context.Output, value.Trim());
                        return value;
                    case TargetKind.Missing:
                        _report.Add(StaticSiteIssueKind.Missing, context.Output, Readable(value));
                        return value;
                    case TargetKind.Outside:
                        _report.Add(StaticSiteIssueKind.OutsideProject, context.Output, Readable(value));
                        return value;
                    default:
                        return value;
                }
            }

            /// <summary>An address without MdExplorer's own query (connectionId), as the report shows it.</summary>
            private static string Readable(string value)
                => Regex.Replace(value.Trim(), @"[?&]connectionId=[^&#]*", string.Empty, RegexOptions.IgnoreCase);

            private Target Resolve(string value, Context context)
            {
                var address = value?.Trim();
                if (string.IsNullOrEmpty(address) || address.StartsWith("#"))
                {
                    return new Target { Kind = TargetKind.None };
                }
                if (address.StartsWith("//"))
                {
                    return new Target { Kind = TargetKind.External };
                }
                if (Scheme.IsMatch(address))
                {
                    if (!address.StartsWith("http:", StringComparison.OrdinalIgnoreCase) && !address.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
                    {
                        return new Target { Kind = TargetKind.None };
                    }
                    if (!Uri.TryCreate(address, UriKind.Absolute, out var uri) || !uri.IsLoopback)
                    {
                        return new Target { Kind = TargetKind.External };
                    }
                    // The service's own address, written whole (http://localhost:5000/api/mdexplorer/…).
                    address = uri.PathAndQuery + uri.Fragment;
                }

                var hash = address.IndexOf('#');
                var fragment = hash < 0 ? string.Empty : address.Substring(hash);
                var withoutFragment = hash < 0 ? address : address.Substring(0, hash);
                var question = withoutFragment.IndexOf('?');
                var query = question < 0 ? string.Empty : withoutFragment.Substring(question + 1);
                var rawPath = question < 0 ? withoutFragment : withoutFragment.Substring(0, question);
                string path;
                try
                {
                    path = Uri.UnescapeDataString(rawPath).Replace('\\', '/');
                }
                catch (UriFormatException)
                {
                    return new Target { Kind = TargetKind.Missing };
                }
                var pages = PagesOf(query);

                if (context.IsAsset)
                {
                    var asset = path.StartsWith("/")
                        ? StaticSitePaths.Combine(string.Empty, path)
                        : StaticSitePaths.Combine(context.Folder, path);
                    return asset == null ? new Target { Kind = TargetKind.Missing } : new Target { Kind = TargetKind.Asset, Path = asset, Fragment = fragment };
                }

                string projectPath;
                if (path.StartsWith(ServicePages, StringComparison.OrdinalIgnoreCase))
                {
                    projectPath = StaticSitePaths.Combine(string.Empty, path.Substring(ServicePages.Length));
                }
                else if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
                {
                    return new Target { Kind = TargetKind.Service };
                }
                else if (path.StartsWith("/"))
                {
                    var rooted = StaticSitePaths.Combine(string.Empty, path);
                    if (rooted != null && WebRootHas(rooted))
                    {
                        return new Target { Kind = TargetKind.Asset, Path = rooted, Fragment = fragment };
                    }
                    projectPath = rooted;
                }
                else
                {
                    projectPath = StaticSitePaths.Combine(context.Folder, path);
                }

                if (projectPath == null)
                {
                    return new Target { Kind = TargetKind.Outside };
                }
                if (!ProjectHas(projectPath) && ProjectHas(projectPath + ".md"))
                {
                    // A link without the extension, as the service accepts it.
                    projectPath += ".md";
                }
                if (!IsInsideProject(projectPath))
                {
                    return new Target { Kind = TargetKind.Outside };
                }
                if (!ProjectHas(projectPath))
                {
                    // A folder is not a page: nothing to open in a zip, as a missing file.
                    return new Target { Kind = TargetKind.Missing };
                }
                return new Target { Kind = TargetKind.Project, Path = projectPath, Pages = pages, Fragment = fragment };
            }

            /// <summary>Whether a project path stays in the project's folder once the disk resolves it (links, junctions aside).</summary>
            private bool IsInsideProject(string projectPath)
            {
                var full = Path.GetFullPath(StaticSitePaths.OnDisk(_request.ProjectRoot, projectPath));
                return full.StartsWith(_projectRootFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
            }

            private static string PagesOf(string query)
            {
                foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var equals = pair.IndexOf('=');
                    var name = equals < 0 ? pair : pair.Substring(0, equals);
                    if (string.Equals(name, "pages", StringComparison.OrdinalIgnoreCase) && equals >= 0)
                    {
                        return Uri.UnescapeDataString(pair.Substring(equals + 1).Replace('+', ' '));
                    }
                }
                return null;
            }

            private string RewriteCss(string css, Context context)
            {
                css = CssUrl.Replace(css, m => $"url({m.Groups[1].Value}{RewriteAddress(m.Groups[2].Value, context, null)}{m.Groups[1].Value})");
                return CssImport.Replace(css, m => $"@import {m.Groups[1].Value}{RewriteAddress(m.Groups[2].Value, context, null)}{m.Groups[1].Value}");
            }

            private static string StartPage(string target)
            {
                var encoded = System.Net.WebUtility.HtmlEncode(target);
                return $@"<!DOCTYPE html>
<html><head><meta charset=""utf-8""><meta http-equiv=""refresh"" content=""0; url={encoded}""><title>MdExplorer</title></head>
<body><p><a href=""{encoded}"">{encoded}</a></p></body></html>
";
            }
        }
    }

    /// <summary>The export cannot be made as asked: the message says what to change.</summary>
    public sealed class StaticSiteException : Exception
    {
        public StaticSiteException(string message) : base(message) { }
    }
}
