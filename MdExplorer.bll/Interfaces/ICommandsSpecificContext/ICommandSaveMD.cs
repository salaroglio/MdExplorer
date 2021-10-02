using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Interfaces
{
    public interface ICommandSaveMD<in T,  U>
    {
        (string, U) GetMDAndFileNameToSave(string markdown, T additionalInfo);
    }
}
