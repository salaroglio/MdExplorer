using System;
using System.Collections.Concurrent;
using System.IO;
using MdExplorer.Features.Agents.Workflow.Scheduler;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Dove vive il registro dei giri: sul ramo <c>mde/giri</c> del repository, mai nel ramo su cui lavora la persona. Il
    /// ramo ha una sua copia di lavoro nascosta dentro la cartella di git (non compare nell'albero dei file né tra le
    /// modifiche), e si pubblica su origin per conto suo. Ogni file del registro ha un solo computer che lo scrive (W16):
    /// due computer che pubblicano insieme si allineano con un rebase che non può andare in conflitto, e la copia della
    /// persona non viene mai toccata.
    /// </summary>
    public interface IRoundStore
    {
        /// <summary>Il progetto è un repository git: senza, non ci sono giri (il registro vive in git).</summary>
        bool IsAvailable(string projectPath);

        /// <summary>La radice da passare a <see cref="RoundLedger"/>: la copia di lavoro del ramo, creata se manca.</summary>
        string Root(string projectPath);

        /// <summary>Scarica le novità del registro da origin (se c'è) e ci si allinea.</summary>
        void Refresh(string projectPath);

        /// <summary>Committa la cartella del giro sul ramo e la pubblica. Null se fatto; altrimenti cosa non è andato.</summary>
        string Publish(string projectPath, string roundId, string message);
    }

    public class RoundStore : IRoundStore
    {
        public const string Branch = "mde/giri";
        private const string RemoteRef = "refs/remotes/origin/" + Branch;
        /// <summary>Il commit vuoto da cui nasce il ramo: l'albero vuoto di git.</summary>
        private const string EmptyTree = "4b825dc642cb6eb9a060e54bf8d69288fbee4904";

        private readonly ILogger<RoundStore> _logger;
        private readonly ConcurrentDictionary<string, string> _roots = new(StringComparer.OrdinalIgnoreCase);

        public RoundStore(ILogger<RoundStore> logger)
        {
            _logger = logger;
        }

        public bool IsAvailable(string projectPath)
            => _roots.ContainsKey(projectPath) || Git(projectPath, "rev-parse", "--git-dir").Code == 0;

        public string Root(string projectPath)
        {
            if (_roots.TryGetValue(projectPath, out var known) && File.Exists(Path.Combine(known, ".git")))
                return known;

            var common = Git(projectPath, "rev-parse", "--path-format=absolute", "--git-common-dir");
            if (common.Code != 0)
                throw new InvalidOperationException($"Il progetto non è un repository git, e il registro dei giri vive in git: {common.Err.Trim()}");
            var root = Path.Combine(common.Out.Trim(), "mde-giri");

            if (!File.Exists(Path.Combine(root, ".git")))
            {
                Git(projectPath, "worktree", "prune");
                if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
                if (HasRemote(projectPath)) Git(projectPath, "fetch", "--quiet", "origin", Branch);

                var hasLocal = Git(projectPath, "rev-parse", "--verify", "--quiet", "refs/heads/" + Branch).Code == 0;
                var hasRemote = Git(projectPath, "rev-parse", "--verify", "--quiet", RemoteRef).Code == 0;
                if (!hasLocal && !hasRemote)
                {
                    // Il ramo nasce da un commit vuoto: non ha niente in comune con il lavoro della persona.
                    var first = Git(projectPath, "commit-tree", EmptyTree, "-m", "registro dei giri del workflow");
                    if (first.Code != 0) throw new InvalidOperationException($"Il ramo {Branch} non si crea: {first.Err.Trim()}");
                    Must(projectPath, "branch", Branch, first.Out.Trim());
                }
                else if (!hasLocal)
                    Must(projectPath, "branch", Branch, RemoteRef);
                Must(projectPath, "worktree", "add", "--quiet", root, Branch);
                _logger.LogInformation("[Giri] registro dei giri di {Project} sul ramo {Branch} ({Root})", projectPath, Branch, root);
            }
            _roots[projectPath] = root;
            return root;
        }

        public void Refresh(string projectPath)
        {
            var root = Root(projectPath);
            if (!HasRemote(projectPath)) return;
            var fetch = Git(root, "fetch", "--quiet", "origin", Branch);
            if (fetch.Code != 0) return;   // il ramo su origin non c'è ancora: nasce con la prima pubblicazione
            Rebase(root);
        }

        public string Publish(string projectPath, string roundId, string message)
        {
            var root = Root(projectPath);
            var folder = RoundLedger.Folder + "/" + roundId;
            var add = Git(root, "add", "--", folder);
            if (add.Code != 0) return "git add: " + add.Err.Trim();
            var commit = Git(root, "commit", "--quiet", "-m", message, "--", folder);
            if (commit.Code != 0 && !(commit.Out + commit.Err).Contains("nothing to commit"))
                return "commit: " + (commit.Err.Trim().Length > 0 ? commit.Err.Trim() : commit.Out.Trim());
            if (!HasRemote(projectPath)) return null;

            // Git fa da arbitro: se un altro computer ha pubblicato prima, ci si allinea e si riprova.
            string last = null;
            for (var attempt = 0; attempt < 3; attempt++)
            {
                var push = Git(root, "push", "--quiet", "origin", Branch);
                if (push.Code == 0) return null;
                last = push.Err.Trim();
                if (Git(root, "fetch", "--quiet", "origin", Branch).Code != 0) break;
                var problem = Rebase(root);
                if (problem != null) return problem;
            }
            return "non pubblicato: " + last;
        }

        /// <summary>Porta i commit locali del registro sopra quelli di origin. Un conflitto vorrebbe dire due scrittori sullo stesso file: si annulla e si dice.</summary>
        private string Rebase(string root)
        {
            var rebase = Git(root, "rebase", "--quiet", "origin/" + Branch);
            if (rebase.Code == 0) return null;
            Git(root, "rebase", "--abort");
            var why = $"il registro dei giri non si allinea con origin ({rebase.Err.Trim()}): due computer hanno scritto lo stesso file";
            _logger.LogError("[Giri] {Why}", why);
            return why;
        }

        private static bool HasRemote(string projectPath) => Git(projectPath, "remote", "get-url", "origin").Code == 0;

        private static void Must(string cwd, params string[] args)
        {
            var r = Git(cwd, args);
            if (r.Code != 0) throw new InvalidOperationException($"git {string.Join(" ", args)}: {r.Err.Trim()}");
        }

        private static (int Code, string Out, string Err) Git(string cwd, params string[] args) => GitCli.Run(cwd, args);
    }
}
