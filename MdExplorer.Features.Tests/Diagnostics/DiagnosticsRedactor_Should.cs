using MdExplorer.Features.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Diagnostics
{
    /// <summary>
    /// L'oscuramento dei log prima di «Invia diagnostica». I casi vengono dai log veri del 06/11/2026 (Windows dell'utente,
    /// utente di dominio ACME\RSSMRA80A) più i segreti che MDE maneggia (relay, GitHub, Telegram, chiavi dei modelli).
    /// </summary>
    [TestClass]
    public class DiagnosticsRedactor_Should
    {
        private static readonly RedactionContext Windows = new RedactionContext
        {
            UserName = "RSSMRA80A", Domain = "ACME", UserProfile = @"C:\Users\RSSMRA80A",
        };

        private static string R(string text, RedactionContext ctx = null) => DiagnosticsRedactor.Redact(text, ctx ?? Windows).Text;

        [TestMethod]
        public void Hide_the_user_folder_in_every_form_it_appears_in_the_logs()
        {
            Assert.AreEqual(@"[git] repository <utente>\AppData\Local\MdExplorer\Logs",
                R(@"[git] repository C:\Users\RSSMRA80A\AppData\Local\MdExplorer\Logs"));
            Assert.AreEqual("<utente>/AppData/Roaming/mdexplorer", R("C:/Users/RSSMRA80A/AppData/Roaming/mdexplorer"));
            Assert.AreEqual(@"{""path"":""<utente>\\AppData""}", R(@"{""path"":""C:\\Users\\RSSMRA80A\\AppData""}"));
            Assert.AreEqual("<utente>/Documents/x.md", R("/home/mario/Documents/x.md", new RedactionContext { UserName = "mario", UserProfile = "/home/mario" }));
        }

        [TestMethod]
        public void Hide_the_user_name_alone_and_with_the_domain()
        {
            Assert.AreEqual("Si è collegato <utente> da 192.168.41.140", R(@"Si è collegato acme\rssmra80a da 192.168.41.140"));
            Assert.AreEqual("utente <utente>, ok", R("utente RSSMRA80A, ok"));
        }

        [TestMethod]
        public void Hide_the_person_s_name_with_any_separator()
        {
            var ctx = new RedactionContext { UserName = "RSSMRA80A", OtherNames = new[] { "mario.rossi" } };
            Assert.AreEqual("User <utente> opened project", R("User mario.rossi opened project", ctx));
            Assert.AreEqual("origin -> https://github.com/<utente>_acme/ordini", R("origin -> https://github.com/mario-rossi_acme/ordini", ctx));
            Assert.AreEqual("autore <utente>", R("autore Mario Rossi", ctx));
            Assert.AreEqual("mariolina e rossini restano", R("mariolina e rossini restano", ctx));
        }

        [TestMethod]
        public void Keep_the_name_of_the_visual_basic_language()
        {
            Assert.AreEqual("codice Visual Basic incorporato nel report", R("codice Visual Basic incorporato nel report"), "è il linguaggio, non un'intestazione Basic");
            Assert.AreEqual("Authorization: Basic <segreto>", R("Authorization: Basic dXNlcjpwYXNzd29yZA=="), "l'intestazione vera resta oscurata");
        }

        [TestMethod]
        public void Hide_email_addresses()
        {
            var r = DiagnosticsRedactor.Redact("User mario.rossi opened project, author mario.rossi@example.com", Windows);
            Assert.AreEqual("User mario.rossi opened project, author <email>", r.Text);
            Assert.AreEqual(1, r.Counts[DiagnosticsRedactor.Email]);
        }

        [TestMethod]
        public void Hide_the_room_secret_and_other_key_values()
        {
            Assert.AreEqual("  roomSecret: <segreto>", R("  roomSecret: w_s7ovtQSazlzEdzk1Aa2JCrmEZY6kttBqm4tyy0eYg"));
            Assert.AreEqual("x-api-key=<segreto> sent", R("x-api-key=abcdef123456 sent"));
            Assert.AreEqual(@"{""ApiKey"":""<segreto>""}", R(@"{""ApiKey"":""Zm9vYmFyYmF6""}"));
            Assert.AreEqual("password: <segreto>", R("password: hunter22"));
        }

        [TestMethod]
        public void Hide_tokens_with_a_known_shape()
        {
            Assert.AreEqual("token <segreto> usato", R("token ghp_abcdefghijklmnopqrstuvwxyz0123456789 usato"));
            Assert.AreEqual("Authorization: Bearer <segreto>", R("Authorization: Bearer eyJhbGciOi.payload123456.sig1234567890"));
            Assert.AreEqual("bot <segreto> attivo", R("bot 123456789:AAHdqTcvCH1vGWJxfSeofSAs0K5PALDsawA attivo"));
            Assert.AreEqual("chiave <segreto>", R("chiave sk-ant-api03-abcdefghijklmnop"));
        }

        [TestMethod]
        public void Hide_credentials_inside_a_url_but_keep_the_host()
        {
            Assert.AreEqual("clone https://<segreto>@repository.example.com/scm/repo/x.git",
                R("clone https://mario:Segreta123@repository.example.com/scm/repo/x.git"));
        }

        [TestMethod]
        public void Leave_alone_what_is_needed_to_understand_a_problem()
        {
            const string perf = "[PERF] 18:05:26.596 [DB-HOLD commit] MdExplorer.db: 6553 ms | ProjectsManager.McpGroupsArgument < SetNewProject";
            Assert.AreEqual(perf, R(perf));
            const string project = @"Opening project for connection E3xXL3rxbCzobTipvY0iKw: C:\sviluppo\acme\ordini";
            Assert.AreEqual(project, R(project));
            const string git = "Remote found: origin -> https://github.com/mario-rossi_acme/ordini";
            Assert.AreEqual(git, R(git));
            Assert.AreEqual(0, DiagnosticsRedactor.Redact(perf + "\n" + project, Windows).Total);
        }

        [TestMethod]
        public void Count_what_it_hid_by_kind()
        {
            var r = DiagnosticsRedactor.Redact("a@b.it c@d.it roomSecret: abcdefgh C:\\Users\\RSSMRA80A\\x", Windows);
            Assert.AreEqual(2, r.Counts[DiagnosticsRedactor.Email]);
            Assert.AreEqual(1, r.Counts[DiagnosticsRedactor.Secret]);
            Assert.AreEqual(1, r.Counts[DiagnosticsRedactor.User]);
        }
    }
}
