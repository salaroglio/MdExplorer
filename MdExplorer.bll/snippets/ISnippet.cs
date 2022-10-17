using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.snippets
{
    public interface ISnippet<T> where T : DictionarySnippetParam
    {
        public int Id { get; }
        public string Name { get; }
        public string Group { get; }
        public string GetSnippet(T parameters =null);
        public void SetAssets(string assetsPath);
    }
}
