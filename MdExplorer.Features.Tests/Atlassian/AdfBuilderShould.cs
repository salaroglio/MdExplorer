using MdExplorer.Features.Services.Atlassian;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Atlassian
{
    [TestClass]
    public class AdfBuilderShould
    {
        [TestMethod]
        public void BuildMinimalAdfDocWithText()
        {
            var json = AdfBuilder.FromPlainText("Hello world");
            StringAssert.Contains(json, "\"type\":\"doc\"");
            StringAssert.Contains(json, "Hello world");
        }

        [TestMethod]
        public void RoundTripThroughRenderer()
        {
            // Builder and renderer are inverses for plain text.
            var json = AdfBuilder.FromPlainText("First line\nSecond line");
            var text = AdfRenderer.ToText(json);
            StringAssert.Contains(text, "First line");
            StringAssert.Contains(text, "Second line");
        }

        [TestMethod]
        public void ProduceValidDocForEmptyInput()
        {
            var json = AdfBuilder.FromPlainText("");
            StringAssert.Contains(json, "\"type\":\"doc\"");
            StringAssert.Contains(json, "\"version\":1");
        }
    }
}
