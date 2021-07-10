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
            var textToPars = "m↓ShowMd(file.md)";
            var command = new MDShowMD("",null);
            var match = command.GetMatches(textToPars);
            Assert.AreEqual(match.Count, 1);
        }

        [TestMethod]
        public void Should_Get_MdShowMd_And_The_Content_Inside_Parenthesis()
        {
            var textToPars = "m↓ShowMd(file.md)";
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
            Assert.AreEqual(stringMatched0, @" 
                                    @startuml 
                            @enduml ");
        }


    }
}
