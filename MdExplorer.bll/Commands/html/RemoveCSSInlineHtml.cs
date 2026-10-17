using MdExplorer.Features.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.html
{
    public class RemoveCSSInlineHtml: RemoveCSSInline,ICommandHtml
    {
        /// <summary>Runs on slide decks: removes the CSSInline blocks the diagrams and images read.</summary>
        public bool WorksInSlides => true;

        public RemoveCSSInlineHtml(Microsoft.Extensions.Logging.ILogger<RemoveCSSInline> logger, Utilities.IHelper helper) : base(logger, helper)
        {
        }
    }
}
