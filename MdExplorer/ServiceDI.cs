using MdExplorer.Service.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services;

namespace MdExplorer.Service
{
   
    public static class ServiceDI
    {
        public static IServiceCollection AddServiceFeatures(this IServiceCollection services)
        {
            services.AddSingleton<ProcessUtil>();
            services.AddSingleton<IMdIgnoreService, MdIgnoreService>();
            return services;
        }
    }
}
