using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Reveal.Interfaces
{
    public interface IYamlParser<T>
    {
        T GetDescriptor(string markdown);
    }
}
