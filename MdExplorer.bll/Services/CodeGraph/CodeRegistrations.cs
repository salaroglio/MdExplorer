using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Una registrazione: «questa astrazione si risolve in questa implementazione», dichiarata FUORI dal punto in cui si
    /// chiama (30/10/2026). ServiceLoader è la prima; Guice, Spring, .NET, Angular, chiave → handler hanno la stessa forma.
    /// </summary>
    public sealed class CodeRegistration
    {
        /// <summary>Il meccanismo che la dichiara (<c>serviceloader</c>): lo dice l'arco, così chi legge sa da dove viene.</summary>
        public string Mechanism { get; init; }
        public string AbstractionQualifiedName { get; init; }
        public string ImplementationQualifiedName { get; init; }
        /// <summary>Il file che la dichiara (percorso logico) e la riga.</summary>
        public string Path { get; init; }
        public int Line { get; init; }
    }

    /// <summary>
    /// Le registrazioni del progetto diventano archi <c>resolvesTo</c> (30/10/2026, utente: «è un meccanismo generico che
    /// può ripresentarsi?»): dall'astrazione all'implementazione registrata e, metodo per metodo, dal metodo
    /// dell'astrazione a quello con lo stesso nome dell'implementazione. Così una chiamata che arriva a un metodo astratto
    /// prosegue nel codice che gira davvero, e l'impatto di un'implementazione risale a chi chiama l'interfaccia. Puro:
    /// le classi le dà il grafo (<see cref="CodeOwnerMember"/>), i file chi chiama.
    /// </summary>
    public static class CodeRegistrations
    {
        public const string Relation = "resolvesTo";
        public const string ServiceLoader = "serviceloader";

        /// <summary>
        /// Un file <c>META-INF/services/&lt;interfaccia&gt;</c> di Java: sta in una cartella <c>services</c> dentro
        /// <c>META-INF</c>, e il suo nome è il nome qualificato dell'interfaccia.
        /// </summary>
        public static bool IsServiceLoaderFile(string logicalPath)
        {
            var parts = (logicalPath ?? string.Empty).Replace('\\', '/').Split('/');
            return parts.Length >= 3 && parts[^2] == "services" && parts[^3] == "META-INF" && parts[^1].Length > 0;
        }

        /// <summary>
        /// Le righe di un file ServiceLoader: una classe per riga; <c>#</c> commenta fino a fine riga; le classi interne
        /// (<c>Esterna$Interna</c>) col punto, come le scrive il grafo.
        /// </summary>
        public static List<CodeRegistration> ParseServiceLoader(string logicalPath, string text)
        {
            var abstraction = Java(logicalPath.Replace('\\', '/').Split('/')[^1]);
            var list = new List<CodeRegistration>();
            var lines = (text ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var hash = line.IndexOf('#');
                if (hash >= 0) line = line[..hash];
                line = line.Trim().TrimStart('﻿');
                if (line.Length == 0) continue;
                list.Add(new CodeRegistration { Mechanism = ServiceLoader, AbstractionQualifiedName = abstraction, ImplementationQualifiedName = Java(line), Path = logicalPath, Line = i + 1 });
            }
            return list;
        }

        private static string Java(string binaryName) => binaryName.Trim().Replace('$', '.');

        /// <summary>
        /// Gli archi delle registrazioni, con le classi del grafo (<paramref name="owners"/>, chiave: i nomi qualificati delle
        /// astrazioni e delle implementazioni). Una classe con quel nome in più programmi = archi <c>ambiguous</c>; un metodo
        /// dell'astrazione con più metodi omonimi nell'implementazione (overload) = <c>ambiguous</c>. Quello che il grafo non
        /// ha (una libreria fuori dal perimetro) non diventa un arco: si dice in <paramref name="misses"/>.
        /// </summary>
        public static List<ScriptEdge> Edges(IEnumerable<CodeRegistration> registrations, IEnumerable<CodeOwnerMember> owners, out List<string> misses)
        {
            var byName = owners.GroupBy(o => o.OwnerQualifiedName, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
            var edges = new List<ScriptEdge>();
            misses = new List<string>();
            foreach (var r in registrations)
            {
                byName.TryGetValue(r.AbstractionQualifiedName, out var abs);
                byName.TryGetValue(r.ImplementationQualifiedName, out var impl);
                if (abs == null || impl == null)
                {
                    misses.Add(UiText.Say($"{r.Path}:{r.Line}: {(abs == null ? r.AbstractionQualifiedName : r.ImplementationQualifiedName)} non è nel grafo",
                        $"{r.Path}:{r.Line}: {(abs == null ? r.AbstractionQualifiedName : r.ImplementationQualifiedName)} is not in the graph"));
                    continue;
                }
                var absIds = abs.Select(o => o.OwnerId).Distinct(StringComparer.Ordinal).ToList();
                var implIds = impl.Select(o => o.OwnerId).Distinct(StringComparer.Ordinal).ToList();
                var classConfidence = absIds.Count == 1 && implIds.Count == 1 ? "extracted" : "ambiguous";
                foreach (var a in absIds)
                    foreach (var b in implIds)
                        edges.Add(Edge(a, b, classConfidence, r));
                var implMethods = impl.Where(o => o.MemberId != null).GroupBy(o => o.MemberName, StringComparer.Ordinal)
                    .ToDictionary(g => g.Key, g => g.Select(o => o.MemberId).Distinct(StringComparer.Ordinal).ToList(), StringComparer.Ordinal);
                foreach (var m in abs.Where(o => o.MemberId != null).GroupBy(o => o.MemberId, StringComparer.Ordinal).Select(g => g.First()))
                {
                    if (!implMethods.TryGetValue(m.MemberName, out var targets)) continue;   // ereditato da una superclasse: non qui
                    var confidence = targets.Count == 1 && classConfidence == "extracted" ? "extracted" : "ambiguous";
                    foreach (var t in targets) edges.Add(Edge(m.MemberId, t, confidence, r));
                }
            }
            return edges.GroupBy(e => (e.SourceId, e.TargetId)).Select(g => g.First())
                .OrderBy(e => e.Path, StringComparer.Ordinal).ThenBy(e => e.Line).ThenBy(e => e.SourceId, StringComparer.Ordinal).ThenBy(e => e.TargetId, StringComparer.Ordinal)
                .ToList();
        }

        private static ScriptEdge Edge(string source, string target, string confidence, CodeRegistration r)
            => new() { SourceId = source, TargetId = target, Relation = Relation, Confidence = confidence, Path = r.Path, Line = r.Line };
    }
}
