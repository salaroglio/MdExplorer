using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Services.RemoteFolders;
using MdExplorer.RemoteProtocol;
using MdExplorer.Services.CodeGraph;
using MdExplorer.Services.Git;
using MdExplorer.Services.RemoteFolders;
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
            _sessions = new RemoteSessions(_transport, () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), NullLogger<RemoteSessions>.Instance);
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
            Assert.AreEqual(5, _transport.Commands, "porte + avvio: il serverino c'è già, non si ricarica");
            Assert.AreEqual(stamp, File.GetLastWriteTimeUtc(installed));
        }

        [TestMethod]
        public async Task Say_which_gate_is_closed()
        {
            var missing = await Assert.ThrowsExceptionAsync<RemoteUnavailableException>(() => _sessions.OpenAsync(Server(Path.Combine(_base, "manca"))));
            Assert.AreEqual(RemoteGate.Folder, missing.Gate);
            StringAssert.Contains(missing.Message, "manca");

            var sftpOnly = new RemoteSessions(new CannedTransport("This service allows sftp connections only.", "", 1), () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), NullLogger<RemoteSessions>.Instance);
            Assert.AreEqual(RemoteGate.Commands, (await Assert.ThrowsExceptionAsync<RemoteUnavailableException>(() => sftpOnly.OpenAsync(Server("/srv")))).Gate);

            var denied = new RemoteSessions(new CannedTransport("", "dev@10.0.0.5: Permission denied (publickey).", 255), () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), NullLogger<RemoteSessions>.Instance);
            var access = await Assert.ThrowsExceptionAsync<RemoteUnavailableException>(() => denied.OpenAsync(Server("/srv")));
            Assert.AreEqual(RemoteGate.Access, access.Gate);
            StringAssert.Contains(access.Message, "ssh-copy-id");

            var unknownHost = new RemoteSessions(new CannedTransport("", "Host key verification failed.", 255), () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), NullLogger<RemoteSessions>.Instance);
            StringAssert.Contains((await Assert.ThrowsExceptionAsync<RemoteUnavailableException>(() => unknownHost.OpenAsync(Server("/srv")))).Message, "known_hosts");

            var noAgent = new RemoteSessions(_transport, () => RemoteAgentBinary.Load(Path.Combine(_base, "non-c-e")), NullLogger<RemoteSessions>.Instance);
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
            RemoteSessions Lab() => new(new SshTransport("ssh", new[] { "-F", config }), () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), NullLogger<RemoteSessions>.Instance);

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
                new RemoteSessions(new CannedTransport("", "ssh: connect to host server port 22: Connection refused", 255), () => RemoteAgentBinary.Load(RemoteAgentBinary.DefaultLocalPath), NullLogger<RemoteSessions>.Instance),
                NullLogger<CodeRootService>.Instance);
            var state = (await down.ListAsync(_project)).Single();
            Assert.AreEqual(CodeRootStatus.Unreachable, state.Status);
            StringAssert.Contains(state.Reason, "Connection refused", "il messaggio di ssh resta nel motivo");
        }
    }
}
