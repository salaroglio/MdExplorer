using MdExplorer.Features.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class CommandRunner : ICommandRunner
    {
        private readonly ICommand[] _commands;

        public CommandRunner(ICommand[] commands)
        {            
            _commands = commands;
        }
        public string CreateMD(string markdownText)
        {
            foreach (var item in _commands)
            {
                markdownText = item.TransformInNewMDFromMD(markdownText);
            }
            return markdownText;
        }
    }
}
