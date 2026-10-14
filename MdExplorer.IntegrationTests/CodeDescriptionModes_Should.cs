using System;
using MdExplorer.Service.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>Punto 2b del 14/10/2026: <c>codeGraph.describe</c> nel .development.yml, letto senza tolleranza.</summary>
    [TestClass]
    public class CodeDescriptionModes_Should
    {
        [TestMethod]
        public void Read_the_two_modes_and_default_to_on_demand()
        {
            Assert.AreEqual(CodeDescriptionModes.All, CodeDescriptionModes.Parse("all", "x"));
            Assert.AreEqual(CodeDescriptionModes.OnDemand, CodeDescriptionModes.Parse("onDemand", "x"));
            Assert.AreEqual(CodeDescriptionModes.OnDemand, CodeDescriptionModes.Parse(null, "x"), "assente = il comportamento di prima");
        }

        [TestMethod]
        public void Refuse_an_unknown_mode_saying_what_to_write()
        {
            var ex = Assert.ThrowsException<InvalidOperationException>(() => CodeDescriptionModes.Parse("sempre", "'program'"));
            StringAssert.Contains(ex.Message, "'program'");
            StringAssert.Contains(ex.Message, "codeGraph.describe");
            StringAssert.Contains(ex.Message, "«all» o «onDemand»");
        }
    }
}
