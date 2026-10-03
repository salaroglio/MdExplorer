using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.Git;
using MdExplorer.Services.Git.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il <b>flusso di lavoro fra due sviluppatori</b>, repository per repository.
    /// <para>
    /// Le azioni dei pannelli git (pubblica, scarica, aggiorna all'ultima, allinea, annulla
    /// l'unione, cambio ramo) sono incastrate in sei regole. Ogni test mette due persone sullo
    /// stesso progetto con un submodule e verifica una regola dai due lati: l'azione sbagliata
    /// viene <b>rifiutata prima di toccare qualcosa</b>, e quella giusta porta a uno stato che
    /// l'interfaccia sa nominare.
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class RepoWorkflow_Should
    {
        // ---- niente torna indietro da solo ----

        [TestMethod]
        public async Task Leave_a_submodule_that_is_ahead_where_it_is_when_the_project_is_pulled()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, _, parentOrigin) = Setup(ctx, "flusso-avanti");
            var (collega, _) = Colleague(ctx, "flusso-avanti", parentOrigin);

            // Io: un commit nel figlio, non ancora registrato nel progetto.
            File.WriteAllText(Path.Combine(sub, "mio.md"), "# mio\n");
            Git(sub, "add -A"); Git(sub, "commit -m mio");
            var mine = Head(sub);

            // Il collega pubblica una modifica al solo progetto.
            File.WriteAllText(Path.Combine(collega, "README.md"), "# doc\ndel collega\n");
            Git(collega, "commit -am collega"); Git(collega, "push -q origin main");

            var pulled = await Pull(ctx, path);
            Assert.IsTrue(pulled.Success, pulled.ErrorMessage);

            // Prima il pull faceva 'submodule update' alla cieca: il figlio tornava al commit
            // registrato, con HEAD staccato, e il file appena committato spariva dalla cartella.
            Assert.AreEqual(mine, Head(sub), "un figlio più avanti del registrato non si riporta indietro.");
            Assert.AreEqual("main", Branch(sub), "e resta sul suo ramo.");
            Assert.IsTrue(File.Exists(Path.Combine(sub, "mio.md")));

            var view = await View(ctx, path);
            Assert.AreEqual(SubmoduleRelation.Ahead, view.Repos.Single(r => r.Path == "figlio").Relation);
            CollectionAssert.Contains(view.Repos[0].PointersToRegister.ToList(), "figlio",
                "la versione nuova è lavoro del progetto: è lì che si registra.");
        }

        [TestMethod]
        public async Task Move_a_submodule_forward_on_its_branch_when_the_project_brings_a_newer_version()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, _, parentOrigin) = Setup(ctx, "flusso-segue");
            var (collega, suo) = Colleague(ctx, "flusso-segue", parentOrigin);
            var theirs = ColleaguePublishesChildAndProject(collega, suo, "v2");

            var pulled = await Pull(ctx, path);
            Assert.IsTrue(pulled.Success, pulled.ErrorMessage);

            Assert.AreEqual(theirs, Head(sub), "il figlio segue la versione che il progetto registra.");
            Assert.AreEqual("main", Branch(sub), "avanzando sul ramo: HEAD non si stacca.");

            var view = await View(ctx, path);
            Assert.AreEqual(SubmoduleRelation.Same, view.Repos.Single(r => r.Path == "figlio").Relation);
            Assert.AreEqual(0, view.Repos[0].PointersToRegister.Count);
        }

        [TestMethod]
        public async Task Block_the_project_commit_while_a_submodule_is_behind_the_recorded_version()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, _, parentOrigin) = Setup(ctx, "flusso-indietro");
            var (collega, suo) = Colleague(ctx, "flusso-indietro", parentOrigin);
            var theirs = ColleaguePublishesChildAndProject(collega, suo, "v2");

            // Uno scaricamento fatto FUORI da MdExplorer: il progetto registra v2, il figlio è a v1.
            // git segna «commit cambiato» con la stessa sigla di quando sei tu ad averlo spostato.
            Git(path, "pull -q --no-rebase");
            Assert.AreNotEqual(theirs, Head(sub));

            var view = await View(ctx, path);
            var child = view.Repos.Single(r => r.Path == "figlio");
            Assert.IsTrue(child.Relation is SubmoduleRelation.Behind or SubmoduleRelation.Unknown, child.Relation);
            Assert.AreEqual(0, view.Repos[0].PointersToRegister.Count, "non c'è una versione nuova da registrare: c'è da allineare.");
            Assert.IsNotNull(view.Repos[0].CommitBlocker);
            StringAssert.Contains(view.Repos[0].CommitBlocker, "figlio");

            // Il commit lo rifiuta anche il servizio, non solo il pulsante spento: committando si
            // registrerebbe di nuovo v1, annullando il lavoro del collega.
            var commit = await Commit(ctx, path, "non deve passare");
            Assert.IsFalse(commit.Success);
            StringAssert.Contains(commit.ErrorMessage, "figlio");

            var aligned = await Sync(ctx, s => s.AlignAsync(path, "figlio"));
            Assert.IsNull(aligned.Refused, aligned.Refused);
            Assert.IsTrue(aligned.Success, aligned.Message);
            Assert.IsTrue(aligned.ContentChanged, "i file del figlio sono cambiati: chi li ha aperti va avvisato.");
            CollectionAssert.Contains(aligned.ChangedFiles.ToList(), "figlio/codice.md");

            Assert.AreEqual(theirs, Head(sub));
            Assert.AreEqual("main", Branch(sub));
            Assert.IsNull((await View(ctx, path)).Repos[0].CommitBlocker);
        }

        // ---- il submodule in cui hai lavorato si sincronizza per primo ----

        [TestMethod]
        public async Task Refuse_the_project_pull_when_both_sides_moved_the_same_submodule_and_show_the_way_out()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, _, parentOrigin) = Setup(ctx, "flusso-entrambi");
            var (collega, suo) = Colleague(ctx, "flusso-entrambi", parentOrigin);
            ColleaguePublishesChildAndProject(collega, suo, "del collega", file: "collega.md");

            // Io: un commit nel figlio E la sua registrazione nel progetto.
            File.WriteAllText(Path.Combine(sub, "mio.md"), "# mio\n");
            Git(sub, "add -A"); Git(sub, "commit -m mio");
            Assert.IsTrue((await Commit(ctx, path, "registra il mio figlio")).Success);

            await Sync(ctx, s => s.FetchAllAsync(path));

            // Scaricare il progetto adesso darebbe «CONFLICT (submodule)»: un'unione a metà su una
            // cartella. Si rifiuta prima, dicendo l'ordine giusto.
            var root = (await View(ctx, path)).Repos[0];
            Assert.IsNotNull(root.PullBlocker, "il blocco si legge sulla riga, prima di premere.");
            StringAssert.Contains(root.PullBlocker, "figlio");

            var refused = await Sync(ctx, s => s.PullAsync(path, ""));
            Assert.IsNotNull(refused.Refused);
            Assert.AreNotEqual(0, Git(path, "rev-parse -q --verify MERGE_HEAD").Code, "niente unione a metà.");

            // L'ordine giusto: prima dentro il figlio (git unisce i due lavori)…
            var updated = await Sync(ctx, s => s.PullAsync(path, "figlio"));
            Assert.IsTrue(updated.Success, updated.Message ?? updated.Refused);
            Assert.IsTrue(File.Exists(Path.Combine(sub, "collega.md")) && File.Exists(Path.Combine(sub, "mio.md")));

            // …poi si registra la versione unita nel progetto…
            root = (await View(ctx, path)).Repos[0];
            CollectionAssert.Contains(root.PointersToRegister.ToList(), "figlio");
            Assert.IsTrue((await Commit(ctx, path, "registra la versione unita")).Success);

            // …e solo adesso il progetto si scarica, senza conflitti.
            root = (await View(ctx, path)).Repos[0];
            Assert.IsNull(root.PullBlocker, root.PullBlocker);
            var pulled = await Sync(ctx, s => s.PullAsync(path, ""));
            Assert.IsTrue(pulled.Success, pulled.Message ?? pulled.Refused);
            Assert.AreNotEqual(0, Git(path, "rev-parse -q --verify MERGE_HEAD").Code);

            var after = await View(ctx, path);
            Assert.AreEqual(SubmoduleRelation.Same, after.Repos.Single(r => r.Path == "figlio").Relation);
            Assert.IsNull(after.Repos[0].CommitBlocker);
        }

        [TestMethod]
        public async Task Say_that_a_submodule_diverges_and_let_update_to_latest_join_the_two()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, _, parentOrigin) = Setup(ctx, "flusso-diverge");
            var (collega, suo) = Colleague(ctx, "flusso-diverge", parentOrigin);
            ColleaguePublishesChildAndProject(collega, suo, "del collega", file: "collega.md");

            // Io: un commit nel figlio, NON registrato nel progetto.
            File.WriteAllText(Path.Combine(sub, "mio.md"), "# mio\n");
            Git(sub, "add -A"); Git(sub, "commit -m mio");
            var mine = Head(sub);

            var pulled = await Sync(ctx, s => s.PullAsync(path, ""));
            Assert.IsTrue(pulled.Success, pulled.Message ?? pulled.Refused);
            Assert.IsTrue(pulled.Warnings.Any(w => w.Contains("figlio")), "un figlio lasciato dov'era va detto.");
            Assert.AreEqual(mine, Head(sub), "il mio lavoro non si sposta da solo.");

            var view = await View(ctx, path);
            Assert.AreEqual(SubmoduleRelation.Diverged, view.Repos.Single(r => r.Path == "figlio").Relation);
            Assert.IsNotNull(view.Repos[0].CommitBlocker, "committare ora scarterebbe la versione del collega.");

            var updated = await Sync(ctx, s => s.PullAsync(path, "figlio"));
            Assert.IsTrue(updated.Success, updated.Message ?? updated.Refused);

            view = await View(ctx, path);
            Assert.AreEqual(SubmoduleRelation.Ahead, view.Repos.Single(r => r.Path == "figlio").Relation);
            Assert.IsNull(view.Repos[0].CommitBlocker);
            CollectionAssert.Contains(view.Repos[0].PointersToRegister.ToList(), "figlio");
        }

        // ---- cosa è disponibile lo sa solo il remoto del submodule ----

        [TestMethod]
        public async Task Know_that_a_submodule_has_updates_only_after_asking_its_own_remote()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, _, parentOrigin) = Setup(ctx, "flusso-disponibili");
            var (_, suo) = Colleague(ctx, "flusso-disponibili", parentOrigin);

            // Il collega pubblica nel figlio SENZA registrarlo nel progetto.
            File.WriteAllText(Path.Combine(suo, "codice.md"), "# codice\nv2\n");
            Git(suo, "commit -am v2"); Git(suo, "push -q origin main");
            var theirs = Head(suo);

            // Il fetch del progetto non aggiorna i riferimenti del figlio: senza chiedere al SUO
            // remoto, il figlio risulta allineato a torto.
            Git(path, "fetch -q origin");
            Assert.AreEqual(0, (await View(ctx, path)).Repos.Single(r => r.Path == "figlio").Behind);

            var outcomes = await Sync(ctx, s => s.FetchAllAsync(path));
            Assert.IsTrue(outcomes.All(o => o.Ok), string.Join(" | ", outcomes.Select(o => o.Repo + ": " + o.Error)));

            var child = (await View(ctx, path)).Repos.Single(r => r.Path == "figlio");
            Assert.AreEqual(1, child.Behind, "ora si sa che c'è una versione nuova disponibile.");
            Assert.IsNull(child.PullBlocker, child.PullBlocker);

            // «Aggiorna all'ultima» scarica e basta: registrare la versione nuova è un passo a parte.
            var updated = await Sync(ctx, s => s.PullAsync(path, "figlio"));
            Assert.IsTrue(updated.Success, updated.Message ?? updated.Refused);
            Assert.AreEqual(theirs, Head(sub));
            CollectionAssert.Contains(updated.ChangedFiles.ToList(), "figlio/codice.md");

            var view = await View(ctx, path);
            Assert.AreEqual(SubmoduleRelation.Ahead, view.Repos.Single(r => r.Path == "figlio").Relation);
            CollectionAssert.Contains(view.Repos[0].PointersToRegister.ToList(), "figlio");
            Assert.AreEqual(string.Empty, Git(path, "log origin/main..HEAD --oneline").Out.Trim(),
                "nessun commit fatto al posto dell'utente.");
        }

        // ---- si pubblica dal basso, e prima si scarica ----

        [TestMethod]
        public async Task Refuse_to_publish_the_project_before_the_submodule_it_points_to()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, _, parentOrigin) = Setup(ctx, "flusso-dal-basso");

            File.WriteAllText(Path.Combine(sub, "codice.md"), "# codice\nv2\n");
            Git(sub, "commit -am v2");
            Assert.IsTrue((await Commit(ctx, path, "registra v2")).Success);

            var prima = Git(parentOrigin, "rev-parse refs/heads/main").Out.Trim();

            var root = (await View(ctx, path)).Repos[0];
            Assert.IsNotNull(root.PushBlocker);
            StringAssert.Contains(root.PushBlocker, "figlio");

            var refused = await Sync(ctx, s => s.PushAsync(path, null, ""));
            Assert.IsNotNull(refused.Refused, "pubblicare il progetto ora romperebbe il repository per chi clona.");
            Assert.AreEqual(prima, Git(parentOrigin, "rev-parse refs/heads/main").Out.Trim(), "il remoto non è stato toccato.");

            // Il figlio si può sempre pubblicare; dopo, il progetto si sblocca.
            var child = await Sync(ctx, s => s.PushAsync(path, null, "figlio"));
            Assert.IsTrue(child.Success, child.Message ?? child.Refused);
            Assert.IsNull((await View(ctx, path)).Repos[0].PushBlocker);

            var project = await Sync(ctx, s => s.PushAsync(path, null, ""));
            Assert.IsTrue(project.Success, project.Message ?? project.Refused);

            // La prova vera: un altro riesce a clonare.
            var clone = Path.Combine(ctx.Factory.DataDir, "clone-dal-basso");
            Git(ctx.Factory.DataDir, $"clone -q \"{parentOrigin}\" \"{clone}\"");
            Assert.AreEqual(0, Git(clone, "submodule update --init").Code);
            StringAssert.Contains(File.ReadAllText(Path.Combine(clone, "figlio", "codice.md")), "v2");
        }

        [TestMethod]
        public async Task Refuse_to_publish_a_repository_that_is_also_behind()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, _, _, parentOrigin) = Setup(ctx, "flusso-prima-scarica");
            var (collega, _) = Colleague(ctx, "flusso-prima-scarica", parentOrigin);

            File.WriteAllText(Path.Combine(collega, "collega.md"), "# collega\n");
            Git(collega, "add -A"); Git(collega, "commit -m collega"); Git(collega, "push -q origin main");

            File.WriteAllText(Path.Combine(path, "mio.md"), "# mio\n");
            Assert.IsTrue((await Commit(ctx, path, "mio")).Success);
            await Sync(ctx, s => s.FetchAllAsync(path));

            var refused = await Sync(ctx, s => s.PushAsync(path, null, ""));
            Assert.IsNotNull(refused.Refused);
            StringAssert.Contains(refused.Refused, "scarica");

            // Scaricato (file diversi: git unisce da solo), si pubblica.
            Assert.IsTrue((await Sync(ctx, s => s.PullAsync(path, ""))).Success);
            var pushed = await Sync(ctx, s => s.PushAsync(path, null, ""));
            Assert.IsTrue(pushed.Success, pushed.Message ?? pushed.Refused);
        }

        // ---- ogni stato a metà è visibile e ha un'uscita ----

        [TestMethod]
        public async Task Show_a_half_done_merge_and_undo_it()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, _, _, parentOrigin) = Setup(ctx, "flusso-annulla");
            var (collega, _) = Colleague(ctx, "flusso-annulla", parentOrigin);
            SameLineOnBothSides(ctx, path, collega);

            var pulled = await Sync(ctx, s => s.PullAsync(path, ""));
            Assert.IsFalse(pulled.Success);
            Assert.IsNull(pulled.Refused);
            StringAssert.Contains(pulled.Message, "unione");

            var root = (await View(ctx, path)).Repos[0];
            Assert.IsTrue(root.MergeInProgress, "lo stato a metà si vede sulla riga.");
            CollectionAssert.Contains(root.Conflicts.ToList(), "README.md");
            Assert.IsNotNull(root.CommitBlocker, "il file ha ancora i segni di conflitto.");
            Assert.IsNotNull(root.PushBlocker);
            Assert.IsNotNull(root.PullBlocker);

            var undone = await Sync(ctx, s => s.AbortMergeAsync(path, ""));
            Assert.IsTrue(undone.Success, undone.Message ?? undone.Refused);

            root = (await View(ctx, path)).Repos[0];
            Assert.IsFalse(root.MergeInProgress);
            StringAssert.Contains(File.ReadAllText(Path.Combine(path, "README.md")), "riga mia");
            Assert.AreEqual(1, root.Ahead);
            Assert.AreEqual(1, root.Behind, "si è tornati esattamente a prima dello scaricamento.");
        }

        [TestMethod]
        public async Task Close_a_half_done_merge_with_a_commit_once_the_file_is_fixed()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, _, _, parentOrigin) = Setup(ctx, "flusso-risolve");
            var (collega, _) = Colleague(ctx, "flusso-risolve", parentOrigin);
            SameLineOnBothSides(ctx, path, collega);

            await Sync(ctx, s => s.PullAsync(path, ""));

            // Coi segni di conflitto ancora dentro il commit non parte: lo stage di tutto
            // segnerebbe il file come risolto e i segni finirebbero nel repository.
            var early = await Commit(ctx, path, "troppo presto");
            Assert.IsFalse(early.Success);
            StringAssert.Contains(early.ErrorMessage, "README.md");

            File.WriteAllText(Path.Combine(path, "README.md"), "# doc\nriga mia e del collega\n");
            Assert.IsNull((await View(ctx, path)).Repos[0].CommitBlocker);

            var closed = await Commit(ctx, path, "unisce i due lavori");
            Assert.IsTrue(closed.Success, closed.ErrorMessage);
            Assert.AreNotEqual(0, Git(path, "rev-parse -q --verify MERGE_HEAD").Code, "l'unione è conclusa.");
            Assert.AreEqual(2, Git(path, "rev-list --parents -n 1 HEAD").Out.Trim().Split(' ').Length - 1,
                "il commit ha due genitori: è l'unione, non un commit qualunque.");

            var root = (await View(ctx, path)).Repos[0];
            Assert.IsFalse(root.MergeInProgress);
            Assert.AreEqual(0, root.Behind);
            Assert.IsNull(root.PushBlocker, root.PushBlocker);
        }

        // ---- si cambia ramo solo da pulito ----

        [TestMethod]
        public async Task Change_branch_only_from_a_clean_state_and_bring_submodules_to_the_new_branch_version()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, _, _) = Setup(ctx, "flusso-ramo");
            var v1 = Head(sub);

            // 'vecchia' registra il figlio a v1; 'main' lo porta a v2.
            Git(path, "branch vecchia");
            File.WriteAllText(Path.Combine(sub, "codice.md"), "# codice\nv2\n");
            Git(sub, "commit -am v2"); Git(sub, "push -q origin main");
            var v2 = Head(sub);
            Assert.IsTrue((await Commit(ctx, path, "registra v2")).Success);

            // Con lavoro non salvato nel figlio il cambio non parte.
            File.WriteAllText(Path.Combine(sub, "codice.md"), "# codice\nbozza\n");
            var refused = await Checkout(ctx, path, "vecchia");
            Assert.IsFalse(refused.Success);
            StringAssert.Contains(refused.ErrorMessage, "figlio");
            Assert.AreEqual("main", Branch(path), "il ramo non è cambiato.");
            Git(sub, "checkout -- codice.md");

            var switched = await Checkout(ctx, path, "vecchia");
            Assert.IsTrue(switched.Success, switched.ErrorMessage);

            // Prima il figlio restava a v2 e il progetto lo segnalava come «da committare»:
            // committando si registrava su 'vecchia' una versione che quel ramo non aveva scelto.
            Assert.AreEqual(v1, Head(sub), "il figlio va alla versione del ramo nuovo.");
            var view = await View(ctx, path);
            Assert.AreEqual(SubmoduleRelation.Same, view.Repos.Single(r => r.Path == "figlio").Relation);
            Assert.AreEqual(0, view.Repos[0].PointersToRegister.Count);
            Assert.AreEqual(0, view.Repos[0].Uncommitted.Count);

            // A v1 il figlio ha HEAD staccato (il suo ramo sta a v2): lo stato dice dove si può tornare.
            var child = view.Repos.Single(r => r.Path == "figlio");
            Assert.IsTrue(child.Detached);
            Assert.AreEqual("main", child.DetachedTarget);
            Assert.AreEqual(1, child.Behind);

            var back = await Checkout(ctx, path, "main");
            Assert.IsTrue(back.Success, back.ErrorMessage);
            Assert.AreEqual(v2, Head(sub));
            Assert.AreEqual("main", Branch(sub), "tornato a v2, dove sta il suo ramo, il figlio è di nuovo sul ramo.");
        }

        [TestMethod]
        public async Task Not_start_a_merge_as_a_side_effect_of_changing_branch()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, _, _, parentOrigin) = Setup(ctx, "flusso-ramo-unione");
            var (collega, _) = Colleague(ctx, "flusso-ramo-unione", parentOrigin);

            // 'main' ha un commit mio e uno del collega sulla stessa riga: divergono.
            SameLineOnBothSides(ctx, path, collega);
            Git(path, "branch altra HEAD~1");
            Assert.IsTrue((await Checkout(ctx, path, "altra")).Success);

            // Fino all'01/10/2026 tornare su 'main' faceva un pull: qui lasciava un'unione a metà,
            // in silenzio, come effetto collaterale di un cambio di ramo (visto nell'app vera).
            var back = await Checkout(ctx, path, "main");
            Assert.IsTrue(back.Success, back.ErrorMessage);
            Assert.AreNotEqual(0, Git(path, "rev-parse -q --verify MERGE_HEAD").Code, "cambiare ramo non unisce niente.");
            StringAssert.Contains(File.ReadAllText(Path.Combine(path, "README.md")), "riga mia");

            var root = (await View(ctx, path)).Repos[0];
            Assert.IsFalse(root.MergeInProgress);
            Assert.AreEqual(1, root.Ahead);
            Assert.AreEqual(1, root.Behind, "ciò che il remoto ha in più resta da scaricare: lo decide chi lavora.");
        }

        [TestMethod]
        public async Task Bring_a_branch_up_to_date_when_changing_to_it_if_it_only_has_to_move_forward()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, _, _, parentOrigin) = Setup(ctx, "flusso-ramo-avanti");
            var (collega, _) = Colleague(ctx, "flusso-ramo-avanti", parentOrigin);

            Git(path, "branch altra");
            Assert.IsTrue((await Checkout(ctx, path, "altra")).Success);

            File.WriteAllText(Path.Combine(collega, "README.md"), "# doc\nriga del collega\n");
            Git(collega, "commit -am collega"); Git(collega, "push -q origin main");

            // Nessun commit mio su 'main': tornandoci lo si porta in cima, senza unire niente.
            var back = await Checkout(ctx, path, "main");
            Assert.IsTrue(back.Success, back.ErrorMessage);
            StringAssert.Contains(File.ReadAllText(Path.Combine(path, "README.md")), "riga del collega");
            Assert.AreEqual(0, (await View(ctx, path)).Repos[0].Behind);
        }

        [TestMethod]
        public async Task Put_a_detached_submodule_back_on_its_branch_with_update_to_latest()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, _, _) = Setup(ctx, "flusso-staccato");

            File.WriteAllText(Path.Combine(sub, "codice.md"), "# codice\nv2\n");
            Git(sub, "commit -am v2"); Git(sub, "push -q origin main");
            var v2 = Head(sub);
            // Il figlio torna, staccato, alla versione che il progetto registra ancora (v1).
            Git(path, "submodule update -q");
            Assert.AreEqual("HEAD", Branch(sub));

            var child = (await View(ctx, path)).Repos.Single(r => r.Path == "figlio");
            Assert.IsNotNull(child.CommitBlocker, "con HEAD staccato non si committa.");
            Assert.IsNull(child.PullBlocker, "ma c'è un'uscita: tornare sul ramo.");
            Assert.AreEqual("main", child.DetachedTarget);

            var updated = await Sync(ctx, s => s.PullAsync(path, "figlio"));
            Assert.IsTrue(updated.Success, updated.Message ?? updated.Refused);
            Assert.AreEqual("main", Branch(sub));
            Assert.AreEqual(v2, Head(sub));
            Assert.IsNull((await View(ctx, path)).Repos.Single(r => r.Path == "figlio").CommitBlocker);
        }

        // ---- infrastruttura ----

        private static async Task<WorkingChangesView> View(AgentCityContext ctx, string projectPath)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IWorkingChangesService>().GetAsync(projectPath, null);
        }

        private static async Task<T> Sync<T>(AgentCityContext ctx, Func<IRepoSyncService, Task<T>> action)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return await action(scope.ServiceProvider.GetRequiredService<IRepoSyncService>());
        }

        private static async Task<GitOperationResult> Pull(AgentCityContext ctx, string projectPath)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IModernGitService>().PullAsync(projectPath);
        }

        private static async Task<GitOperationResult> Checkout(AgentCityContext ctx, string repositoryPath, string branch)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IModernGitService>().CheckoutBranchAsync(repositoryPath, branch);
        }

        private static async Task<GitOperationResult> Commit(AgentCityContext ctx, string repositoryPath, string message)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IModernGitService>()
                .CommitAsync(repositoryPath, message, new GitAuthor { Name = "prova", Email = "prova@example.com" });
        }

        private static string Head(string dir) => Git(dir, "rev-parse HEAD").Out.Trim();
        private static string Branch(string dir) => Git(dir, "rev-parse --abbrev-ref HEAD").Out.Trim();

        /// <summary>Tutti e due cambiano la stessa riga del README: il collega pubblica, io committo.</summary>
        private static void SameLineOnBothSides(AgentCityContext ctx, string path, string collega)
        {
            File.WriteAllText(Path.Combine(collega, "README.md"), "# doc\nriga del collega\n");
            Git(collega, "commit -am collega"); Git(collega, "push -q origin main");

            File.WriteAllText(Path.Combine(path, "README.md"), "# doc\nriga mia\n");
            Git(path, "commit -am mia");
            Git(path, "fetch -q origin");
        }

        /// <summary>Il collega porta avanti il figlio, lo registra nel progetto e pubblica tutto. Torna il commit del figlio.</summary>
        private static string ColleaguePublishesChildAndProject(string collega, string suo, string content, string file = "codice.md")
        {
            File.WriteAllText(Path.Combine(suo, file), "# codice\n" + content + "\n");
            Git(suo, "add -A"); Git(suo, "commit -m \"figlio del collega\""); Git(suo, "push -q origin main");
            Git(collega, "add -A"); Git(collega, "commit -m \"bump figlio\""); Git(collega, "push -q origin main");
            return Head(suo);
        }

        /// <summary>Il secondo sviluppatore: un altro clone dello stesso progetto, col figlio sul suo ramo.</summary>
        private static (string Path, string Sub) Colleague(AgentCityContext ctx, string name, string parentOrigin)
        {
            var collega = Path.Combine(ctx.Factory.DataDir, "collega-" + name);
            Git(ctx.Factory.DataDir, $"clone -q \"{parentOrigin}\" \"{collega}\"");
            Git(collega, "submodule update --init -q");
            Identity(collega);
            var suo = Path.Combine(collega, "figlio");
            Identity(suo);
            Git(suo, "checkout -q main");
            return (collega, suo);
        }

        private static (int Code, string Out) Git(string cwd, string args)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "-c protocol.file.allow=always " + args,
                    WorkingDirectory = cwd,
                    UseShellExecute = false, RedirectStandardOutput = true,
                    RedirectStandardError = true, CreateNoWindow = true,
                }
            };
            p.StartInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
            p.Start();
            var o = p.StandardOutput.ReadToEnd();
            p.StandardError.ReadToEnd();
            p.WaitForExit(60000);
            return (p.ExitCode, o);
        }

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;

        private static (string Path, string Sub, string ChildOrigin, string ParentOrigin) Setup(
            AgentCityContext ctx, string name)
        {
            var (_, path) = ctx.SeedProject(name);
            var origins = Path.Combine(ctx.Factory.DataDir, "origins");

            var childOrigin = Path.Combine(origins, name + "-figlio.git");
            Directory.CreateDirectory(childOrigin);
            Git(childOrigin, "init --bare");
            Git(childOrigin, "symbolic-ref HEAD refs/heads/main");

            var childWork = Path.Combine(ctx.Factory.DataDir, "work", name + "-figlio");
            Directory.CreateDirectory(childWork);
            InitRepo(childWork);
            File.WriteAllText(Path.Combine(childWork, "codice.md"), "# codice\nv1\n");
            Git(childWork, "add -A");
            Git(childWork, "commit -m base-figlio");
            Git(childWork, $"remote add origin \"{childOrigin}\"");
            Git(childWork, "push -u origin main");

            var parentOrigin = Path.Combine(origins, name + ".git");
            Directory.CreateDirectory(parentOrigin);
            Git(parentOrigin, "init --bare");
            Git(parentOrigin, "symbolic-ref HEAD refs/heads/main");

            InitRepo(path);
            File.WriteAllText(Path.Combine(path, "README.md"), "# doc\n");
            Git(path, "add -A");
            Git(path, "commit -m base");
            Git(path, $"submodule add \"{childOrigin}\" figlio");
            Git(path, "commit -m \"aggiungi submodule\"");
            Git(path, $"remote add origin \"{parentOrigin}\"");
            Git(path, "push -u origin main");

            var sub = Path.Combine(path, "figlio");
            Identity(sub);
            Git(sub, "checkout main");
            return (path, sub, childOrigin, parentOrigin);
        }

        private static void InitRepo(string dir)
        {
            Git(dir, "init -b main");
            // Il codice di produzione invoca git SENZA 'protocol.file.allow', ed e' giusto: git
            // blocca i submodule da percorso locale per una vulnerabilita' nota. Qui i remoti sono
            // cartelle del test, quindi il permesso lo dichiara il repository di prova.
            Git(dir, "config protocol.file.allow always");
            Identity(dir);
        }

        private static void Identity(string dir)
        {
            Git(dir, "config user.email carlo@test.local");
            Git(dir, "config user.name Test");
            Git(dir, "config commit.gpgsign false");
            Git(dir, "config protocol.file.allow always");
        }
    }
}
