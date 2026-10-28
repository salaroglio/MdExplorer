using System;
using System.IO;
using MdExplorer.Service.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// La sezione <c>redmine:</c> di .development.yml (sprint 2026-10-27-Redmine, RM2). Si legge in modo stretto: un file
    /// con righe sbagliate è un errore che dice quale riga, non «Redmine non configurato» (regola #2).
    /// </summary>
    public static class RedmineConfigFile
    {
        /// <summary>La sezione, o null se il file non c'è o non ha <c>redmine:</c>.</summary>
        public static RedmineConfig Read(string projectPath)
        {
            var file = Path.Combine(projectPath ?? "", DevelopmentConfigCleanup.FileName);
            return File.Exists(file) ? DevelopmentConfigCleanup.ReadStrict(file).Redmine : null;
        }

        /// <summary>Scrive la sezione (null la toglie) lasciando le altre come sono.</summary>
        public static void Write(string projectPath, RedmineConfig config)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                throw new DirectoryNotFoundException($"Project path does not exist: {projectPath}");
            var file = Path.Combine(projectPath, DevelopmentConfigCleanup.FileName);
            if (!File.Exists(file))
                throw new FileNotFoundException(
                    $"'{file}' does not exist: the project's base configuration must be created first.", file);

            var dev = DevelopmentConfigCleanup.ReadStrict(file);
            dev.Redmine = config;
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();
            File.WriteAllText(file, serializer.Serialize(dev));
        }
    }
}
