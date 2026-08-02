using Ad.Tools.Dal;
using Ad.Tools.Dal.Concrete;
using Ad.Tools.Dal.Extensions;
using Ad.Tools.FluentMigrator.Interfaces;
using FluentMigrator.Runner;
using LibGit2Sharp;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
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
            // Fuseki/Jena skills are deployed only for projects configured for Fuseki.
            var fusekiEnabled = IsFusekiEnabled(serviceProvider, pathFromParameter);
            ConfigTemplates(pathFromParameter, null, addCopilotInstructions, fusekiEnabled);

            // Initialize Git repository only if requested
            bool gitInitialized = false;
            if (initializeGit)
            {
                gitInitialized = InitializeGitRepository(pathFromParameter);
            }

            // Ensure the per-install MDE artifacts are git-ignored. Runs for BOTH freshly
            // initialized and pre-existing repositories (e.g. a documentation repo shared
            // across clients), unlike InitializeGitRepository which bails out when .git exists.
            EnsureGitignoreEntries(pathFromParameter);

            // For repositories where those artifacts were already committed (the typical
            // shared-docs case), .gitignore alone is not enough — stop tracking them so they
            // stop being versioned. Files are kept on disk; the staged removal is committed
            // with the client's next commit.
            UntrackPerInstallArtifacts(pathFromParameter);

            // Mark Search answer documents are session artifacts: previous sessions'
            // files are meaningless (their chat context is gone), so start clean.
            CleanMarkSearchArtifacts(pathFromParameter);

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
        /// Reads the global UserDB to tell whether the project at <paramref name="projectPath"/>
        /// has the Apache Jena Fuseki integration enabled. Used to gate the deployment
        /// of the Fuseki/Jena skills. Safe: returns false on any error or when the
        /// project / settings row does not exist yet.
        /// </summary>
        private static bool IsFusekiEnabled(IServiceProvider serviceProvider, string projectPath)
        {
            if (serviceProvider == null || string.IsNullOrWhiteSpace(projectPath))
                return false;
            try
            {
                var db = serviceProvider.GetService<IUserSettingsDB>();
                if (db == null) return false;

                var normalized = projectPath.TrimEnd('/', '\\');
                db.BeginTransaction();
                try
                {
                    var project = db.GetDal<Project>().GetList()
                        .FirstOrDefault(p => p.Path != null &&
                            string.Equals(p.Path.TrimEnd('/', '\\'), normalized, StringComparison.OrdinalIgnoreCase));
                    if (project == null) return false;

                    var settings = db.GetDal<ProjectFusekiSettings>().GetList()
                        .FirstOrDefault(s => s.Project.Id == project.Id);
                    return settings?.Enabled ?? false;
                }
                finally
                {
                    db.Commit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProjectsManager] IsFusekiEnabled check failed: {ex.Message}");
                return false;
            }
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

        public static void ConfigTemplates(string mdPath, IServiceCollection services = null, bool addCopilotInstructions = true, bool fusekiEnabled = false)
        {
            //var directory = $"{Path.GetDirectoryName(mdPath)}{Path.DirectorySeparatorChar}.md";
            var directory = $"{mdPath}{Path.DirectorySeparatorChar}.md";
            var directoryEmoji = $"{directory}{Path.DirectorySeparatorChar}EmojiForPandoc";
            Directory.CreateDirectory(directory);
            Directory.CreateDirectory($"{directory}{Path.DirectorySeparatorChar}templates");
            Directory.CreateDirectory(directoryEmoji);

            // Copy configuration files to project root if they don't exist
            CopyConfigurationFilesToProject(mdPath, addCopilotInstructions, fusekiEnabled);

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

            // External apps documentation
            var directoryExternalApps = $"{directory}{Path.DirectorySeparatorChar}external-apps";
            Directory.CreateDirectory(directoryExternalApps);

            var dualModeGuidePath = $"{directoryExternalApps}{Path.DirectorySeparatorChar}MdE-Dual-Mode-App-Guide.md";
            if (!File.Exists(dualModeGuidePath))
            {
                FileUtil.ExtractResFile("MdExplorer.Service.external_apps.MdE-Dual-Mode-App-Guide.md", dualModeGuidePath);
            }

            var protocolDocPath = $"{directoryExternalApps}{Path.DirectorySeparatorChar}MdE-External-App-Protocol.md";
            if (!File.Exists(protocolDocPath))
            {
                FileUtil.ExtractResFile("MdExplorer.Service.external_apps.MdE-External-App-Protocol.md", protocolDocPath);
            }

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
        
        private static void CopyConfigurationFilesToProject(string projectPath, bool addCopilotInstructions = true, bool fusekiEnabled = false)
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

                    // Copilot agent skills — version-aware install/update.
                    // Each MdE-managed skill has an `mde:` block in its frontmatter; the updater
                    // upgrades it on every project open if the embedded version is newer, but
                    // leaves user-customized skills alone (when `origin` differs or is missing).
                    MdeSkillUpdater.EnsureAllSkillsInstalled(projectPath, fusekiEnabled);
                }

                // Create .vscode folder with MCP server configuration
                CreateVsCodeMcpConfig(projectPath);

                // Create .copilot folder with MCP server configuration (for Copilot CLI)
                CreateCopilotCliMcpConfig(projectPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying configuration files: {ex.Message}");
                // Non-critical error, continue without the files
            }
        }

        private static void CreateVsCodeMcpConfig(string projectPath)
        {
            try
            {
                var vscodePath = Path.Combine(projectPath, ".vscode");
                Directory.CreateDirectory(vscodePath);

                var mcpJsonPath = Path.Combine(vscodePath, "mcp.json");

                // Build the MdExplorer server entry.
                // NOTE: the MCP server does NOT take a launch-time project argument — tools
                // receive the project name per call (the LLM discovers it via GetProjects).
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                // ResolveMcpExecutable already prefers the isolated "mcp/" subfolder next to the
                // Service (where the self-contained publish lands, its 10.x closure kept apart from
                // the Service's 8.x), then falls back to a pre-built exe on a dev box.
                var mcpExePath = ResolveMcpExecutable(baseDir);

                System.Text.Json.Nodes.JsonObject serverEntry;
                if (mcpExePath != null)
                {
                    serverEntry = new System.Text.Json.Nodes.JsonObject
                    {
                        ["type"] = "stdio",
                        ["command"] = mcpExePath
                    };
                }
                else
                {
                    // No pre-built exe found (dev box with sources but nothing built yet).
                    // Last resort only — see ResolveMcpExecutable for why "dotnet run" is unreliable.
                    var mcpProjectPath = FindMcpProjectPath(baseDir);
                    serverEntry = new System.Text.Json.Nodes.JsonObject
                    {
                        ["type"] = "stdio",
                        ["command"] = "dotnet",
                        ["args"] = new System.Text.Json.Nodes.JsonArray("run", "--project", mcpProjectPath)
                    };
                }

                System.Text.Json.Nodes.JsonObject root;

                if (File.Exists(mcpJsonPath))
                {
                    // Parse existing mcp.json and always update mdexplorer entry
                    var existingJson = File.ReadAllText(mcpJsonPath);
                    root = System.Text.Json.Nodes.JsonNode.Parse(existingJson)?.AsObject()
                           ?? new System.Text.Json.Nodes.JsonObject();

                    if (root["servers"] is not System.Text.Json.Nodes.JsonObject servers)
                    {
                        servers = new System.Text.Json.Nodes.JsonObject();
                        root["servers"] = servers;
                    }

                    servers["mdexplorer"] = serverEntry;
                }
                else
                {
                    // Create new mcp.json
                    root = new System.Text.Json.Nodes.JsonObject
                    {
                        ["servers"] = new System.Text.Json.Nodes.JsonObject
                        {
                            ["mdexplorer"] = serverEntry
                        }
                    };
                }

                var json = root.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(mcpJsonPath, json);
                Console.WriteLine($"Updated VS Code MCP configuration: {mcpJsonPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating VS Code MCP configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates/updates the global ~/.copilot/mcp-config.json with the MdExplorer MCP server
        /// for the given project. Copilot CLI only supports user-level config (not per-project).
        /// Server key includes project name to avoid conflicts between projects.
        /// </summary>
        private static void CreateCopilotCliMcpConfig(string projectPath)
        {
            try
            {
                // Copilot CLI only reads ~/.copilot/mcp-config.json (global, not per-project)
                var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var copilotPath = Path.Combine(userHome, ".copilot");
                Directory.CreateDirectory(copilotPath);

                var mcpJsonPath = Path.Combine(copilotPath, "mcp-config.json");

                // Single global "mdexplorer" entry - no --project arg needed.
                // The MCP server connects to the running MdExplorer instance and
                // works with whatever project is currently open.
                const string serverKey = "mdexplorer";

                // Copilot CLI requires "tools" array (empty = allow all tools discovered at runtime)
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                // ResolveMcpExecutable already prefers the isolated "mcp/" subfolder next to the
                // Service (where the self-contained publish lands, its 10.x closure kept apart from
                // the Service's 8.x), then falls back to a pre-built exe on a dev box.
                var mcpExePath = ResolveMcpExecutable(baseDir);

                System.Text.Json.Nodes.JsonObject serverEntry;
                if (mcpExePath != null)
                {
                    serverEntry = new System.Text.Json.Nodes.JsonObject
                    {
                        ["command"] = mcpExePath,
                        ["tools"] = new System.Text.Json.Nodes.JsonArray("*")
                    };
                }
                else
                {
                    // No pre-built exe found (dev box with sources but nothing built yet).
                    // Last resort only — see ResolveMcpExecutable for why "dotnet run" is unreliable.
                    var mcpProjectPath = FindMcpProjectPath(baseDir);
                    serverEntry = new System.Text.Json.Nodes.JsonObject
                    {
                        ["command"] = "dotnet",
                        ["args"] = new System.Text.Json.Nodes.JsonArray("run", "--project", mcpProjectPath),
                        ["tools"] = new System.Text.Json.Nodes.JsonArray("*")
                    };
                }

                System.Text.Json.Nodes.JsonObject root;

                if (File.Exists(mcpJsonPath))
                {
                    var existingJson = File.ReadAllText(mcpJsonPath);
                    root = System.Text.Json.Nodes.JsonNode.Parse(existingJson)?.AsObject()
                           ?? new System.Text.Json.Nodes.JsonObject();

                    if (root["mcpServers"] is not System.Text.Json.Nodes.JsonObject servers)
                    {
                        servers = new System.Text.Json.Nodes.JsonObject();
                        root["mcpServers"] = servers;
                    }

                    // Write if the entry is missing, OR self-heal a stale/broken entry whose
                    // launch target no longer resolves (old install path that changed between
                    // installs, or a "dotnet run --project ..." fallback written on a client
                    // that has neither the .NET SDK nor the sources). A working user
                    // customization (an existing, resolvable command) is left untouched.
                    // Only heal when we actually have a real MdExplorer.Mcp.exe to point at,
                    // so we never replace an entry with another broken fallback.
                    // Heal when the entry is missing, when a real exe is available and the current
                    // entry is broken (stale/unresolvable path), OR when a real exe is available and
                    // the current entry still uses the fragile "dotnet run" form (self-locks its bin
                    // and fails to launch — the root cause of the "Failed to connect" symptom).
                    bool entryMissing = !servers.ContainsKey(serverKey);
                    bool haveRealExe = mcpExePath != null;
                    bool entryBroken = servers[serverKey] is System.Text.Json.Nodes.JsonObject existingEntry
                                       && McpEntryLaunchTargetMissing(existingEntry);
                    bool entryIsDotnetRun = servers[serverKey] is System.Text.Json.Nodes.JsonObject dotnetRunEntry
                                       && IsDotnetRunEntry(dotnetRunEntry);
                    if (entryMissing || (haveRealExe && (entryBroken || entryIsDotnetRun)))
                    {
                        servers[serverKey] = serverEntry;
                    }
                }
                else
                {
                    root = new System.Text.Json.Nodes.JsonObject
                    {
                        ["mcpServers"] = new System.Text.Json.Nodes.JsonObject
                        {
                            [serverKey] = serverEntry
                        }
                    };
                }

                var json = root.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(mcpJsonPath, json);
                Console.WriteLine($"Copilot CLI MCP configuration ensured: {mcpJsonPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Copilot CLI MCP configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns true when an existing "mdexplorer" MCP entry cannot actually launch because
        /// its configured command does not resolve on this machine: a direct exe path that no
        /// longer exists (e.g. the install moved), or a "dotnet run --project &lt;path&gt;" fallback
        /// pointing at sources that are not present (a client without the SDK/repo). Such entries
        /// are safe to overwrite; a working command path is treated as a deliberate customization.
        /// </summary>
        private static bool McpEntryLaunchTargetMissing(System.Text.Json.Nodes.JsonObject entry)
        {
            var command = entry?["command"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(command))
                return true;

            // Fallback form: { "command": "dotnet", "args": ["run", "--project", "<path>"] }.
            // This only works where the project source exists (a dev machine).
            if (string.Equals(command, "dotnet", StringComparison.OrdinalIgnoreCase)
                || string.Equals(command, "dotnet.exe", StringComparison.OrdinalIgnoreCase))
            {
                if (entry["args"] is System.Text.Json.Nodes.JsonArray args)
                {
                    for (int i = 0; i < args.Count - 1; i++)
                    {
                        if (string.Equals(args[i]?.GetValue<string>(), "--project", StringComparison.OrdinalIgnoreCase))
                        {
                            var projArg = args[i + 1]?.GetValue<string>();
                            if (string.IsNullOrWhiteSpace(projArg))
                                return true;
                            var csproj = projArg.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                                ? projArg
                                : Path.Combine(projArg, "MdExplorer.Mcp.csproj");
                            return !File.Exists(csproj) && !Directory.Exists(projArg);
                        }
                    }
                }
                // "dotnet" with no resolvable --project target -> unusable.
                return true;
            }

            // Direct form: the command is a path to MdExplorer.Mcp.exe.
            return !File.Exists(command);
        }

        /// <summary>
        /// Resolves a ready-to-launch MdExplorer.Mcp executable to point the MCP client config at.
        /// Returns the full path of a pre-built exe, or null if none can be found.
        ///
        /// A DIRECT exe is required. The old fallback launched the stdio server via
        /// "dotnet run --project ...", which rebuilds the project into bin/ on every start and then
        /// runs from there; a still-running (or orphaned) server instance keeps those bin DLLs
        /// locked, so the rebuild of the next launch fails and the server never comes up — Copilot
        /// CLI then reports "taking longer than expected / Failed to connect". "dotnet run" also
        /// pollutes stdout with build output, corrupting the JSON-RPC stdio channel.
        /// </summary>
        internal static string? ResolveMcpExecutable(string baseDir)
        {
            var exeName = OperatingSystem.IsWindows() ? "MdExplorer.Mcp.exe" : "MdExplorer.Mcp";

            // 1) Next to the running service (installed client, publish at payload root).
            var atRoot = Path.Combine(baseDir, exeName);
            if (File.Exists(atRoot))
                return atRoot;

            // 2) Isolated "mcp/" subfolder next to the running service (isolated publish layout).
            var atMcpSub = Path.Combine(baseDir, "mcp", exeName);
            if (File.Exists(atMcpSub))
                return atMcpSub;

            // 3) Dev machine: the MCP project's own build output. Its bin folder carries an isolated
            //    dependency closure, so the exe launches cleanly on its own. Prefer the most recently
            //    built exe (Release or Debug, any target-framework subfolder).
            var mcpProjectDir = FindMcpProjectPath(baseDir);
            var built = new[] { "Release", "Debug" }
                .Select(cfg => Path.Combine(mcpProjectDir, "bin", cfg))
                .Where(Directory.Exists)
                .SelectMany(dir => Directory.EnumerateFiles(dir, exeName, SearchOption.AllDirectories))
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();

            return built;
        }

        /// <summary>
        /// True when an MCP entry uses the fragile "dotnet run" launch form (command == dotnet).
        /// Such an entry rebuilds and self-locks the server's bin output on every launch, so it is
        /// always replaced with a direct exe path whenever one is available.
        /// </summary>
        private static bool IsDotnetRunEntry(System.Text.Json.Nodes.JsonObject entry)
        {
            var command = entry?["command"]?.GetValue<string>();
            return string.Equals(command, "dotnet", StringComparison.OrdinalIgnoreCase)
                || string.Equals(command, "dotnet.exe", StringComparison.OrdinalIgnoreCase);
        }

        private static string FindMcpProjectPath(string baseDir)
        {
            // Walk up from bin/Debug/net8.0-windows/win-x64/ to find MdExplorer.Mcp
            var dir = new DirectoryInfo(baseDir);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "MdExplorer.Mcp", "MdExplorer.Mcp.csproj");
                if (File.Exists(candidate))
                    return Path.Combine(dir.FullName, "MdExplorer.Mcp");
                dir = dir.Parent;
            }
            // Fallback: return a relative path that the user can fix
            return "MdExplorer.Mcp";
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
                    // Posti di lavoro degli agenti: vivono dentro il progetto ma sono aree di
                    // lavoro, non contenuto. Senza questa riga comparirebbero come non tracciati
                    // e un 'add -A' di un agente tenterebbe di committarsi dentro il proprio
                    // worktree.
                    gitignoreContent.AppendLine(".worktrees/");
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

        /// <summary>
        /// Ensures the project's .gitignore excludes the per-install artifacts that MdExplorer
        /// generates under .github (skills/prompts/agents named mde-*) and the instance-specific
        /// .vscode/mcp.json. These depend on the installed MDE version — different per client —
        /// so they must never be committed to a documentation repository shared across clients.
        /// Idempotent: appends the block at most once, and only when the folder is a Git repo.
        /// </summary>
        /// <param name="projectPath">Path to the project folder</param>
        public static void EnsureGitignoreEntries(string projectPath)
        {
            try
            {
                // A .gitignore is only meaningful inside a Git working tree; a plain folder
                // has nothing to track, so there's nothing to exclude.
                if (!Directory.Exists(Path.Combine(projectPath, ".git")))
                {
                    return;
                }

                var gitignorePath = Path.Combine(projectPath, ".gitignore");
                var existing = File.Exists(gitignorePath) ? File.ReadAllText(gitignorePath) : string.Empty;

                var block = new StringBuilder();

                // Idempotency marker: the wildcard pattern is unique enough to detect a previous run.
                if (!existing.Contains(".github/**/mde-*"))
                {
                    block.AppendLine("# MDE per-install artifacts — managed by MdExplorer, do not commit");
                    block.AppendLine("# (skill/prompt/agent files generated under .github vary by the installed");
                    block.AppendLine("#  MDE version, and the MCP config is instance-specific)");
                    block.AppendLine(".github/**/mde-*");
                    block.AppendLine(".vscode/mcp.json");
                }

                // Separate marker: repos that got the per-install block from an older MDE
                // version must still receive this entry.
                if (!existing.Contains(".md/mark-search/"))
                {
                    block.AppendLine("# MDE Mark Search temporary answer documents — wiped on every project open");
                    block.AppendLine(".md/mark-search/");
                }

                // Idem per i posti di lavoro degli agenti: la cartella e' nata dopo, quindi i
                // progetti gia' esistenti — che sono la maggioranza — non hanno la riga e senza
                // questo blocco vedrebbero apparire migliaia di file non tracciati al primo
                // risveglio di un agente.
                if (!existing.Contains(".worktrees/"))
                {
                    block.AppendLine("# MDE — posti di lavoro degli agenti: aree di lavoro, non contenuto");
                    block.AppendLine(".worktrees/");
                }

                if (block.Length == 0)
                {
                    return;
                }

                // Separate from any pre-existing content if it doesn't already end with a newline.
                if (existing.Length > 0 && !existing.EndsWith("\n") && !existing.EndsWith("\r\n"))
                {
                    block.Insert(0, Environment.NewLine);
                }

                File.AppendAllText(gitignorePath, block.ToString());
                Console.WriteLine($"Ensured MDE per-install .gitignore entries at: {gitignorePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring .gitignore entries: {ex.Message}");
                // Non-critical: the project can continue without the .gitignore update.
            }
        }

        /// <summary>
        /// Deletes the temporary Mark Search answer documents ({project}/.md/mark-search/*).
        /// They are per-session artifacts: the AI conversation that produced them does not
        /// survive a project re-open, so stale answers would only be confusing. Non-critical:
        /// failures are logged and ignored.
        /// </summary>
        /// <param name="projectPath">Path to the project folder</param>
        public static void CleanMarkSearchArtifacts(string projectPath)
        {
            try
            {
                var folder = Path.Combine(projectPath, ".md", "mark-search");
                if (!Directory.Exists(folder))
                {
                    return;
                }

                foreach (var file in Directory.GetFiles(folder))
                {
                    File.Delete(file);
                }
                Console.WriteLine($"Cleaned Mark Search temporary answers at: {folder}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning Mark Search artifacts: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops tracking the per-install MDE artifacts (the same set covered by
        /// <see cref="EnsureGitignoreEntries"/>) that were already committed to the repository,
        /// equivalent to <c>git rm --cached</c>: the index entries are removed but the files
        /// are left on disk. This is what actually frees a shared documentation repo from the
        /// instance-specific artifacts; .gitignore alone never untracks already-committed files.
        /// Idempotent: once untracked there is nothing left to remove on subsequent opens.
        /// </summary>
        /// <param name="projectPath">Path to the project folder</param>
        public static void UntrackPerInstallArtifacts(string projectPath)
        {
            try
            {
                if (!Directory.Exists(Path.Combine(projectPath, ".git")))
                {
                    return;
                }

                using var repo = new Repository(projectPath);

                // Collect tracked paths first, then remove — never mutate the index while
                // enumerating it. IndexEntry.Path is repo-relative with '/' separators.
                var toRemove = repo.Index
                    .Select(e => e.Path)
                    .Where(IsPerInstallArtifact)
                    .ToList();

                if (toRemove.Count == 0)
                {
                    return;
                }

                foreach (var path in toRemove)
                {
                    // removeFromWorkingDirectory: false => keep the file on disk (git rm --cached).
                    Commands.Remove(repo, path, false);
                }

                Console.WriteLine($"Untracked {toRemove.Count} per-install MDE artifact(s) in: {projectPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error untracking per-install artifacts: {ex.Message}");
                // Non-critical: the project can continue.
            }
        }

        /// <summary>
        /// True when a repo-relative path is a per-install MDE artifact: the instance-specific
        /// <c>.vscode/mcp.json</c>, or anything under <c>.github/</c> with a path segment named
        /// <c>mde-*</c> (covers both <c>mde-*</c> files and files inside <c>mde-*</c> directories).
        /// Mirrors the <c>.github/**/mde-*</c> + <c>.vscode/mcp.json</c> .gitignore patterns.
        /// </summary>
        private static bool IsPerInstallArtifact(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return false;
            }

            var p = relativePath.Replace('\\', '/');

            if (string.Equals(p, ".vscode/mcp.json", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (p.StartsWith(".github/", StringComparison.OrdinalIgnoreCase))
            {
                // Skip segment [0] (".github"); any later segment starting with "mde-" matches.
                var segments = p.Split('/');
                for (int i = 1; i < segments.Length; i++)
                {
                    if (segments[i].StartsWith("mde-", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
