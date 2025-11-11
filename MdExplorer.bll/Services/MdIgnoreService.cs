using MdExplorer.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services
{
    /// <summary>
    /// Service implementation for managing .mdignore file patterns
    /// </summary>
    public class MdIgnoreService : IMdIgnoreService
    {
        private readonly ILogger<MdIgnoreService> _logger;
        private readonly object _lockObject = new object();
        private List<string> _ignorePatterns = new List<string>();
        private string _lastLoadedPath = string.Empty;

        public MdIgnoreService(ILogger<MdIgnoreService> logger)
        {
            _logger = logger;
        }

        public void LoadPatterns(string projectPath)
        {
            lock (_lockObject)
            {
                // Avoid reloading if already loaded for this path
                if (_lastLoadedPath == projectPath && _ignorePatterns.Any())
                {
                    return;
                }

                _ignorePatterns.Clear();
                _lastLoadedPath = projectPath;

                var mdIgnorePath = Path.Combine(projectPath, ".mdignore");

                if (File.Exists(mdIgnorePath))
                {
                    try
                    {
                        var lines = File.ReadAllLines(mdIgnorePath);
                        foreach (var line in lines)
                        {
                            var trimmedLine = line.Trim();
                            // Skip empty lines and comments
                            if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("#"))
                            {
                                _ignorePatterns.Add(trimmedLine);
                            }
                        }
                        _logger.LogInformation($"Loaded {_ignorePatterns.Count} patterns from .mdignore at {mdIgnorePath}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error loading .mdignore file from {mdIgnorePath}");
                    }
                }
                else
                {
                    _logger.LogDebug($"No .mdignore file found at {mdIgnorePath}");
                }
            }
        }

        public bool ShouldIgnorePath(string fullPath, string projectPath)
        {
            // Ensure patterns are loaded
            LoadPatterns(projectPath);

            if (_ignorePatterns == null || _ignorePatterns.Count == 0)
                return false;

            // Get relative path from project root
            var relativePath = GetRelativePath(fullPath, projectPath);

            foreach (var pattern in _ignorePatterns)
            {
                if (IsPatternMatch(relativePath, pattern))
                {
                    _logger.LogDebug($"Path '{relativePath}' matched ignore pattern '{pattern}'");
                    return true;
                }
            }

            return false;
        }

        public bool ShouldIncludeFile(string fullPath, string projectPath)
        {
            // Check if it's a markdown file
            if (!Path.GetExtension(fullPath).Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Check if it should be ignored
            return !ShouldIgnorePath(fullPath, projectPath);
        }

        public bool ShouldIncludeFolder(string fullPath, string projectPath)
        {
            // Folders containing .md in their name are typically system folders
            if (fullPath.Contains(".md"))
            {
                return false;
            }

            // Check if it should be ignored
            return !ShouldIgnorePath(fullPath, projectPath);
        }

        public IReadOnlyList<string> GetLoadedPatterns()
        {
            lock (_lockObject)
            {
                return _ignorePatterns.AsReadOnly();
            }
        }

        private string GetRelativePath(string fullPath, string projectPath)
        {
            if (fullPath.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
            {
                var relativePath = fullPath.Substring(projectPath.Length).TrimStart(Path.DirectorySeparatorChar);
                // Normalize path separators for consistent matching
                return relativePath.Replace(Path.DirectorySeparatorChar, '/');
            }
            return fullPath;
        }

        private bool IsPatternMatch(string path, string pattern)
        {
            // Handle exact matches
            if (pattern == path)
                return true;

            // gitignore convention: pattern starting with / matches only from root
            bool anchoredToRoot = pattern.StartsWith("/");
            if (anchoredToRoot)
            {
                pattern = pattern.Substring(1); // Remove leading /
            }

            // Handle directory patterns (ending with /)
            if (pattern.EndsWith("/"))
            {
                var dirPattern = pattern.TrimEnd('/');

                if (anchoredToRoot)
                {
                    // /bin/ matches only bin/ at root level
                    if (path == dirPattern || path.StartsWith(dirPattern + "/"))
                        return true;
                }
                else
                {
                    // bin/ matches bin/ at ANY level (gitignore standard)
                    // Match: bin/, src/bin/, project/lib/bin/, etc.
                    if (path == dirPattern ||
                        path.StartsWith(dirPattern + "/") ||
                        path.Contains("/" + dirPattern + "/") ||
                        path.EndsWith("/" + dirPattern))
                        return true;
                }
            }

            // Handle wildcard patterns
            if (pattern.Contains("*"))
            {
                var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
                if (Regex.IsMatch(path, regexPattern))
                    return true;

                // If not anchored to root, try matching at any level
                if (!anchoredToRoot)
                {
                    var pathParts = path.Split('/');
                    foreach (var part in pathParts)
                    {
                        if (Regex.IsMatch(part, regexPattern))
                            return true;
                    }
                }
            }

            // Handle patterns without / (should match at any level)
            if (!pattern.Contains("/") && !anchoredToRoot)
            {
                var pathParts = path.Split('/');
                return pathParts.Any(part => part == pattern);
            }

            // Handle patterns with path separators
            if (anchoredToRoot)
            {
                return path == pattern || path.StartsWith(pattern + "/");
            }
            else
            {
                // Match at any level
                return path == pattern ||
                       path.StartsWith(pattern + "/") ||
                       path.Contains("/" + pattern + "/") ||
                       path.EndsWith("/" + pattern);
            }
        }
    }
}