using MdExplorer.Features.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests
{
    [TestClass]
    public class TestMatches
    {
        [TestMethod]
        public void Should_Get_MdShowMd()
        {
            var textToPars = "mdShowMd(file.md)";
            var command = new MDShowMD("",null);
            var match = command.GetMatches(textToPars);
            Assert.AreEqual(match.Count, 1);
        }

        [TestMethod]
        public void Should_Get_MdShowMd_And_The_Content_Inside_Parenthesis()
        {
            var textToPars = "mdShowMd(file.md)";
            var command = new MDShowMD("",null);
            var match = command.GetMatches(textToPars);
            var stringMatched = match[0].Groups[1].Value;
            Assert.AreEqual(stringMatched, "file.md");
        }

        [TestMethod]
        public void Should_Get_FromPlantumlToPng_And_The_Content_Inside_BackTicks()
        {
            var textToPars = @"```plantuml 
                                    @startuml 
                            @enduml ```";
                               
            var command = new FromPlantumlToPng("", null,null,null,null);
            var match = command.GetMatches(textToPars);
            var stringMatched = match[0].Groups[1].Value;
            Assert.AreEqual(stringMatched, @" 
                                    @startuml 
                            @enduml ");
        }

        [TestMethod]
        public void Should_Get_FromPlantumlToPng_Double_Groups()
        {
            var textToPars = @"```plantuml 
                                    @startuml 
                    testo 1
                            @enduml ```
                testo inutile
                ```plantuml 
                                    @startuml 
                        daje
                            @enduml ```";

            var command = new FromPlantumlToPng("", null, null,null,null);
            var match = command.GetMatches(textToPars);
            var stringMatched0 = match[0].Groups[1].Value;
            var stringMatched1 = match[1].Groups[1].Value;
            // Due blocchi plantuml distinti: il gruppo cattura l'intero contenuto di ciascuno.
            Assert.AreEqual(2, match.Count);
            StringAssert.Contains(stringMatched0, "@startuml");
            StringAssert.Contains(stringMatched0, "testo 1");
            StringAssert.Contains(stringMatched1, "daje");
        }


        [TestMethod]
        public void Should_Get_Link_To_Reset_BackPath_Coming_from_Transform_In_PNG()
        {
            // GetMatchesAfterConversion opera sull'HTML DOPO la conversione (vedi
            // FromLinkToApplication.TransformAfterConversion): intercetta i <img src="....md/<nome>.png">
            // per riscriverne il back-path. Group[1] è il nome file tra ".md/" e ".png".
            var htmlAfterConversion = $@"# Titolo
                            <img src=""../../../../.md/-12344455.png"">

                            testo di mezzo

                            <img src=""../../../../.md/3333.png"">";

            var command = new FromPlantumlToPng("", null, null, null, null);
            var match = command.GetMatchesAfterConversion(htmlAfterConversion);

            Assert.AreEqual(2, match.Count);
            Assert.AreEqual("-12344455", match[0].Groups[1].Value);
            Assert.AreEqual("3333", match[1].Groups[1].Value);
        }


        [TestMethod]
        public void Should_match_images()
        {
            var textToParse = $@"testo di prova
                ![alt text](Icons\plus.png ""Title"")
                prova di nuovo 
            ![alt text](Icons\plus.png ""Title"")
                    ne deve prendere 2
                ";
            var command = new ManageEmojiAsImages( null, null);
            var match = command.GetMatches(textToParse);
            // Regex: !\[alt text\]\(([^"]*)  -> Group[1] = il path fino alla virgoletta del titolo.
            var stringMatched0 = match[0].Groups[1].Value;

            Assert.AreEqual(2, match.Count);
            StringAssert.Contains(stringMatched0, "plus.png");
        }

    }
}
