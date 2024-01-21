using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.ActionLinkModifiers.Interfaces
{
    public interface IWorkLink
    {
        LinkDetail[] GetLinksFromMarkdown(string markdown);
        LinkDetail[] GetLinksFromFile(string filepath);
        void SetLinkIntoFile(string filepath, string oldLink, string newLink);
    }
}
