using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MdExplorer.Service.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Service.Services
{
    public class FoldersIgnoreService
    {
        private readonly ILogger<FoldersIgnoreService> _logger;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private FoldersIgnoreConfiguration _configuration;
        private string _currentProjectPath;

        public FoldersIgnoreService(
            ILogger<FoldersIgnoreService> logger,
            FileSystemWatcher fileSystemWatcher)
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;

            _logger.LogWarning($"[FoldersIgnoreService] 🚀 SERVICE INITIALIZED - FileSystemWatcher.Path: '{_fileSystemWatcher.Path}'");

            LoadConfiguration();

            // Reload configuration when project changes
            _fileSystemWatcher.Changed += (sender, e) =>
            {
                if (e.FullPath.EndsWith(".mdFoldersIgnore"))
                {
                    _logger.LogWarning($"[FoldersIgnoreService] 📝 .mdFoldersIgnore file changed, reloading configuration");
                    LoadConfiguration();
                }
            };
        }

        public void LoadConfiguration()
        {
            try
            {
                _logger.LogWarning($"[FoldersIgnoreService.LoadConfiguration] 🔍 STARTING - FileSystemWatcher.Path: '{_fileSystemWatcher.Path}'");

                var configFilePath = Path.Combine(_fileSystemWatcher.Path, ".mdFoldersIgnore");
                _logger.LogWarning($"[FoldersIgnoreService.LoadConfiguration] 📂 Looking for file at: '{configFilePath}'");

                if (File.Exists(configFilePath))
                {
                    _logger.LogWarning($"[FoldersIgnoreService.LoadConfiguration] ✅ FILE EXISTS at '{configFilePath}'");

                    var yamlContent = File.ReadAllText(configFilePath);
                    _logger.LogWarning($"[FoldersIgnoreService.LoadConfiguration] 📄 File content length: {yamlContent.Length} characters");

                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    _configuration = deserializer.Deserialize<FoldersIgnoreConfiguration>(yamlContent);
                    _currentProjectPath = _fileSystemWatcher.Path;

                    if (_configuration == null)
                    {
                        _logger.LogWarning($"[FoldersIgnoreService.LoadConfiguration] ⚠️ Configuration deserialized as NULL, using empty config");
                        _configuration = new FoldersIgnoreConfiguration();
                    }
                    else
                    {
                        _logger.LogWarning($"[FoldersIgnoreService.LoadConfiguration] ✅ LOADED CONFIG:");
                        _logger.LogWarning($"  - IgnoredFolders count: {_configuration.IgnoredFolders?.Count ?? 0}");
                        if (_configuration.IgnoredFolders != null)
                        {
                            foreach (var folder in _configuration.IgnoredFolders)
                            {
                                _logger.LogWarning($"    -> Ignoring folder: '{folder}'");
                            }
                        }
                        _logger.LogWarning($"  - IgnoredPatterns count: {_configuration.IgnoredPatterns?.Count ?? 0}");
                        if (_configuration.IgnoredPatterns != null)
                        {
                            foreach (var pattern in _configuration.IgnoredPatterns)
                            {
                                _logger.LogWarning($"    -> Ignoring pattern: '{pattern}'");
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"[FoldersIgnoreService.LoadConfiguration] ❌ FILE NOT FOUND at '{configFilePath}'");
                    _configuration = new FoldersIgnoreConfiguration();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading folders ignore configuration");
                _configuration = new FoldersIgnoreConfiguration();
            }
        }

        public bool ShouldIgnoreFolder(string folderPath)
        {
            _logger.LogWarning($"[FoldersIgnoreService.ShouldIgnoreFolder] 🔍 Checking folder: '{folderPath}'");
            _logger.LogWarning($"[FoldersIgnoreService.ShouldIgnoreFolder] Current project path: '{_currentProjectPath}' vs FileSystemWatcher.Path: '{_fileSystemWatcher.Path}'");

            // Reload configuration if project path has changed
            if (_currentProjectPath != _fileSystemWatcher.Path)
            {
                _logger.LogWarning($"[FoldersIgnoreService.ShouldIgnoreFolder] ⚠️ Path changed, reloading configuration");
                LoadConfiguration();
            }

            if (_configuration == null)
            {
                _logger.LogWarning($"[FoldersIgnoreService.ShouldIgnoreFolder] ❌ Configuration is NULL, returning false");
                return false;
            }

            var folderName = Path.GetFileName(folderPath);
            _logger.LogWarning($"[FoldersIgnoreService.ShouldIgnoreFolder] 📁 Folder name extracted: '{folderName}'");

            // Check exact folder name matches
            if (_configuration.IgnoredFolders != null)
            {
                _logger.LogWarning($"[FoldersIgnoreService.ShouldIgnoreFolder] Checking against {_configuration.IgnoredFolders.Count} ignored folders");

                foreach (var ignored in _configuration.IgnoredFolders)
                {
                    var matches = string.Equals(folderName, ignored, StringComparison.OrdinalIgnoreCase);
                    _logger.LogWarning($"[FoldersIgnoreService.ShouldIgnoreFolder]   Comparing '{folderName}' with '{ignored}': {matches}");

                    if (matches)
                    {
                        _logger.LogWarning($"[FoldersIgnoreService.ShouldIgnoreFolder] ✅ FOLDER IGNORED by exact match: '{folderName}' matches '{ignored}'");
                        return true;
                    }
                }
            }
            else
            {
                _logger.LogWarning($"[FoldersIgnoreService.ShouldIgnoreFolder] IgnoredFolders is NULL");
            }

            // Check pattern matches
            if (_configuration.IgnoredPatterns != null)
            {
                _logger.LogWarning($"[FoldersIgnoreService.ShouldIgnoreFolder] Checking against {_configuration.IgnoredPatterns.Count} patterns");

                foreach (var pattern in _configuration.IgnoredPatterns)
                {
                    var matches = MatchesPattern(folderName, pattern);
                    _logger.LogWarning($"[FoldersIgnoreService.ShouldIgnoreFolder]   Pattern '{pattern}' vs '{folderName}': {matches}");

                    if (matches)
                    {
                        _logger.LogWarning($"[FoldersIgnoreService.ShouldIgnoreFolder] ✅ FOLDER IGNORED by pattern: '{folderName}' matches pattern '{pattern}'");
                        return true;
                    }
                }
            }
            else
            {
                _logger.LogWarning($"[FoldersIgnoreService.ShouldIgnoreFolder] IgnoredPatterns is NULL");
            }

            _logger.LogWarning($"[FoldersIgnoreService.ShouldIgnoreFolder] ❌ Folder NOT ignored: '{folderName}'");
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