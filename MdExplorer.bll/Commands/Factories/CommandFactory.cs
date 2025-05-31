using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Features.Configuration.Interfaces;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// La factory ha lo scopo di creare gli array di command per conto della 
    /// dependency injection. Questa factory "aggiunge" ai commands una serie di parametri
    /// precotti tipo l'indirizzo del server di MdExplorer (è dinamico, ogni volta si inventa una porta nuova)
    /// il log. Per cui è possibile loggare dentro i commands
    /// ISession, per cui è possibile accedere al database SQLite ed avere le info che servono
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommandFactory<T> : ICommandFactory<T> where T : ICommand
    {

        private static object lockGetCommands = new object();
        private readonly IServer _server;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDALFactory<IUserSettingsDB> _dalFactory;
        private readonly PlantumlServer _plantumlServer;
        private readonly IHelper _helper;
        private readonly IServerCache _serverCache;

        private string _serverAddress { get; set; }

        public CommandFactory(IServer server,
                                IServiceProvider serviceProvider,
                                IDALFactory<IUserSettingsDB> dalFactory,
                                PlantumlServer plantumlServer,
                                IHelper helper,
                                IServerCache serverCache
            )//IServerAddressesFeature serverAddresses
        {
            _server = server;
            _serviceProvider = serviceProvider;
            _dalFactory = dalFactory;
            _plantumlServer = plantumlServer;
            _helper = helper;
            _serverCache = serverCache;
            var features = _server.Features;
            var addressesFeature = features.Get<IServerAddressesFeature>();

            var enumAddress = addressesFeature.Addresses.GetEnumerator();
            enumAddress.MoveNext();
            _serverAddress = enumAddress.Current;//.Replace("127.0.0.1","localhost");

        }

        public T[] GetCommands()
        {
            // E' facile, tiro su tutte le classi presenti dentro ICommand,
            // le instanzio e le butto dentro all'array
            lock (lockGetCommands)
            {
                var listToReturn = new List<T>();

                var commandInstances = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(_ => (typeof(T).IsAssignableFrom(_) && !_.IsInterface));

                foreach (var item in commandInstances)
                {

                    Type loggerType = typeof(ILogger<>);
                    Type genericLogger = loggerType.MakeGenericType(item);
                    var currentLogger = _serviceProvider.GetService(genericLogger);

                    var session = _dalFactory.OpenSession();

                    // preparazione dell'array di parameters da passare
                    var ctors = item.GetConstructors();
                    // al momento gestisco un solo constructor
                    var ctor = ctors[0];
                    var paramsTo = new List<object>();
                    foreach (var param in ctor.GetParameters())
                    {
                        if (param.Name == "ServerAddress")
                        {
                            paramsTo.Add(_serverAddress);

                        }
                        if (param.Name == "logger")
                        {
                            paramsTo.Add(currentLogger);

                        }
                        if (param.Name == "session")
                        {
                            paramsTo.Add(session);
                        }
                        if (param.Name == "plantumlServer")
                        {
                            paramsTo.Add(_plantumlServer);
                        }
                        if (param.Name == "helper")
                        {
                            paramsTo.Add(_helper);
                        }
                        if (param.Name == "serverCache")
                        {
                            paramsTo.Add(_serverCache);
                        }
                        if (param.Name == "extensionConfiguration")
                        {
                            var extensionConfig = _serviceProvider.GetService<IApplicationExtensionConfiguration>();
                            paramsTo.Add(extensionConfig);
                        }
                        
                    }
                    listToReturn.Add((T)Activator.CreateInstance(item, args: paramsTo.ToArray())); //new object[] { _serverAddress, currentLogger }
                }
                return listToReturn.ToArray();
            }
        }
    }
}
