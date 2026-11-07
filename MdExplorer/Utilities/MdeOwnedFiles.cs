using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Utilities
{
    /// <summary>Come si risolve un conflitto su un file di MDE (sprint docs-internal/Sprints/2026-11-07-File-Di-Sistema-MDE.md).</summary>
    public enum MdeMergeKind
    {
        /// <summary>YAML unito voce per voce (<c>DevelopmentYamlMerge</c>).</summary>
        YamlEntries,
        /// <summary>File misto: di MDE è solo un blocco, il resto è del gruppo.</summary>
        Block,
    }

    /// <summary>Un file del repository che MDE scrive da sé.</summary>
    public sealed record MdeOwnedFile(string RelativePath, MdeMergeKind Kind, string Why);

    /// <summary>
    /// I file del repository che sono di MDE (S0 dello sprint «File di sistema di MDE», 07/11/2026): MDE li scrive da solo, e
    /// lasciati modificati senza commit bloccavano il pull (progetto A, progetto B). Da qui li prendono la scrittura
    /// (<see cref="MdeRepoWriter"/>), il commit automatico e la risoluzione dei conflitti nel pull.
    /// <para>
    /// NON ci sono: <c>.gitignore</c> (resta dell'utente: MDE può aggiungere righe ma lo notifica), le skill/agenti <c>mde-*</c> e
    /// <c>.vscode/mcp.json</c> (già esclusi da git con <c>.gitignore</c>, vedi <see cref="HarnessLayout.GitignorePatterns"/>).
    /// </para>
    /// </summary>
    public static class MdeOwnedFiles
    {
        public const string DevelopmentYml = ".development.yml";

        public static IReadOnlyList<MdeOwnedFile> All { get; } = BuildAll();

        private static IReadOnlyList<MdeOwnedFile> BuildAll()
        {
            var list = new List<MdeOwnedFile>
            {
                new(DevelopmentYml, MdeMergeKind.YamlEntries,
                    "la configurazione del progetto per il gruppo: cartelle, ambiente agentico, partecipanti, chat"),
            };
            // Il file di istruzioni di ogni ambiente agentico: di MDE è il paragrafo sulle sue skill.
            foreach (var layout in HarnessLayout.All)
                if (!string.IsNullOrWhiteSpace(layout.InstructionsFile) && list.All(f => f.RelativePath != layout.InstructionsFile))
                    list.Add(new(layout.InstructionsFile, MdeMergeKind.Block,
                        $"le istruzioni per {layout.Id}: MDE ne aggiorna il paragrafo sulle sue skill"));
            return list;
        }

        /// <summary>Il file di MDE a quel percorso (relativo alla radice del progetto, con / o \), o null.</summary>
        public static MdeOwnedFile Find(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return null;
            static string Norm(string p)
            {
                p = p.Replace('\\', '/');
                while (p.StartsWith("./")) p = p.Substring(2);
                return p.TrimStart('/');
            }
            var normalized = Norm(relativePath);
            return All.FirstOrDefault(f => string.Equals(Norm(f.RelativePath), normalized, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Il file di MDE per un percorso assoluto dentro il progetto, o null.</summary>
        public static MdeOwnedFile Find(string projectPath, string fullPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || string.IsNullOrWhiteSpace(fullPath)) return null;
            var relative = Path.GetRelativePath(projectPath, fullPath);
            return relative.StartsWith("..") ? null : Find(relative);
        }
    }
}
