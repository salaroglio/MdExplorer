using Markdig;
using Markdig.Extensions.JiraLinks;

namespace MdExplorer.Features.Services.SourceMapping
{
    /// <summary>
    /// The Markdig pipeline of the document view, in one place: the page is rendered with it, and a
    /// correction made on the page (<see cref="RenderedTextEditor"/>) reads the file with it. Two
    /// copies would drift apart, and corrections would be checked against a rendering the page
    /// does not have.
    /// </summary>
    public static class DocumentViewPipeline
    {
        /// <param name="jiraUrl">The Jira server whose issue keys become links; null when Jira links are off.</param>
        public static MarkdownPipeline Build(string jiraUrl)
        {
            var builder = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseDiagrams()
                .UsePipeTables()
                .UsePreciseSourceLocation()
                .UseBootstrap();

            if (!string.IsNullOrWhiteSpace(jiraUrl))
            {
                builder.UseJiraLinks(new JiraLinkOptions(jiraUrl));
            }

            return builder
                .UseEmojiAndSmiley()
                .UseYamlFrontMatter()
                .UseGenericAttributes()
                .Build();
        }
    }
}
