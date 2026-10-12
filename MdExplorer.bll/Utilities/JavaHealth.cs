using System;
using System.IO;

namespace MdExplorer.Features.Utilities
{
    /// <summary>Da dove viene il valore di <c>JavaPath</c>: scritto dalla scoperta automatica o dall'utente nelle impostazioni.</summary>
    public static class JavaPathSources
    {
        public const string Auto = "auto";
        public const string User = "user";
    }

    /// <summary>Le due impostazioni che il controllo di salute legge e scrive: il percorso di Java e chi l'ha messo.</summary>
    public interface IJavaSettingsStore
    {
        (string Path, string Source) Read();
        void Write(string path, string source);
    }

    public enum JavaHealthOutcome
    {
        /// <summary>Il Java impostato c'è, gira ed è almeno la versione minima.</summary>
        Ok,
        /// <summary>Il Java impostato era assente, vecchio o rotto, l'aveva scritto la scoperta automatica: sostituito con il migliore trovato.</summary>
        Replaced,
        /// <summary>Il Java impostato è vecchio o rotto ma l'ha scritto l'utente: non si tocca, si avvisa e si dice qual è il migliore trovato.</summary>
        UserPathNotSuitable,
        /// <summary>Nessun Java adatto sul computer: il valore resta (PlantUML può ancora usarlo) e si chiede di installare la versione minima.</summary>
        NoSuitableJava,
    }

    public sealed class JavaHealthReport
    {
        public JavaHealthOutcome Outcome { get; init; }
        public int MinimumMajor { get; init; }
        public string ConfiguredPath { get; init; }
        public int? ConfiguredMajor { get; init; }
        public string ConfiguredProblem { get; init; }
        /// <summary>empty · missing · broken · unreadable · old: la UI lo traduce.</summary>
        public string ConfiguredProblemKind { get; init; }
        public string BestPath { get; init; }
        public int? BestMajor { get; init; }
        /// <summary>Il Java che il servizio userà da adesso (dopo un'eventuale sostituzione).</summary>
        public string EffectivePath { get; init; }
    }

    /// <summary>
    /// Il controllo di salute di Java (12/10/2026): <c>JavaPath</c> lo scrive MdExplorer da solo la prima volta che trova un
    /// Java e poi non lo rimetteva mai in discussione, così un JRE 8 trovato nel 2025 per PlantUML bloccava Fuseki nel 2026.
    /// Regole: un valore scritto dalla scoperta (<see cref="JavaPathSources.Auto"/>, o senza sorgente) che non è adatto viene
    /// sostituito con il migliore Java trovato; un valore scritto dall'utente non si tocca, si avvisa. Niente di tutto questo
    /// è silenzioso: il chiamante scrive il rapporto nel log e negli avvisi di avvio.
    /// </summary>
    public sealed class JavaHealth
    {
        private readonly IJavaLocator _locator;
        private readonly IJavaSettingsStore _store;

        public JavaHealth(IJavaLocator locator, IJavaSettingsStore store)
        {
            _locator = locator ?? throw new ArgumentNullException(nameof(locator));
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public JavaHealthReport Check(int minimumMajor)
        {
            var (configured, source) = _store.Read();
            var userSet = string.Equals(source, JavaPathSources.User, StringComparison.OrdinalIgnoreCase);

            string problem = null, kind = null;
            JavaProbe probe = null;
            if (string.IsNullOrWhiteSpace(configured)) { problem = "nessun percorso impostato"; kind = "empty"; }
            else if (!File.Exists(configured)) { problem = "il file non esiste più"; kind = "missing"; }
            else
            {
                probe = _locator.Probe(configured);
                if (!probe.IsRunnable) { problem = "non eseguibile: " + probe.Error; kind = "broken"; }
                else if (probe.Major == null) { problem = "versione non leggibile"; kind = "unreadable"; }
                else if (probe.Major < minimumMajor) { problem = $"Java {probe.Major}, serve almeno Java {minimumMajor}"; kind = "old"; }
            }

            if (problem == null)
            {
                return new JavaHealthReport
                {
                    Outcome = JavaHealthOutcome.Ok, MinimumMajor = minimumMajor,
                    ConfiguredPath = configured, ConfiguredMajor = probe?.Major, EffectivePath = configured,
                };
            }

            var best = _locator.FindBest(minimumMajor);
            if (best == null)
            {
                return new JavaHealthReport
                {
                    Outcome = JavaHealthOutcome.NoSuitableJava, MinimumMajor = minimumMajor,
                    ConfiguredPath = configured, ConfiguredMajor = probe?.Major, ConfiguredProblem = problem, ConfiguredProblemKind = kind,
                    EffectivePath = configured,
                };
            }

            if (userSet && !string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
            {
                return new JavaHealthReport
                {
                    Outcome = JavaHealthOutcome.UserPathNotSuitable, MinimumMajor = minimumMajor,
                    ConfiguredPath = configured, ConfiguredMajor = probe?.Major, ConfiguredProblem = problem, ConfiguredProblemKind = kind,
                    BestPath = best.Path, BestMajor = best.Major, EffectivePath = configured,
                };
            }

            _store.Write(best.Path, JavaPathSources.Auto);
            return new JavaHealthReport
            {
                Outcome = JavaHealthOutcome.Replaced, MinimumMajor = minimumMajor,
                ConfiguredPath = configured, ConfiguredMajor = probe?.Major, ConfiguredProblem = problem, ConfiguredProblemKind = kind,
                BestPath = best.Path, BestMajor = best.Major, EffectivePath = best.Path,
            };
        }
    }
}
