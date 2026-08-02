using System;
using System.IO;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// I posti di lavoro degli agenti vivono dentro il progetto ma non ne fanno parte.
    /// <para>
    /// Ogni worktree è una copia intera della documentazione. Se l'applicazione la trattasse come
    /// contenuto, aprire un progetto con due agenti attivi vorrebbe dire tre copie dello stesso
    /// documento nell'indice, tre risultati identici in ricerca e un albero in cui
    /// <c>.worktrees/slot-1/guida.md</c> convive con <c>guida.md</c> senza che si capisca quale sia
    /// la tua. Per questo l'esclusione è <b>incorporata</b> e non configurabile: non dipende dal
    /// fatto che qualcuno abbia scritto la riga giusta in un file di configurazione.
    /// </para>
    /// </summary>
    [TestClass]
    public class AgentWorktreesExclusion_Should
    {
        [TestMethod]
        public void Hide_the_worktrees_folder_even_without_any_configuration()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("escl-nocfg");
            var svc = ctx.Factory.Services.GetRequiredService<FoldersIgnoreService>();

            // Il caso vero: progetto appena aperto, nessun .mdFoldersIgnore. Prima di questa
            // regola la risposta era "no, mostrala".
            Assert.IsTrue(svc.ShouldIgnoreFolderForProject(
                Path.Combine(path, FoldersIgnoreService.AgentWorktreesFolder), path));

            // Una cartella qualsiasi resta visibile: l'esclusione è mirata, non un colabrodo.
            Assert.IsFalse(svc.ShouldIgnoreFolderForProject(Path.Combine(path, "capitoli"), path));
        }

        [TestMethod]
        public void Recognise_anything_living_under_a_worktree()
        {
            var project = Path.Combine(Path.GetTempPath(), "progetto-x");
            var inside = Path.Combine(project, ".worktrees", "slot-1", "docs", "guida.md");

            Assert.IsTrue(FoldersIgnoreService.IsInsideAgentWorktrees(inside, project),
                "un file in fondo a un worktree è comunque roba dell'agente");

            // Il confronto è sul segmento, non sul prefisso della stringa: una cartella che
            // COMINCIA per ".worktrees" senza esserlo resta un documento tuo.
            Assert.IsFalse(FoldersIgnoreService.IsInsideAgentWorktrees(
                Path.Combine(project, ".worktrees-vecchi", "nota.md"), project));

            Assert.IsFalse(FoldersIgnoreService.IsInsideAgentWorktrees(
                Path.Combine(project, "docs", "guida.md"), project));
        }

        [TestMethod]
        public void Be_added_to_the_gitignore_of_a_project_that_already_existed()
        {
            // Il caso che conta: i progetti di documentazione esistono già e hanno già un
            // .gitignore, quindi il blocco scritto alla creazione non li tocca mai. Senza questo,
            // al primo risveglio di un agente comparirebbero migliaia di file non tracciati.
            var project = Path.Combine(Path.GetTempPath(), "mde-escl-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(project, ".git"));
            var gitignore = Path.Combine(project, ".gitignore");
            File.WriteAllText(gitignore, "# scritto a mano tempo fa\nbozze/\n");

            try
            {
                MdExplorer.Service.ProjectsManager.EnsureGitignoreEntries(project);

                var content = File.ReadAllText(gitignore);
                StringAssert.Contains(content, ".worktrees/");
                StringAssert.Contains(content, "bozze/", "quello che c'era prima resta dov'era");

                // Il progetto si riapre tutti i giorni: la riga non deve moltiplicarsi.
                MdExplorer.Service.ProjectsManager.EnsureGitignoreEntries(project);
                Assert.AreEqual(1, CountOccurrences(File.ReadAllText(gitignore), ".worktrees/"));
            }
            finally
            {
                Directory.Delete(project, true);
            }
        }

        private static int CountOccurrences(string haystack, string needle)
        {
            var count = 0;
            for (var i = haystack.IndexOf(needle, StringComparison.Ordinal); i >= 0;
                 i = haystack.IndexOf(needle, i + needle.Length, StringComparison.Ordinal))
            {
                count++;
            }
            return count;
        }
    }
}
