using Ad.Tools.Dal;
using Ad.Tools.Dal.Concrete;
using Ad.Tools.FluentMigrator.Interfaces;
using FluentMigrator.Runner;
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

namespace MdExplorer.Service
{
    public class ProjectsManager
    {
        public static void SetNewProject(IServiceProvider serviceProvider, string pathFromParameter)
        {
            ConfigTemplates(pathFromParameter, null);
            var appdata = Environment.GetEnvironmentVariable("LocalAppData");
            var databasePath = $@"Data Source = {appdata + Path.DirectorySeparatorChar}MdExplorer.db";
            var currentDirectory = pathFromParameter;
            var hash = Helper.HGetHashString(currentDirectory);
            var databasePathEngine = $@"Data Source = {appdata + Path.DirectorySeparatorChar}MdEngine_{hash}.db";
            var databasePathProject = $@"Data Source = {currentDirectory + Path.DirectorySeparatorChar + ".md" + Path.DirectorySeparatorChar}MdProject_{hash}.db";

            UpgradeDatabases(databasePath, databasePathEngine, databasePathProject);

            serviceProvider.ReplaceDalFeatures(typeof(SettingsMap).Assembly,
                                    new DatabaseSQLite(), typeof(IUserSettingsDB),
                                    databasePath);
            serviceProvider.ReplaceDalFeatures(typeof(MarkdownFileMap).Assembly,
                                   new DatabaseSQLite(), typeof(IEngineDB),
                                   databasePathEngine);
            serviceProvider.ReplaceDalFeatures(typeof(SemanticClusterMap).Assembly,
                                   new DatabaseSQLite(), typeof(IProjectDB),
                                   databasePathProject);

            // Migration complete
        }

        /// <summary>
        /// This is a function called when a new Folder is selected.
        /// When a new folder is selected i need reinitialize all dbs and prepare eventually new one or migrate them
        /// </summary>
        /// <param name="services"></param>
        /// <param name="pathFromParameter"></param>
        public static void SetProjectInitialization(IServiceCollection services, string pathFromParameter)
        {            
            
            var appdata = Environment.GetEnvironmentVariable("LocalAppData");
            var databasePath = $@"Data Source = {appdata + Path.DirectorySeparatorChar}MdExplorer.db";
            var currentDirectory = ConfigFileSystemWatchers(services, pathFromParameter);
            ConfigTemplates(currentDirectory, services);
            var hash = Helper.HGetHashString(currentDirectory);
            var databasePathEngine = $@"Data Source = {appdata + Path.DirectorySeparatorChar}MdEngine_{hash}.db";
            var databasePathProject = $@"Data Source = {appdata + Path.DirectorySeparatorChar}MdProject_{hash}.db";

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
        /// <param name="databasePath"></param>
        /// <param name="databasePathEngine"></param>
        private static void UpgradeDatabases(string databasePath, string databasePathEngine = null, string databaseProject = null)
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
            if (databasePathEngine!=null)
            {
                localServices = new ServiceCollection();
                localServices.AddFluentMigratorFeatures(
                                              (rb) =>
                                              {
                                                  rb.AddSQLite()
                                                  .WithGlobalConnectionString(databasePathEngine)
                                                  .ScanIn(typeof(ME2021_07_23_001).Assembly)
                                                  .For.Migrations();
                                              }, "SQLite");
                builder = localServices.BuildServiceProvider();
                Migrate(builder);
            }
            if (databaseProject != null)
            {
                localServices = new ServiceCollection();
                localServices.AddFluentMigratorFeatures(
                                              (rb) =>
                                              {
                                                  rb.AddSQLite()
                                                  .WithGlobalConnectionString(databaseProject)
                                                  .ScanIn(typeof(MP2022_10_09_001).Assembly)
                                                  .For.Migrations();
                                              }, "SQLite");
                builder = localServices.BuildServiceProvider();
                Migrate(builder);
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

private static string ConfigFileSystemWatchers(IServiceCollection services, string pathFromParameter)
{
    string effectivePath = pathFromParameter;

    // Check if pathFromParameter is null, empty, or not a valid directory path.
    // Path.GetDirectoryName on a simple string like "5000" returns an empty string.
    if (string.IsNullOrEmpty(effectivePath) || !Directory.Exists(effectivePath))
    {
        // If pathFromParameter is not a valid directory, default to the application's base directory.
        // This ensures FileSystemWatcher always gets a valid directory.
        effectivePath = AppDomain.CurrentDomain.BaseDirectory;
    }
    
    // It's generally better to register a factory or a configured instance once.
    // Registering FileSystemWatcher as a singleton that gets configured here.
    services.AddSingleton<FileSystemWatcher>(sp => new FileSystemWatcher(effectivePath));
    
    return effectivePath; // Return the path that was actually used.
}

        private static void ConfigTemplates(string mdPath, IServiceCollection services = null)
        {
            var publishFolder = $"{mdPath}{Path.DirectorySeparatorChar}mdPublish";
            Directory.CreateDirectory(publishFolder);
            //var directory = $"{Path.GetDirectoryName(mdPath)}{Path.DirectorySeparatorChar}.md";
            var directory = $"{mdPath}{Path.DirectorySeparatorChar}.md";
            var directoryEmoji = $"{directory}{Path.DirectorySeparatorChar}EmojiForPandoc";
            Directory.CreateDirectory(directory);
            Directory.CreateDirectory($"{directory}{Path.DirectorySeparatorChar}templates");
            Directory.CreateDirectory(directoryEmoji);
            
            // Copy configuration files to project root if they don't exist
            CopyConfigurationFilesToProject(mdPath);

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
            //FileUtil.ExtractResFile("MdExplorer.Service.templates.word.reference.docx", $@"{
            //        directory}{
            //        Path.DirectorySeparatorChar}templates{
            //        Path.DirectorySeparatorChar}word{
            //        Path.DirectorySeparatorChar}reference.docx");
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
        
        private static void CopyConfigurationFilesToProject(string projectPath)
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying configuration files: {ex.Message}");
                // Non-critical error, continue without the files
            }
        }
    }
}
