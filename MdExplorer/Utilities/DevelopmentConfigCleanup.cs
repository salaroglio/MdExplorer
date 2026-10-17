using System;
using System.IO;
using MdExplorer.Features.Configuration;
using MdExplorer.Service.Models;
using Microsoft.Extensions.Logging;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Deletes from a project's <c>.development.yml</c> the keys MdExplorer no longer has
    /// (<see cref="RetiredConfigKeys"/>), when the project is opened. The file is the team's and is in git:
    /// the change shows as one line removed, to commit as a cleanup, and the service's log says which keys.
    /// </summary>
    public static class DevelopmentConfigCleanup
    {
        public const string FileName = ".development.yml";

        /// <summary>
        /// Reads a project's <c>.development.yml</c> <b>strictly</b>: the retired keys are taken out first, by
        /// name; any other key the model does not have is an error that names the file, the line and the key —
        /// a typo must not read as «nothing configured». Null content gives an empty configuration.
        /// </summary>
        public static DevelopmentConfig ReadStrict(string path)
        {
            var yaml = RetiredConfigKeys.Remove(File.ReadAllText(path), out _);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            try
            {
                return deserializer.Deserialize<DevelopmentConfig>(yaml) ?? new DevelopmentConfig();
            }
            catch (YamlException ex)
            {
                var why = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException(
                    $"'{path}', riga {ex.Start.Line}: {why} Correggi o togli quella riga: MdExplorer non legge un file di configurazione con righe che non conosce.", ex);
            }
        }

        /// <summary>True when the file was rewritten. A file that cannot be read or written is left alone and the reason is logged.</summary>
        public static bool CleanFile(string projectPath, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return false;
            var path = Path.Combine(projectPath, FileName);
            if (!File.Exists(path)) return false;

            try
            {
                var text = File.ReadAllText(path);
                var cleaned = RetiredConfigKeys.Remove(text, out var removed);
                if (removed.Count == 0) return false;

                File.WriteAllText(path, cleaned);
                logger?.LogWarning("[Config] '{File}': tolte le righe ritirate {Keys}. Il file risulta modificato: va committato.",
                    path, string.Join(", ", removed));
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "[Config] '{File}': pulizia delle righe ritirate non riuscita; il file resta com'è.", path);
                return false;
            }
        }
    }
}
