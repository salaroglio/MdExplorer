using Ad.Tools.Dal;
using Ad.Tools.Dal.Concrete;
using Ad.Tools.FluentMigrator.Interfaces;
using FluentMigrator.Runner;
using LibGit2Sharp;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.DataAccess.Engine;
using MdExplorer.DataAccess.Project.Mapping;
using MdExplorer.Features.Utilities;
using MdExplorer.Migrations;
using MdExplorer.Migrations.EngineDb.Version202107;
using MdExplorer.Migrations.ProjectDb.Version202109;
using MdExplorer.Migrations.ProjectDb.Version2022;
using MDExplorer.DataAccess.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Ad.Tools.FluentMigrator.FluentMigratorDI;
using MdExplorer.Utilities;

namespace MdExplorer.Service
{
    public class ProjectsManager
    {
        public static bool SetNewProject(IServiceProvider serviceProvider, string pathFromParameter, bool initializeGit = true, bool addCopilotInstructions = true)
        {
            ConfigTemplates(pathFromParameter, null, addCopilotInstructions);

            // Initialize Git repository only if requested
            bool gitInitialized = false;
            if (initializeGit)
            {
                gitInitialized = InitializeGitRepository(pathFromParameter);
            }

            var appdata = CrossPlatformPath.GetAppDataPath();
            var databasePath = $"Data Source = {Path.Combine(appdata, "MdExplorer.db")}";
            var currentDirectory = pathFromParameter;
            var hash = Helper.HGetHashString(currentDirectory);
            var databasePathEngine = $"Data Source = {Path.Combine(appdata, $"MdEngine_{hash}.db")}";
            var databasePathProject = $"Data Source = {Path.Combine(currentDirectory, ".md", $"MdProject_{hash}.db")}";

            // NOTE: Don't migrate MdExplorer.db here - it's already migrated at startup in SetProjectInitialization
            // Only migrate the project-specific databases (Engine and Project)
            UpgradeDatabases(null, databasePathEngine, databasePathProject);

            serviceProvider.ReplaceDalFeatures(typeof(SettingsMap).Assembly,
                                    new DatabaseSQLite(), typeof(IUserSettingsDB),
                                    databasePath);
            serviceProvider.ReplaceDalFeatures(typeof(MarkdownFileMap).Assembly,
                                   new DatabaseSQLite(), typeof(IEngineDB),
                                   databasePathEngine);
            serviceProvider.ReplaceDalFeatures(typeof(SemanticClusterMap).Assembly,
                                   new DatabaseSQLite(), typeof(IProjectDB),
                                   databasePathProject);

            // Update application extension configuration with new project path
            var extensionConfig = serviceProvider.GetService<Features.Configuration.Interfaces.IApplicationExtensionConfiguration>();
            extensionConfig?.SetProjectPath(pathFromParameter);

            // Migration complete
            return gitInitialized;
        }

        /// <summary>
        /// This is a function called when a new Folder is selected.
        /// When a new folder is selected i need reinitialize all dbs and prepare eventually new one or migrate them
        /// </summary>
        /// <param name="services"></param>
        /// <param name="pathFromParameter"></param>
        public static void SetProjectInitialization(IServiceCollection services, string pathFromParameter)
        {

            var appdata = CrossPlatformPath.GetAppDataPath();
            var databasePath = $"Data Source = {Path.Combine(appdata, "MdExplorer.db")}";
            var currentDirectory = ConfigFileSystemWatchers(services, pathFromParameter);

            // Only configure templates if we have a valid project path (not the app directory)
            // This prevents writing to Program Files which requires admin permissions
            if (!string.IsNullOrEmpty(pathFromParameter) && Directory.Exists(pathFromParameter))
            {
                ConfigTemplates(currentDirectory, services);
            }
            var hash = Helper.HGetHashString(currentDirectory);
            var databasePathEngine = $"Data Source = {Path.Combine(appdata, $"MdEngine_{hash}.db")}";
            var databasePathProject = $"Data Source = {Path.Combine(appdata, $"MdProject_{hash}.db")}";

            UpgradeDatabases(databasePath, databasePathEngine, databasePathProject);

            services.AddDalFeatures(typeof(SettingsMap).Assembly,
                                    new DatabaseSQLite(), typeof(IUserSettingsDB),
                                    databasePath);

            services.AddDalFeatures(typeof(MarkdownFileMap).Assembly,
                                   new DatabaseSQLite(), typeof(IEngineDB),
                                   databasePathEngine);

            services.AddDalFeatures(typeof(SemanticClusterMap).Assembly,
                                   new DatabaseSQLite(), typeof(IProjectDB),
                                   databasePathProject);
        }

