using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Middleware
{
    public class ServerAddressMiddleware
    {
        private readonly IFeatureCollection _features;
        private readonly IServer _server;

        public ServerAddressMiddleware(RequestDelegate _, IServer server)
        {
            _features = server.Features;
            _server = server;
        }

        public async Task Invoke(HttpContext context)
        {
            // fetch the addresses
            var addressFeature = _features.Get<IServerAddressesFeature>();
            var addresses = addressFeature.Addresses;
            
            // Write the addresses as a comma separated list
            await context.Response.WriteAsync(string.Join(",", addresses));
        }
    }
}
