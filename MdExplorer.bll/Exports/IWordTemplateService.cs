using MdExplorer.Features.Yaml.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MdExplorer.Features.Exports
{
    public interface IWordTemplateService
    {
        /// <summary>
        /// Inserisce le pagine predefinite nel contenuto markdown
        /// </summary>
        Task<string> InsertPredefinedPagesAsync(
            string markdownContent, 
            MdExplorerDocumentDescriptor descriptor,
            string projectPath);
        
        /// <summary>
        /// Sostituisce i tag in un template
        /// </summary>
        string ReplaceTags(string template, Dictionary<string, string> tags);
    }
}