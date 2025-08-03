using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Services;
using Ad.Tools.Dal.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Features.Services
{
    public class SearchService : ISearchService
    {
        private readonly IEngineDB _engineDB;
        private readonly ILogger<SearchService> _logger;

        public SearchService(IEngineDB engineDB, ILogger<SearchService> logger)
        {
            _engineDB = engineDB;
            _logger = logger;
        }

        public async Task<SearchResult> SearchAsync(string searchTerm, SearchType searchType = SearchType.All, int maxResults = 50)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new SearchResult
            {
                SearchTerm = searchTerm
            };

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                result.SearchDurationMs = stopwatch.ElapsedMilliseconds;
                return result;
            }

            var searchLower = searchTerm.ToLower();

            if (searchType == SearchType.All || searchType == SearchType.Files)
            {
                result.Files = await SearchFilesAsync(searchLower, maxResults);
                result.TotalFiles = result.Files.Count;
            }

            if (searchType == SearchType.All || searchType == SearchType.Links)
            {
                result.Links = await SearchLinksAsync(searchLower, maxResults);
                result.TotalLinks = result.Links.Count;
            }

            stopwatch.Stop();
            result.SearchDurationMs = stopwatch.ElapsedMilliseconds;

            return result;
        }

        public Task<List<FileSearchResult>> SearchFilesAsync(string searchTerm, int maxResults = 50)
        {
            return Task.Run(() =>
            {
                _engineDB.BeginTransaction();
                try
                {
                    var markdownFileDal = _engineDB.GetDal<MarkdownFile>();
                    var searchLower = searchTerm.ToLower();

                    // Get all files from database
                    var allFiles = markdownFileDal.GetList().ToList();
                    
                    // Check for duplicates in database - ONLY LOG IF DUPLICATES FOUND
                    var duplicatePaths = allFiles
                        .GroupBy(f => f.Path?.ToLower())
                        .Where(g => g.Count() > 1)
                        .ToList();
                    
                    if (duplicatePaths.Any())
                    {
                        _logger.LogWarning($"🔍 DUPLICATES in DB: Found {duplicatePaths.Count} paths with duplicates for search '{searchTerm}'");
                        foreach (var group in duplicatePaths.Take(3)) // Only show first 3 duplicate groups
                        {
                            _logger.LogWarning($"  Duplicate: {group.Key} appears {group.Count()} times");
                            foreach (var file in group)
                            {
                                _logger.LogWarning($"    ID: {file.Id}, File: {file.FileName}");
                            }
                        }
                    }

                    // Perform the search
                    var results = allFiles
                        .Where(f => f.FileName.ToLower().Contains(searchLower) ||
                                   f.Path.ToLower().Contains(searchLower))
                        .Take(maxResults)
                        .Select(f => new FileSearchResult
                        {
                            Id = f.Id,
                            FileName = f.FileName,
                            Path = f.Path,
                            FileType = f.FileType,
                            MatchedField = DetermineMatchedField(f, searchLower),
                            HighlightedText = GetHighlightedText(f, searchLower)
                        })
                        .ToList();

                    _engineDB.Commit();
                    return results;
                }
                catch
                {
                    _engineDB.Rollback();
                    throw;
                }
            });
        }

        public Task<List<LinkSearchResult>> SearchLinksAsync(string searchTerm, int maxResults = 50)
        {
            return Task.Run(() =>
            {
                _engineDB.BeginTransaction();
                try
                {
                    var linkDal = _engineDB.GetDal<LinkInsideMarkdown>();
                    var searchLower = searchTerm.ToLower();

                    var results = linkDal.GetList()
                        .Where(l => (l.Path != null && l.Path.ToLower().Contains(searchLower)) ||
                                   (l.FullPath != null && l.FullPath.ToLower().Contains(searchLower)) ||
                                   (l.MdTitle != null && l.MdTitle.ToLower().Contains(searchLower)) ||
                                   (l.HTMLTitle != null && l.HTMLTitle.ToLower().Contains(searchLower)) ||
                                   (l.MdContext != null && l.MdContext.ToLower().Contains(searchLower)))
                        .Take(maxResults)
                        .Select(l => new LinkSearchResult
                        {
                            Id = l.Id,
                            Path = l.Path,
                            FullPath = l.FullPath,
                            MdTitle = l.MdTitle,
                            HtmlTitle = l.HTMLTitle,
                            MdContext = l.MdContext,
                            Source = l.Source,
                            LinkedCommand = l.LinkedCommand,
                            MarkdownFileName = l.MarkdownFile != null ? l.MarkdownFile.FileName : null,
                            MarkdownFilePath = l.MarkdownFile != null ? l.MarkdownFile.Path : null,
                            MatchedField = DetermineLinkMatchedField(l, searchLower),
                            HighlightedText = GetLinkHighlightedText(l, searchLower)
                        })
                        .ToList();

                    _engineDB.Commit();
                    return results;
                }
                catch
                {
                    _engineDB.Rollback();
                    throw;
                }
            });
        }

        private string DetermineMatchedField(MarkdownFile file, string searchTerm)
        {
            if (file.FileName?.ToLower().Contains(searchTerm) == true)
                return "FileName";
            if (file.Path?.ToLower().Contains(searchTerm) == true)
                return "Path";
            return "Unknown";
        }

        private string DetermineLinkMatchedField(LinkInsideMarkdown link, string searchTerm)
        {
            if (link.MdTitle?.ToLower().Contains(searchTerm) == true)
                return "Title";
            if (link.MdContext?.ToLower().Contains(searchTerm) == true)
                return "Context";
            if (link.Path?.ToLower().Contains(searchTerm) == true)
                return "Path";
            if (link.HTMLTitle?.ToLower().Contains(searchTerm) == true)
                return "HTMLTitle";
            if (link.FullPath?.ToLower().Contains(searchTerm) == true)
                return "FullPath";
            return "Unknown";
        }

        private string GetHighlightedText(MarkdownFile file, string searchTerm)
        {
            if (file.FileName?.ToLower().Contains(searchTerm) == true)
                return HighlightText(file.FileName, searchTerm);
            if (file.Path?.ToLower().Contains(searchTerm) == true)
                return HighlightText(file.Path, searchTerm);
            return file.FileName;
        }

        private string GetLinkHighlightedText(LinkInsideMarkdown link, string searchTerm)
        {
            if (link.MdTitle?.ToLower().Contains(searchTerm) == true)
                return HighlightText(link.MdTitle, searchTerm);
            if (link.MdContext?.ToLower().Contains(searchTerm) == true)
                return HighlightText(link.MdContext, searchTerm);
            if (link.Path?.ToLower().Contains(searchTerm) == true)
                return HighlightText(link.Path, searchTerm);
            return link.MdTitle ?? link.Path;
        }

        private string HighlightText(string text, string searchTerm)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchTerm))
                return text;

            // Simple highlight by wrapping matched text in <mark> tags
            var index = text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var before = text.Substring(0, index);
                var match = text.Substring(index, searchTerm.Length);
                var after = text.Substring(index + searchTerm.Length);
                return $"{before}<mark>{match}</mark>{after}";
            }
            return text;
        }
    }
}