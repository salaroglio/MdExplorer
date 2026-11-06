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
        /// <summary>Gli script sono Turtle (schema 3, dal 14/10/2026: il programma è nell'id dei simboli).</summary>
        public const string AstScriptSuffix = ".ast.ttl";
        public const string DocScriptSuffix = ".doc.ttl";
        /// <summary>
        /// Gli archi prodotti da una regola di arricchimento (D42, 23/10/2026), in <c>.mde-code/_rules/&lt;id&gt;.rules.ttl</c>
        /// accanto alla regola <c>&lt;id&gt;.rule.json</c>. Si caricano dopo gli ast: gli archi vanno fra simboli che ci sono già.
        /// </summary>
        public const string RuleScriptSuffix = ".rules.ttl";
        public const string RuleDefinitionSuffix = ".rule.json";
        public const string RulesFolderName = "_rules";
        /// <summary>Il vecchio formato JSON Lines: si riconosce solo per cancellarlo (<see cref="ICodeGraphStore.RetireOldScriptsAsync"/>).</summary>
        public const string LegacyAstSuffix = ".ast.jsonl";
        public const string LegacyDocSuffix = ".doc.jsonl";

        public static string GetScriptsRoot(string projectPath) => Path.Combine(projectPath, ScriptsFolderName);
        public static string GetRulesFolder(string projectPath) => Path.Combine(projectPath, ScriptsFolderName, RulesFolderName);

        /// <summary>Vero per un file <c>*.ast.jsonl</c> o <c>*.doc.jsonl</c> che sta sotto una cartella <c>.mde-code</c>.</summary>
        public static bool IsCodeScriptFile(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return false;
            if (!IsLoadableScript(fullPath) && !IsLegacyScript(fullPath))
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
        public static bool IsRuleScript(string path) => path.EndsWith(RuleScriptSuffix, StringComparison.OrdinalIgnoreCase);
        /// <summary>Uno script che il caricatore manda a Fuseki: ast, doc o regola.</summary>
        public static bool IsLoadableScript(string path) => IsAstScript(path) || IsDocScript(path) || IsRuleScript(path);
        public static CodeScriptKind KindOf(string path)
            => IsAstScript(path) ? CodeScriptKind.Ast : IsRuleScript(path) ? CodeScriptKind.Rule : CodeScriptKind.Doc;
        public static string KindName(CodeScriptKind kind) => kind switch { CodeScriptKind.Ast => "ast", CodeScriptKind.Rule => "rule", _ => "doc" };
        public static string SuffixOf(CodeScriptKind kind) => kind switch { CodeScriptKind.Ast => AstScriptSuffix, CodeScriptKind.Rule => RuleScriptSuffix, _ => DocScriptSuffix };
        public static bool IsLegacyScript(string path)
            => path.EndsWith(LegacyAstSuffix, StringComparison.OrdinalIgnoreCase) || path.EndsWith(LegacyDocSuffix, StringComparison.OrdinalIgnoreCase);

        /// <summary>Percorso relativo alla radice del progetto, con <c>/</c>: è la chiave con cui gli script si ricordano nel DB.</summary>
        public static string ToProjectRelative(string projectPath, string absolutePath)
            => Path.GetRelativePath(projectPath, absolutePath).Replace('\\', '/');
    }
}
