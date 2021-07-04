using MdExplorer.Features.Interfaces;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class CommandFactory<T> : ICommandFactory<T> where T:ICommand
    {
        
        private static object lockGetCommands = new object();      
        private readonly IServer _server;
        private readonly IServiceProvider _serviceProvider;

        private string _serverAddress { get; set; }


        public CommandFactory(IServer server, IServiceProvider serviceProvider)//IServerAddressesFeature serverAddresses
        {
            _server = server;
            _serviceProvider = serviceProvider;
            var features = _server.Features;
            var addressesFeature = features.Get<IServerAddressesFeature>();
            
            var enumAddress = addressesFeature.Addresses.GetEnumerator();
            enumAddress.MoveNext();
            _serverAddress = enumAddress.Current;

        }

        

        public T[] GetCommands()
        {
            // E' facile, tiro su tutte le classi presenti dentro ICommand,
            // le instanzio e le butto dentro all'array
            lock (lockGetCommands)
            {
                var listToReturn = new List<T>();
                var messengers = Assembly.GetExecutingAssembly().GetTypes().Where(_ => (typeof(T).IsAssignableFrom(_) && !_.IsInterface));
                foreach (var item in messengers)
                {
                    Type loggerType = typeof(ILogger<>);
                    Type genericLogger = loggerType.MakeGenericType(item);
                    var currentLogger =  _serviceProvider.GetService(genericLogger);
                    listToReturn.Add((T)Activator.CreateInstance(item,args: new object[] { _serverAddress, currentLogger }));
                }
                return listToReturn.ToArray();
            }            
        }       
    }
}
