using Markdig;
using MdExplorer.Abstractions.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Implementations.Features
{
    public class MarkDownFeature : IMarkDownFeature
    {
        private readonly MarkdownPipeline _markdownPipeline;

        public MarkDownFeature(MarkdownPipeline markdownPipeline)
        {
            _markdownPipeline = markdownPipeline;
        }
        public async Task<string> GetHtmlAsync(string filePath)
        {
            var readText = await File.ReadAllTextAsync(filePath);
            var result = Markdown.ToHtml(readText, _markdownPipeline);
            StringWriter tw = new StringWriter();
            var markDownDocument = Markdown.ToHtml(readText, tw, _markdownPipeline);
            return result;
        }
    }
}
