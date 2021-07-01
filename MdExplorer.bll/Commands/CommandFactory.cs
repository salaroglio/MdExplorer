using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class CommandFactory : ICommandFactory
    {
        static object lockGetCommands;
        public ICommand[] GetCommands()
        {
            // E' facile, tiro su tutte le classi presenti dentro ICommand,
            // le instanzio e le butto dentro all'array
            lock (lockGetCommands)
            {
                var listToReturn = new List<ICommand>();
                var messengers = Assembly.GetExecutingAssembly().GetTypes().Where(_ => (typeof(ICommand).IsAssignableFrom(_)));
                foreach (var item in messengers)
                {
                    listToReturn.Add((ICommand)Activator.CreateInstance(item));
                }
                return listToReturn.ToArray();
            }            
        }
    }
}
