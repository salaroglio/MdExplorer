using System;
using System.IO;
using MdExplorer.Features.Configuration;
using MdExplorer.Service.Models;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// L'unico cancello per leggere <c>.development.yml</c> (F2 dello sprint docs-internal/Sprints/2026-11-07-Schema-Development-Yml.md).
    /// Il file si confronta con lo schema che questo MDE porta dentro di sé: i gruppi che non combaciano sono spenti, cioè il modello
    /// li vede come non configurati, e <see cref="DevelopmentConfigRead.IsOff"/> lo dice a chi deve distinguere «assente» da «spento»
    /// prima di creare qualcosa. Al posto della lettura rigorosa di prima, che si fermava alla prima voce sconosciuta, e di quella
    /// tollerante, che la scartava e poi, riscrivendo, la cancellava.
    /// </summary>
    public static class DevelopmentConfigFile
    {
        public const string FileName = DevelopmentConfigText.FileName;

        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        public static string PathIn(string projectPath) => Path.Combine(projectPath ?? string.Empty, FileName);

        /// <summary>Il file (percorso del file, non del progetto); se non c'è, una configurazione vuota con <c>Exists=false</c>.</summary>
        public static DevelopmentConfigRead Read(string path)
        {
            if (!File.Exists(path)) return new DevelopmentConfigRead(path, false, new DevelopmentConfig(), DevelopmentSchemaCheck.Run(string.Empty));
            var usable = DevelopmentConfigText.Prune(File.ReadAllText(path));
            DevelopmentConfig config;
            try
            {
                config = string.IsNullOrWhiteSpace(usable.Text) ? new DevelopmentConfig() : (Deserializer.Deserialize<DevelopmentConfig>(usable.Text) ?? new DevelopmentConfig());
            }
            catch (YamlException ex)
            {
                // Il controllo ha detto che combacia e il modello non lo legge: è un difetto di MDE (schema e modello divergono), non del file.
                throw new InvalidOperationException(
                    $"'{path}': il controllo dello schema accetta il file ma il modello non lo legge ({ex.InnerException?.Message ?? ex.Message}). È un difetto di MDE: segnalalo. / " +
                    $"'{path}': the schema check accepts the file but the model cannot read it ({ex.InnerException?.Message ?? ex.Message}). It is an MDE defect: report it.", ex);
            }
            return new DevelopmentConfigRead(path, true, config, usable.Check);
        }

        /// <summary>Solo la configurazione, con i gruppi spenti tolti; vuota se il file non c'è.</summary>
        public static DevelopmentConfig Config(string path) => Read(path).Config;

        /// <summary>Lo stesso, a partire dalla cartella del progetto.</summary>
        public static DevelopmentConfigRead ReadProject(string projectPath) => Read(PathIn(projectPath));

        /// <summary>
        /// Il motivo per non scrivere il file, o null. Fino alla scrittura per gruppo (F3) MDE riscrive il file intero dal modello, che
        /// non ha i gruppi spenti: scrivere ora li cancellerebbe e il commit automatico pubblicherebbe la cancellazione. Quindi, se
        /// c'è un gruppo spento, non si scrive niente. Le due lingue insieme, senza UiText: si arriva qui anche dentro una transazione.
        /// </summary>
        public static string WhyNotWritable(string path)
        {
            if (!File.Exists(path)) return null;
            var check = DevelopmentConfigText.Prune(File.ReadAllText(path)).Check;
            if (check.AllGood) return null;
            return check.Describe((it, en) => it + " / " + en) + " — MDE non riscrive il file per non perdere quei gruppi. / MDE does not rewrite the file so as not to lose those groups.";
        }
    }

    public sealed class DevelopmentConfigRead
    {
        public DevelopmentConfigRead(string path, bool exists, DevelopmentConfig config, DevelopmentSchemaCheck check)
        {
            Path = path; Exists = exists; Config = config; Check = check;
        }

        public string Path { get; }
        public bool Exists { get; }

        /// <summary>Mai null; i gruppi spenti non ci sono.</summary>
        public DevelopmentConfig Config { get; }

        public DevelopmentSchemaCheck Check { get; }

        /// <summary>Il gruppo (<c>harness</c>, <c>codeRoots[2]</c>) è spento: c'è nel file, ma questo MDE non lo usa e non lo ricrea.</summary>
        public bool IsOff(string group) => Check.IsOff(group);
    }
}