        /// <summary>
        /// Migrate database is done using "custom" serviceCollection.
        /// </summary>
        /// <param name="databasePath">Main MdExplorer.db path (can be null to skip migration)</param>
        /// <param name="databasePathEngine">Engine database path (optional)</param>
        /// <param name="databaseProject">Project database path (optional)</param>
        private static void UpgradeDatabases(string databasePath, string databasePathEngine = null, string databaseProject = null)
        {
            // Migrate main database (MdExplorer.db) only if path is provided
            if (databasePath != null)
            {
                IServiceCollection localServices = new ServiceCollection();
                localServices.AddFluentMigratorFeatures(
                                                (rb) =>
                                                {
                                                    rb.AddSQLite()
                                                    .WithGlobalConnectionString(databasePath)
                                                    .ScanIn(typeof(M2021_06_23_001).Assembly)
                                                    .For.Migrations();
                                                }, "SQLite");
                var builder = localServices.BuildServiceProvider();
                Migrate(builder);
            }
            if (databasePathEngine!=null)
            {
                IServiceCollection engineServices = new ServiceCollection();
                engineServices.AddFluentMigratorFeatures(
                                              (rb) =>
                                              {
                                                  rb.AddSQLite()
                                                  .WithGlobalConnectionString(databasePathEngine)
                                                  .ScanIn(typeof(ME2021_07_23_001).Assembly)
                                                  .For.Migrations();
                                              }, "SQLite");
                var engineBuilder = engineServices.BuildServiceProvider();
                Migrate(engineBuilder);
            }
            if (databaseProject != null)
            {
                IServiceCollection projectServices = new ServiceCollection();
                projectServices.AddFluentMigratorFeatures(
                                              (rb) =>
                                              {
                                                  rb.AddSQLite()
                                                  .WithGlobalConnectionString(databaseProject)
                                                  .ScanIn(typeof(MP2022_10_09_001).Assembly)
                                                  .For.Migrations();
                                              }, "SQLite");
                var projectBuilder = projectServices.BuildServiceProvider();
                Migrate(projectBuilder);
            }

        }

        private static void Migrate(ServiceProvider builder)
        {
            var scope = builder.CreateScope();
            var migrateUserSettings = scope.ServiceProvider.GetService<IEngineMigrator>();
            migrateUserSettings.UpgradeDatabase();
            scope.Dispose();
            builder.Dispose();
        }

/// <summary>
/// Validates and resolves the effective project path.
/// If the provided path is invalid, defaults to the application's base directory.
/// </summary>
/// <param name="services">Service collection (kept for backward compatibility but no longer used)</param>
/// <param name="pathFromParameter">The path parameter to validate</param>
/// <returns>The validated effective path</returns>
private static string ConfigFileSystemWatchers(IServiceCollection services, string pathFromParameter)
{
    string effectivePath = pathFromParameter;

    // Check if pathFromParameter is null, empty, or not a valid directory path.
    // Path.GetDirectoryName on a simple string like "5000" returns an empty string.
    if (string.IsNullOrEmpty(effectivePath) || !Directory.Exists(effectivePath))
    {
        // If pathFromParameter is not a valid directory, default to the application's base directory.
        effectivePath = AppDomain.CurrentDomain.BaseDirectory;
    }

    // NOTE: Legacy FileSystemWatcher singleton has been removed.
    // Multi-client support now uses IFileSystemWatcherManager for per-connection watchers.
    // The FileSystemWatcherManager is registered in Startup.cs.

    return effectivePath; // Return the path that was actually used.
}

