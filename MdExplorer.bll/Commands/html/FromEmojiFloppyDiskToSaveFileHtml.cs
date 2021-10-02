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
    public class FromEmojiFloppyDiskToSaveFileHtml : FromEmojiFloppyDiskToSaveFile, ICommandHtml
    {
        public FromEmojiFloppyDiskToSaveFileHtml(ILogger<FromEmojiFloppyDiskToSaveFile> logger, IServerCache serverCache) : base(logger, serverCache)
        {
        }
    }
}
