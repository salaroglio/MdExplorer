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

            public string Read(string relative) => File.ReadAllText(Path.Combine(Root, relative.Replace('/', Path.DirectorySeparatorChar)));

            public void Dispose()
            {
                try { _ctx.Factory.Services.GetRequiredService<IDatabaseManager>().UnregisterConnection(_conn); } catch { }
                _ctx.Dispose();
            }
        }
    }
}
