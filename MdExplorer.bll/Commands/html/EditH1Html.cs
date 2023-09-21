using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Interfaces.ICommandsSpecificContext;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Features.Commands.html
{
    internal class EditH1Html : EditH1, ICommandHtml
    {
        public EditH1Html(ILogger<EditH1> logger) : base(logger)
        {
        }
       
    }
}
