using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Utilities
{
    public class HelperPdf : Helper, IHelperPdf
    {
        public HelperPdf(ILogger<Helper> logger) : base(logger)
        {
        }

        public override string GetBackPath(RequestInfo requestInfo)
        {
            return ".\\.md";
        }
    }
}
