using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.snippets
{
    
    public enum ParameterName
    {
        StringDocumentTitle,
        ProjectPath,
        DocumentType        
    }

    public class DictionarySnippetParam : Dictionary<ParameterName, object> { }
}
