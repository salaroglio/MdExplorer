using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Controllers;
using MdExplorer.Features.Commands;
using MdExplorer.Hubs;
using MdExplorer.Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using System.Linq;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Service.Controllers.TabBar.Automapper;
using System.Collections.Generic;
using AutoMapper;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Utilities;
using MdExplorer.Services.DatabaseManager;

namespace MdExplorer.Service.Controllers.TabBar
{
    [ApiController]
    [Route("/api/tabcontroller/{action}")]
    public class TabBarController : MdControllerBase<TabBarController>
    {
        private readonly ILogger<TabBarController> _logger;
        private readonly IMapper _mapper;
        private readonly IOptions<MdExplorerAppSettings> _options;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly IUserSettingsDB _sessionDB;
        private readonly ICommandRunner _commandRunner;

        public TabBarController(ILogger<TabBarController> logger,
                                    IMapper mapper,
                                    IOptions<MdExplorerAppSettings> options,
                                    IHubContext<MonitorMDHub> hubContext,
                                    IUserSettingsDB session,
                                    IEngineDB engineDB,
                                    IWorkLink[] modifiers,
                                    IHelper helper,
                                    ICommandRunnerHtml commandRunner,
                                    IDatabaseManager databaseManager = null) : base(logger, options, hubContext, session, engineDB, commandRunner,modifiers,helper, databaseManager)
        {
            _logger = logger;
            _mapper = mapper;
            _options = options;
            _hubContext = hubContext;
            _sessionDB = session;
            _commandRunner = commandRunner;
        }

        [HttpGet]
        public IActionResult GetTOCData([FromQuery]string fullPathFile)
        {
            _userSettingsDB.BeginTransaction();
            var docSettingDal = _userSettingsDB.GetDal<DocumentSetting>();
            var currentDocSetting = docSettingDal.GetList().Where(_ => _.DocumentPath == fullPathFile).FirstOrDefault();
            if (currentDocSetting == null)
            {

                currentDocSetting = new DocumentSetting { DocumentPath = fullPathFile,ShowTOC=true };
                docSettingDal.Save(currentDocSetting);
            }
            _userSettingsDB.Commit();
            return Ok( currentDocSetting);
        }

        [HttpPost]
        public IActionResult SaveTOCData([FromBody] DocumentSettingDto documentSetting)
        {
            _userSettingsDB.BeginTransaction();
            var docSettingDal = _userSettingsDB.GetDal<DocumentSetting>();
            var currentDocSetting = docSettingDal.GetList().Where(_ => _.DocumentPath == documentSetting.DocumentPath).FirstOrDefault();            
            if (currentDocSetting == null)
            {
                currentDocSetting = new DocumentSetting
                {
                    ShowTOC = documentSetting.ShowTOC,
                    RefsWidth = documentSetting.RefsWidth,
                    TocWidth = documentSetting.TocWidth,
                    DocumentPath = documentSetting.DocumentPath,
                    ShowRefs = documentSetting.ShowRefs,
                };
            }
            else
            {
                currentDocSetting.ShowTOC = documentSetting.ShowTOC;
                currentDocSetting.TocWidth = documentSetting.TocWidth;
                currentDocSetting.RefsWidth = documentSetting.RefsWidth;
                currentDocSetting.ShowRefs = documentSetting.ShowRefs;

            }
            docSettingDal.Save(currentDocSetting);
            _userSettingsDB.Commit();
            return Ok("done");
        }

      

        [HttpGet]
        public IActionResult GetRefsData([FromQuery] string fullPathFile)
        {
            // Return empty list if link indexing is disabled
            try
            {
                var projectPath = GetProjectPath();
                _userSettingsDB.Clear();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList()
                    .FirstOrDefault(p => p.Path == projectPath);
                if (project == null)
                {
                    project = projectDal.GetList().ToList()
                        .FirstOrDefault(p => string.Equals(p.Path, projectPath, StringComparison.OrdinalIgnoreCase));
                }
                if (project?.LinkIndexingEnabled == false)
                {
                    return Ok(new List<LinkInsideMarkdownDto>());
                }
            }
            catch { /* fallthrough to normal behavior */ }

            //_session.BeginTransaction();
            _logger.LogInformation($"[GetRefsData] Searching references for: '{fullPathFile}'");

            // Normalize the path to remove double backslashes
            var normalizedPath = fullPathFile.Replace("\\\\", "\\");
            _logger.LogInformation($"[GetRefsData] Normalized path: '{normalizedPath}'");

            var docLinkInsideMarkdownDal = GetEngineDB().GetDal<LinkInsideMarkdown>();
            var allLinks = docLinkInsideMarkdownDal.GetList().ToList();

            _logger.LogInformation($"[GetRefsData] Total links in database: {allLinks.Count}");

            // Log PlantUML links specifically
            var plantumlLinks = allLinks.Where(_ => _.Source == "WorkLinkFromPlantuml").ToList();
            _logger.LogInformation($"[GetRefsData] PlantUML links in database: {plantumlLinks.Count}");
            foreach (var link in plantumlLinks)
            {
                _logger.LogInformation($"[GetRefsData] PlantUML link - FullPath: '{link.FullPath}', From file: '{link.MarkdownFile?.Path}'");
            }

            // Log first 5 links for debugging
            foreach (var link in allLinks.Take(5))
            {
                _logger.LogInformation($"[GetRefsData] Sample link - FullPath: '{link.FullPath}', Source: '{link.Source}'");
            }

            var links = allLinks.Where(_ => _.FullPath.Contains(normalizedPath)).ToList();

            _logger.LogInformation($"[GetRefsData] Found {links.Count} matching references");
            foreach (var link in links)
            {
                _logger.LogInformation($"[GetRefsData] Match - FullPath: '{link.FullPath}', Source: '{link.Source}', File: '{link.MarkdownFile?.FileName}'");
            }

            var linkDtoList = _mapper.Map<List<LinkInsideMarkdownDto>>(links);
            //_session.Commit();
            return Ok(linkDtoList);
        }

