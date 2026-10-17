using System;
using System.IO;
using System.Text;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>L'inverso di <see cref="CodeScriptParser"/>: scrive uno <see cref="CodeScript"/> come script Turtle del codice (schema 3).</summary>
    public static class CodeScriptWriter
    {
        public static string Serialize(CodeScript script) => CodeScriptTurtle.Write(script);

        /// <summary>Scrive lo script solo se il contenuto cambia: così il FileSystemWatcher non vede salvataggi a vuoto.</summary>
        public static bool WriteIfChanged(string absolutePath, string content)
        {
            if (File.Exists(absolutePath) && string.Equals(File.ReadAllText(absolutePath, Encoding.UTF8), content, StringComparison.Ordinal))
                return false;
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
            File.WriteAllText(absolutePath, content, new UTF8Encoding(false));
            return true;
        }

        /// <summary>
        /// Dove sta lo script AST (o doc) di un sorgente: <c>.mde-code/&lt;percorso logico&gt;.ast.ttl</c>. Il percorso logico
        /// di una radice esterna comincia con <c>@alias</c>: gli script restano dentro <c>.mde-code/@alias/…</c>.
        /// </summary>
        public static string ScriptPathFor(string projectPath, string sourceProjectRelative, CodeScriptKind kind)
            => Path.Combine(CodeGraphPaths.GetScriptsRoot(projectPath),
                sourceProjectRelative.Replace('/', Path.DirectorySeparatorChar) + (kind == CodeScriptKind.Ast ? CodeGraphPaths.AstScriptSuffix : CodeGraphPaths.DocScriptSuffix));
    }
}
