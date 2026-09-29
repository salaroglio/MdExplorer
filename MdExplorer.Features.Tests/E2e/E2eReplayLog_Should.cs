using MdExplorer.Features.E2e;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace MdExplorer.Features.Tests.E2e
{
    [TestClass]
    public class E2eReplayLog_Should
    {
        private string _run;

        // A HAR as Playwright writes it (shape of the one recorded on the-internet, 29/09/2026): the login POST
        // carries the password in its body, the answer sets a session cookie.
        private const string Har = @"{""log"":{""entries"":[
 {""startedDateTime"":""2026-09-29T06:50:32.000Z"",""time"":120.4,""_resourceType"":""document"",
  ""request"":{""method"":""GET"",""url"":""https://example.org/login"",""headers"":[{""name"":""cookie"",""value"":""rack.session=SEGRETO""}],""cookies"":[]},
  ""response"":{""status"":200,""content"":{""size"":2345},""headers"":[]}},
 {""startedDateTime"":""2026-09-29T06:50:32.100Z"",""time"":8,""_resourceType"":""stylesheet"",
  ""request"":{""method"":""GET"",""url"":""https://example.org/css/app.css"",""headers"":[]},
  ""response"":{""status"":200,""content"":{""size"":900},""headers"":[]}},
 {""startedDateTime"":""2026-09-29T06:50:33.500Z"",""time"":180,""_resourceType"":""document"",
  ""request"":{""method"":""POST"",""url"":""https://example.org/authenticate"",""headers"":[],""postData"":{""text"":""username=tomsmith&password=SuperSecretPassword!""}},
  ""response"":{""status"":303,""content"":{""size"":0},""headers"":[{""name"":""set-cookie"",""value"":""rack.session=SEGRETO""}]}},
 {""startedDateTime"":""2026-09-29T06:50:34.000Z"",""time"":-1,""_resourceType"":""fetch"",
  ""request"":{""method"":""GET"",""url"":""https://cdn.other.org/x?q=a|b"",""headers"":[]},
  ""response"":{""status"":0,""content"":{},""headers"":[]}}
]}}";

        private static readonly string[] Console =
        {
            "2026-09-29T06:50:33.700Z\tinfo\t[mde] ready grafo cob:bs522",
            "2026-09-29T06:50:34.100Z\terror\tFailed to load resource: SuperSecretPassword! rejected",
        };

        private static readonly IReadOnlyDictionary<string, string> Secrets = new Dictionary<string, string> { ["x.password"] = "SuperSecretPassword!" };

        [TestInitialize]
        public void Setup()
        {
            _run = Path.Combine(Path.GetTempPath(), "e2e-log-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_run);
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_run, true);

        [TestMethod]
        public void List_the_calls_with_time_duration_and_size()
        {
            var md = E2eReplayLog.Build(1, Har, Console, "https://example.org");

            StringAssert.Contains(md, "| 0.00 | GET | /login | 200 | 120 ms | 2345 | document |");
            StringAssert.Contains(md, "| 1.50 | POST | /authenticate | 303 | 180 ms | 0 | document |");
            StringAssert.Contains(md, "| 2.00 | GET | https://cdn.other.org/x?q=a\\|b | fallita | ? | ? | fetch |", "another site: whole address, pipes escaped");
            StringAssert.Contains(md, "Risorse statiche non elencate: 1");
            StringAssert.Contains(md, "| 1.70 | info | [mde] ready grafo cob:bs522 |");
        }

        [TestMethod]
        public void Leave_out_headers_cookies_and_bodies()
        {
            var md = E2eReplayLog.Build(1, Har, Console, "https://example.org");

            Assert.IsFalse(md.Contains("SEGRETO"), "cookies and headers stay out");
            Assert.IsFalse(md.Contains("username="), "request bodies stay out");
        }

        [TestMethod]
        public void Write_the_log_without_credentials_and_delete_the_recordings()
        {
            File.WriteAllText(Path.Combine(_run, "MdeE2e.Login.Test_T1.har"), Har);
            File.WriteAllLines(Path.Combine(_run, "MdeE2e.Login.Test_T1.console.tsv"), Console);
            File.WriteAllText(Path.Combine(_run, "MdeE2e.Altro.Test_T9.har"), Har);

            var written = E2eReplayLog.Process(_run, new Dictionary<string, int> { ["MdeE2e.Login.Test_T1"] = 1 }, Secrets, "https://example.org");

            CollectionAssert.AreEqual(new[] { "registro.T1.md" }, new List<string>(written));
            var md = File.ReadAllText(Path.Combine(_run, "registro.T1.md"));
            Assert.IsFalse(md.Contains("SuperSecretPassword!"), "a credential printed in the console is replaced by its key");
            StringAssert.Contains(md, "{{x.password}}");
            CollectionAssert.AreEquivalent(new[] { "registro.T1.md" }, Array.ConvertAll(Directory.GetFiles(_run), Path.GetFileName),
                "the raw recordings hold form bodies and cookies: all deleted, also those of an unknown class");
        }

        [TestMethod]
        public void Delete_the_recordings_of_an_interrupted_replay()
        {
            File.WriteAllText(Path.Combine(_run, "MdeE2e.Login.Test_T1.har"), Har);
            File.WriteAllText(Path.Combine(_run, "T1-05.png"), "png");

            E2eReplayLog.DeleteRaw(_run);

            CollectionAssert.AreEqual(new[] { "T1-05.png" }, Array.ConvertAll(Directory.GetFiles(_run), Path.GetFileName));
        }
    }
}
