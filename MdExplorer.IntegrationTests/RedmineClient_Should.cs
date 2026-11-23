using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.Redmine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il client di Redmine (docs-internal/Sprints/2026-10-27-Redmine.md). Le prove «dal vivo» girano contro un Redmine di
    /// prova (immagine ufficiale <c>redmine:6.0</c> con i dati inventati di F0) solo se ci sono <c>MDE_REDMINE_TEST_URL</c>,
    /// <c>MDE_REDMINE_TEST_READER_KEY</c> (utente in sola lettura) e <c>MDE_REDMINE_TEST_DEV_KEY</c> (sviluppatore);
    /// senza, sono inconcludenti. Le altre usano risposte finte e girano sempre.
    /// </summary>
    [TestClass]
    public class RedmineClient_Should
    {
        private const string Project = "banca-esempio";

        [TestInitialize]
        public void Setup() => UiText.Use(null);

        private static string Env(string name)
        {
            var v = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(v)) Assert.Inconclusive($"{name} non impostata: niente Redmine di prova.");
            return v;
        }

        private static RedmineConnection Reader() => new() { BaseUrl = Env("MDE_REDMINE_TEST_URL"), ApiKey = Env("MDE_REDMINE_TEST_READER_KEY") };
        private static RedmineConnection Dev() => new() { BaseUrl = Env("MDE_REDMINE_TEST_URL"), ApiKey = Env("MDE_REDMINE_TEST_DEV_KEY") };

        private static RedmineClient Client(HttpMessageHandler handler = null) => new(new Factory(handler ?? new HttpClientHandler()));

        [TestMethod]
        public async Task Say_who_the_key_belongs_to()
        {
            var me = await Client().VerifyAsync(Reader());
            Assert.AreEqual("lettore", me.Login);
        }

        [TestMethod]
        public async Task Find_issues_by_text_in_notes_too()
        {
            var found = await Client().SearchIssuesAsync(Reader(), new RedmineIssueQuery { Projects = { Project }, Text = "USD", Status = "*" });
            Assert.IsTrue(found.Any(i => i.Subject.StartsWith("Bonifico estero", StringComparison.Ordinal)), "USD sta in una nota della issue del bonifico");
        }

        [TestMethod]
        public async Task Read_an_issue_with_its_notes_and_custom_fields()
        {
            var first = (await Client().SearchIssuesAsync(Reader(), new RedmineIssueQuery { Projects = { Project }, Text = "Bonifico estero", Status = "*" })).First();
            var issue = await Client().GetIssueAsync(Reader(), first.Id);
            Assert.IsTrue(issue.Journals.Any(j => j.Notes?.Contains("USD") == true));
            Assert.IsTrue(issue.CustomFields.Any(c => c.Name == "Ambiente"));
        }

        [TestMethod]
        public async Task Refuse_to_create_for_a_read_only_user_before_calling_redmine()
        {
            var conn = Reader();   // fuori dalla lambda: senza Redmine di prova il test è inconcludente, non fallito
            var ex = await Assert.ThrowsExactlyAsync<RedmineApiException>(() =>
                Client().CreateIssueAsync(conn, new RedmineIssueChange { Project = Project, Tracker = "Segnalazione", Subject = "non deve nascere" }));
            StringAssert.Contains(ex.Message, "add_issues", "il permesso mancante, non il 422 fuorviante di Redmine");
            Assert.AreEqual(HttpStatusCode.Forbidden, ex.StatusCode);
        }

        [TestMethod]
        public async Task Report_the_required_custom_field_on_create()
        {
            var conn = Dev();   // fuori dalla lambda: senza Redmine di prova il test è inconcludente, non fallito
            var ex = await Assert.ThrowsExactlyAsync<RedmineApiException>(() =>
                Client().CreateIssueAsync(conn, new RedmineIssueChange { Project = Project, Tracker = "Segnalazione", Subject = "senza ambiente" }));
            StringAssert.Contains(ex.Message, "Ambiente");
        }

        [TestMethod]
        public async Task Create_then_update_with_a_note_and_a_new_status()
        {
            var created = await Client().CreateIssueAsync(Dev(), new RedmineIssueChange
            {
                Project = Project, Tracker = "Segnalazione", Subject = "Creata dal test " + Guid.NewGuid().ToString("N")[..6],
                Description = "Testo in **markdown**.", CustomFields = { ["Ambiente"] = "Collaudo" },
            });
            Assert.AreEqual("Collaudo", created.CustomFields.Single(c => c.Name == "Ambiente").Value);

            var updated = await Client().UpdateIssueAsync(Dev(), created.Id, new RedmineIssueChange { Status = "In elaborazione", Notes = "Presa in carico dal test.", AssignedTo = "me" });
            Assert.AreEqual("In elaborazione", updated.Status.Name);
            Assert.IsTrue(updated.Journals.Any(j => j.Notes == "Presa in carico dal test."));
            Assert.IsNotNull(updated.AssignedTo);
        }

        [TestMethod]
        public async Task Reject_a_status_that_is_not_allowed_instead_of_a_silent_204()
        {
            var any = (await Client().SearchIssuesAsync(Dev(), new RedmineIssueQuery { Projects = { Project }, Status = "open" })).First();
            var ex = await Assert.ThrowsExactlyAsync<RedmineApiException>(() =>
                Client().UpdateIssueAsync(Dev(), any.Id, new RedmineIssueChange { Status = "Inesistente" }));
            StringAssert.Contains(ex.Message, "Inesistente");
        }

        [TestMethod]
        public async Task Refuse_a_note_from_a_read_only_user()
        {
            var any = (await Client().SearchIssuesAsync(Reader(), new RedmineIssueQuery { Projects = { Project }, Status = "*" })).First();
            var ex = await Assert.ThrowsExactlyAsync<RedmineApiException>(() =>
                Client().UpdateIssueAsync(Reader(), any.Id, new RedmineIssueChange { Notes = "non deve arrivare" }));
            StringAssert.Contains(ex.Message, "add_issue_notes");
        }

        // ── Risposte finte: girano sempre ───────────────────────────

        /// <summary>Il caso reale del 27/10: un firewall davanti a Redmine risponde con una pagina HTML, la chiave non arriva.</summary>
        [TestMethod]
        public async Task Explain_a_firewall_page_instead_of_the_api()
        {
            var html = "<html><head><title>The URL you requested has been blocked</title></head><body>Attack ID: 12345<br>Message ID: 000012345678</body></html>";
            var client = Client(new Fake(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(html, Encoding.UTF8, "text/html") }));
            var ex = await Assert.ThrowsExactlyAsync<RedmineApiException>(() => client.VerifyAsync(new RedmineConnection { BaseUrl = "https://redmine.example.test", ApiKey = "k" }));
            StringAssert.Contains(ex.Message, "The URL you requested has been blocked");
            StringAssert.Contains(ex.Message, "Message ID: 000012345678");
            StringAssert.Contains(ex.Message, "X-Redmine-API-Key");
        }

        [TestMethod]
        public async Task Never_return_the_api_key_of_the_user()
        {
            var json = "{\"user\":{\"id\":5,\"login\":\"lettore\",\"firstname\":\"Lara\",\"lastname\":\"Lettore\",\"api_key\":\"segreta\"}}";
            var client = Client(new Fake(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") }));
            var me = await client.VerifyAsync(new RedmineConnection { BaseUrl = "https://redmine.example.test", ApiKey = "k" });
            Assert.IsFalse(System.Text.Json.JsonSerializer.Serialize(me).Contains("segreta"));
        }

        [TestMethod]
        public async Task Send_the_key_in_the_redmine_header()
        {
            string header = null, agent = null;
            var client = Client(new Fake(r =>
            {
                header = r.Headers.TryGetValues("X-Redmine-API-Key", out var v) ? v.Single() : null;
                agent = r.Headers.UserAgent.ToString();
                return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("") };
            }));
            var ex = await Assert.ThrowsExactlyAsync<RedmineApiException>(() => client.VerifyAsync(new RedmineConnection { BaseUrl = "https://redmine.example.test/", ApiKey = "abc" }));
            Assert.AreEqual("abc", header);
            StringAssert.StartsWith(agent, "MdExplorer", "MDE si presenta: gli amministratori lo riconoscono nei log");
            Assert.IsTrue(ex.IsAuthFailure);
        }

        private static readonly RedmineConnection Fictional = new() { BaseUrl = "https://redmine.example.test/", ApiKey = "abc" };

        [TestMethod]
        public async Task List_the_attachments_of_an_issue_with_the_id_that_downloads_them()
        {
            var json = "{\"issue\":{\"id\":7,\"subject\":\"Errore in stampa\",\"attachments\":[" +
                       "{\"id\":87,\"filename\":\"schermata.png\",\"filesize\":2048,\"content_type\":\"image/png\",\"description\":\"la maschera\"," +
                       "\"content_url\":\"http://interno.example.test/attachments/download/87/schermata.png\",\"author\":{\"id\":5,\"name\":\"Lara Lettore\"}}]}}";
            var client = Client(new Fake(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") }));
            var issue = await client.GetIssueAsync(Fictional, 7);
            var attachment = issue.Attachments.Single();
            Assert.AreEqual(87, attachment.Id);
            Assert.AreEqual("schermata.png", attachment.FileName);
            Assert.AreEqual("image/png", attachment.ContentType);
            Assert.AreEqual(2048L, attachment.FileSize);
            Assert.AreEqual("Lara Lettore", attachment.Author.Name);
        }

        [TestMethod]
        public async Task Download_an_attachment_from_the_configured_server_with_the_key()
        {
            var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 1, 2, 3 };
            string asked = null, key = null;
            var client = Client(new Fake(r =>
            {
                asked = r.RequestUri.AbsoluteUri;
                key = r.Headers.TryGetValues("X-Redmine-API-Key", out var v) ? v.Single() : null;
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            }));
            using var target = new System.IO.MemoryStream();
            var written = await client.DownloadAttachmentAsync(Fictional, new RedmineAttachment { Id = 87, FileName = "schermata di prova.png", ContentType = "image/png" }, target);

            // Dal server configurato, non dal content_url della risposta (che porta il nome interno di Redmine).
            Assert.AreEqual("https://redmine.example.test/attachments/download/87/schermata%20di%20prova.png", asked);
            Assert.AreEqual("abc", key);
            Assert.AreEqual(bytes.Length, written);
            CollectionAssert.AreEqual(bytes, target.ToArray());
        }

        [TestMethod]
        public async Task Refuse_a_web_page_served_in_place_of_an_image()
        {
            var html = "<html><head><title>Accesso negato</title></head><body>Request ID: 4411</body></html>";
            var client = Client(new Fake(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(html, Encoding.UTF8, "text/html") }));
            using var target = new System.IO.MemoryStream();
            var ex = await Assert.ThrowsExactlyAsync<RedmineApiException>(() =>
                client.DownloadAttachmentAsync(Fictional, new RedmineAttachment { Id = 87, FileName = "schermata.png", ContentType = "image/png" }, target));
            StringAssert.Contains(ex.Message, "Accesso negato");
            Assert.AreEqual(0, target.Length, "della pagina non si salva niente");
        }

        [TestMethod]
        public async Task Refuse_an_attachment_too_big_before_asking_for_it()
        {
            var asked = false;
            var client = Client(new Fake(_ => { asked = true; return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(new byte[1]) }; }));
            using var target = new System.IO.MemoryStream();
            var ex = await Assert.ThrowsExactlyAsync<RedmineApiException>(() =>
                client.DownloadAttachmentAsync(Fictional, new RedmineAttachment { Id = 9, FileName = "archivio.zip", FileSize = RedmineClient.MaxAttachmentBytes + 1 }, target));
            StringAssert.Contains(ex.Message, "archivio.zip");
            Assert.IsFalse(asked);
        }

        private sealed class Factory : IHttpClientFactory
        {
            private readonly HttpMessageHandler _handler;
            public Factory(HttpMessageHandler handler) => _handler = handler;
            public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
        }

        private sealed class Fake : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _answer;
            public Fake(Func<HttpRequestMessage, HttpResponseMessage> answer) => _answer = answer;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => Task.FromResult(_answer(request));
        }
    }
}
