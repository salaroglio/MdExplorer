using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Il passaggio una tantum degli script JSON Lines (schema 1) ad ABox Turtle (schema 2), D21 del grafo / R4 delle radici
    /// esterne (12/10/2026). Ogni <c>.ast.jsonl</c>/<c>.doc.jsonl</c> sotto <c>.mde-code</c> si rilegge, si riscrive
    /// <c>.ttl</c> accanto e si cancella: le descrizioni scritte da Mark non si perdono, l'estrattore non cambia e quindi
    /// niente da riparsare. Uno script che non si legge resta com'è e lo si dice nel log (non si butta lavoro).
    /// </summary>
    public static class CodeScriptLegacy
    {
        /// <summary>Converte gli script del progetto; restituisce quanti ne ha convertiti.</summary>
        public static int ConvertProject(string projectPath, ILogger logger = null)
        {
            var root = CodeGraphPaths.GetScriptsRoot(projectPath);
            if (!Directory.Exists(root)) return 0;
            var converted = 0;
            foreach (var legacy in Directory.EnumerateFiles(root, "*.jsonl", SearchOption.AllDirectories).Where(CodeGraphPaths.IsLegacyScript).ToList())
            {
                var rel = CodeGraphPaths.ToProjectRelative(projectPath, legacy);
                try
                {
                    var target = legacy.Substring(0, legacy.Length - ".jsonl".Length) + ".ttl";
                    if (File.Exists(target))
                    {
                        // il .ttl l'ha già scritto un'analisi più recente: il vecchio non deve sovrascriverlo
                        File.Delete(legacy);
                        logger?.LogInformation("[CodeGraph] {Script}: c'è già il .ttl, il JSON Lines si toglie e basta", rel);
                        continue;
                    }
                    var script = CodeScriptParser.Parse(File.ReadAllText(legacy, Encoding.UTF8), rel);
                    CodeScriptWriter.WriteIfChanged(target, CodeScriptWriter.Serialize(script));
                    // la prova che il .ttl si rilegge, prima di cancellare l'originale
                    CodeScriptParser.Parse(File.ReadAllText(target, Encoding.UTF8), CodeGraphPaths.ToProjectRelative(projectPath, target));
                    File.Delete(legacy);
                    converted++;
                }
                catch (Exception ex)
                {
                    logger?.LogWarning("[CodeGraph] script JSON Lines non convertito, resta com'è: {Script} — {Why}", rel, ex.Message);
                }
            }
            if (converted > 0) logger?.LogInformation("[CodeGraph] '{Project}': {Count} script JSON Lines convertiti in Turtle", projectPath, converted);
            return converted;
        }
    }
}
