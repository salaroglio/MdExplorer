using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.html
{
    internal class ManageLinkAbsolutePathHtml : ManageLinkAbsolutePath, ICommandHtml
    {
        public ManageLinkAbsolutePathHtml(ILogger<ManageLinkAbsolutePath> logger) : base(logger)
        {
        }
    }
}
