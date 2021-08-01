using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.ActionLinkModifiers.Interfaces
{
    public interface IGetModifier
    {
        string[] GetLinks(string markdown);
    }
}
