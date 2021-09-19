using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.html
{
    public class FromEmojiCalendarToDatepickerHtml : FromEmojiCalendarToDatepicker, ICommandHtml
    {
        public FromEmojiCalendarToDatepickerHtml(ILogger<FromEmojiCalendarToDatepickerHtml> logger, IServerCache serverCache) : base(logger, serverCache)
        {
        }
    }
}
