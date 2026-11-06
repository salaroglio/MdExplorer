using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.Markdown
{
    public class FromExternalDataToPlantumlMD : FromExternalDataToPlantuml, ICommandMD
    {
        public FromExternalDataToPlantumlMD(ILogger<FromExternalDataToPlantuml> logger, MdExplorer.Features.Agents.IOwnershipSource ownership = null)
            : base(logger, ownership)
        {
        }
    }
}
