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

            LoadConfiguration();

            // Reload configuration when project changes
            _fileSystemWatcher.Changed += (sender, e) =>
            {
                if (e.FullPath.EndsWith(".mdFoldersIgnore"))
                {
                    LoadConfiguration();
                }
            };
        }

        public void LoadConfiguration()
        {
            try
            {
                var configFilePath = Path.Combine(_fileSystemWatcher.Path, ".mdFoldersIgnore");

                if (File.Exists(configFilePath))
                {
                    var yamlContent = File.ReadAllText(configFilePath);

                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    _configuration = deserializer.Deserialize<FoldersIgnoreConfiguration>(yamlContent);
                    _currentProjectPath = _fileSystemWatcher.Path;

                    if (_configuration == null)
                    {
                        _configuration = new FoldersIgnoreConfiguration();
                    }
                }
                else
                {
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
            // Reload configuration if project path has changed
            if (_currentProjectPath != _fileSystemWatcher.Path)
            {
                LoadConfiguration();
            }

            if (_configuration == null)
            {
                return false;
            }

            var folderName = Path.GetFileName(folderPath);

            // Check exact folder name matches
            if (_configuration.IgnoredFolders != null)
            {
                foreach (var ignored in _configuration.IgnoredFolders)
                {
                    if (string.Equals(folderName, ignored, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            // Check pattern matches
            if (_configuration.IgnoredPatterns != null)
            {
                foreach (var pattern in _configuration.IgnoredPatterns)
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