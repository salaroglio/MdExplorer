using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// L'API che usano l'editor dei file delle radici e i remote_* di MarkAgent (<c>POST api/coderoots/files</c>) dice il
    /// formato VERO del file (26/11/2026): un .bs che la regola vuole CP-850 ma che qualcuno ha riscritto in UTF-8 si legge
    /// come UTF-8 («è», non «├¿») e la risposta porta la regola; uno misto porta quante lettere UTF-8 ha dentro.
    /// La regola è scritta a mano nel .development.yml, senza punto e con «CP850»: vale lo stesso.
    /// </summary>
    [TestClass]
    public class CodeRootFileEncoding_Should
    {
        static CodeRootFileEncoding_Should() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        [TestMethod]
        public async Task Say_the_real_format_of_a_file_against_the_rule_of_the_root()
        {
            using var ctx = new AgentCityContext();
            var (_, project) = ctx.SeedProject("codifiche");
            var legacy = Path.Combine(Path.GetTempPath(), "mde-legacy-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(legacy);
            try
            {
                const string line = "      * Calcolo della quantità già consegnata: perché il saldo è più alto?\n";
                var cp850 = Encoding.GetEncoding("ibm850");
                File.WriteAllBytes(Path.Combine(legacy, "ORDINI.BS"), Encoding.UTF8.GetBytes(line));
                File.WriteAllBytes(Path.Combine(legacy, "GIUSTO.BS"), cp850.GetBytes(line));
                File.WriteAllBytes(Path.Combine(legacy, "MISTO.BS"), Concat(cp850.GetBytes(line), Encoding.UTF8.GetBytes("      * aggiunta: città\n")));
                File.AppendAllText(Path.Combine(project, ".development.yml"),
                    "codeRoots:\n- alias: legacy\n  encodings:\n  - extensions:\n    - BS\n    encoding: CP850\n");

                var (status, body) = await ctx.PostJson("/api/coderoots/link", JsonSerializer.Serialize(new { projectPath = project, alias = "legacy", localPath = legacy }));
                Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

                async Task<JsonElement> Read(string file)
                {
                    var (s, b) = await ctx.PostJson("/api/coderoots/files", JsonSerializer.Serialize(new { projectPath = project, op = "read", path = "@legacy/" + file }));
                    Assert.AreEqual(System.Net.HttpStatusCode.OK, s, b);
                    return JsonDocument.Parse(b).RootElement;
                }
                string Str(JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
                int Int(JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 0;

                var wrong = await Read("ORDINI.BS");
                Assert.AreEqual(("utf-8", "ibm850", line), (Str(wrong, "encoding"), Str(wrong, "ruleEncoding"), Str(wrong, "text")),
                    "due byte per lettera: si mostra UTF-8, e la regola voleva CP-850");
                var right = await Read("GIUSTO.BS");
                Assert.AreEqual(("ibm850", "ibm850", line, 0), (Str(right, "encoding"), Str(right, "ruleEncoding"), Str(right, "text"), Int(right, "utf8Letters")));
                var mixedRead = await Read("MISTO.BS");
                Assert.AreEqual(("ibm850", 1), (Str(mixedRead, "encoding"), Int(mixedRead, "utf8Letters")), "una lettera UTF-8 dentro un CP-850");
            }
            finally { Directory.Delete(legacy, true); }
        }

        private static byte[] Concat(byte[] a, byte[] b)
        {
            var all = new byte[a.Length + b.Length];
            a.CopyTo(all, 0);
            b.CopyTo(all, a.Length);
            return all;
        }
    }
}
