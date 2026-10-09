using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.pdf
{
    public class FromExternalDataToPlantumlPdf : FromExternalDataToPlantuml, ICommandPdf
    {
        public FromExternalDataToPlantumlPdf(ILogger<FromExternalDataToPlantuml> logger)
            : base(logger)
        {
        }
    }
}