        public static void ConfigTemplates(string mdPath, IServiceCollection services = null, bool addCopilotInstructions = true)
        {
            //var directory = $"{Path.GetDirectoryName(mdPath)}{Path.DirectorySeparatorChar}.md";
            var directory = $"{mdPath}{Path.DirectorySeparatorChar}.md";
            var directoryEmoji = $"{directory}{Path.DirectorySeparatorChar}EmojiForPandoc";
            Directory.CreateDirectory(directory);
            Directory.CreateDirectory($"{directory}{Path.DirectorySeparatorChar}templates");
            Directory.CreateDirectory(directoryEmoji);

            // Copy configuration files to project root if they don't exist
            CopyConfigurationFilesToProject(mdPath, addCopilotInstructions);

            var assembly = Assembly.GetExecutingAssembly();
            var embeddedSubfolder = "MdExplorer.Service.EmojiForPandoc.";
            var embeddedEmojies = assembly.GetManifestResourceNames();
            var selectedEmojies = embeddedEmojies.Where(_ => _.Contains(embeddedSubfolder))
                    .Select(_ => new { EmbeddedName = _, Name = _.Replace(embeddedSubfolder, string.Empty) }).ToArray();
            if (services != null)
            {
                services.AddSingleton(typeof(IServerCache), new ServerCache { Emojies = selectedEmojies.Select(_ => _.Name).ToArray() });
            }            
            
            foreach (var itemEmoj in selectedEmojies)
            {
                FileUtil.ExtractResFile(itemEmoj.EmbeddedName, $@"{directoryEmoji}{Path.DirectorySeparatorChar}{itemEmoj.Name}");
            }
            //Directory.CreateDirectory(@".md");
            Directory.CreateDirectory($@"{directory}{
                Path.DirectorySeparatorChar}templates");
            Directory.CreateDirectory($@"{directory}{
                Path.DirectorySeparatorChar}templates{
                Path.DirectorySeparatorChar}pdf");
            Directory.CreateDirectory($@"{directory}{
                Path.DirectorySeparatorChar}templates{
                Path.DirectorySeparatorChar}word");

            FileUtil.ExtractResFile("MdExplorer.Service.templates.pdf.eisvogel.tex", $@"{
                directory}{
                Path.DirectorySeparatorChar}templates{
                Path.DirectorySeparatorChar}pdf{
                Path.DirectorySeparatorChar}eisvogel.tex");
            // Crea template reference.docx (template di default)
            var referencePath = $@"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}reference.docx";
            if (!File.Exists(referencePath))
            {
                FileUtil.ExtractResFile("MdExplorer.Service.templates.word.reference.docx", referencePath);
            }
            
            var minutePath = $@"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}minute.docx";
            if (!File.Exists(minutePath))
            {
                FileUtil.ExtractResFile("MdExplorer.Service.templates.word.reference.docx", minutePath);
            }
            var projectPath = $@"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}project.docx";
            if (!File.Exists(projectPath))
            {
                FileUtil.ExtractResFile("MdExplorer.Service.templates.word.reference.docx", projectPath);
            }
            
            // NUOVO: Crea directory per template pages
            Directory.CreateDirectory($"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages");
            Directory.CreateDirectory($"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages{Path.DirectorySeparatorChar}covers");
            Directory.CreateDirectory($"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages{Path.DirectorySeparatorChar}disclaimers");
            Directory.CreateDirectory($"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages{Path.DirectorySeparatorChar}appendices");

            // NUOVO: Copia template pages da embedded resources
            CopyPageTemplates(directory);
            
        }
        
        private static void CopyPageTemplates(string mdDirectory)
        {
            var pageTemplates = new[]
            {
                ("covers.standard.md", $@"templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages{Path.DirectorySeparatorChar}covers{Path.DirectorySeparatorChar}standard.md"),
                ("covers.project.md", $@"templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages{Path.DirectorySeparatorChar}covers{Path.DirectorySeparatorChar}project.md"),
                ("disclaimers.confidential.md", $@"templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages{Path.DirectorySeparatorChar}disclaimers{Path.DirectorySeparatorChar}confidential.md"),
                ("appendices.signatures.md", $@"templates{Path.DirectorySeparatorChar}word{Path.DirectorySeparatorChar}pages{Path.DirectorySeparatorChar}appendices{Path.DirectorySeparatorChar}signatures.md")
            };

            foreach (var (resourceName, destinationPath) in pageTemplates)
            {
                var fullPath = $"{mdDirectory}{Path.DirectorySeparatorChar}{destinationPath}";
                if (!File.Exists(fullPath))
                {
                    FileUtil.ExtractResFile($"MdExplorer.Service.templates.word.pages.{resourceName}", fullPath);
                }
            }
        }
        
        private static void CopyConfigurationFilesToProject(string projectPath, bool addCopilotInstructions = true)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();

                // Copy .mdapplicationtoopen file
                var mdApplicationToOpenPath = Path.Combine(projectPath, ".mdapplicationtoopen");
                if (!File.Exists(mdApplicationToOpenPath))
                {
                    FileUtil.ExtractResFile("MdExplorer.Service..mdapplicationtoopen", mdApplicationToOpenPath);
                    Console.WriteLine($"Created configuration file: {mdApplicationToOpenPath}");
                }

                // Copy .mdchangeignore file
                var mdChangeIgnorePath = Path.Combine(projectPath, ".mdchangeignore");
                if (!File.Exists(mdChangeIgnorePath))
                {
                    FileUtil.ExtractResFile("MdExplorer.Service..mdchangeignore", mdChangeIgnorePath);
                    Console.WriteLine($"Created configuration file: {mdChangeIgnorePath}");
                }

