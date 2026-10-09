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
        public const string AstScriptSuffix = ".ast.jsonl";
        public const string DocScriptSuffix = ".doc.jsonl";

        public static string GetScriptsRoot(string projectPath) => Path.Combine(projectPath, ScriptsFolderName);

        /// <summary>Vero per un file <c>*.ast.jsonl</c> o <c>*.doc.jsonl</c> che sta sotto una cartella <c>.mde-code</c>.</summary>
        public static bool IsCodeScriptFile(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return false;
            if (!fullPath.EndsWith(AstScriptSuffix, StringComparison.OrdinalIgnoreCase) &&
                !fullPath.EndsWith(DocScriptSuffix, StringComparison.OrdinalIgnoreCase))
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

        /// <summary>Percorso relativo alla radice del progetto, con <c>/</c>: è la chiave con cui gli script si ricordano nel DB.</summary>
        public static string ToProjectRelative(string projectPath, string absolutePath)
            => Path.GetRelativePath(projectPath, absolutePath).Replace('\\', '/');
    }
}
