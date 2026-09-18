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
            services.AddScoped<ISearchService, SearchService>();
            services.AddSingleton<IMarkdownFtsService>(sp => new MarkdownFtsService(
                MdExplorer.Utilities.CrossPlatformPath.GetAppDataPath(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MarkdownFtsService>>()));
            services.AddSingleton<ITextFtsService>(sp => new TextFtsService(
                MdExplorer.Utilities.CrossPlatformPath.GetAppDataPath(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TextFtsService>>()));
            return services;
        }
    }
}
