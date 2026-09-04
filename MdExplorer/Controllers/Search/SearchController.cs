using AutoMapper;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Utilities;
using MdExplorer.Hubs;
using MdExplorer.Service.Controllers.Search.Dto;
using MdExplorer.Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Service.Controllers.Search
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : MdControllerBase<SearchController>
    {
        private readonly ILogger<SearchController> _logger;
        private readonly ISearchService _searchService;
        private readonly IMapper _mapper;
        private readonly ITextFtsService _textFtsService;

        public SearchController(
            ILogger<SearchController> logger,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB userSettingsDB,
            IEngineDB engineDB,
            ICommandRunnerHtml commandRunner,
            IWorkLink[] modifiers,
            IHelper helper,
            ISearchService searchService,
            IMapper mapper,
            MdExplorer.Services.DatabaseManager.IDatabaseManager databaseManager,
            ITextFtsService textFtsService = null)
            : base(logger, options, hubContext, userSettingsDB, engineDB, commandRunner, modifiers, helper, databaseManager)
        {
            _logger = logger;
            _searchService = searchService;
            _mapper = mapper;
            _textFtsService = textFtsService;
        }

        [HttpGet("quick")]
        public async Task<IActionResult> QuickSearch([FromQuery] string term, [FromQuery] int maxResults = 20)
        {
            _logger.LogInformation($"[SearchController] QuickSearch called with term: '{term}'");

            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(new SearchResultDto 
                { 
                    SearchTerm = term,
                    SearchDurationMs = 0
                });
            }

            try
            {
                var result = await _searchService.SearchAsync(term, Abstractions.Services.SearchType.All, maxResults, GetProjectPath());
                
                // Map to DTOs
                var dto = new SearchResultDto
                {
                    SearchTerm = result.SearchTerm,
                    TotalFiles = result.TotalFiles,
                    TotalLinks = result.TotalLinks,
                    TotalContents = result.TotalContents,
                    SearchDurationMs = result.SearchDurationMs,
                    Contents = result.Contents.Select(c => new ContentSearchResultDto
                    {
                        MarkdownFileId = c.MarkdownFileId,
                        FileName = c.FileName,
                        Path = c.Path,
                        Snippet = c.Snippet,
                        Score = c.Score
                    }).ToList(),
                    Files = result.Files.Select(f => new FileSearchResultDto
                    {
                        Id = f.Id,
                        FileName = f.FileName,
                        Path = f.Path,
                        FileType = f.FileType,
                        MatchedField = f.MatchedField,
                        HighlightedText = f.HighlightedText
                    }).ToList(),
                    Links = result.Links.Select(l => new LinkSearchResultDto
                    {
                        Id = l.Id,
                        Path = l.Path,
                        FullPath = l.FullPath,
                        MdTitle = l.MdTitle,
                        HtmlTitle = l.HtmlTitle,
                        MdContext = l.MdContext,
                        Source = l.Source,
                        LinkedCommand = l.LinkedCommand,
                        MarkdownFileName = l.MarkdownFileName,
                        MarkdownFilePath = l.MarkdownFilePath,
                        MatchedField = l.MatchedField,
                        HighlightedText = l.HighlightedText
                    }).ToList()
                };

                _logger.LogInformation($"[SearchController] Search completed. Found {dto.TotalFiles} files and {dto.TotalLinks} links in {dto.SearchDurationMs}ms");
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SearchController] Error during search for term: '{term}'");
                return StatusCode(500, new { error = "Errore durante la ricerca", details = ex.Message });
            }
        }

        [HttpPost("advanced")]
        public async Task<IActionResult> AdvancedSearch([FromBody] SearchRequestDto request)
        {
            _logger.LogInformation($"[SearchController] AdvancedSearch called with term: '{request.SearchTerm}', type: {request.SearchType}");

            if (request == null || string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                return Ok(new SearchResultDto 
                { 
                    SearchTerm = request?.SearchTerm ?? "",
                    SearchDurationMs = 0
                });
            }

            try
            {
                var searchType = request.SearchType switch
                {
                    Dto.SearchType.Files => Abstractions.Services.SearchType.Files,
                    Dto.SearchType.Links => Abstractions.Services.SearchType.Links,
                    Dto.SearchType.Content => Abstractions.Services.SearchType.Content,
                    _ => Abstractions.Services.SearchType.All
                };

                var result = await _searchService.SearchAsync(request.SearchTerm, searchType, request.MaxResults, GetProjectPath());
                
                // Map to DTOs
                var dto = new SearchResultDto
                {
                    SearchTerm = result.SearchTerm,
                    TotalFiles = result.TotalFiles,
                    TotalLinks = result.TotalLinks,
                    TotalContents = result.TotalContents,
                    SearchDurationMs = result.SearchDurationMs,
                    Contents = result.Contents.Select(c => new ContentSearchResultDto
                    {
                        MarkdownFileId = c.MarkdownFileId,
                        FileName = c.FileName,
                        Path = c.Path,
                        Snippet = c.Snippet,
                        Score = c.Score
                    }).ToList(),
                    Files = result.Files.Select(f => new FileSearchResultDto
                    {
                        Id = f.Id,
                        FileName = f.FileName,
                        Path = f.Path,
                        FileType = f.FileType,
                        MatchedField = f.MatchedField,
                        HighlightedText = f.HighlightedText
                    }).ToList(),
                    Links = result.Links.Select(l => new LinkSearchResultDto
                    {
                        Id = l.Id,
                        Path = l.Path,
                        FullPath = l.FullPath,
                        MdTitle = l.MdTitle,
                        HtmlTitle = l.HtmlTitle,
                        MdContext = l.MdContext,
                        Source = l.Source,
                        LinkedCommand = l.LinkedCommand,
                        MarkdownFileName = l.MarkdownFileName,
                        MarkdownFilePath = l.MarkdownFilePath,
                        MatchedField = l.MatchedField,
                        HighlightedText = l.HighlightedText
                    }).ToList()
                };

                _logger.LogInformation($"[SearchController] Advanced search completed. Found {dto.TotalFiles} files and {dto.TotalLinks} links in {dto.SearchDurationMs}ms");
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SearchController] Error during advanced search for term: '{request.SearchTerm}'");
                return StatusCode(500, new { error = "Errore durante la ricerca avanzata", details = ex.Message });
            }
        }

        [HttpGet("files")]
        public async Task<IActionResult> SearchFiles([FromQuery] string term, [FromQuery] int maxResults = 50)
        {
            _logger.LogInformation($"[SearchController] SearchFiles called with term: '{term}'");

            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(new { files = new FileSearchResultDto[0], totalFiles = 0 });
            }

            try
            {
                var results = await _searchService.SearchFilesAsync(term, maxResults);
                var dtos = results.Select(f => new FileSearchResultDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    Path = f.Path,
                    FileType = f.FileType,
                    MatchedField = f.MatchedField,
                    HighlightedText = f.HighlightedText
                }).ToList();

                _logger.LogInformation($"[SearchController] File search completed. Found {dtos.Count} files");
                return Ok(new { files = dtos, totalFiles = dtos.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SearchController] Error during file search for term: '{term}'");
                return StatusCode(500, new { error = "Errore durante la ricerca dei file", details = ex.Message });
            }
        }

        [HttpGet("content")]
        public async Task<IActionResult> SearchContent([FromQuery] string term, [FromQuery] int maxResults = 50)
        {
            _logger.LogInformation($"[SearchController] SearchContent called with term: '{term}'");

            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(new { contents = new ContentSearchResultDto[0], totalContents = 0 });
            }

            var projectPath = GetProjectPath();
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                return BadRequest(new { error = "Nessun progetto aperto per questa connessione: la ricerca nel contenuto richiede un progetto aperto (ConnectionId valido)." });
            }

            try
            {
                var results = await _searchService.SearchContentAsync(term, projectPath, maxResults);
                var dtos = results.Select(c => new ContentSearchResultDto
                {
                    MarkdownFileId = c.MarkdownFileId,
                    FileName = c.FileName,
                    Path = c.Path,
                    Snippet = c.Snippet,
                    Score = c.Score
                }).ToList();

                _logger.LogInformation($"[SearchController] Content search completed. Found {dtos.Count} matches");
                return Ok(new { contents = dtos, totalContents = dtos.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SearchController] Error during content search for term: '{term}'");
                return StatusCode(500, new { error = "Errore durante la ricerca nel contenuto", details = ex.Message });
            }
        }

        [HttpGet("links")]
        public async Task<IActionResult> SearchLinks([FromQuery] string term, [FromQuery] int maxResults = 50)
        {
            _logger.LogInformation($"[SearchController] SearchLinks called with term: '{term}'");

            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(new { links = new LinkSearchResultDto[0], totalLinks = 0 });
            }

            try
            {
                var results = await _searchService.SearchLinksAsync(term, maxResults);
                var dtos = results.Select(l => new LinkSearchResultDto
                {
                    Id = l.Id,
                    Path = l.Path,
                    FullPath = l.FullPath,
                    MdTitle = l.MdTitle,
                    HtmlTitle = l.HtmlTitle,
                    MdContext = l.MdContext,
                    Source = l.Source,
                    LinkedCommand = l.LinkedCommand,
                    MarkdownFileName = l.MarkdownFileName,
                    MarkdownFilePath = l.MarkdownFilePath,
                    MatchedField = l.MatchedField,
                    HighlightedText = l.HighlightedText
                }).ToList();

                _logger.LogInformation($"[SearchController] Link search completed. Found {dtos.Count} links");
                return Ok(new { links = dtos, totalLinks = dtos.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SearchController] Error during link search for term: '{term}'");
                return StatusCode(500, new { error = "Errore durante la ricerca dei link", details = ex.Message });
            }
        }

        /// <summary>
        /// Whether the SEPARATE non-markdown text index is active for the current
        /// project. Drives visibility of the "Text files" results tab. Independent
        /// from the markdown search, which is always available.
        /// </summary>
        [HttpGet("text-status")]
        public IActionResult TextStatus()
        {
            var (enabled, _) = ReadTextIndexingSettings();
            return Ok(new { enabled });
        }

        /// <summary>
        /// Full-text search over the project's NON-markdown text files (separate
        /// FTS side-car). Returns an empty set (not an error) when the project has
        /// text indexing OFF, so the UI can render the tab without special-casing.
        /// </summary>
        [HttpGet("text-content")]
        public IActionResult SearchTextContent([FromQuery] string term, [FromQuery] int maxResults = 50)
        {
            _logger.LogInformation($"[SearchController] SearchTextContent called with term: '{term}'");

            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(new { textContents = new TextContentSearchResultDto[0], totalTextContents = 0, enabled = ReadTextIndexingSettings().enabled });
            }

            var projectPath = GetProjectPath();
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                return BadRequest(new { error = "Nessun progetto aperto per questa connessione: la ricerca nei file di testo richiede un progetto aperto (ConnectionId valido)." });
            }

            var (enabled, _) = ReadTextIndexingSettings();
            if (!enabled || _textFtsService == null)
            {
                return Ok(new { textContents = new TextContentSearchResultDto[0], totalTextContents = 0, enabled = false });
            }

            try
            {
                var results = _textFtsService.SearchContent(projectPath, term, maxResults);
                var dtos = results.Select(c => new TextContentSearchResultDto
                {
                    TextFileId = c.MarkdownFileId, // reused Guid slot in the shared FTS result shape
                    FileName = c.FileName,
                    Path = c.Path,
                    Extension = System.IO.Path.GetExtension(c.Path)?.ToLowerInvariant(),
                    Snippet = c.Snippet,
                    Score = c.Score
                }).ToList();

                _logger.LogInformation($"[SearchController] Text content search completed. Found {dtos.Count} matches");
                return Ok(new { textContents = dtos, totalTextContents = dtos.Count, enabled = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SearchController] Error during text content search for term: '{term}'");
                return StatusCode(500, new { error = "Errore durante la ricerca nei file di testo", details = ex.Message });
            }
        }

        /// <summary>
        /// Reads the current project's IndexAllTextFiles flag + effective allow-list.
        /// Defaults to OFF (text index is strictly opt-in).
        /// </summary>
        private (bool enabled, System.Collections.Generic.HashSet<string> extensions) ReadTextIndexingSettings()
        {
            try
            {
                var currentPath = GetProjectPath();
                if (string.IsNullOrEmpty(currentPath))
                {
                    return (false, null);
                }

                _userSettingsDB.Clear();
                var projectDal = _userSettingsDB.GetDal<MdExplorer.Abstractions.Entities.UserDB.Project>();
                var project = projectDal.GetList().FirstOrDefault(p => p.Path == currentPath)
                    ?? projectDal.GetList().ToList()
                        .FirstOrDefault(p => string.Equals(p.Path, currentPath, StringComparison.OrdinalIgnoreCase));

                if (project == null || !project.IndexAllTextFiles)
                {
                    return (false, null);
                }

                return (true, TextFileClassifier.GetEffectiveExtensions(project.TextFileExtensions));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[SearchController] Could not read text-indexing settings, defaulting to OFF");
                return (false, null);
            }
        }
    }
}