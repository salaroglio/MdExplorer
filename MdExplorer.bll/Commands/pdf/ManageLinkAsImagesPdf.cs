using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.pdf
{
    public class ManageLinkAsImagesPdf : ManageLinkAsImages, ICommandPdf
    {
        public ManageLinkAsImagesPdf(ILogger<ManageLinkAsImages> logger, IHelper helper) : base(logger, helper)
        {
        }
    }
}