                // Copy .mdFoldersIgnore file
                var mdFoldersIgnorePath = Path.Combine(projectPath, ".mdFoldersIgnore");
                if (!File.Exists(mdFoldersIgnorePath))
                {
                    FileUtil.ExtractResFile("MdExplorer.Service..mdFoldersIgnore", mdFoldersIgnorePath);
                    Console.WriteLine($"Created folders ignore configuration file: {mdFoldersIgnorePath}");
                }

                // Copy .development.yml file
                var developmentConfigPath = Path.Combine(projectPath, ".development.yml");
                if (!File.Exists(developmentConfigPath))
                {
                    FileUtil.ExtractResFile("MdExplorer.Service..development.yml", developmentConfigPath);
                    Console.WriteLine($"Created development configuration file: {developmentConfigPath}");
                }

                // Create .github folder and copy copilot-instructions.md only if requested
                if (addCopilotInstructions)
                {
                    var githubPath = Path.Combine(projectPath, ".github");
                    Directory.CreateDirectory(githubPath);

                    var copilotInstructionsPath = Path.Combine(githubPath, "copilot-instructions.md");
                    if (!File.Exists(copilotInstructionsPath))
                    {
                        FileUtil.ExtractResFile("MdExplorer.Service.copilot-instructions.md", copilotInstructionsPath);
                        Console.WriteLine($"Created GitHub Copilot instructions file: {copilotInstructionsPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying configuration files: {ex.Message}");
                // Non-critical error, continue without the files
            }
        }

        /// <summary>
        /// Initializes a Git repository in the project folder if not already present
        /// </summary>
        /// <param name="projectPath">Path to the project folder</param>
        /// <returns>True if Git was initialized, false if it already existed</returns>
        public static bool InitializeGitRepository(string projectPath)
        {
            try
            {
                var gitPath = Path.Combine(projectPath, ".git");

                // Check if Git repository already exists
                if (Directory.Exists(gitPath))
                {
                    Console.WriteLine($"Git repository already exists at: {projectPath}");
                    return false;
                }

                // Initialize Git repository
                Repository.Init(projectPath);
                Console.WriteLine($"Git repository initialized at: {projectPath}");

                // Set initial branch to "main" (modern standard)
                using (var repo = new Repository(projectPath))
                {
                    // Create initial empty commit to establish the branch
                    var signature = new Signature("MdExplorer", "noreply@mdexplorer.net", DateTimeOffset.Now);
                    repo.Commit("Initial commit", signature, signature, new CommitOptions { AllowEmptyCommit = true });

                    // Rename branch from master to main
                    var currentBranch = repo.Head;
                    if (currentBranch.FriendlyName == "master")
                    {
                        repo.Branches.Rename(currentBranch, "main");
                        Console.WriteLine($"Branch renamed from 'master' to 'main'");
                    }
                }

                // Create .gitignore file with MdExplorer specific patterns
                var gitignorePath = Path.Combine(projectPath, ".gitignore");
                if (!File.Exists(gitignorePath))
                {
                    var gitignoreContent = new StringBuilder();
                    gitignoreContent.AppendLine("# MdExplorer specific files and folders");
                    gitignoreContent.AppendLine(".md/");
                    gitignoreContent.AppendLine(".mdword/");
                    gitignoreContent.AppendLine("");
                    gitignoreContent.AppendLine("# P2P shared files (metadata.json is tracked for P2P info sharing)");
                    gitignoreContent.AppendLine(".p2pshare/files/");
                    gitignoreContent.AppendLine(".p2pshare/received/");
                    gitignoreContent.AppendLine("");
                    gitignoreContent.AppendLine("# Database files");
                    gitignoreContent.AppendLine("*.db");
                    gitignoreContent.AppendLine("*.db-shm");
                    gitignoreContent.AppendLine("*.db-wal");
                    gitignoreContent.AppendLine("");
                    gitignoreContent.AppendLine("# Temporary files");
                    gitignoreContent.AppendLine("*.tmp");
                    gitignoreContent.AppendLine("*.temp");
                    gitignoreContent.AppendLine("~*");
                    gitignoreContent.AppendLine("");
                    gitignoreContent.AppendLine("# Log files");
                    gitignoreContent.AppendLine("*.log");
                    gitignoreContent.AppendLine("");
                    gitignoreContent.AppendLine("# OS specific files");
                    gitignoreContent.AppendLine(".DS_Store");
                    gitignoreContent.AppendLine("Thumbs.db");
                    gitignoreContent.AppendLine("desktop.ini");

                    File.WriteAllText(gitignorePath, gitignoreContent.ToString());
                    Console.WriteLine($"Created .gitignore file at: {gitignorePath}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Git repository: {ex.Message}");
                // Non-critical error, project can continue without Git
                return false;
            }
        }
    }
}
