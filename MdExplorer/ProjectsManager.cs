using Ad.Tools.Dal;
using Ad.Tools.Dal.Concrete;
using Ad.Tools.FluentMigrator.Interfaces;
using FluentMigrator.Runner;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.DataAccess.Engine;
using MdExplorer.Features.Utilities;
using MdExplorer.Migrations;
using MdExplorer.Migrations.EngineDb.Version202107;
using MDExplorer.DataAccess.Mapping;
using Microsoft.Extensions.DependencyInjection;
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
            SetTemplates(pathFromParameter);
            var appdata = Environment.GetEnvironmentVariable("LocalAppData");
            var databasePath = $@"Data Source = {appdata}\MdExplorer.db";
            var currentDirectory = pathFromParameter;
            var hash = Helper.HGetHashString(currentDirectory);
            var databasePathEngine = $@"Data Source = {appdata}\MdEngine_{hash}.db";
            var databasePathProject = $@"Data Source = {appdata}\MdProject_{hash}.db";

            UpgradeDatabases(databasePath, databasePathEngine);

            serviceProvider.ReplaceDalFeatures(typeof(SettingsMap).Assembly,
                                    new DatabaseSQLite(), typeof(IUserSettingsDB),
                                    databasePath);

            serviceProvider.ReplaceDalFeatures(typeof(MarkdownFileMap).Assembly,
                                   new DatabaseSQLite(), typeof(IEngineDB),
                                   databasePathEngine);

            serviceProvider.ReplaceDalFeatures(typeof(SemanticCluster).Assembly,
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
            var currentPath = ConfigFileSystemWatchers(services, pathFromParameter);
            ConfigTemplates(currentPath, services);
            var appdata = Environment.GetEnvironmentVariable("LocalAppData");
            var databasePath = $@"Data Source = {appdata}\MdExplorer.db";
            var currentDirectory = currentPath;
            var hash = Helper.HGetHashString(currentDirectory);
            var databasePathEngine = $@"Data Source = {appdata}\MdEngine_{hash}.db";
            var databasePathProject = $@"Data Source = {appdata}\MdProject_{hash}.db";

            UpgradeDatabases(databasePath, databasePathEngine);

            services.AddDalFeatures(typeof(SettingsMap).Assembly,
                                    new DatabaseSQLite(), typeof(IUserSettingsDB),
                                    databasePath);



            services.AddDalFeatures(typeof(MarkdownFileMap).Assembly,
                                   new DatabaseSQLite(), typeof(IEngineDB),
                                   databasePathEngine);



            services.AddDalFeatures(typeof(SemanticCluster).Assembly,
                                   new DatabaseSQLite(), typeof(IProjectDB),
                                   databasePathProject);
        }

        /// <summary>
        /// Migrate database is done using "custom" serviceCollection.
        /// At this point of my knowledge i'm suspecting there is a bug in FluentMigrator
        /// witch doesn't support multiple migration of different database at the same time 
        /// </summary>
        /// <param name="databasePath"></param>
        /// <param name="databasePathEngine"></param>
        private static void UpgradeDatabases(string databasePath, string databasePathEngine)
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
            var defaultPath = pathFromParameter ?? Directory.GetCurrentDirectory(); // @".\Documents";

            services.AddSingleton(new FileSystemWatcher { Path = defaultPath });
            var _fileSystemWatcher = new FileSystemWatcher { Path = defaultPath };
            services.AddSingleton(_fileSystemWatcher);
            return defaultPath;
        }

        private static void SetTemplates(string mdPath)
        {
            var directory = $"{Path.GetDirectoryName(mdPath)}{Path.DirectorySeparatorChar}.md";
            var directoryEmoji = $"{directory}{Path.DirectorySeparatorChar}EmojiForPandoc";
            Directory.CreateDirectory(directory);
            Directory.CreateDirectory($"{directory}{Path.DirectorySeparatorChar}templates");
            Directory.CreateDirectory(directoryEmoji);

            var assembly = Assembly.GetExecutingAssembly();
            var embeddedSubfolder = "MdExplorer.Service.EmojiForPandoc.";
            var embeddedEmojies = assembly.GetManifestResourceNames();
            var selectedEmojies = embeddedEmojies.Where(_ => _.Contains(embeddedSubfolder))
                    .Select(_ => new { EmbeddedName = _, Name = _.Replace(embeddedSubfolder, string.Empty) }).ToArray();
            foreach (var itemEmoj in selectedEmojies)
            {
                FileUtil.ExtractResFile(itemEmoj.EmbeddedName, $@"{directoryEmoji}{Path.DirectorySeparatorChar}{itemEmoj.Name}");
            }

            FileUtil.ExtractResFile("MdExplorer.Service.eisvogel.tex", $@"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}eisvogel.tex");
            FileUtil.ExtractResFile("MdExplorer.Service.reference.docx", $@"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}reference.docx");

        }

        private static void ConfigTemplates(string mdPath, IServiceCollection services)
        {
            var directory = $"{Path.GetDirectoryName(mdPath)}{Path.DirectorySeparatorChar}.md";
            var directoryEmoji = $"{directory}{Path.DirectorySeparatorChar}EmojiForPandoc";
            Directory.CreateDirectory(directory);
            Directory.CreateDirectory($"{directory}{Path.DirectorySeparatorChar}templates");
            Directory.CreateDirectory(directoryEmoji);

            var assembly = Assembly.GetExecutingAssembly();
            var embeddedSubfolder = "MdExplorer.Service.EmojiForPandoc.";
            var embeddedEmojies = assembly.GetManifestResourceNames();
            var selectedEmojies = embeddedEmojies.Where(_ => _.Contains(embeddedSubfolder))
                    .Select(_ => new { EmbeddedName = _, Name = _.Replace(embeddedSubfolder, string.Empty) }).ToArray();
            services.AddSingleton(typeof(IServerCache), new ServerCache { Emojies = selectedEmojies.Select(_ => _.Name).ToArray() });
            foreach (var itemEmoj in selectedEmojies)
            {
                FileUtil.ExtractResFile(itemEmoj.EmbeddedName, $@"{directoryEmoji}{Path.DirectorySeparatorChar}{itemEmoj.Name}");
            }

            FileUtil.ExtractResFile("MdExplorer.Service.eisvogel.tex", $@"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}eisvogel.tex");
            FileUtil.ExtractResFile("MdExplorer.Service.reference.docx", $@"{directory}{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}reference.docx");
        }
    }
}
