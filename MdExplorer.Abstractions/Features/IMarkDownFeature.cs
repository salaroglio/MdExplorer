using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Features
{
    public interface IMarkDownFeature
    {
        Task<string> GetHtmlAsync(string filePath);
    }
}
