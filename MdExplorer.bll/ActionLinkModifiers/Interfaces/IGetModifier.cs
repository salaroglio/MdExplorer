using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.ActionLinkModifiers.Interfaces
{
    public interface IGetModifier
    {
        LinkDetail[] GetLinks(string markdown);
        LinkDetail[] GetLinksFromFile(string filepath);
    }
}
