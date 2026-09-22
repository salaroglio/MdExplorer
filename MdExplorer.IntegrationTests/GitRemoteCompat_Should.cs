using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.Git.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La rete di sicurezza dello sprint «un solo meccanismo di autenticazione git»
    /// (docs-internal/Sprints/2026-09-22-Git-Un-Solo-Meccanismo-Di-Autenticazione.md).
    /// <para>
    /// Fotografa ciò che i <b>progetti già collegati</b> e il <b>primo clone</b> fanno oggi,
    /// esercitando gli stessi endpoint che usa l'app Angular contro un server git HTTP vero
    /// con autenticazione Basic (<see cref="GitBasicAuthServer"/>). Deve restare verde prima,
    /// durante e dopo ogni fase: se un test qui cambia esito, è cambiato qualcosa per l'utente.
    /// </para>
    /// </summary>
    [TestClass]
    public class GitRemoteCompat_Should
    {
        private const string User = "carlo";
        private const string Password = "segreto-di-prova";
        private static readonly TimeSpan FailFast = TimeSpan.FromSeconds(30);

        // ---------------------------------------------------------------- progetti già collegati

        [TestMethod]
        public async Task ProgettoCollegato_CredenzialeNelHelper_PushRemoteStatusPullFetch()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            using var server = NewServer(ctx);
            var url = server.CreateBareRepository("collegato");
            StoreCredential(ctx, server);
            var path = SeedLinkedProject(ctx, url, "collegato");

            // push
            var push = await PostJson(ctx, "/api/ModernGit/push", new { repositoryPath = path, remoteName = "origin", branchName = "main" });
            Assert.IsTrue(push.Success, "push: " + push.Error);
            Assert.IsTrue(server.Unauthorized > 0, "il server deve aver chiesto le credenziali almeno una volta (401)");

            // remote-status
            var status = await GetJson(ctx, "/api/ModernGit/remote-status?repositoryPath=" + Uri.EscapeDataString(path));
            Assert.IsTrue(status.GetProperty("hasRemote").GetBoolean());
            Assert.IsTrue(status.GetProperty("canAuthenticate").GetBoolean(), "remote-status: " + status);

            // un collega pubblica un commit
            var collega = Path.Combine(ctx.Factory.DataDir, "work", "collega");
            Git(ctx.Factory.DataDir, "clone", "-q", server.UrlWithUser(url), collega);
            File.WriteAllText(Path.Combine(collega, "dal-collega.md"), "# ciao\n");
            Git(collega, "add", "-A"); Git(collega, "commit", "-qm", "dal collega"); Git(collega, "push", "-q");

            // get-data-to-pull (toolbar)
            var data = await GetJson(ctx, "/api/ModernGitToolbar/get-data-to-pull?projectPath=" + Uri.EscapeDataString(path));
            Assert.IsTrue(data.GetProperty("connectionIsActive").GetBoolean(), "get-data-to-pull: " + data);
            Assert.IsTrue(data.GetProperty("somethingIsToPull").GetBoolean(), "get-data-to-pull: " + data);

            // fetch, poi pull
            var fetch = await PostJson(ctx, "/api/ModernGit/fetch", new { repositoryPath = path, remoteName = "origin" });
            Assert.IsTrue(fetch.Success, "fetch: " + fetch.Error);
            var pull = await PostJson(ctx, "/api/ModernGit/pull", new { repositoryPath = path, remoteName = "origin", branchName = "main" });
            Assert.IsTrue(pull.Success, "pull: " + pull.Error);
            Assert.IsTrue(File.Exists(Path.Combine(path, "dal-collega.md")), "il file del collega deve arrivare col pull");
        }

        [TestMethod]
        public async Task ProgettoCollegato_DueAccountSulloStessoHost_UsaQuelloConfiguratoNelRepo()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            using var server = NewServer(ctx);
            var url = server.CreateBareRepository("due-account");
            // L'account sbagliato sta PRIMA nello store: senza sapere l'utente, git prenderebbe lui.
            File.WriteAllText(ctx.Factory.GitCredentialsFile,
                $"http://aziendale:password-aziendale@127.0.0.1:{server.Port}\n" +
                $"http://{User}:{Password}@127.0.0.1:{server.Port}\n");
            var path = SeedLinkedProject(ctx, url, "due-account");
            // Il modo nativo di dire a git quale account usare per questo host.
            Git(path, "config", $"credential.{server.BaseUrl}.username", User);

            var push = await PostJson(ctx, "/api/ModernGit/push", new { repositoryPath = path, remoteName = "origin", branchName = "main" });
            Assert.IsTrue(push.Success, "push con due account: " + push.Error);
        }

        [TestMethod]
        public async Task ProgettoCollegato_SenzaCredenziale_FallisceSubitoEDiceQualeHost()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            using var server = NewServer(ctx);
            var url = server.CreateBareRepository("senza-credenziale");
            var path = SeedLinkedProject(ctx, url, "senza-credenziale");

            var sw = Stopwatch.StartNew();
            var push = await PostJson(ctx, "/api/ModernGit/push", new { repositoryPath = path, remoteName = "origin", branchName = "main" });
            sw.Stop();
            Assert.IsFalse(push.Success, "senza credenziale il push NON deve riuscire");
            Assert.IsFalse(string.IsNullOrWhiteSpace(push.Error), "l'errore deve dire qualcosa");
            Assert.IsTrue(sw.Elapsed < FailFast, $"deve fallire subito, non dopo {sw.Elapsed}");
            var status = await GetJson(ctx, "/api/ModernGit/remote-status?repositoryPath=" + Uri.EscapeDataString(path));
            Assert.IsFalse(status.GetProperty("canAuthenticate").GetBoolean(), "remote-status deve dire che non si autentica: " + status);
        }

        [TestMethod]
        public async Task ProgettoCollegato_CredenzialeSoloNelDbDiMde_PushFunziona()
        {
            // Chi ha salvato utente e password dentro MdExplorer (dialogo account) e mai nel
            // credential helper di git: prima dello sprint ci pensa un resolver, dopo il trasloco.
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            using var server = NewServer(ctx);
            var url = server.CreateBareRepository("solo-db");
            var path = SeedLinkedProject(ctx, url, "solo-db");

            using (var scope = ctx.Factory.Services.CreateScope())
            {
                var accounts = scope.ServiceProvider.GetRequiredService<IGitAccountService>();
                await accounts.CreateAccountWithCredentialAsync(path, "Generic", "Server di prova", User,
                    httpsPassword: Password, preferredAuthMethod: "username_password");
            }

            // Il trasloco: all'avvio della versione nuova (qui a chiamata, perché il test spegne
            // l'hosted service) MdExplorer consegna il segreto a git e verifica che git lo ritrovi.
            var move = await PostJson(ctx, "/api/ModernGit/credential-move", new { });
            using (var doc = JsonDocument.Parse(move.Raw))
            {
                Assert.AreEqual(1, doc.RootElement.GetProperty("moved").GetInt32(), "trasloco: " + move.Raw);
                Assert.AreEqual(0, doc.RootElement.GetProperty("failed").GetInt32(), "trasloco: " + move.Raw);
            }
            var store = File.ReadAllText(ctx.Factory.GitCredentialsFile);
            // lo store scrive la porta come %3a: basta utente, password e host
            Assert.IsTrue(store.Contains($"{User}:{Password}@127.0.0.1"), "la credenziale deve essere nel helper di git: " + store);
            var (_, cfg) = Git(path, "config", "--get", $"credential.{server.BaseUrl}.username");
            Assert.AreEqual(User, cfg.Trim(), "il repo deve sapere quale utente usare per questo host");

            var push = await PostJson(ctx, "/api/ModernGit/push", new { repositoryPath = path, remoteName = "origin", branchName = "main" });
            Assert.IsTrue(push.Success, "push con credenziale solo nel DB: " + push.Error);

            // E il segreto in chiaro non è più nel DB di MdExplorer: rifare il trasloco non trova niente.
            var again = await PostJson(ctx, "/api/ModernGit/credential-move", new { });
            using (var doc = JsonDocument.Parse(again.Raw))
                Assert.IsTrue(doc.RootElement.GetProperty("nothingLeft").GetBoolean(), "secondo trasloco: " + again.Raw);
        }

        // ---------------------------------------------------------------- primo collegamento

        [TestMethod]
        public async Task PrimoCollegamento_RepoVuotoSulServer_SetupRemoteGenericCollegaEPubblica()
        {
            // Il caso d'uso dello sprint: progetto locale mai collegato, repository vuoto appena
            // creato sul server, credenziale nel credential helper. Dalla maschera «Setup Remote».
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            using var server = NewServer(ctx);
            var url = server.CreateBareRepository("carlo/appena-creato");
            StoreCredential(ctx, server);
            var (_, path) = ctx.SeedProject("mai-collegato");
            Git(path, "init", "-q", "-b", "main");
            File.WriteAllText(Path.Combine(path, "README.md"), "# mai collegato\n");
            Git(path, "add", "-A"); Git(path, "commit", "-qm", "primo");

            var setup = await PostJson(ctx, "/api/ModernGit/setup-remote-generic",
                new { repositoryPath = path, remoteUrl = url, remoteName = "origin", pushAfterAdd = true, saveCredentials = false });
            Assert.IsTrue(setup.Success, "setup-remote-generic: " + setup.Error + " | " + setup.Raw);
            Assert.IsTrue(setup.Raw.Contains("\"pushSucceeded\":true") || !setup.Raw.Contains("pushSucceeded"),
                "il primo push deve riuscire: " + setup.Raw);

            // Il server ha davvero ricevuto il commit, e da qui in poi il progetto è «collegato».
            var (_, refs) = Git(path, "ls-remote", "--heads", url);
            Assert.IsTrue(refs.Contains("refs/heads/main"), "sul server deve esserci main: " + refs + " | risposta del setup: " + setup.Raw);
            File.WriteAllText(Path.Combine(path, "secondo.md"), "# secondo\n");
            Git(path, "add", "-A"); Git(path, "commit", "-qm", "secondo");
            var push = await PostJson(ctx, "/api/ModernGit/push", new { repositoryPath = path, remoteName = "origin", branchName = "main" });
            Assert.IsTrue(push.Success, "il push successivo dalla toolbar: " + push.Error);
        }

        // ---------------------------------------------------------------- primo clone

        [TestMethod]
        public async Task PrimoClone_ConUtenteEPassword_ClonaESalvaLaCredenzialePerIlPushDopo()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            using var server = NewServer(ctx);
            var url = server.CreateBareRepository("da-clonare");
            SeedRemoteContent(ctx, server, url, "da-clonare");
            var localPath = Path.Combine(ctx.Factory.DataDir, "projects", "clonato");

            var clone = await PostJson(ctx, "/api/ModernGit/clone", new { url, localPath, useSavedToken = false, username = User, password = Password });
            Assert.IsTrue(clone.Success, "clone: " + clone.Error);
            Assert.IsTrue(File.Exists(Path.Combine(localPath, "README.md")), "il contenuto del remoto deve esserci");

            // Il progetto appena clonato è un progetto «collegato»: si lavora e si pubblica
            // senza ridigitare niente.
            File.WriteAllText(Path.Combine(localPath, "nuovo.md"), "# nuovo\n");
            Git(localPath, "add", "-A"); Git(localPath, "commit", "-qm", "dal clone");
            var push = await PostJson(ctx, "/api/ModernGit/push", new { repositoryPath = localPath, remoteName = "origin", branchName = "main" });
            Assert.IsTrue(push.Success, "push dopo il clone: " + push.Error);
        }

        [TestMethod]
        public async Task PrimoClone_CredenzialeGiaNelHelper_ClonaSenzaChiederla()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            using var server = NewServer(ctx);
            var url = server.CreateBareRepository("gia-noto");
            SeedRemoteContent(ctx, server, url, "gia-noto");
            StoreCredential(ctx, server);
            var localPath = Path.Combine(ctx.Factory.DataDir, "projects", "clonato-noto");

            var clone = await PostJson(ctx, "/api/ModernGit/clone", new { url, localPath, useSavedToken = true });
            Assert.IsTrue(clone.Success, "clone con credenziale nel helper: " + clone.Error);
            Assert.IsTrue(File.Exists(Path.Combine(localPath, "README.md")));
        }

        [TestMethod]
        public async Task PrimoClone_PasswordSbagliata_FallisceSubitoEInChiaro()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            using var server = NewServer(ctx);
            var url = server.CreateBareRepository("password-sbagliata");
            SeedRemoteContent(ctx, server, url, "password-sbagliata");
            var localPath = Path.Combine(ctx.Factory.DataDir, "projects", "clonato-male");

            var sw = Stopwatch.StartNew();
            var clone = await PostJson(ctx, "/api/ModernGit/clone", new { url, localPath, useSavedToken = false, username = User, password = "sbagliata" });
            sw.Stop();
            Assert.IsFalse(clone.Success, "con la password sbagliata il clone NON deve riuscire");
            Assert.IsFalse(string.IsNullOrWhiteSpace(clone.Error));
            Assert.IsTrue(sw.Elapsed < FailFast, $"deve fallire subito, non dopo {sw.Elapsed}");
            // E la password sbagliata NON deve restare nello store: al prossimo tentativo git la riuserebbe.
            var store = File.Exists(ctx.Factory.GitCredentialsFile) ? File.ReadAllText(ctx.Factory.GitCredentialsFile) : string.Empty;
            Assert.IsFalse(store.Contains("sbagliata"), "la password sbagliata è rimasta nel credential store: " + store);
        }

        [TestMethod]
        public async Task PrimoClone_ConSubmodule_PopolaAncheIlFiglio()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            using var server = NewServer(ctx);
            var childUrl = server.CreateBareRepository("figlio");
            SeedRemoteContent(ctx, server, childUrl, "figlio", "prodotto.md");
            var parentUrl = server.CreateBareRepository("padre");
            StoreCredential(ctx, server);

            // Il padre con dentro il figlio come submodule, pubblicato.
            var work = Path.Combine(ctx.Factory.DataDir, "work", "padre");
            Git(ctx.Factory.DataDir, "clone", "-q", parentUrl, work);
            File.WriteAllText(Path.Combine(work, "README.md"), "# padre\n");
            Git(work, "submodule", "add", childUrl, "figlio");
            Git(work, "add", "-A"); Git(work, "commit", "-qm", "padre con figlio"); Git(work, "push", "-q", "-u", "origin", "main");

            var localPath = Path.Combine(ctx.Factory.DataDir, "projects", "padre-clonato");
            var clone = await PostJson(ctx, "/api/ModernGit/clone", new { url = parentUrl, localPath, useSavedToken = true });
            Assert.IsTrue(clone.Success, "clone con submodule: " + clone.Error);
            Assert.IsTrue(File.Exists(Path.Combine(localPath, "figlio", "prodotto.md")), "il submodule deve essere popolato");
        }

        // ---------------------------------------------------------------- infrastruttura

        private static GitBasicAuthServer NewServer(AgentCityContext ctx)
            => new GitBasicAuthServer(Path.Combine(ctx.Factory.DataDir, "origins"), User, Password);

        private static void StoreCredential(AgentCityContext ctx, GitBasicAuthServer server)
            => File.AppendAllText(ctx.Factory.GitCredentialsFile, $"http://{User}:{Password}@127.0.0.1:{server.Port}\n");

        /// <summary>Un progetto locale con un commit e l'origin già impostato, come uno «staffato» tempo fa.</summary>
        private static string SeedLinkedProject(AgentCityContext ctx, string remoteUrl, string name)
        {
            var (_, path) = ctx.SeedProject(name);
            Git(path, "init", "-q", "-b", "main");
            File.WriteAllText(Path.Combine(path, "README.md"), $"# {name}\n");
            Git(path, "add", "-A"); Git(path, "commit", "-qm", "primo");
            Git(path, "remote", "add", "origin", remoteUrl);
            // Un progetto pubblicato tempo fa ha il branch che traccia origin/main (push -u).
            Git(path, "config", "branch.main.remote", "origin");
            Git(path, "config", "branch.main.merge", "refs/heads/main");
            return path;
        }

        /// <summary>Mette un commit nel remoto, così un clone ha qualcosa da portare a casa.</summary>
        private static void SeedRemoteContent(AgentCityContext ctx, GitBasicAuthServer server, string remoteUrl, string name, string file = "README.md")
        {
            var work = Path.Combine(ctx.Factory.DataDir, "work", name + "-seed");
            // Password nell'URL e credential helper SPENTO: git altrimenti la salverebbe nello store
            // al primo successo, e i test del clone passerebbero per merito del seed, non di MdExplorer.
            Git(ctx.Factory.DataDir, "-c", "credential.helper=", "clone", "-q", server.UrlWithCredentials(remoteUrl), work);
            File.WriteAllText(Path.Combine(work, file), $"# {name}\n");
            Git(work, "add", "-A"); Git(work, "commit", "-qm", "contenuto");
            Git(work, "-c", "credential.helper=", "push", "-q", "-u", "origin", "main");
        }

        private sealed record Outcome(bool Success, string Error, string Raw);

        private static async Task<Outcome> PostJson(AgentCityContext ctx, string route, object body)
        {
            var resp = await ctx.Client.PostAsJsonAsync(route, body);
            var raw = await resp.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var ok = doc.RootElement.TryGetProperty("success", out var s) && s.GetBoolean();
                var err = doc.RootElement.TryGetProperty("error", out var e) ? e.ToString() : (ok ? null : raw);
                return new Outcome(ok, err, raw);
            }
            catch (JsonException)
            {
                return new Outcome(false, raw, raw);
            }
        }

        private static async Task<JsonElement> GetJson(AgentCityContext ctx, string route)
        {
            var raw = await ctx.Client.GetStringAsync(route);
            return JsonDocument.Parse(raw).RootElement.Clone();
        }

        private static (int Code, string Out) Git(string cwd, params string[] args)
        {
            var psi = new ProcessStartInfo("git")
            {
                WorkingDirectory = cwd, UseShellExecute = false,
                RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true,
            };
            foreach (var a in args) psi.ArgumentList.Add(a);
            using var p = Process.Start(psi)!;
            var stdout = p.StandardOutput.ReadToEnd();
            var stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0)
                throw new InvalidOperationException($"git {string.Join(' ', args)} in {cwd} → exit {p.ExitCode}: {stderr}");
            return (p.ExitCode, stdout);
        }

        private static bool GitAvail()
        {
            try { return Git(Path.GetTempPath(), "--version").Code == 0; } catch { return false; }
        }
    }
}
