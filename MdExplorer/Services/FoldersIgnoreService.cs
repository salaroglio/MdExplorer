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
                    return EnsureMdeManagedFolders(config);
                }
                else
                {
                    _logger.LogDebug($".mdFoldersIgnore file not found at {configFilePath}");
                    return EnsureMdeManagedFolders(new FoldersIgnoreConfiguration());
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
        /// <summary>
        /// Cartelle escluse SEMPRE, senza bisogno che qualcuno le configuri. Oggi ce n'è una:
        /// i posti di lavoro degli agenti. Vivono dentro il progetto (scelta del 2026-08-02) e
        /// ognuno contiene una <b>copia intera della documentazione</b>: senza questa esclusione
        /// l'indice conterrebbe lo stesso documento tante volte quanti sono i worktree, la
        /// ricerca sarebbe piena di doppioni e l'albero mostrerebbe
        /// <c>.worktrees/…/llm-wiki/log.md</c> come se fosse un tuo file.
        /// </summary>
        public const string AgentWorktreesFolder = ".worktrees";

        /// <summary>
        /// True quando <paramref name="fullPath"/> sta dentro i posti di lavoro degli agenti.
        /// <para>
        /// Serve dove il controllo per cartella non arriva: un file markdown non attraversa la
        /// catena delle cartelle ignorate, quindi senza questo un <c>.agent.md</c> copiato dentro
        /// un worktree verrebbe registrato come un agente in più — un gemello dell'originale, con
        /// lo stesso nome, che risponderebbe alle stesse convocazioni.
        /// </para>
        /// </summary>
        public static bool IsInsideAgentWorktrees(string fullPath, string projectPath)
        {
            if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(projectPath))
            {
                return false;
            }

            var root = Path.Combine(projectPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                AgentWorktreesFolder);

            return fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || fullPath.StartsWith(root + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase);
        }

        public bool ShouldIgnoreFolderForProject(string folderPath, string projectPath)
        {
            // Regola incorporata: vale anche quando il progetto non ha (ancora) una
            // configurazione, che è precisamente il caso di un progetto appena aperto.
            if (string.Equals(Path.GetFileName(folderPath), AgentWorktreesFolder, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

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

        // Defensive: MdExplorer-managed folders must always be hidden from the tree, even if the
        // user's .mdFoldersIgnore predates them (legacy projects opened with a build that introduced
        // new managed folders like .mde-doc).
        private static FoldersIgnoreConfiguration EnsureMdeManagedFolders(FoldersIgnoreConfiguration config)
        {
            config.IgnoredFolders ??= new List<string>();
            foreach (var managed in new[] { ".md", ".mde-doc" })
            {
                if (!config.IgnoredFolders.Any(f => string.Equals(f, managed, StringComparison.OrdinalIgnoreCase)))
                {
                    config.IgnoredFolders.Add(managed);
                }
            }
            return config;
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
