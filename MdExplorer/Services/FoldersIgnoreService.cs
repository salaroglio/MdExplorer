using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MdExplorer.Service.Models;
using MdExplorer.Services.FileSystemWatcherManager;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Service.Services
{
    public class FoldersIgnoreService
    {
        private readonly ILogger<FoldersIgnoreService> _logger;
        private readonly IFileSystemWatcherManager _fileSystemWatcherManager;

        // Cache configurations per project path to avoid re-reading file on every call
        private readonly ConcurrentDictionary<string, FoldersIgnoreConfiguration> _configurationCache = new();

        public FoldersIgnoreService(
            ILogger<FoldersIgnoreService> logger,
            IFileSystemWatcherManager fileSystemWatcherManager)
        {
            _logger = logger;
            _fileSystemWatcherManager = fileSystemWatcherManager;
        }

        /// <summary>
        /// Load configuration for a specific project path
        /// </summary>
        public FoldersIgnoreConfiguration LoadConfiguration(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                _logger.LogWarning("LoadConfiguration called with null/empty projectPath");
                return new FoldersIgnoreConfiguration();
            }

            try
            {
                var configFilePath = Path.Combine(projectPath, ".mdFoldersIgnore");

                if (File.Exists(configFilePath))
                {
                    var yamlContent = File.ReadAllText(configFilePath);

                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    var config = deserializer.Deserialize<FoldersIgnoreConfiguration>(yamlContent);

                    if (config == null)
                    {
                        config = new FoldersIgnoreConfiguration();
                    }

                    _logger.LogDebug($"Loaded FoldersIgnore configuration from {configFilePath}");
                    return config;
                }
                else
                {
                    _logger.LogDebug($".mdFoldersIgnore file not found at {configFilePath}");
                    return new FoldersIgnoreConfiguration();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading folders ignore configuration from {projectPath}");
                return new FoldersIgnoreConfiguration();
            }
        }

        /// <summary>
        /// Get or load configuration for a project, with caching
        /// </summary>
        private FoldersIgnoreConfiguration GetConfiguration(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                return new FoldersIgnoreConfiguration();
            }

            return _configurationCache.GetOrAdd(projectPath, path => LoadConfiguration(path));
        }

        /// <summary>
        /// Invalidate cached configuration for a project (call when .mdFoldersIgnore changes)
        /// </summary>
        public void InvalidateCache(string projectPath)
        {
            if (!string.IsNullOrEmpty(projectPath))
            {
                _configurationCache.TryRemove(projectPath, out _);
                _logger.LogDebug($"Invalidated FoldersIgnore cache for {projectPath}");
            }
        }

        /// <summary>
        /// Check if a folder should be ignored for a specific connection
        /// </summary>
        /// <param name="folderPath">Full path to the folder to check</param>
        /// <param name="connectionId">SignalR connection ID</param>
        /// <returns>True if folder should be ignored</returns>
        public bool ShouldIgnoreFolder(string folderPath, string connectionId)
        {
            var projectPath = _fileSystemWatcherManager.GetProjectPath(connectionId);
            return ShouldIgnoreFolderForProject(folderPath, projectPath);
        }

        /// <summary>
        /// Check if a folder should be ignored for a specific project path
        /// </summary>
        /// <param name="folderPath">Full path to the folder to check</param>
        /// <param name="projectPath">Project root path</param>
        /// <returns>True if folder should be ignored</returns>
        public bool ShouldIgnoreFolderForProject(string folderPath, string projectPath)
        {
            var configuration = GetConfiguration(projectPath);

            if (configuration == null)
            {
                return false;
            }

            var folderName = Path.GetFileName(folderPath);

            // Check exact folder name matches
            if (configuration.IgnoredFolders != null)
            {
                foreach (var ignored in configuration.IgnoredFolders)
                {
                    if (string.Equals(folderName, ignored, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            // Check pattern matches
            if (configuration.IgnoredPatterns != null)
            {
                foreach (var pattern in configuration.IgnoredPatterns)
                {
                    if (MatchesPattern(folderName, pattern))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool MatchesPattern(string folderName, string pattern)
        {
            // Convert simple wildcard pattern to regex-like matching
            // * matches any characters, ? matches single character

            if (string.IsNullOrEmpty(pattern))
                return false;

            // Simple implementation of wildcard matching
            var patternIndex = 0;
            var folderIndex = 0;
            var starIndex = -1;
            var matchIndex = 0;

            while (folderIndex < folderName.Length)
            {
                if (patternIndex < pattern.Length &&
                    (pattern[patternIndex] == '?' ||
                     char.ToLowerInvariant(pattern[patternIndex]) == char.ToLowerInvariant(folderName[folderIndex])))
                {
                    patternIndex++;
                    folderIndex++;
                }
                else if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
                {
                    starIndex = patternIndex;
                    matchIndex = folderIndex;
                    patternIndex++;
                }
                else if (starIndex != -1)
                {
                    patternIndex = starIndex + 1;
                    matchIndex++;
                    folderIndex = matchIndex;
                }
                else
                {
                    return false;
                }
            }

            while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
            {
                patternIndex++;
            }

            return patternIndex == pattern.Length;
        }
    }
}
