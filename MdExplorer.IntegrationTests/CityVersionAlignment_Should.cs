using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using MdExplorer.Features.Agents.Workflow;
using MdExplorer.Features.Agents.Workflow.Scheduler;
using MdExplorer.Features.Configuration;
using MdExplorer.Features.Federation;
using MdExplorer.Services;
using MdExplorer.Services.Federation;
using MdExplorer.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// N7 della valutazione del 07/11, deciso con l'utente: la città degli agenti lavora solo tra MDE che gestiscono la stessa versione
    /// di <c>.development.yml</c>. Chi ne gestisce una più bassa ha la città spenta e l'invito ad aggiornare MDE all'ultima versione;
    /// chi ne gestisce una più alta la scrive nel file. Il registro dei giri e i messaggi fra i computer cambiano con quella versione:
    /// un test ferma chi li cambia senza alzarla.
    /// </summary>
    [TestClass]
    public class CityVersionAlignment_Should
    {
        private string _dir;

        [TestInitialize]
        public void Create() { _dir = Path.Combine(Path.GetTempPath(), "citta-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(_dir); }

        [TestCleanup]
        public void Remove() { if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true); }

        private string Yml => Path.Combine(_dir, ".development.yml");

        private const string Body = "folders: []\nharness:\n  target: claude\nagentCity:\n  enabled: true\n  roomSecret: SEGRETO\n";

        [TestMethod]
        public void Switch_the_city_off_for_an_mde_that_handles_a_lower_version()
        {
            var newer = DevelopmentSchemaSnapshots.Current + 1;
            var check = DevelopmentSchemaCheck.Run($"# mde-schema: {newer}\n" + Body);

            Assert.IsTrue(check.IsOff("agentCity"), "la città resta spenta");
            Assert.IsFalse(check.IsOff("harness"), "il resto funziona");
            var notice = check.Describe((it, en) => it);
            StringAssert.Contains(notice, $"Questo progetto usa la versione {newer} di .development.yml, il tuo MDE gestisce fino alla {DevelopmentSchemaSnapshots.Current}");
            StringAssert.Contains(notice, "per lavorare nella città degli agenti aggiorna MDE all'ultima versione");

            File.WriteAllText(Yml, $"# mde-schema: {newer}\n" + Body);
            var metadata = new ProjectMetadataService(NullLogger<ProjectMetadataService>.Instance);
            Assert.ThrowsException<DevelopmentConfigOffException>(() => metadata.EnsureRoomSecret(_dir), "nemmeno la stanza della chat");

            Assert.IsFalse(DevelopmentSchemaCheck.Run($"# mde-schema: {DevelopmentSchemaSnapshots.Current}\n" + Body).IsOff("agentCity"), "stessa versione: la città va");
        }

        [TestMethod]
        public void Raise_the_version_in_the_file_when_the_city_is_on()
        {
            File.WriteAllText(Yml, "# mde-schema: 1\n" + Body);
            Assert.IsTrue(DevelopmentConfigWriter.AlignForCity(Yml));
            Assert.AreEqual($"# mde-schema: {DevelopmentSchemaSnapshots.Current}\n" + Body, File.ReadAllText(Yml), "solo il numero, il resto byte per byte");
            Assert.IsFalse(DevelopmentConfigWriter.AlignForCity(Yml), "già allineato");

            File.WriteAllText(Yml, Body);
            Assert.IsTrue(DevelopmentConfigWriter.AlignForCity(Yml), "senza numero = versione 1");
            StringAssert.StartsWith(File.ReadAllText(Yml), $"# mde-schema: {DevelopmentSchemaSnapshots.Current}\n");

            var off = Body.Replace("enabled: true", "enabled: false");
            File.WriteAllText(Yml, off);
            Assert.IsFalse(DevelopmentConfigWriter.AlignForCity(Yml), "città spenta: nessuno da avvisare");
            Assert.AreEqual(off, File.ReadAllText(Yml));
        }

        // ---- i formati condivisi della città ----

        /// <summary>
        /// L'impronta dei formati che i computer della città si scambiano, per versione di .development.yml. Cambi la forma di uno di
        /// questi tipi? Alza <c>DevelopmentSchemaSnapshots.Current</c> (con l'istantanea nuova, anche identica) e aggiungi qui l'impronta:
        /// così chi è indietro ha la città spenta invece di bloccarsi su un giro o un messaggio che non capisce.
        /// </summary>
        private static readonly Dictionary<int, string> CityFormats = new()
        {
            [2] = "d415107d2c0c4165",
        };

        private static readonly Type[] Shared =
        {
            typeof(RoundHeader), typeof(StepRecord), typeof(WorkflowCardSet), typeof(CityPresence),
            typeof(FederatedRequestPayload), typeof(FederatedResultPayload), typeof(WorkflowNewsPayload),
        };

        private static string Shape()
        {
            var sb = new StringBuilder();
            var seen = new HashSet<Type>();
            void Walk(Type t)
            {
                t = Nullable.GetUnderlyingType(t) ?? t;
                if (t.IsPrimitive || t == typeof(string) || t == typeof(decimal) || t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(Guid) || t == typeof(TimeSpan)) return;
                if (!seen.Add(t)) return;
                if (t.IsEnum) { sb.Append(t.Name).Append(" enum ").Append(string.Join(",", Enum.GetNames(t))).Append('\n'); return; }
                if (t.IsGenericType && typeof(IEnumerable).IsAssignableFrom(t)) { foreach (var a in t.GetGenericArguments()) Walk(a); return; }
                if (t.IsArray) { Walk(t.GetElementType()); return; }
                sb.Append(t.Name).Append(" {");
                foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null).OrderBy(p => p.Name, StringComparer.Ordinal))
                    sb.Append(' ').Append(p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name).Append(':').Append(Name(p.PropertyType));
                sb.Append(" }\n");
                foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance)) Walk(p.PropertyType);
            }
            static string Name(Type t) => t.IsGenericType ? t.Name.Split('`')[0] + "<" + string.Join(",", t.GetGenericArguments().Select(Name)) + ">" : t.Name;
            foreach (var t in Shared) Walk(t);
            return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString())))[..16].ToLowerInvariant();
        }

        [TestMethod]
        public void Raise_the_version_when_a_shared_city_format_changes()
        {
            var now = Shape();
            var current = DevelopmentSchemaSnapshots.Current;
            Assert.IsTrue(CityFormats.TryGetValue(current, out var known),
                $"Manca l'impronta dei formati della città per la versione {current}: aggiungi [{current}] = \"{now}\" a CityFormats.");
            Assert.AreEqual(known, now,
                $"Un formato condiviso della città (registro dei giri, schede, messaggi fra i computer) è cambiato con la versione {current}: " +
                $"alza DevelopmentSchemaSnapshots.Current, aggiungi l'istantanea e [{current + 1}] = \"{now}\" a CityFormats. " +
                "Altrimenti un MDE indietro resta nella città e si blocca su ciò che non capisce.");
        }
    }
}
