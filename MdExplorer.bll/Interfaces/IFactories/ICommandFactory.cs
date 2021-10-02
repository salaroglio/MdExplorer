using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public interface ICommandFactory<T>
    {
        T[] GetCommands();
        //string ServerAddress { get; set; }
    }
}
