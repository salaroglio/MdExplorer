using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Features.Services.RemoteFolders;
using MdExplorer.RemoteProtocol;
using MdExplorer.Services.CodeGraph;
using MdExplorer.Services.Git;
using MdExplorer.Services.RemoteFolders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Cartelle remote via SSH, F2 (sprint 2026-10-21): diagnosi, caricamento e canale con il serverino VERO
    /// (<c>Binaries/remote/linux-x64/mde-remote</c>), su un trasporto che al posto di ssh lancia <c>sh -c</c> con una
    /// HOME temporanea. La prova con ssh vero sta nel laboratorio Docker del piano (Esiti di F2).
    /// </summary>
    [TestClass]
    public class RemoteFolders_Should
    {
        /// <summary>«ssh host comando» senza ssh: il comando gira qui, con la HOME del «server».</summary>
        private sealed class LocalShellTransport : IRemoteTransport
        {
            private readonly string _home;
            public int Commands;
            public LocalShellTransport(string home) { _home = home; }
            public ProcessStartInfo Command(string destination, string remoteCommand)
            {
                Commands++;
                var psi = SshTransport.Redirected("sh");
                psi.ArgumentList.Add("-c");
                psi.ArgumentList.Add(remoteCommand);
                psi.Environment["HOME"] = _home;
                return psi;
            }
        }

        /// <summary>Un ssh che risponde sempre allo stesso modo (account solo SFTP, accesso negato…).</summary>
        private sealed class CannedTransport : IRemoteTransport
        {
            private readonly string _stdout, _stderr; private readonly int _exit;
            public CannedTransport(string stdout, string stderr, int exit) { _stdout = stdout; _stderr = stderr; _exit = exit; }
            public ProcessStartInfo Command(string destination, string remoteCommand)
            {
                var psi = SshTransport.Redirected("sh");
                psi.ArgumentList.Add("-c");
                psi.ArgumentList.Add($"printf '%s' {RemoteShell.Quote(_stdout)}; printf '%s' {RemoteShell.Quote(_stderr)} >&2; exit {_exit}");
                return psi;
            }
        }

        private sealed class MemoryStore : ICodeRootBindingStore
        {
            public readonly Dictionary<string, string> Map = new();
            public string Get(string p, string a) => Map.TryGetValue(p + "|" + a, out var v) ? v : null;
            public void Set(string p, string a, string l) => Map[p + "|" + a] = l;
            public void Remove(string p, string a) => Map.Remove(p + "|" + a);
            public IReadOnlyCollection<string> DismissedReminders(string p) => new HashSet<string>();
            public void DismissReminders(string p, IEnumerable<string> aliases) { }
        }

        private string _base, _home, _project;
        private LocalShellTransport _transport;
        private RemoteSessions _sessions;
        private NativeGitRunner _git;

        [TestInitialize]
        public void Setup()
        {
            if (OperatingSystem.IsWindows()) Assert.Inconclusive("il serverino è un eseguibile Linux: si prova su Linux");
            _base = Path.Combine(Path.GetTempPath(), "mde-remotefolders", Guid.NewGuid().ToString("N"));
            _home = Path.Combine(_base, "home");
            _project = Path.Combine(_base, "doc");
            Directory.CreateDirectory(_home);
            Directory.CreateDirectory(_project);
            _transport = new LocalShellTransport(_home);
            _sessions = new RemoteSessions(_transport, () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), new MemoryRemoteServerSettings(allowAll: true), NullLogger<RemoteSessions>.Instance);
            _git = new NativeGitRunner(NullLogger<NativeGitRunner>.Instance);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _sessions?.Dispose();
            try { if (_base != null) Directory.Delete(_base, true); } catch { }
        }

        private RemoteLocation Server(string absolutePath) => RemoteLocation.Of("server", absolutePath);

        private async Task<string> CloneOf(string name, string remote)
        {
            var dir = Path.Combine(_base, name);
            Directory.CreateDirectory(Path.Combine(dir, "src", "Api"));
            File.WriteAllText(Path.Combine(dir, "src", "Api", "Foo.cs"), "class Foo {}");
            Assert.IsTrue((await _git.RunAsync(dir, new[] { "init", "-q" })).Ok);
            Assert.IsTrue((await _git.RunAsync(dir, new[] { "remote", "add", "origin", remote })).Ok);
            return dir;
        }

        private CodeRootService Roots(ICodeRootBindingStore store) => new(store, _git, _sessions, NullLogger<CodeRootService>.Instance);

        [TestMethod]
        public async Task Upload_the_agent_once_into_the_home_and_reuse_the_open_channel()
        {
            var folder = Path.Combine(_base, "code");
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "a.txt"), "uno");

            var channel = await _sessions.OpenAsync(Server(folder));
            var binary = RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath);
            var installed = Path.Combine(_home, ".cache", "mdexplorer", binary.Fingerprint, "mde-remote");
            Assert.AreEqual("~/.cache/mdexplorer/" + binary.Fingerprint, binary.InstallFolder(null).Display);
            Assert.IsTrue(File.Exists(installed), "caricato nella home, nella cartella con l'impronta del binario");
            Assert.AreEqual(binary.Size, new FileInfo(installed).Length);
            Assert.AreEqual(3, _transport.Commands, "porte + caricamento + avvio");
            Assert.AreEqual(folder, channel.Hello.Root);
            Assert.AreEqual("uno", (await channel.SendAsync(new RemoteRequest { Op = RemoteOps.Read, Path = "a.txt" })).Text);

            Assert.AreSame(channel, await _sessions.OpenAsync(Server(folder)), "il canale vivo si riusa");
            Assert.AreEqual(3, _transport.Commands);

            var stamp = File.GetLastWriteTimeUtc(installed);
            channel.Dispose();
            var again = await _sessions.OpenAsync(Server(folder));
            Assert.AreNotSame(channel, again, "un canale chiuso si riapre");
            Assert.AreEqual(4, _transport.Commands, "solo l'avvio: il server è già preparato in questa sessione");
            Assert.AreEqual(stamp, File.GetLastWriteTimeUtc(installed));
        }

        [TestMethod]
        public async Task Ask_consent_before_the_first_upload_and_never_upload_without_it()
        {
            var folder = Path.Combine(_base, "code");
            Directory.CreateDirectory(folder);
            var settings = new MemoryRemoteServerSettings();
            using var sessions = new RemoteSessions(_transport, () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), settings, NullLogger<RemoteSessions>.Instance);
            var ask = await Assert.ThrowsExceptionAsync<RemoteUploadConsentRequiredException>(() => sessions.OpenAsync(Server(folder)));
            Assert.AreEqual("server", ask.Destination);
            Assert.IsTrue(ask.Size > 1_000_000);
            StringAssert.StartsWith(ask.Folder, "~/.cache/mdexplorer/");
            Assert.AreEqual(0, Directory.GetFiles(_home, "mde-remote", SearchOption.AllDirectories).Length, "senza consenso niente arriva sul server");

            settings.AllowUpload("server");
            Assert.IsNotNull(await sessions.OpenAsync(Server(folder)));
            Assert.AreEqual(1, Directory.GetFiles(_home, "mde-remote", SearchOption.AllDirectories).Length);
        }

        [TestMethod]
        public async Task Put_the_agent_in_the_folder_chosen_for_the_server()
        {
            var settings = new MemoryRemoteServerSettings(allowAll: true);
            settings.SetAgentFolder("server", Path.Combine(_base, "opt", "mde"));
            using var sessions = new RemoteSessions(_transport, () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), settings, NullLogger<RemoteSessions>.Instance);
            var channel = await sessions.OpenAsync(Server(_base));
            Assert.IsNotNull(channel.Hello);
            Assert.AreEqual(1, Directory.GetFiles(Path.Combine(_base, "opt", "mde"), "mde-remote", SearchOption.AllDirectories).Length, "assoluta: lì");
            Assert.IsFalse(Directory.Exists(Path.Combine(_home, ".cache")), "non nella home");

            settings.SetAgentFolder("server", "bin/mde");
            Assert.AreEqual("~/bin/mde/" + RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath).Fingerprint,
                RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath).InstallFolder(settings.AgentFolder("server")).Display, "relativa: sotto la home");
            foreach (var bad in new[] { "~/x", "../x", "a/../../x", "a\nb" })
                Assert.ThrowsException<InvalidOperationException>(() => settings.SetAgentFolder("server", bad), bad);
        }

        [TestMethod]
        public async Task Browse_the_server_folders_from_the_home_to_choose_what_to_link()
        {
            Directory.CreateDirectory(Path.Combine(_home, "progetti", "backend"));
            Directory.CreateDirectory(Path.Combine(_home, "documenti"));
            File.WriteAllText(Path.Combine(_home, "nota.txt"), "x");
            var home = await _sessions.BrowseServerAsync("server", null);
            Assert.AreEqual(_home, home.Home);
            Assert.AreEqual(_home, home.Path);
            CollectionAssert.AreEqual(new[] { ".cache", "documenti", "progetti" }, home.Folders.Select(f => f.Name).ToArray(), "solo le cartelle, anche quelle nascoste");
            var inside = await _sessions.BrowseServerAsync("server", Path.Combine(_home, "progetti"));
            Assert.AreEqual("backend", inside.Folders.Single().Name);
        }

        /// <summary>
        /// Nello UserDB vero: consenso e cartella del serverino per PIÙ server. Setting.Name è UNIQUE — il 21/10 la riga del
        /// secondo server non entrava (prova dal vivo con lab-nx); le impostazioni in memoria non lo avrebbero visto.
        /// </summary>
        [TestMethod]
        public void Remember_consent_and_agent_folder_for_each_server_in_the_user_database()
        {
            using var ctx = new AgentCityContext();
            var settings = ctx.Factory.Services.GetRequiredService<IRemoteServerSettings>();
            settings.AllowUpload("build-server");
            settings.AllowUpload("lab-nx");
            settings.AllowUpload("lab-nx");
            settings.SetAgentFolder("build-server", "/opt/mde");
            settings.SetAgentFolder("lab-nx", "bin/mde/");
            Assert.IsTrue(settings.UploadAllowed("build-server"));
            Assert.IsTrue(settings.UploadAllowed("lab-nx"));
            Assert.IsFalse(settings.UploadAllowed("altro"));
            Assert.AreEqual("/opt/mde", settings.AgentFolder("build-server"));
            Assert.AreEqual("bin/mde", settings.AgentFolder("lab-nx"));
            settings.SetAgentFolder("lab-nx", "");
            Assert.IsNull(settings.AgentFolder("lab-nx"), "vuota = il default");
            Assert.AreEqual("/opt/mde", settings.AgentFolder("build-server"));
        }

        /// <summary>
        /// F4: gli strumenti remote_* (gruppo MCP «remote») si propongono accesi solo dove su QUESTA macchina c'è una radice
        /// collegata via ssh://: un progetto con radici solo locali non ne paga il peso nel contesto.
        /// </summary>
        [TestMethod]
        public void Propose_the_remote_tools_only_for_projects_with_a_remote_root_here()
        {
            using var ctx = new AgentCityContext();
            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
            var withRemote = Path.Combine(_base, "con-remota");
            var onlyLocal = Path.Combine(_base, "solo-locale");
            db.BeginTransaction();
            foreach (var p in new[] { withRemote, onlyLocal })
                Ad.Tools.Dal.Extensions.SessionExtensions.GetDal<MdExplorer.Abstractions.Entities.UserDB.Project>(db)
                    .Save(new MdExplorer.Abstractions.Entities.UserDB.Project { Name = Path.GetFileName(p), Path = p, LastUpdate = DateTime.UtcNow });
            var bindings = Ad.Tools.Dal.Extensions.SessionExtensions.GetDal<MdExplorer.Abstractions.Entities.UserDB.CodeRootBinding>(db);
            bindings.Save(new MdExplorer.Abstractions.Entities.UserDB.CodeRootBinding { ProjectPath = withRemote, Alias = "backend", LocalPath = "ssh://lab-dev/srv/proj", BoundAt = DateTime.UtcNow });
            bindings.Save(new MdExplorer.Abstractions.Entities.UserDB.CodeRootBinding { ProjectPath = onlyLocal, Alias = "backend", LocalPath = Path.Combine(_base, "code"), BoundAt = DateTime.UtcNow });
            Ad.Tools.Dal.Extensions.SessionExtensions.Commit(db);

            db.BeginTransaction();
            var projects = Ad.Tools.Dal.Extensions.SessionExtensions.GetDal<MdExplorer.Abstractions.Entities.UserDB.Project>(db).GetList().ToList();
            var remote = MdExplorer.Utilities.McpToolGroupsSettings.DefaultFor(projects.Single(x => x.Path == withRemote), db);
            var local = MdExplorer.Utilities.McpToolGroupsSettings.DefaultFor(projects.Single(x => x.Path == onlyLocal), db);
            Ad.Tools.Dal.Extensions.SessionExtensions.Commit(db);
            CollectionAssert.Contains(remote.ToList(), "remote");
            CollectionAssert.DoesNotContain(local.ToList(), "remote");
        }

        [TestMethod]
        public void Read_the_hosts_of_the_ssh_config_with_includes()
        {
            var ssh = Path.Combine(_base, "ssh");
            Directory.CreateDirectory(Path.Combine(ssh, "config.d"));
            File.WriteAllText(Path.Combine(ssh, "config"), "Host build-server dev-box\n  User dev\nHost *.internal !x\n  User y\nInclude " + Path.Combine(ssh, "config.d", "*") + "\n# Host commentato\n");
            File.WriteAllText(Path.Combine(ssh, "config.d", "lab"), "Host lab\n  HostName 10.0.0.5\n");
            CollectionAssert.AreEqual(new[] { "build-server", "dev-box", "lab" }, SshConfigHosts.Read(Path.Combine(ssh, "config")).ToArray());
            Assert.AreEqual(0, SshConfigHosts.Read(Path.Combine(ssh, "manca")).Count);
        }

        [TestMethod]
        public async Task Say_which_gate_is_closed()
        {
            var missing = await Assert.ThrowsExceptionAsync<RemoteUnavailableException>(() => _sessions.OpenAsync(Server(Path.Combine(_base, "manca"))));
            Assert.AreEqual(RemoteGate.Folder, missing.Gate);
            StringAssert.Contains(missing.Message, "manca");

            var sftpOnly = new RemoteSessions(new CannedTransport("This service allows sftp connections only.", "", 1), () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), new MemoryRemoteServerSettings(allowAll: true), NullLogger<RemoteSessions>.Instance);
            Assert.AreEqual(RemoteGate.Commands, (await Assert.ThrowsExceptionAsync<RemoteUnavailableException>(() => sftpOnly.OpenAsync(Server("/srv")))).Gate);

            var denied = new RemoteSessions(new CannedTransport("", "dev@10.0.0.5: Permission denied (publickey).", 255), () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), new MemoryRemoteServerSettings(allowAll: true), NullLogger<RemoteSessions>.Instance);
            var access = await Assert.ThrowsExceptionAsync<RemoteUnavailableException>(() => denied.OpenAsync(Server("/srv")));
            Assert.AreEqual(RemoteGate.Access, access.Gate);
            StringAssert.Contains(access.Message, "ssh-copy-id");

            var unknownHost = new RemoteSessions(new CannedTransport("", "Host key verification failed.", 255), () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), new MemoryRemoteServerSettings(allowAll: true), NullLogger<RemoteSessions>.Instance);
            StringAssert.Contains((await Assert.ThrowsExceptionAsync<RemoteUnavailableException>(() => unknownHost.OpenAsync(Server("/srv")))).Message, "known_hosts");

            var noAgent = new RemoteSessions(_transport, () => RemoteAgentBinary.Load(Path.Combine(_base, "non-c-e")), new MemoryRemoteServerSettings(allowAll: true), NullLogger<RemoteSessions>.Instance);
            StringAssert.Contains((await Assert.ThrowsExceptionAsync<RemoteUnavailableException>(() => noAgent.OpenAsync(Server(_base)))).Message, "publish-linux-x64.sh");
        }

        [TestMethod]
        public async Task Classify_the_start_failures_by_exit_code()
        {
            var at = Server("/srv/proj");
            Assert.AreEqual(RemoteGate.Execution, RemoteDiagnosis.StartFailure(at, 126, "bash: /home/nx/.cache/mdexplorer/x/mde-remote: Permission denied").Gate);
            StringAssert.Contains(RemoteDiagnosis.StartFailure(at, 126, "").Message, "noexec");
            Assert.AreEqual(RemoteGate.Folder, RemoteDiagnosis.StartFailure(at, 3, "root not found: /srv/proj").Gate);
            Assert.AreEqual(RemoteGate.Access, RemoteDiagnosis.StartFailure(at, 255, "Connection refused").Gate);
            Assert.AreEqual(RemoteGate.Channel, RemoteDiagnosis.StartFailure(at, 1, "boh").Gate);
            await Task.CompletedTask;
        }

        [TestMethod]
        public async Task Link_a_remote_clone_with_the_same_identity_rules_and_browse_it()
        {
            var clone = await CloneOf("server-clone", "https://github.com/acme/backend.git");
            var store = new MemoryStore();
            var roots = Roots(store);

            var linked = await roots.LinkAsync(_project, "backend", "ssh://server" + Path.Combine(clone, "src"));
            Assert.AreEqual(CodeRootStatus.Linked, linked.Status, linked.Reason);
            Assert.AreEqual("https://github.com/acme/backend.git", linked.Repository, "identità dal remote letto da .git sul server");
            Assert.AreEqual("src", linked.Subpath);
            Assert.AreEqual("ssh://server" + clone, store.Map.Values.Single(), "nello UserDB la radice del repository, come in locale");
            Assert.IsNull(linked.RootPath, "sul disco di questa macchina non c'è");
            Assert.AreEqual("ssh://server" + Path.Combine(clone, "src"), linked.Remote.ToString());

            var resolver = await roots.CreateResolverAsync(_project);
            Assert.AreEqual(0, resolver.Linked.Count(), "niente --add-dir né analisi per una radice remota");
            var files = new RemoteCodeRootFiles(_sessions);
            var top = await files.ListAsync(resolver.LinkedRemote.Single(), "", default);
            Assert.AreEqual("@backend/Api", top.Single().LogicalPath);
            var api = await files.ListAsync(resolver.LinkedRemote.Single(), resolver.RemoteOf("@backend/Api").Value.Relative, default);
            Assert.AreEqual("@backend/Api/Foo.cs", api.Single().LogicalPath);
            Assert.IsTrue(api.Single().IsText);
            Assert.AreEqual("class Foo {}", (await files.ReadAsync(resolver.LinkedRemote.Single(), "Api/Foo.cs", default)).Text);

            // un altro clone dello stesso repository, altrove sul server: stesso alias
            var other = await CloneOf("altro-clone", "git@github.com:acme/backend.git");
            var colleague = Roots(new MemoryStore());
            Assert.AreEqual(CodeRootStatus.Linked, (await colleague.LinkAsync(_project, "backend", "ssh://server" + other)).Status);

            // un clone di un altro repository: rifiutato col motivo
            var wrong = await CloneOf("sbagliato", "https://github.com/acme/frontend.git");
            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Roots(new MemoryStore()).LinkAsync(_project, "backend", "ssh://server" + wrong));
            StringAssert.Contains(ex.Message, "frontend");

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => roots.ChangesAsync(_project, "backend"), "file modificati: non ancora sulle radici remote");
        }

        /// <summary>
        /// Con ssh vero, nel laboratorio Docker del piano (docs-internal/Sprints/proofs/2026-10-21-remote-ssh-f0/sshlab):
        /// MDE_REMOTE_LAB_SSH_CONFIG = il suo ssh_config, MDE_REMOTE_LAB_PORT = la porta (2222). Senza, inconcludente.
        /// </summary>
        [TestMethod]
        public async Task Work_over_real_ssh_in_the_lab()
        {
            var config = Environment.GetEnvironmentVariable("MDE_REMOTE_LAB_SSH_CONFIG");
            var port = Environment.GetEnvironmentVariable("MDE_REMOTE_LAB_PORT");
            if (string.IsNullOrEmpty(config) || string.IsNullOrEmpty(port)) Assert.Inconclusive("laboratorio SSH non indicato");
            RemoteSessions Lab() => new(new SshTransport("ssh", new[] { "-F", config }), () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), new MemoryRemoteServerSettings(allowAll: true), NullLogger<RemoteSessions>.Instance);

            using var lab = Lab();
            var roots = new CodeRootService(new MemoryStore(), _git, lab, NullLogger<CodeRootService>.Instance);
            var linked = await roots.LinkAsync(_project, "proj", "ssh://lab-dev/srv/proj");
            Assert.AreEqual(CodeRootStatus.Linked, linked.Status, linked.Reason);
            var files = new RemoteCodeRootFiles(lab);
            var resolver = await roots.CreateResolverAsync(_project);
            Assert.AreEqual("@proj/src", (await files.ListAsync(resolver.LinkedRemote.Single(), "", default)).Single().LogicalPath);

            // con la porta: la forma URI di OpenSSH (utente e porta nel destinatario, chiave e known_hosts dal config)
            var byPort = await lab.OpenAsync(RemoteLocation.Parse($"ssh://dev@127.0.0.1:{port}/srv/proj"));
            Assert.AreEqual("/srv/proj", byPort.Hello.Root);

            Assert.AreEqual(RemoteGate.Execution, (await Assert.ThrowsExceptionAsync<RemoteUnavailableException>(() => lab.OpenAsync(RemoteLocation.Parse("ssh://lab-nx/srv/proj")))).Gate, "home noexec");
            Assert.AreEqual(RemoteGate.Commands, (await Assert.ThrowsExceptionAsync<RemoteUnavailableException>(() => lab.OpenAsync(RemoteLocation.Parse("ssh://lab-sftp/srv/proj")))).Gate, "solo SFTP");
            Assert.AreEqual(RemoteGate.Access, (await Assert.ThrowsExceptionAsync<RemoteUnavailableException>(() => lab.OpenAsync(RemoteLocation.Parse("ssh://lab-unknown/srv/proj")))).Gate, "host sconosciuto");
            Assert.AreEqual(RemoteGate.Folder, (await Assert.ThrowsExceptionAsync<RemoteUnavailableException>(() => lab.OpenAsync(RemoteLocation.Parse("ssh://lab-dev/srv/manca")))).Gate);
        }

        [TestMethod]
        public async Task Show_a_remote_root_waiting_for_consent_with_its_own_status()
        {
            var folder = Path.Combine(_base, "code");
            Directory.CreateDirectory(folder);
            var store = new MemoryStore();
            await Roots(store).LinkAsync(_project, "code", "ssh://server" + folder);

            // un'altra installazione (UserDB senza consenso) e un server su cui il serverino non c'è più
            Directory.Delete(Path.Combine(_home, ".cache"), true);
            using var noConsent = new RemoteSessions(_transport, () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), new MemoryRemoteServerSettings(), NullLogger<RemoteSessions>.Instance);
            var state = (await new CodeRootService(store, _git, noConsent, NullLogger<CodeRootService>.Instance).ListAsync(_project)).Single();
            Assert.AreEqual(CodeRootStatus.ConsentRequired, state.Status, "il server risponde: non è «non raggiungibile»");
            StringAssert.Contains(state.Reason, ".cache/mdexplorer");
        }

        [TestMethod]
        public async Task Show_an_unreachable_or_vanished_remote_root_with_its_reason()
        {
            var folder = Path.Combine(_base, "code");
            Directory.CreateDirectory(folder);
            var store = new MemoryStore();
            await Roots(store).LinkAsync(_project, "code", "ssh://server" + folder);

            _sessions.Dispose();
            Directory.Delete(folder, true);
            var gone = (await Roots(store).ListAsync(_project)).Single();
            Assert.AreEqual(CodeRootStatus.NotFound, gone.Status);
            StringAssert.Contains(gone.Reason, folder);

            var down = new CodeRootService(store, _git,
                new RemoteSessions(new CannedTransport("", "ssh: connect to host server port 22: Connection refused", 255), () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), new MemoryRemoteServerSettings(allowAll: true), NullLogger<RemoteSessions>.Instance),
                NullLogger<CodeRootService>.Instance);
            var state = (await down.ListAsync(_project)).Single();
            Assert.AreEqual(CodeRootStatus.Unreachable, state.Status);
            StringAssert.Contains(state.Reason, "Connection refused", "il messaggio di ssh resta nel motivo");
        }
    }
}
