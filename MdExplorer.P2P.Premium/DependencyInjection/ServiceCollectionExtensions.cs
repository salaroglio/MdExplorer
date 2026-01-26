using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MdExplorer.P2P.Premium.Services;

namespace MdExplorer.P2P.Premium.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering P2P Premium services in DI container.
    /// This class is called via reflection from Startup.cs when the Premium DLL is detected.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private const string P2P_API_BASE_URL = "http://127.0.0.1:48124";

        /// <summary>
        /// Add P2P Premium services to the service collection.
        /// This method is invoked via reflection from MdExplorer.Service/Startup.cs
        /// </summary>
        /// <param name="services">Service collection</param>
        public static IServiceCollection AddP2PServices(this IServiceCollection services)
        {
            // Register HttpClient for P2P service proxy
            services.AddHttpClient<IP2PService, P2PServiceProxy>(client =>
            {
                client.BaseAddress = new Uri(P2P_API_BASE_URL);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }

        /// <summary>
        /// Get the assembly containing P2P Premium controllers for dynamic registration.
        /// Used by Startup.cs to add controllers from this assembly.
        /// </summary>
        public static Assembly GetPremiumAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        /// <summary>
        /// Check if P2P service is available (helper method)
        /// </summary>
        public static async Task<bool> IsP2PServiceAvailableAsync(IServiceProvider serviceProvider)
        {
            try
            {
                var p2pService = serviceProvider.GetService<IP2PService>();
                if (p2pService == null) return false;
                return await p2pService.IsAvailableAsync();
            }
            catch
            {
                return false;
            }
        }
    }
}
