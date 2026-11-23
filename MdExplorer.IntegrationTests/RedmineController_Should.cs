using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Redmine dal servizio (sprint 2026-10-27-Redmine): impostazioni divise fra .development.yml e UserDB, e le regole di
    /// MDE sulle scritture (RM9). Contro il Redmine di prova di <see cref="RedmineClient_Should"/>; senza, inconcludenti —
    /// tranne le impostazioni, che non chiamano Redmine.
    /// </summary>
    [TestClass]
    public class RedmineController_Should
    {
        private static string Env(string name)
        {
            var v = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(v)) Assert.Inconclusive($"{name} non impostata: niente Redmine di prova.");
            return v;
        }

        private static (AgentCityContext Ctx, Guid Id, string Path) Project()
        {
            var ctx = new AgentCityContext();
            var (id, path) = ctx.SeedProject("redmine-" + Guid.NewGuid().ToString("N")[..6]);
            File.WriteAllText(System.IO.Path.Combine(path, ".development.yml"), "folders: []\n");
            return (ctx, id, path);
        }

        private static async Task Save(AgentCityContext ctx, Guid id, string url, string key, string[] projects, string textFormatting)
        {
            var resp = await ctx.Client.PutAsJsonAsync($"/api/redmine/settings/{id}", new { enabled = true, baseUrl = url, apiKey = key, projects, textFormatting });
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode, await resp.Content.ReadAsStringAsync());
        }

        private static async Task<(HttpStatusCode Status, JsonElement Body)> Send(HttpResponseMessage resp)
            => (resp.StatusCode, JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync()));

        /// <summary>Un Redmine finto, in ascolto su una porta libera: una issue con un'immagine allegata.</summary>
        private static (HttpListener Server, string Url, byte[] Image) FakeRedmineWithAnImage()
        {
            var port = 0;
            using (var probe = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0)) { probe.Start(); port = ((IPEndPoint)probe.LocalEndpoint).Port; probe.Stop(); }
            var url = $"http://127.0.0.1:{port}/";
            var image = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4 };
            var server = new HttpListener();
            server.Prefixes.Add(url);
            server.Start();
            _ = Task.Run(async () =>
            {
                while (server.IsListening)
                {
                    HttpListenerContext c;
                    try { c = await server.GetContextAsync(); } catch { return; }
                    var path = c.Request.Url.AbsolutePath;
                    byte[] body; var type = "application/json";
                    if (c.Request.Headers["X-Redmine-API-Key"] != "chiave-di-prova") { c.Response.StatusCode = 401; body = Array.Empty<byte>(); }
                    else if (path == "/issues/7.json")
                        body = System.Text.Encoding.UTF8.GetBytes("{\"issue\":{\"id\":7,\"subject\":\"Errore in stampa\",\"project\":{\"id\":1,\"name\":\"Banca esempio\"}," +
                            "\"attachments\":[{\"id\":87,\"filename\":\"../schermata.png\",\"filesize\":12,\"content_type\":\"image/png\"}]}}");
                    else if (path.StartsWith("/attachments/download/87/", StringComparison.Ordinal)) { body = image; type = "image/png"; }
                    else { c.Response.StatusCode = 404; body = Array.Empty<byte>(); }
                    c.Response.ContentType = type;
                    c.Response.ContentLength64 = body.Length;
                    await c.Response.OutputStream.WriteAsync(body);
                    c.Response.Close();
                }
            });
            return (server, url, image);
        }

        [TestMethod]
        public async Task Download_an_attachment_of_an_issue_into_the_project_so_that_the_llm_can_open_it()
        {
            var (server, url, image) = FakeRedmineWithAnImage();
            var (ctx, id, path) = Project();
            using (ctx)
            try
            {
                await Save(ctx, id, url, "chiave-di-prova", new[] { "banca-esempio" }, "markdown");

                var (status, body) = await Send(await ctx.Client.PostAsJsonAsync("/api/redmine/issue/7/attachment/87/download", new { projectId = id }));
                Assert.AreEqual(HttpStatusCode.OK, status, body.ToString());
                Assert.AreEqual("image/png", body.GetProperty("mimeType").GetString());
                // Il nome viene da Redmine: non deve poter uscire dalla cartella degli allegati.
                Assert.AreEqual(".md/redmine-attachments/7/87_schermata.png", body.GetProperty("relativePath").GetString());
                var saved = body.GetProperty("path").GetString();
                CollectionAssert.AreEqual(image, File.ReadAllBytes(saved));
                Assert.AreEqual(image.Length, body.GetProperty("size").GetInt64());

                // Un allegato che non è di quella issue non si scarica, e la risposta dice quali ci sono.
                var (missing, why) = await Send(await ctx.Client.PostAsJsonAsync("/api/redmine/issue/7/attachment/99/download", new { projectId = id }));
                Assert.AreEqual(HttpStatusCode.BadRequest, missing);
                Assert.AreEqual(87, why.GetProperty("attachments")[0].GetProperty("id").GetInt32());
            }
            finally { server.Stop(); }
        }

        [TestMethod]
        public async Task Keep_the_key_out_of_the_development_file_and_never_send_it_back()
        {
            var (ctx, id, path) = Project();
            using (ctx)
            {
                await Save(ctx, id, "https://redmine.example.test/", "chiave-segreta-123", new[] { "banca-esempio" }, "markdown");
                var yml = File.ReadAllText(System.IO.Path.Combine(path, ".development.yml"));
                StringAssert.Contains(yml, "redmine:");
                StringAssert.Contains(yml, "https://redmine.example.test");
                StringAssert.Contains(yml, "banca-esempio");
                Assert.IsFalse(yml.Contains("chiave-segreta-123"), "la chiave non va su Git");

                var settings = await ctx.Client.GetStringAsync($"/api/redmine/settings/{id}");
                Assert.IsFalse(settings.Contains("chiave-segreta-123"), "la chiave non torna al client");
                StringAssert.Contains(settings, "\"hasKey\":true");
            }
        }

        [TestMethod]
        public async Task Refuse_an_unknown_text_formatting()
        {
            var (ctx, id, _) = Project();
            using (ctx)
            {
                var resp = await ctx.Client.PutAsJsonAsync($"/api/redmine/settings/{id}", new { enabled = true, baseUrl = "https://r.example.test", textFormatting = "html" });
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        }

        [TestMethod]
        public async Task Read_and_write_through_the_service()
        {
            var url = Env("MDE_REDMINE_TEST_URL");
            var (ctx, id, _) = Project();
            using (ctx)
            {
                await Save(ctx, id, url, Env("MDE_REDMINE_TEST_DEV_KEY"), new[] { "banca-esempio" }, "markdown");

                var test = await Send(await ctx.Client.PostAsJsonAsync("/api/redmine/test-connection", new { projectId = id }));
                Assert.IsTrue(test.Body.GetProperty("success").GetBoolean(), test.Body.ToString());

                var found = await Send(await ctx.Client.GetAsync($"/api/redmine/issues?projectId={id}&text=valuta&status=*"));
                Assert.AreEqual(HttpStatusCode.OK, found.Status, found.Body.ToString());
                Assert.IsTrue(found.Body.GetProperty("count").GetInt32() >= 1);

                var created = await Send(await ctx.Client.PostAsJsonAsync("/api/redmine/issue", new
                {
                    projectId = id,
                    change = new { project = "banca-esempio", tracker = "Supporto", subject = "Dal servizio", customFields = new { Ambiente = "Produzione" } },
                }));
                Assert.AreEqual(HttpStatusCode.OK, created.Status, created.Body.ToString());
                var issueId = created.Body.GetProperty("issue").GetProperty("id").GetInt32();

                var noted = await Send(await ctx.Client.PutAsJsonAsync($"/api/redmine/issue/{issueId}", new { projectId = id, change = new { notes = "Nota dal servizio." } }));
                Assert.AreEqual(HttpStatusCode.OK, noted.Status, noted.Body.ToString());
                Assert.AreEqual("markdown", noted.Body.GetProperty("textFormatting").GetString());
            }
        }

        [TestMethod]
        public async Task Write_only_to_the_configured_redmine_projects()
        {
            var url = Env("MDE_REDMINE_TEST_URL");
            var (ctx, id, _) = Project();
            using (ctx)
            {
                // Lo sviluppatore può scrivere in «portale-clienti» per Redmine, ma il progetto MDE elenca solo «banca-esempio».
                await Save(ctx, id, url, Env("MDE_REDMINE_TEST_DEV_KEY"), new[] { "banca-esempio" }, "markdown");
                var refused = await Send(await ctx.Client.PostAsJsonAsync("/api/redmine/issue", new
                {
                    projectId = id,
                    change = new { project = "portale-clienti", tracker = "Supporto", subject = "Non deve nascere", customFields = new { Ambiente = "Collaudo" } },
                }));
                Assert.AreEqual(HttpStatusCode.BadRequest, refused.Status);
                StringAssert.Contains(refused.Body.GetProperty("error").GetString(), "portale-clienti");

                var options = await Send(await ctx.Client.GetAsync($"/api/redmine/project-options?projectId={id}&project=portale-clienti"));
                Assert.IsFalse(options.Body.GetProperty("writable").GetBoolean());
            }
        }

        [TestMethod]
        public async Task Require_the_text_formatting_before_writing_text()
        {
            var url = Env("MDE_REDMINE_TEST_URL");
            var (ctx, id, _) = Project();
            using (ctx)
            {
                await Save(ctx, id, url, Env("MDE_REDMINE_TEST_DEV_KEY"), new[] { "banca-esempio" }, null);
                var refused = await Send(await ctx.Client.PutAsJsonAsync("/api/redmine/issue/1", new { projectId = id, change = new { notes = "senza formato" } }));
                Assert.AreEqual(HttpStatusCode.BadRequest, refused.Status);
                StringAssert.Contains(refused.Body.GetProperty("error").GetString(), "markdown");
            }
        }
    }
}
