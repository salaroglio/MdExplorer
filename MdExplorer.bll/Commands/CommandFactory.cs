using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
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
        
        private static object lockGetCommands = new object();      
        private readonly IServer _server;

        private string _serverAddress { get; set; }


        public CommandFactory(IServer server)//IServerAddressesFeature serverAddresses
        {
            _server = server;
            var features = _server.Features;
            var addressesFeature = features.Get<IServerAddressesFeature>();
            
            var enumAddress = addressesFeature.Addresses.GetEnumerator();
            enumAddress.MoveNext();
            _serverAddress = enumAddress.Current;

            //this._serverAddress = serverAddresses.Addresses.First();
        }

        

        public ICommand[] GetCommands()
        {
            // E' facile, tiro su tutte le classi presenti dentro ICommand,
            // le instanzio e le butto dentro all'array
            lock (lockGetCommands)
            {
                var listToReturn = new List<ICommand>();
                var messengers = Assembly.GetExecutingAssembly().GetTypes().Where(_ => (typeof(ICommand).IsAssignableFrom(_) && !_.IsInterface));
                foreach (var item in messengers)
                {
                    listToReturn.Add((ICommand)Activator.CreateInstance(item,args: new string[] { _serverAddress }));
                }
                return listToReturn.ToArray();
            }            
        }

    }
}