        [HttpGet]
        public IActionResult GetKnowledgeGraph([FromQuery] string fullPathFile, [FromQuery] int depth = 1)
        {
            try
            {
                var projectPath = GetProjectPath();
                _userSettingsDB.Clear();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList().FirstOrDefault(p => p.Path == projectPath);
                if (project == null)
                {
                    project = projectDal.GetList().ToList()
                        .FirstOrDefault(p => string.Equals(p.Path, projectPath, StringComparison.OrdinalIgnoreCase));
                }
                if (project?.LinkIndexingEnabled == false)
                {
                    return Ok(new KnowledgeGraphDto { CenterId = fullPathFile });
                }
            }
            catch { /* fallthrough */ }

            if (depth < 1) depth = 1;
            if (depth > 3) depth = 3;

            string NormPath(string p) => string.IsNullOrEmpty(p) ? p : p.Replace("\\\\", "\\");

            static bool IsExternalUrl(string p)
            {
                if (string.IsNullOrWhiteSpace(p)) return false;
                var t = p.TrimStart();
                return System.Text.RegularExpressions.Regex.IsMatch(t, @"^[a-z][a-z0-9+\-.]*:(//|[^\\/])", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                    && !System.Text.RegularExpressions.Regex.IsMatch(t, @"^[a-zA-Z]:[\\/]");
            }

            static string ExternalCluster(string url)
            {
                if (string.IsNullOrWhiteSpace(url)) return "external";
                var t = url.TrimStart();
                try
                {
                    var u = new Uri(t);
                    if (!string.IsNullOrEmpty(u.Host)) return u.Host.ToLowerInvariant();
                    if (u.Scheme == "mailto")
                    {
                        var at = t.IndexOf('@');
                        if (at > 0 && at < t.Length - 1)
                        {
                            var after = t.Substring(at + 1);
                            var qx = after.IndexOfAny(new[] { '?', '#' });
                            return (qx >= 0 ? after.Substring(0, qx) : after).ToLowerInvariant();
                        }
                    }
                    return u.Scheme;
                }
                catch { return "external"; }
            }

            static string ExternalLabel(string url)
            {
                if (string.IsNullOrWhiteSpace(url)) return url;
                var t = url.TrimStart();
                try
                {
                    var u = new Uri(t);
                    var path = u.AbsolutePath?.TrimEnd('/') ?? string.Empty;
                    if (!string.IsNullOrEmpty(path) && path != "/")
                    {
                        var segs = path.Split('/');
                        var last = segs[segs.Length - 1];
                        if (!string.IsNullOrEmpty(last)) return Uri.UnescapeDataString(last);
                    }
                    return u.Host;
                }
                catch { return url; }
            }

            var normalizedCenter = NormPath(fullPathFile ?? string.Empty);
            var rawProjectRoot = GetProjectPath() ?? string.Empty;
            var projectRoot = NormPath(rawProjectRoot).TrimEnd('\\', '/');
            _logger.LogInformation($"[GetKnowledgeGraph] center='{normalizedCenter}' root='{projectRoot}' depth={depth}");

            var dal = GetEngineDB().GetDal<LinkInsideMarkdown>();
            var allLinks = dal.GetList().ToList();

            string ToRelative(string full)
            {
                if (string.IsNullOrEmpty(full)) return full;
                var fullN = NormPath(full);
                if (!string.IsNullOrEmpty(projectRoot)
                    && fullN.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                {
                    var rel = fullN.Substring(projectRoot.Length).TrimStart('\\', '/');
                    return rel.Replace('\\', '/');
                }
                return fullN.Replace('\\', '/');
            }

            var graph = new KnowledgeGraphDto { CenterId = normalizedCenter };
            var nodeMap = new Dictionary<string, KnowledgeGraphNodeDto>(StringComparer.OrdinalIgnoreCase);
            var edgeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            KnowledgeGraphNodeDto Upsert(string fullPath, string label, string mdContext, bool isCenter)
            {
                if (string.IsNullOrWhiteSpace(fullPath)) return null;
                var key = fullPath;
                if (!nodeMap.TryGetValue(key, out var node))
                {
                    node = new KnowledgeGraphNodeDto
                    {
                        Id = key,
                        FullPath = fullPath,
                        Label = string.IsNullOrWhiteSpace(label) ? System.IO.Path.GetFileName(fullPath) : label,
                        MdContext = mdContext,
                        IsCenter = isCenter,
                        RelativePath = ToRelative(fullPath),
                    };
                    nodeMap[key] = node;
                }
                else if (isCenter)
                {
                    node.IsCenter = true;
                }
                return node;
            }

            KnowledgeGraphNodeDto UpsertExternal(string url, string mdContext)
            {
                if (string.IsNullOrWhiteSpace(url)) return null;
                var key = url.TrimStart();
                if (!nodeMap.TryGetValue(key, out var node))
                {
                    node = new KnowledgeGraphNodeDto
                    {
                        Id = key,
                        FullPath = key,
                        Label = ExternalLabel(key),
                        MdContext = mdContext,
                        IsCenter = false,
                        IsExternal = true,
                        ExternalUrl = key,
                        Cluster = ExternalCluster(key),
                        RelativePath = null,
                    };
                    nodeMap[key] = node;
                }
                return node;
            }

            void AddEdge(string sourceId, string targetId, string linkSource)
            {
                if (string.IsNullOrEmpty(sourceId) || string.IsNullOrEmpty(targetId)) return;
                if (string.Equals(sourceId, targetId, StringComparison.OrdinalIgnoreCase)) return;
                var key = sourceId + "→" + targetId + "|" + linkSource;
                if (!edgeKeys.Add(key)) return;
                var linkType = LinkTypeFromSource(linkSource);
                graph.Links.Add(new KnowledgeGraphLinkDto
                {
                    Source = sourceId,
                    Target = targetId,
                    LinkType = linkType,
                    Source_LinkSource = linkSource,
                });
                if (nodeMap.TryGetValue(sourceId, out var sn)) sn.OutDegree++;
                if (nodeMap.TryGetValue(targetId, out var tn)) tn.InDegree++;
            }

            Upsert(normalizedCenter, System.IO.Path.GetFileName(normalizedCenter), null, isCenter: true);

            var frontier = new List<string> { normalizedCenter };
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { normalizedCenter };

            for (int hop = 0; hop < depth; hop++)
            {
                var nextFrontier = new List<string>();
                foreach (var current in frontier)
                {
                    // NOTE: link.Path is the ORIGINAL href as written in the markdown
                    //       (e.g. "https://github.com/..." or "../docs/file.md")
                    //       link.FullPath is a path built by concatenating the source dir + href,
                    //       so for external URLs it becomes a franken-path like
                    //       "C:\proj\https:\github.com\salaroglio\MdExplorer". The reliable
                    //       way to recognize external links is therefore link.Path.
                    var inbound = allLinks.Where(l => l.FullPath != null
                        && !IsExternalUrl(l.Path)
                        && NormPath(l.FullPath).IndexOf(current, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                    foreach (var link in inbound)
                    {
                        var srcFile = NormPath(link.MarkdownFile?.Path);
                        if (string.IsNullOrWhiteSpace(srcFile)) continue;
                        Upsert(srcFile, System.IO.Path.GetFileName(srcFile), link.MdContext, isCenter: false);
                        AddEdge(srcFile, current, link.Source);
                        if (!visited.Contains(srcFile)) { visited.Add(srcFile); nextFrontier.Add(srcFile); }
                    }

                    var outbound = allLinks.Where(l => l.MarkdownFile != null && l.MarkdownFile.Path != null
                        && NormPath(l.MarkdownFile.Path).IndexOf(current, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                    foreach (var link in outbound)
                    {
                        if (IsExternalUrl(link.Path))
                        {
                            var extUrl = link.Path.TrimStart();
                            UpsertExternal(extUrl, link.MdContext);
                            AddEdge(current, extUrl, link.Source);
                            // external URLs are leaves: we don't expand from them
                            continue;
                        }
                        var tgt = NormPath(link.FullPath);
                        if (string.IsNullOrWhiteSpace(tgt)) continue;
                        Upsert(tgt, System.IO.Path.GetFileName(tgt), link.MdContext, isCenter: false);
                        AddEdge(current, tgt, link.Source);
                        if (!visited.Contains(tgt)) { visited.Add(tgt); nextFrontier.Add(tgt); }
                    }
                }
                frontier = nextFrontier;
                if (frontier.Count == 0) break;
            }

            graph.Nodes.AddRange(nodeMap.Values);
            _logger.LogInformation($"[GetKnowledgeGraph] nodes={graph.Nodes.Count} links={graph.Links.Count}");
            return Ok(graph);
        }

        private static string LinkTypeFromSource(string source)
        {
            switch (source)
            {
                case "WorkLinkFromMarkdown": return "link";
                case "WorkLinkMdShowMd": return "publication";
                case "WorkLinkMdShowH2": return "excerpt";
                case "WorkLinkFromPlantuml": return "plantuml";
                default: return "other";
            }
        }

    }
}
