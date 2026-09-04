using MdExplorer.Features.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests
{
    [TestClass]
    public class AgentPromptTemplateTests
    {
        private const string AgentBody = "# My agent\n\nYou are a helpful agent.\n";
        private const string Prompt = "Summarize <in-file> into <out-file>.";

        [TestMethod]
        public void Upsert_FirstSave_AppendsManagedSection()
        {
            var result = AgentPromptComposer.UpsertPromptTemplate(AgentBody, Prompt);

            StringAssert.Contains(result, AgentPromptComposer.TemplateStartMarker);
            StringAssert.Contains(result, AgentPromptComposer.TemplateEndMarker);
            StringAssert.Contains(result, Prompt);
            // The original body must be preserved and come first.
            StringAssert.StartsWith(result, "# My agent");
        }

        [TestMethod]
        public void Upsert_IsIdempotent_NoDuplicateSection()
        {
            var once = AgentPromptComposer.UpsertPromptTemplate(AgentBody, Prompt);
            var twice = AgentPromptComposer.UpsertPromptTemplate(once, Prompt);

            Assert.AreEqual(once, twice, "Re-saving the same prompt must not change the file.");
            Assert.AreEqual(1, CountOccurrences(twice, AgentPromptComposer.TemplateStartMarker));
        }

        [TestMethod]
        public void Upsert_ReplacesExistingSection_WithNewPrompt()
        {
            var first = AgentPromptComposer.UpsertPromptTemplate(AgentBody, Prompt);
            var second = AgentPromptComposer.UpsertPromptTemplate(first, "A totally different task.");

            Assert.AreEqual(1, CountOccurrences(second, AgentPromptComposer.TemplateStartMarker));
            StringAssert.Contains(second, "A totally different task.");
            Assert.IsFalse(second.Contains(Prompt), "Old prompt should be gone.");
        }

        [TestMethod]
        public void Extract_RoundTrips_TheSavedPrompt()
        {
            var withTemplate = AgentPromptComposer.UpsertPromptTemplate(AgentBody, Prompt);
            var extracted = AgentPromptComposer.ExtractPromptTemplate(withTemplate);

            Assert.AreEqual(Prompt, extracted);
        }

        [TestMethod]
        public void Extract_ReturnsNull_WhenNoSection()
        {
            Assert.IsNull(AgentPromptComposer.ExtractPromptTemplate(AgentBody));
        }

        [TestMethod]
        public void Strip_RemovesSection_LeavingOriginalBody()
        {
            var withTemplate = AgentPromptComposer.UpsertPromptTemplate(AgentBody, Prompt);
            var stripped = AgentPromptComposer.StripPromptTemplate(withTemplate);

            Assert.AreEqual(AgentBody.TrimEnd(), stripped.TrimEnd());
        }

        [TestMethod]
        public void ComposeRunPrompt_DoesNotLeakTemplateSection()
        {
            var withTemplate = AgentPromptComposer.UpsertPromptTemplate(AgentBody, Prompt);
            var composed = AgentPromptComposer.ComposeRunPrompt(withTemplate, "Do the task now.");

            Assert.IsFalse(composed.Contains(AgentPromptComposer.TemplateStartMarker),
                "The managed template markers must never reach the runtime prompt.");
            StringAssert.Contains(composed, "You are a helpful agent.");
            StringAssert.Contains(composed, "Do the task now.");
        }

        private static int CountOccurrences(string haystack, string needle)
        {
            int count = 0, i = 0;
            while ((i = haystack.IndexOf(needle, i)) >= 0) { count++; i += needle.Length; }
            return count;
        }
    }
}
