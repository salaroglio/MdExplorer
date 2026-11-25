using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.DatabaseManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Spostamento di un documento dall'albero (sprint 2026-11-25-Spostamento-File-E-Link): <c>POST api/mdfiles/MoveMdFile</c>,
    /// la stessa chiamata del trascinamento, su un progetto vero aperto e indicizzato. Dopo lo spostamento ogni link deve
    /// portare dove portava prima: quelli DENTRO il file spostato verso gli stessi bersagli, quelli degli ALTRI file verso il
    /// posto nuovo. Il resto del testo non cambia: il testo visibile dei link, i blocchi di codice, i titoli, le ancore.
    /// <para>Nata dalla prova sulla VM del 25/11/2026, che su Linux ha trovato i link distrutti e non seguiti.</para>
    /// </summary>
    [TestClass]
    public class MoveMdFileLinks_Should
    {
        private string _cwd;

        [TestInitialize]
        public void Init() => _cwd = Directory.GetCurrentDirectory();

        [TestCleanup]
        public void Done() => Directory.SetCurrentDirectory(_cwd);

        // ---- i link DENTRO il file spostato ----

        [TestMethod]
        public async Task Keep_the_links_inside_the_moved_file_pointing_to_the_same_targets()
        {
            using var p = await Project.Open("mv-dentro", ("index.md", "# Indice\n"), ("docs/other.md", "# Other\n"),
                ("docs/img.png", "PNG"), ("docs/sub/README.md", "# Sub\n"),
                ("docs/a.md", "# A\n\nTorna all'[indice](../index.md), vedi [other](other.md#x).\n\n![figura](img.png)\n"));

            await p.Move("docs/a.md", "docs/sub");

            Assert.AreEqual("# A\n\nTorna all'[indice](../../index.md), vedi [other](../other.md#x).\n\n![figura](../img.png)\n",
                p.Read("docs/sub/a.md"));
        }

        [TestMethod]
        public async Task Keep_a_root_relative_link_inside_the_moved_file_as_it_is()
        {
            using var p = await Project.Open("mv-radice-dentro", ("index.md", "# Indice\n"), ("docs/sub/README.md", "# Sub\n"),
                ("docs/a.md", "Vedi la [radice](/index.md).\n"));

            await p.Move("docs/a.md", "docs/sub");

            Assert.AreEqual("Vedi la [radice](/index.md).\n", p.Read("docs/sub/a.md"), "dalla radice il bersaglio è lo stesso ovunque sia il file");
        }

        // ---- i link degli ALTRI file verso il file spostato ----

        [TestMethod]
        public async Task Follow_the_moved_file_from_the_other_documents()
        {
            using var p = await Project.Open("mv-fuori", ("docs/a.md", "# A\n"), ("docs/sub/README.md", "# Sub\n"),
                ("index.md", "Testo con [A](docs/a.md) in mezzo e [la sezione](docs/a.md#sezione).\n"),
                ("docs/other.md", "Vicino: [A](a.md) e [A2](./a.md)\n"));

            await p.Move("docs/a.md", "docs/sub");

            Assert.AreEqual("Testo con [A](docs/sub/a.md) in mezzo e [la sezione](docs/sub/a.md#sezione).\n", p.Read("index.md"));
            Assert.AreEqual("Vicino: [A](sub/a.md) e [A2](sub/a.md)\n", p.Read("docs/other.md"));
        }

        [TestMethod]
        public async Task Follow_a_link_on_the_first_line_of_a_document()
        {
            using var p = await Project.Open("mv-prima-riga", ("docs/a.md", "# A\n"), ("docs/sub/README.md", "# Sub\n"),
                ("index.md", "[A all'inizio](docs/a.md)\n"));

            await p.Move("docs/a.md", "docs/sub");

            Assert.AreEqual("[A all'inizio](docs/sub/a.md)\n", p.Read("index.md"));
        }

        [TestMethod]
        public async Task Follow_a_root_relative_link_and_keep_it_root_relative()
        {
            using var p = await Project.Open("mv-radice-fuori", ("docs/a.md", "# A\n"), ("docs/sub/README.md", "# Sub\n"),
                ("guide/intro.md", "Vedi [A](/docs/a.md).\n"));

            await p.Move("docs/a.md", "docs/sub");

            Assert.AreEqual("Vedi [A](/docs/sub/a.md).\n", p.Read("guide/intro.md"));
        }

        [TestMethod]
        public async Task Follow_a_link_with_a_title_or_angle_brackets_and_keep_them()
        {
            using var p = await Project.Open("mv-titolo", ("docs/a.md", "# A\n"), ("docs/sub/README.md", "# Sub\n"),
                ("index.md", "Con titolo [A](docs/a.md \"il titolo\") e angolari [A](<docs/a.md>).\n"));

            await p.Move("docs/a.md", "docs/sub");

            Assert.AreEqual("Con titolo [A](docs/sub/a.md \"il titolo\") e angolari [A](<docs/sub/a.md>).\n", p.Read("index.md"));
        }

        [TestMethod]
        public async Task Leave_the_visible_text_of_a_link_alone_even_when_it_looks_like_the_path()
        {
            using var p = await Project.Open("mv-testo", ("docs/a.md", "# A\n"), ("docs/sub/README.md", "# Sub\n"),
                ("index.md", "Leggi [docs/a.md](docs/a.md).\n"));

            await p.Move("docs/a.md", "docs/sub");

            Assert.AreEqual("Leggi [docs/a.md](docs/sub/a.md).\n", p.Read("index.md"));
        }

        [TestMethod]
        public async Task Leave_code_blocks_alone()
        {
            using var p = await Project.Open("mv-codice", ("docs/a.md", "# A\n"), ("docs/sub/README.md", "# Sub\n"),
                ("index.md", "Vedi [A](docs/a.md).\n\n```bash\n# esempio: [A](docs/a.md)\n```\n\nIn riga `[A](docs/a.md)`.\n"));

            await p.Move("docs/a.md", "docs/sub");

            Assert.AreEqual("Vedi [A](docs/sub/a.md).\n\n```bash\n# esempio: [A](docs/a.md)\n```\n\nIn riga `[A](docs/a.md)`.\n",
                p.Read("index.md"));
        }

        [TestMethod]
        public async Task Follow_a_reference_style_link_from_its_definition()
        {
            using var p = await Project.Open("mv-riferimento", ("docs/a.md", "# A\n"), ("docs/sub/README.md", "# Sub\n"),
                ("index.md", "Vedi [A][r] e di nuovo [A][r].\n\n[r]: docs/a.md \"A\"\n"));

            await p.Move("docs/a.md", "docs/sub");

            Assert.AreEqual("Vedi [A][r] e di nuovo [A][r].\n\n[r]: docs/sub/a.md \"A\"\n", p.Read("index.md"));
        }

        [TestMethod]
        public async Task Follow_a_file_with_a_space_in_its_name_moved_up_to_the_root()
        {
            using var p = await Project.Open("mv-spazio", ("docs/sub/il file.md", "Torna a [B](../b.md).\n"), ("docs/b.md", "Vedi [F](sub/il%20file.md) e [G](<sub/il file.md>).\n"));

            await p.Move("docs/sub/il file.md", "");

            Assert.AreEqual("Torna a [B](docs/b.md).\n", p.Read("il file.md"));
            Assert.AreEqual("Vedi [F](../il%20file.md) e [G](<../il file.md>).\n", p.Read("docs/b.md"));
        }

        // ---- l'indice dopo lo spostamento ----

        [TestMethod]
        public async Task Keep_the_index_true_so_that_a_second_move_works_too()
        {
            using var p = await Project.Open("mv-due-volte", ("docs/sub/README.md", "# Sub\n"), ("docs/sub2/README.md", "# Sub2\n"),
                ("index.md", "# Indice\n"),
                ("docs/a.md", "Torna all'[indice](../index.md).\n"),
                ("guide.md", "Vedi [A](docs/a.md).\n"));

            await p.Move("docs/a.md", "docs/sub");
            await p.Move("docs/sub/a.md", "docs/sub2");

            Assert.AreEqual("Torna all'[indice](../../index.md).\n", p.Read("docs/sub2/a.md"));
            Assert.AreEqual("Vedi [A](docs/sub2/a.md).\n", p.Read("guide.md"));
        }

        // ---- più documenti insieme (Ctrl/Shift nell'albero) ----

        [TestMethod]
        public async Task Move_several_documents_together_keeping_the_links_between_them_and_to_them()
        {
            using var p = await Project.Open("mv-insieme", ("docs/arrivo/README.md", "# Arrivo\n"),
                ("docs/a.md", "Vedi [B](b.md) e l'[indice](../index.md).\n"),
                ("docs/b.md", "Torna ad [A](./a.md#inizio).\n"),
                ("index.md", "[A](docs/a.md) e [B](docs/b.md)\n"));

            var (status, body) = await p.MoveMany("docs/arrivo", "docs/a.md", "docs/b.md");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);
            Assert.AreEqual("Vedi [B](b.md) e l'[indice](../../index.md).\n", p.Read("docs/arrivo/a.md"));
            Assert.AreEqual("Torna ad [A](a.md#inizio).\n", p.Read("docs/arrivo/b.md"));
            Assert.AreEqual("[A](docs/arrivo/a.md) e [B](docs/arrivo/b.md)\n", p.Read("index.md"));
        }

        [TestMethod]
        public async Task Move_nothing_when_a_name_is_already_in_the_destination()
        {
            using var p = await Project.Open("mv-nome-preso", ("docs/arrivo/b.md", "# Il b che c'era\n"),
                ("docs/a.md", "# A\n"), ("docs/b.md", "# B\n"), ("index.md", "[A](docs/a.md)\n"));

            var (status, body) = await p.MoveMany("docs/arrivo", "docs/a.md", "docs/b.md");

            Assert.AreEqual(System.Net.HttpStatusCode.Conflict, status, body);
            StringAssert.Contains(body, "b.md");
            Assert.AreEqual("# Il b che c'era\n", p.Read("docs/arrivo/b.md"), "il documento che c'era non è stato cancellato");
            Assert.IsTrue(File.Exists(Path.Combine(p.Root, "docs", "a.md")), "nemmeno a.md si è mosso");
            Assert.AreEqual("[A](docs/a.md)\n", p.Read("index.md"));
        }

        [TestMethod]
        public async Task Move_nothing_when_two_chosen_documents_have_the_same_name()
        {
            using var p = await Project.Open("mv-stesso-nome", ("arrivo/README.md", "# Arrivo\n"),
                ("uno/note.md", "# Uno\n"), ("due/note.md", "# Due\n"));

            var (status, body) = await p.MoveMany("arrivo", "uno/note.md", "due/note.md");

            Assert.AreEqual(System.Net.HttpStatusCode.Conflict, status, body);
            Assert.IsTrue(File.Exists(Path.Combine(p.Root, "uno", "note.md")) && File.Exists(Path.Combine(p.Root, "due", "note.md")));
        }

        // ---- cartelle ----

        [TestMethod]
        public async Task Move_a_whole_folder_and_keep_every_link_right()
        {
            using var p = await Project.Open("mv-cartella", ("archivio/README.md", "# Archivio\n"),
                ("docs/guida/a.md", "Torna all'[indice](../../index.md), vedi [B](b.md) e ![figura](img/schema.png).\n"),
                ("docs/guida/b.md", "Torna ad [A](./a.md).\n"),
                ("docs/guida/img/schema.png", "PNG"),
                ("docs/altro.md", "Vedi la [guida](guida/a.md).\n"),
                ("index.md", "[A](docs/guida/a.md) e ![schema](docs/guida/img/schema.png) e [radice](/docs/guida/b.md)\n"));

            var (status, body) = await p.MoveMany("archivio", "docs/guida");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);
            Assert.AreEqual("Torna all'[indice](../../index.md), vedi [B](b.md) e ![figura](img/schema.png).\n", p.Read("archivio/guida/a.md"),
                "dentro la cartella: i link restano relativi fra loro; l'indice è ancora due livelli su");
            Assert.AreEqual("Torna ad [A](./a.md).\n", p.Read("archivio/guida/b.md"), "un link fra documenti della cartella non cambia");
            Assert.AreEqual("Vedi la [guida](../archivio/guida/a.md).\n", p.Read("docs/altro.md"));
            Assert.AreEqual("[A](archivio/guida/a.md) e ![schema](archivio/guida/img/schema.png) e [radice](/archivio/guida/b.md)\n", p.Read("index.md"),
                "anche le immagini dentro la cartella si seguono");
            Assert.IsTrue(p.IndexedPaths().Any(x => x.EndsWith(Path.Combine("archivio", "guida", "a.md"))), "l'indice ha il percorso nuovo");
            Assert.IsFalse(p.IndexedPaths().Any(x => x.Contains(Path.Combine("docs", "guida"))), "l'indice non ha più il vecchio");
        }

        [TestMethod]
        public async Task Refuse_to_move_a_folder_inside_itself()
        {
            using var p = await Project.Open("mv-dentro-se", ("docs/guida/a.md", "# A\n"), ("docs/guida/sotto/README.md", "# Sotto\n"));

            var (status, body) = await p.MoveMany("docs/guida/sotto", "docs/guida");

            Assert.AreEqual(System.Net.HttpStatusCode.Conflict, status, body);
            Assert.IsTrue(File.Exists(Path.Combine(p.Root, "docs", "guida", "a.md")));
        }

        [TestMethod]
        public async Task Let_a_chosen_file_inside_a_chosen_folder_travel_with_the_folder()
        {
            using var p = await Project.Open("mv-cartella-e-file", ("arrivo/README.md", "# Arrivo\n"),
                ("docs/guida/a.md", "# A\n"), ("index.md", "[A](docs/guida/a.md)\n"));

            var (status, body) = await p.MoveMany("arrivo", "docs/guida", "docs/guida/a.md");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);
            Assert.IsTrue(File.Exists(Path.Combine(p.Root, "arrivo", "guida", "a.md")), "a.md è dentro la cartella spostata");
            Assert.AreEqual("[A](arrivo/guida/a.md)\n", p.Read("index.md"));
        }

        // ---- rinomina di un documento ----

        [TestMethod]
        public async Task Rename_a_document_and_follow_it_from_every_link_without_touching_similar_names()
        {
            using var p = await Project.Open("rn-documento",
                ("docs/a.md", "# A\n\nVedi [i dati](data.md).\n"), ("docs/data.md", "# Dati\n"),
                ("index.md", "[a.md](docs/a.md), [sezione](docs/a.md#s), [radice](/docs/a.md), [dati](docs/data.md), [r][r]\n\n[r]: docs/a.md\n"));

            var (status, body) = await p.Rename("docs/a.md", "nuovo");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);
            StringAssert.Contains(body, "nuovo.md", "il .md si aggiunge da sé");
            Assert.IsFalse(File.Exists(Path.Combine(p.Root, "docs", "a.md")));
            Assert.AreEqual("# A\n\nVedi [i dati](data.md).\n", p.Read("docs/nuovo.md"), "i suoi link restano: è nella stessa cartella");
            Assert.AreEqual("[a.md](docs/nuovo.md), [sezione](docs/nuovo.md#s), [radice](/docs/nuovo.md), [dati](docs/data.md), [r][r]\n\n[r]: docs/nuovo.md\n",
                p.Read("index.md"), "testo visibile intatto, data.md non toccato anche se contiene «a.md»");
        }

        [TestMethod]
        public async Task Refuse_a_rename_onto_an_existing_name_or_with_a_folder_in_it()
        {
            using var p = await Project.Open("rn-rifiuti", ("docs/a.md", "# A\n"), ("docs/b.md", "# B che c'era\n"), ("index.md", "[A](docs/a.md)\n"));

            var (taken, takenBody) = await p.Rename("docs/a.md", "b.md");
            var (slash, _) = await p.Rename("docs/a.md", "altra/c.md");

            Assert.AreEqual(System.Net.HttpStatusCode.Conflict, taken, takenBody);
            Assert.AreEqual(System.Net.HttpStatusCode.Conflict, slash);
            Assert.AreEqual("# B che c'era\n", p.Read("docs/b.md"), "il documento che c'era non è stato cancellato");
            Assert.AreEqual("[A](docs/a.md)\n", p.Read("index.md"));
        }

        // ---- infrastruttura ----

        /// <summary>Un progetto vero: file scritti, aperto con SetFolderProject, indicizzato fino ai link.</summary>
        private sealed class Project : IDisposable
        {
            private AgentCityContext _ctx;
            private string _conn;
            public string Root { get; private set; }

            public static async Task<Project> Open(string name, params (string Path, string Text)[] files)
            {
                var p = new Project { _ctx = new AgentCityContext(), _conn = name + "-" + Guid.NewGuid().ToString("N") };
                var (_, root) = p._ctx.SeedProject(name);
                p.Root = root;
                foreach (var (rel, text) in files)
                {
                    var full = Path.Combine(root, rel.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(full));
                    File.WriteAllText(full, text);
                }

                var (status, body) = await p._ctx.PostJson("/api/MdProjects/SetFolderProject?ConnectionId=" + p._conn,
                    JsonSerializer.Serialize(new { path = root }));
                Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);
                var shallow = await p._ctx.Client.GetAsync("/api/mdfiles/GetShallowStructure?ConnectionId=" + p._conn);
                Assert.IsTrue(shallow.IsSuccessStatusCode, "GetShallowStructure: " + shallow.StatusCode);
                await p.WaitIndexed(files.Count(f => f.Path.EndsWith(".md")));
                return p;
            }

            /// <summary>Aspetta che ogni documento abbia i suoi link nell'indice (LinksHash scritto dalla pipeline).</summary>
            private async Task WaitIndexed(int documents)
            {
                var dbm = _ctx.Factory.Services.GetRequiredService<IDatabaseManager>();
                int Done()
                {
                    using var engine = dbm.CreateIsolatedEngineDB(_conn);
                    return engine.GetDal<MarkdownFile>().GetList().Count(m => m.LinksHash != null);
                }
                var deadline = DateTime.UtcNow.AddSeconds(60);
                while (Done() < documents && DateTime.UtcNow < deadline) await Task.Delay(300);
                Assert.AreEqual(documents, Done(), "documenti indicizzati prima dello spostamento");
            }

            /// <summary>Come il trascinamento nell'albero: il percorso relativo del file e la cartella di arrivo, assoluta.</summary>
            public async Task Move(string relativeFile, string relativeFolder)
            {
                var json = JsonSerializer.Serialize(new
                {
                    sourceRelativePath = "/" + relativeFile,
                    sourceFileName = Path.GetFileName(relativeFile),
                    destinationPath = Path.Combine(Root, relativeFolder.Replace('/', Path.DirectorySeparatorChar)),
                });
                var (status, body) = await _ctx.PostJson("/api/mdfiles/MoveMdFile?ConnectionId=" + _conn, json);
                Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);
                Assert.IsTrue(File.Exists(Path.Combine(Root, relativeFolder, Path.GetFileName(relativeFile))), "il file è arrivato");
            }

            /// <summary>Più documenti scelti nell'albero e trascinati insieme; restituisce la risposta senza pretendere che sia riuscita.</summary>
            public Task<(System.Net.HttpStatusCode Status, string Body)> MoveMany(string relativeFolder, params string[] relativeFiles)
                => _ctx.PostJson("/api/mdfiles/MoveMdFiles?ConnectionId=" + _conn, JsonSerializer.Serialize(new
                {
                    sourceRelativePaths = relativeFiles.Select(f => "/" + f).ToArray(),
                    destinationPath = Path.Combine(Root, relativeFolder.Replace('/', Path.DirectorySeparatorChar)),
                }));

            /// <summary>I percorsi dei documenti nell'indice dei file.</summary>
            public System.Collections.Generic.List<string> IndexedPaths()
            {
                using var engine = _ctx.Factory.Services.GetRequiredService<IDatabaseManager>().CreateIsolatedEngineDB(_conn);
                return engine.GetDal<MarkdownFile>().GetList().Select(m => m.Path).ToList();
            }

            public Task<(System.Net.HttpStatusCode Status, string Body)> Rename(string relativeFile, string newName)
                => _ctx.PostJson("/api/mdfiles/RenameMdFile?ConnectionId=" + _conn,
                    JsonSerializer.Serialize(new { sourcePath = "/" + relativeFile, newName }));

            public string Read(string relative) => File.ReadAllText(Path.Combine(Root, relative.Replace('/', Path.DirectorySeparatorChar)));

            public void Dispose()
            {
                try { _ctx.Factory.Services.GetRequiredService<IDatabaseManager>().UnregisterConnection(_conn); } catch { }
                _ctx.Dispose();
            }
        }
    }
}
