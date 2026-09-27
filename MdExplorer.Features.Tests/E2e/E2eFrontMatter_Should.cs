using MdExplorer.Features.E2e;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.E2e
{
    [TestClass]
    public class E2eFrontMatter_Should
    {
        private const string Test =
            "---\n" +
            "title: Login # un commento che deve restare\n" +
            "e2e:\n" +
            "  baseUrl: https://example.org\n" +
            "  run:\n" +
            "    dedicatedSession: false\n" +
            "  artifacts: login.e2e/\n" +
            "author: qualcuno\n" +
            "---\n" +
            "# Login\n";

        [TestMethod]
        public void Read_the_run_settings_and_leave_unsaid_keys_null()
        {
            Assert.AreEqual(new E2eRunSettings(false, null, null), E2eFrontMatter.ReadRunSettings(Test, "t.e2e.md"));
        }

        [TestMethod]
        public void Read_no_settings_from_a_file_without_e2e_block()
        {
            Assert.AreEqual(E2eRunSettings.None, E2eFrontMatter.ReadRunSettings("---\ntitle: x\n---\n# x\n", "x.md"));
            Assert.AreEqual(E2eRunSettings.None, E2eFrontMatter.ReadRunSettings("# senza front matter\n", "x.md"));
        }

        [TestMethod]
        public void Replace_the_run_block_and_touch_nothing_else()
        {
            var written = E2eFrontMatter.WriteRunSettings(Test, new E2eRunSettings(null, true, false), "t.e2e.md");

            Assert.AreEqual(
                "---\n" +
                "title: Login # un commento che deve restare\n" +
                "e2e:\n" +
                "  baseUrl: https://example.org\n" +
                "  run:\n" +
                "    commitAfterRun: true\n" +
                "    headless: false\n" +
                "  artifacts: login.e2e/\n" +
                "author: qualcuno\n" +
                "---\n" +
                "# Login\n", written);
        }

        [TestMethod]
        public void Remove_run_when_nothing_is_said_and_e2e_when_it_is_left_empty()
        {
            var folder = "---\ntitle: cartella\ne2e:\n  run:\n    headless: true\n---\n# cartella\n";
            Assert.AreEqual("---\ntitle: cartella\n---\n# cartella\n", E2eFrontMatter.WriteRunSettings(folder, E2eRunSettings.None, "c.md.directory"));

            var test = E2eFrontMatter.WriteRunSettings(Test, E2eRunSettings.None, "t.e2e.md");
            StringAssert.Contains(test, "e2e:\n  baseUrl: https://example.org\n  artifacts: login.e2e/\n");
            Assert.IsFalse(test.Contains("run:"));
        }

        [TestMethod]
        public void Add_an_e2e_block_to_a_front_matter_that_has_none_keeping_its_line_endings()
        {
            var crlf = "---\r\ntitle: cartella\r\n---\r\n# cartella\n| a | b |\n";
            var written = E2eFrontMatter.WriteRunSettings(crlf, new E2eRunSettings(true, null, null), "c.md.directory");
            Assert.AreEqual("---\r\ntitle: cartella\r\ne2e:\r\n  run:\r\n    dedicatedSession: true\r\n---\r\n# cartella\n| a | b |\n", written);
        }

        [TestMethod]
        public void Give_a_front_matter_to_a_file_without_one()
        {
            var written = E2eFrontMatter.WriteRunSettings("# titolo\n", new E2eRunSettings(null, null, true), "x.md");
            Assert.AreEqual("---\ne2e:\n  run:\n    headless: true\n---\n# titolo\n", written);
        }

        [TestMethod]
        public void Refuse_the_flow_style_and_values_that_are_not_booleans_with_what_to_do()
        {
            var flow = Assert.ThrowsException<E2eFormatException>(() =>
                E2eFrontMatter.ReadRunSettings("---\ne2e: { baseUrl: x }\n---\n", "t.e2e.md"));
            StringAssert.Contains(flow.Message, "forma a blocchi");

            var notBool = Assert.ThrowsException<E2eFormatException>(() =>
                E2eFrontMatter.ReadRunSettings("---\ne2e:\n  run:\n    headless: forse\n---\n", "t.e2e.md"));
            StringAssert.Contains(notBool.Message, "'e2e.run.headless' vale 'forse'");

            var unknown = Assert.ThrowsException<E2eFormatException>(() =>
                E2eFrontMatter.ReadRunSettings("---\ne2e:\n  run:\n    headles: true\n---\n", "t.e2e.md"));
            StringAssert.Contains(unknown.Message, "chiave sconosciuta 'e2e.run.headles'");
        }

        [TestMethod]
        public void Carry_the_e2e_block_over_a_regenerated_file()
        {
            var old = "---\r\nauthor: a\r\ne2e:\r\n  run:\r\n    commitAfterRun: true\r\n---\r\n# vecchio\n";
            var regenerated = "---\r\nauthor: b\r\ndate: oggi\r\n---\r\n# nuovo\n";

            Assert.AreEqual(
                "---\r\nauthor: b\r\ndate: oggi\r\ne2e:\r\n  run:\r\n    commitAfterRun: true\r\n---\r\n# nuovo\n",
                E2eFrontMatter.CarryOverBlock(old, regenerated));
            Assert.AreEqual(regenerated, E2eFrontMatter.CarryOverBlock(null, regenerated));
            Assert.AreEqual(regenerated, E2eFrontMatter.CarryOverBlock("---\r\nauthor: a\r\n---\r\n", regenerated));
        }
    }
}
