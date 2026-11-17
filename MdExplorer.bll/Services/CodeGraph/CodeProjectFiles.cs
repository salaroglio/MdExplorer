using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Il file di progetto a cui appartiene un sorgente (17/11/2026, utente: «un grafo di architettura» — la mappa fra unità
    /// ha bisogno di sapere di quale progetto è ogni file, e il grafo non lo scriveva). La regola è una sola per tutti i
    /// linguaggi: <b>il file di progetto più vicino risalendo le cartelle</b>, lo stesso che usano i compilatori e
    /// <see cref="DotNetProjectMap.Of"/>. Finisce in <c>code:project</c> del <c>code:File</c>, come percorso logico.
    /// <list type="bullet">
    /// <item>un sorgente Visual Basic o C# cerca SOLO i progetti del suo linguaggio (<c>.vbproj</c>, <c>.csproj</c>), il primo
    /// per nome nella cartella: è il progetto con cui l'estrattore lo ha compilato;</item>
    /// <item>gli altri file (pagine, cataloghi, Java, TypeScript, Python…) prendono dalla prima cartella che ha un file di
    /// progetto quello della propria famiglia, poi gli altri nell'ordine di <see cref="General"/>; due progetti .NET nella
    /// stessa cartella per un file che non è né VB né C# non si scelgono: nessun progetto.</item>
    /// </list>
    /// Stringa vuota = cercato e non trovato (o non sceglibile); null = non si è potuto cercare (la cartella non è su questo
    /// disco: una radice remota). Niente si indovina dal nome delle cartelle.
    /// </summary>
    public static class CodeProjectFiles
    {
        /// <summary>Scritto quando si è cercato e sopra il file non c'è nessun progetto.</summary>
        public const string None = "";
        /// <summary>Quanti livelli si risale al massimo, come <see cref="DotNetProjectMap.Of"/>.</summary>
        public const int MaxLevels = 12;

        private static readonly string[] DotNet = { ".csproj", ".vbproj", ".fsproj" };
        private static readonly string[] Jvm = { "pom.xml", "build.gradle.kts", "build.gradle" };
        private static readonly string[] Node = { "package.json" };
        private static readonly string[] Python = { "pyproject.toml", "setup.py" };
        /// <summary>L'ordine fra le famiglie quando la cartella ne ha più di una e il linguaggio non ne ha una sua.</summary>
        private static readonly string[][] General = { DotNet, Jvm, Node, Python };

        /// <summary>L'estensione del progetto di un linguaggio .NET che ha un solo tipo di progetto; null per gli altri.</summary>
        private static string OwnExtension(string languageId) => languageId switch
        {
            "visualbasic" => ".vbproj",
            "csharp" => ".csproj",
            _ => null,
        };

        private static string[] FamilyOf(string languageId) => languageId switch
        {
            "java" or "kotlin" => Jvm,
            "typescript" or "javascript" => Node,
            "python" => Python,
            _ => null,
        };

        /// <summary>
        /// Il progetto di un sorgente come percorso logico (<c>program/Web/Web.vbproj</c>, <c>@alias/Api/Api.csproj</c>; con
        /// <c>../</c> se sta sopra la radice del sorgente), <see cref="None"/> se non c'è, null se la cartella non è su disco.
        /// </summary>
        /// <param name="stopAt">Oltre questa cartella non si risale (la radice del progetto di documentazione); null = solo il limite dei livelli.</param>
        /// <param name="filesOf">I nomi dei file di una cartella (null se la cartella non c'è); null = dal disco.</param>
        public static string Of(CodeSourceFile file, string languageId, string stopAt, Func<string, IReadOnlyList<string>> filesOf = null)
        {
            if (file?.AbsolutePath == null || file.LogicalPath == null) return null;
            var start = Path.GetDirectoryName(Path.GetFullPath(file.AbsolutePath));
            filesOf ??= FilesOnDisk;
            if (filesOf(start) == null) return null;
            var stop = string.IsNullOrEmpty(stopAt) ? null : Path.GetFullPath(stopAt).TrimEnd(Path.DirectorySeparatorChar);
            var own = OwnExtension(languageId);
            var family = FamilyOf(languageId);
            var dir = start;
            for (var up = 0; dir != null && up < MaxLevels; up++)
            {
                var names = (filesOf(dir) ?? Array.Empty<string>()).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
                var undecided = false;
                var found = own != null ? names.FirstOrDefault(n => n.EndsWith(own, StringComparison.OrdinalIgnoreCase)) : Pick(names, family, out undecided);
                if (found != null) return Logical(file.LogicalPath, up, found);
                if (undecided) return None;
                if (stop != null && string.Equals(dir.TrimEnd(Path.DirectorySeparatorChar), stop, StringComparison.OrdinalIgnoreCase)) break;
                dir = Path.GetDirectoryName(dir);
            }
            return None;
        }

        /// <summary>Il progetto fra i file di una cartella; <paramref name="undecided"/> = ce n'è più d'uno .NET e non si sceglie.</summary>
        private static string Pick(List<string> names, string[] family, out bool undecided)
        {
            undecided = false;
            foreach (var markers in (family == null ? General : new[] { family }.Concat(General.Where(g => g != family))))
            {
                var hits = markers == DotNet
                    ? names.Where(n => DotNet.Any(x => n.EndsWith(x, StringComparison.OrdinalIgnoreCase))).ToList()
                    : markers.Select(m => names.FirstOrDefault(n => string.Equals(n, m, StringComparison.OrdinalIgnoreCase))).Where(n => n != null).Take(1).ToList();
                if (hits.Count == 1) return hits[0];
                if (hits.Count > 1) { undecided = true; return null; }
            }
            return null;
        }

        /// <summary>I nomi dei file di una cartella del disco; null se la cartella non c'è.</summary>
        public static IReadOnlyList<string> FilesOnDisk(string directory)
        {
            try { return Directory.Exists(directory) ? Directory.GetFiles(directory).Select(Path.GetFileName).ToList() : null; }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { return new List<string>(); }
        }

        /// <summary>Come <see cref="FilesOnDisk"/>, ma ogni cartella si legge una volta sola: per un'analisi intera.</summary>
        public static Func<string, IReadOnlyList<string>> CachedDisk()
        {
            var seen = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
            return dir => seen.TryGetValue(dir, out var names) ? names : seen[dir] = FilesOnDisk(dir);
        }

        /// <summary>Il percorso logico di un file che sta <paramref name="up"/> cartelle sopra quella del sorgente.</summary>
        private static string Logical(string sourceLogicalPath, int up, string fileName)
        {
            // una pagina scritta dentro un altro file sta nel file che la contiene
            var parts = CodeInlinePages.HostOf(sourceLogicalPath).Split('/').ToList();
            parts.RemoveAt(parts.Count - 1);
            for (var i = 0; i < up; i++)
            {
                // sopra una radice esterna (@alias) non c'è la radice del progetto: resta l'alias, con ../
                if (parts.Count > 0 && parts[^1] != ".." && !(parts.Count == 1 && parts[0].StartsWith("@", StringComparison.Ordinal))) parts.RemoveAt(parts.Count - 1);
                else parts.Add("..");
            }
            parts.Add(fileName);
            return string.Join("/", parts);
        }
    }
}
