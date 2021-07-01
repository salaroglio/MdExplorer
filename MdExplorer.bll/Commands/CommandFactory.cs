using Microsoft.AspNetCore.Hosting.Server.Features;
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
        private readonly string _serverAddress;
        private static object lockGetCommands;

        public CommandFactory(IServerAddressesFeature serverAddresses)
        {
            this._serverAddress = serverAddresses.Addresses.First();
        }
        
        

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
