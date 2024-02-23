using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Features.Commands.html
{
    internal class FromEmojiCameraFlashToVersioningHtml : FromEmojiCameraFlashToVersioning, ICommandHtml
    {
        public FromEmojiCameraFlashToVersioningHtml(ILogger<FromEmojiCameraFlashToVersioning> logger, IServerCache serverCache) : base(logger, serverCache)
        {
        }
    }
}
