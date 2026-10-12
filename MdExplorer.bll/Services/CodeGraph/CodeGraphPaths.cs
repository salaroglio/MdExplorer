using System;
using System.IO;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Dove stanno gli script del grafo del codice dentro il progetto di documentazione (D13):
    /// <c>&lt;radice&gt;/.mde-code/&lt;cartella program&gt;/&lt;file&gt;.ast.jsonl</c> e <c>.doc.jsonl</c>.
    /// Alla radice e non dentro la cartella del codice, perché quella può essere un repository a sé:
    /// gli script viaggiano con la documentazione.
    /// </summary>
    public static class CodeGraphPaths
    {
        public const string ScriptsFolderName = ".mde-code";
        /// <summary>Gli script sono ABox Turtle (schema 2, dal 12/10/2026).</summary>
        public const string AstScriptSuffix = ".ast.ttl";
        public const string DocScriptSuffix = ".doc.ttl";
        /// <summary>Il vecchio formato JSON Lines: si riconosce solo per convertirlo (<see cref="CodeScriptLegacy"/>).</summary>
        public const string LegacyAstSuffix = ".ast.jsonl";
        public const string LegacyDocSuffix = ".doc.jsonl";

        public static string GetScriptsRoot(string projectPath) => Path.Combine(projectPath, ScriptsFolderName);

        /// <summary>Vero per un file <c>*.ast.jsonl</c> o <c>*.doc.jsonl</c> che sta sotto una cartella <c>.mde-code</c>.</summary>
        public static bool IsCodeScriptFile(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return false;
            if (!IsAstScript(fullPath) && !IsDocScript(fullPath) && !IsLegacyScript(fullPath))
            {
                return false;
            }
            var dir = Path.GetDirectoryName(fullPath);
            while (!string.IsNullOrEmpty(dir))
            {
                if (string.Equals(Path.GetFileName(dir), ScriptsFolderName, StringComparison.OrdinalIgnoreCase)) return true;
                dir = Path.GetDirectoryName(dir);
            }
            return false;
        }

        public static bool IsAstScript(string path) => path.EndsWith(AstScriptSuffix, StringComparison.OrdinalIgnoreCase);
        public static bool IsDocScript(string path) => path.EndsWith(DocScriptSuffix, StringComparison.OrdinalIgnoreCase);
        public static bool IsLegacyScript(string path)
            => path.EndsWith(LegacyAstSuffix, StringComparison.OrdinalIgnoreCase) || path.EndsWith(LegacyDocSuffix, StringComparison.OrdinalIgnoreCase);

        /// <summary>Percorso relativo alla radice del progetto, con <c>/</c>: è la chiave con cui gli script si ricordano nel DB.</summary>
        public static string ToProjectRelative(string projectPath, string absolutePath)
            => Path.GetRelativePath(projectPath, absolutePath).Replace('\\', '/');
    }
}
