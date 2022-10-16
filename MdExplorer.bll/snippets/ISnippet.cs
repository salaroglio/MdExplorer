using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.snippets
{
    public interface ISnippet
    {
        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string GetSnippet();
        public void SetAssets(string assetsPath);
    }
}
