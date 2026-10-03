using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.Markdown
{
    public class FromExternalDataToPlantumlMD : FromExternalDataToPlantuml, ICommandMD
    {
        public FromExternalDataToPlantumlMD(ILogger<FromExternalDataToPlantuml> logger)
            : base(logger)
        {
        }
    }
}
