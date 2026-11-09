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
        /// <summary>Copia di MDE da una sorgente che sta nel progetto (le skill dei template): si rifà dalla sorgente.</summary>
        Copy,
        /// <summary>
        /// Skill, agente o prompt <c>mde-*</c> distribuito da MDE, con <c>mde: version</c>: in un conflitto vince la versione più alta
        /// (09/11/2026, decisione dell'utente); chi ha un MDE più vecchio riceve l'invito ad aggiornarlo.
        /// </summary>
        Versioned,
    }

    /// <summary>Un file del repository che MDE scrive da sé.</summary>
    public sealed record MdeOwnedFile(string RelativePath, MdeMergeKind Kind, string Why);

    /// <summary>
    /// I file del repository che sono di MDE (S0 dello sprint «File di sistema di MDE», 07/11/2026): MDE li scrive da solo, e
    /// lasciati modificati senza commit bloccavano il pull (progetto A, progetto B). Da qui li prendono la scrittura
    /// (<see cref="MdeRepoWriter"/>), il commit automatico e la risoluzione dei conflitti nel pull.
    /// <para>
    /// NON ci sono: <c>.gitignore</c> (resta dell'utente: MDE può aggiungere righe ma lo notifica) e <c>.vscode/mcp.json</c> (escluso
    /// da git: ha i percorsi di questa macchina). Le skill, gli agenti e i prompt <c>mde-*</c> nelle cartelle dei motori ci sono dal
    /// 09/11/2026 (<see cref="MdeMergeKind.Versioned"/>): prima erano esclusi da git come «file di questa macchina».
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
            return All.FirstOrDefault(f => string.Equals(Norm(f.RelativePath), normalized, StringComparison.OrdinalIgnoreCase))
                   ?? (IsMdeAsset(normalized)
                       ? new MdeOwnedFile(normalized, MdeMergeKind.Versioned, "una skill, un agente o un prompt di MDE (mde-*)")
                       : null);
        }

        /// <summary>
        /// True se il percorso (relativo, con <c>/</c>) è una skill, un agente o un prompt <c>mde-*</c> nelle cartelle di un motore:
        /// <c>.github/skills/mde-doc/SKILL.md</c>, <c>.claude/agents/mde-skillcreator.md</c>, <c>.github/prompts/mde-code-doc.prompt.md</c>.
        /// </summary>
        public static bool IsMdeAsset(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return false;
            var p = relativePath.Replace('\\', '/').TrimStart('/');
            foreach (var layout in HarnessLayout.All)
                foreach (var folder in new[] { layout.SkillsFolder, layout.AgentsFolder, layout.PromptsFolder })
                    if (p.StartsWith(folder + "/", StringComparison.OrdinalIgnoreCase)
                        && p.Substring(folder.Length + 1).StartsWith("mde-", StringComparison.Ordinal))
                        return true;
            return false;
        }

        /// <summary>Il file di MDE per un percorso assoluto dentro il progetto, o null.</summary>
        public static MdeOwnedFile Find(string projectPath, string fullPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || string.IsNullOrWhiteSpace(fullPath)) return null;
            var relative = Path.GetRelativePath(projectPath, fullPath);
            if (relative.StartsWith("..")) return null;
            var found = Find(relative);
            if (found != null) return found;
            // La copia di una skill di un template (09/11/2026): MDE la mette nella cartella del motore e la toglie dalle altre.
            return MdExplorer.Services.Templates.TemplateSkillFolders.IsTemplateSkillPath(projectPath, relative)
                ? new MdeOwnedFile(relative.Replace('\\', '/'), MdeMergeKind.Copy, "la copia di una skill di un template nella cartella del motore del progetto")
                : null;
        }
    }
}
