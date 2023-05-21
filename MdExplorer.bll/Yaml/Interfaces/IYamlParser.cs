using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Yaml.Interfaces
{
    public interface IYamlParser<T>
    {
        T GetDescriptor(string markdown);
        string SerializeDescriptor(T descriptor);
        string SetDescriptor(T descriptor,string markdown);
    }
}
