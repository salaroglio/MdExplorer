using MdExplorer.Features.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.pdf
{
    public class RemoveCSSInlinePdf : RemoveCSSInline, ICommandPdf
    {
        public RemoveCSSInlinePdf(Microsoft.Extensions.Logging.ILogger<RemoveCSSInline> logger, Utilities.IHelper helper) : base(logger, helper)
        {
        }
    }
}
