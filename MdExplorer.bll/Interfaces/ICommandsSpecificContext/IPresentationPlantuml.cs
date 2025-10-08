using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Interfaces.ICommandsSpecificContext
{
    public interface IPresentationPlantuml
    {
        string GetPng(string markdown, string hashFile, int step, RequestInfo requestInfo);
        (string,int) GetPresentationSvg(string markdown, string hashFile, int step, RequestInfo requestInfo);
    }
}
