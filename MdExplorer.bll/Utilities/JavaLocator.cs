using MdExplorer.Features.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MdExplorer.Features.Utilities
{
    /// <summary>
    /// Il punto UNICO in cui MdExplorer decide quale <c>java</c> usare: lo usano PlantUML e Fuseki.
    /// Ordine: impostazione <c>JavaPath</c> (se punta a un file che esiste) → <see cref="JavaDiscovery"/>
    /// (JAVA_HOME, PATH, registro, percorsi noti; il risultato viene salvato in <c>JavaPath</c>) →
    /// <c>java</c> del PATH come ultima parola, dichiarata nel log di chi la usa.
    /// </summary>
    public interface IJavaLocator
    {
        /// <summary>Percorso dell'eseguibile java (mai vuoto: in ultima istanza <c>"java"</c>).</summary>
        string ResolveJavaPath();

        /// <summary>Come <see cref="ResolveJavaPath()"/> dentro una sessione già aperta dal chiamante.</summary>
        string ResolveJavaPath(ISessionDB session);

        /// <summary>Esegue <c>java -version</c> e ne legge la versione maggiore (21, 17, 8…).</summary>
        JavaProbe Probe(string javaPath);
        /// <summary>Il Java con la versione maggiore più alta fra tutti i candidati del computer, purché almeno <paramref name="minimumMajor"/>; null se nessuno basta. Sonda ogni candidato: costa qualche secondo, si fa all'avvio e su richiesta, mai dentro una richiesta HTTP.</summary>
        JavaProbe FindBest(int minimumMajor);
    }

    public sealed class JavaProbe
    {
        public string Path { get; set; }
        /// <summary>Versione maggiore: 21 per «21.0.11», 8 per «1.8.0_281»; <c>null</c> se non leggibile.</summary>
        public int? Major { get; set; }
        /// <summary>Prima riga di <c>java -version</c>, per i messaggi.</summary>
        public string Raw { get; set; }
        /// <summary>false = java non eseguibile (file assente, permessi, non è un java).</summary>
        public bool IsRunnable { get; set; }
        public string Error { get; set; }
    }

    public class JavaLocator : IJavaLocator
    {
        private readonly IDALFactory<IUserSettingsDB> _dalFactory;

        public JavaLocator(IDALFactory<IUserSettingsDB> dalFactory)
        {
            _dalFactory = dalFactory;
        }

        public string ResolveJavaPath()
        {
            // Sessione propria, quindi transazione propria: la lettura di JavaPath e l'eventuale salvataggio
            // del percorso scoperto non devono restare fuori transazione (SQLite, altri thread: «database is locked»).
            using var session = _dalFactory.OpenSession();
            using var tx = session.BeginTransaction();
            var path = ResolveJavaPath(session);
            tx.Commit();
            return path;
        }

        public string ResolveJavaPath(ISessionDB session)
        {
            var settingDal = session.GetDal<Setting>();
            var javaSetting = settingDal.GetList().FirstOrDefault(_ => _.Name == "JavaPath");
            var javaPath = javaSetting?.ValueString;

            if (string.IsNullOrEmpty(javaPath) || !File.Exists(javaPath))
            {
                var discovered = JavaDiscovery.DiscoverJavaPath();
                if (!string.IsNullOrEmpty(discovered))
                {
                    javaPath = discovered;
                    if (javaSetting != null)
                    {
                        javaSetting.ValueString = discovered;
                        settingDal.Save(javaSetting);
                        session.Flush();
                    }
                }
            }

            return string.IsNullOrEmpty(javaPath) ? "java" : javaPath;
        }

        public JavaProbe FindBest(int minimumMajor)
        {
            JavaProbe best = null;
            foreach (var candidate in JavaDiscovery.EnumerateCandidates())
            {
                var probe = Probe(candidate);
                if (!probe.IsRunnable || probe.Major == null || probe.Major < minimumMajor) continue;
                if (best == null || probe.Major > best.Major) best = probe;
            }
            return best;
        }

        public JavaProbe Probe(string javaPath)
        {
            var probe = new JavaProbe { Path = javaPath };
            try
            {
                var psi = new ProcessStartInfo(javaPath)
                {
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                psi.ArgumentList.Add("-version");
                using var process = Process.Start(psi);
                if (process == null)
                {
                    probe.Error = UiText.Say("Process.Start ha restituito null", "Process.Start returned null");
                    return probe;
                }
                var stderr = process.StandardError.ReadToEndAsync();
                var stdout = process.StandardOutput.ReadToEndAsync();
                if (!process.WaitForExit(10000))
                {
                    try { process.Kill(true); } catch { }
                    probe.Error = UiText.Say("java -version non ha risposto entro 10 secondi", "java -version did not answer within 10 seconds");
                    return probe;
                }
                // java -version storicamente scrive su stderr; alcune JVM su stdout.
                var output = (stderr.Result ?? string.Empty) + (stdout.Result ?? string.Empty);
                probe.Raw = output.Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.Length > 0);
                probe.IsRunnable = true;
                probe.Major = ParseMajor(output);
                return probe;
            }
            catch (Exception ex)
            {
                probe.Error = ex.Message;
                return probe;
            }
        }

        /// <summary>
        /// «openjdk version "21.0.11"» → 21; «java version "1.8.0_281"» → 8; «openjdk 17 2021-09-14» → 17.
        /// </summary>
        public static int? ParseMajor(string versionOutput)
        {
            if (string.IsNullOrWhiteSpace(versionOutput)) return null;
            var m = Regex.Match(versionOutput, "version \"?(\\d+)(?:\\.(\\d+))?");
            if (!m.Success)
                m = Regex.Match(versionOutput, "openjdk (\\d+)(?:\\.(\\d+))?");
            if (!m.Success) return null;
            var major = int.Parse(m.Groups[1].Value);
            if (major == 1 && m.Groups[2].Success) major = int.Parse(m.Groups[2].Value);
            return major;
        }
    }
}
