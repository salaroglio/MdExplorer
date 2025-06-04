using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MdExplorer.Services.Git.Interfaces;
using System.Runtime.InteropServices;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Extension methods for configuring Git services in dependency injection
    /// </summary>
    public static class GitServiceCollectionExtensions
    {
        /// <summary>
        /// Adds modern Git services with native credential management to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">Configuration for Git options</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddModernGitServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register core Git service
            services.AddScoped<IModernGitService, ModernGitService>();
            
            // Register SSH key manager
            services.AddScoped<ISSHKeyManager, SSHKeyManager>();
            
            // Register credential resolvers in priority order
            services.AddScoped<ICredentialResolver, SSHKeyCredentialResolver>();
            services.AddScoped<ICredentialResolver, GitCredentialHelperResolver>();
            
            // Add platform-specific credential stores
            AddPlatformSpecificServices(services);
            
            // Configure options from appsettings.json
            services.Configure<GitAuthenticationOptions>(configuration.GetSection("Git:Authentication"));
            services.Configure<GitOperationOptions>(configuration.GetSection("Git:Operations"));
            
            return services;
        }

        /// <summary>
        /// Adds modern Git services with custom configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureOptions">Action to configure Git options</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddModernGitServices(this IServiceCollection services, 
            System.Action<GitAuthenticationOptions> configureOptions = null)
        {
            // Register core services
            services.AddScoped<IModernGitService, ModernGitService>();
            services.AddScoped<ISSHKeyManager, SSHKeyManager>();
            
            // Register credential resolvers
            services.AddScoped<ICredentialResolver, SSHKeyCredentialResolver>();
            services.AddScoped<ICredentialResolver, GitCredentialHelperResolver>();
            
            // Add platform-specific services
            AddPlatformSpecificServices(services);
            
            // Configure options
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            
            return services;
        }

        private static void AddPlatformSpecificServices(IServiceCollection services)
        {
            // Add platform-specific credential resolvers based on the current OS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddScoped<ICredentialResolver, CredentialStores.WindowsCredentialStoreResolver>();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                services.AddScoped<ICredentialResolver, CredentialStores.MacOSKeychainResolver>();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                services.AddScoped<ICredentialResolver, CredentialStores.LinuxSecretServiceResolver>();
            }
        }
    }

    /// <summary>
    /// Configuration options for Git authentication
    /// </summary>
    public class GitAuthenticationOptions
    {
        /// <summary>
        /// Preferred authentication methods in order of preference
        /// </summary>
        public string[] PreferredMethods { get; set; } = { "SSH", "GitCredentialHelper", "SystemCredentialStore" };

        /// <summary>
        /// Whether to allow user prompts as a fallback
        /// </summary>
        public bool AllowUserPrompt { get; set; } = false;

        /// <summary>
        /// SSH key search paths
        /// </summary>
        public string[] SSHKeySearchPaths { get; set; } = 
        {
            "~/.ssh/id_ed25519",
            "~/.ssh/id_ecdsa",
            "~/.ssh/id_rsa"
        };

        /// <summary>
        /// Timeout for credential helper execution in seconds
        /// </summary>
        public int CredentialHelperTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Whether to cache resolved credentials
        /// </summary>
        public bool CacheCredentials { get; set; } = true;

        /// <summary>
        /// Credential cache timeout in minutes
        /// </summary>
        public int CacheTimeoutMinutes { get; set; } = 15;
    }

    /// <summary>
    /// Configuration options for Git operations
    /// </summary>
    public class GitOperationOptions
    {
        /// <summary>
        /// Default author configuration
        /// </summary>
        public DefaultAuthorOptions DefaultAuthor { get; set; } = new DefaultAuthorOptions();

        /// <summary>
        /// Pull strategy (Merge, Rebase, etc.)
        /// </summary>
        public string PullStrategy { get; set; } = "Merge";

        /// <summary>
        /// Push strategy
        /// </summary>
        public string PushStrategy { get; set; } = "Simple";

        /// <summary>
        /// Default remote name
        /// </summary>
        public string DefaultRemote { get; set; } = "origin";

        /// <summary>
        /// Operation timeout in seconds
        /// </summary>
        public int OperationTimeoutSeconds { get; set; } = 300; // 5 minutes
    }

    /// <summary>
    /// Default author configuration options
    /// </summary>
    public class DefaultAuthorOptions
    {
        /// <summary>
        /// Whether to use Git config for author info
        /// </summary>
        public bool UseGitConfig { get; set; } = true;

        /// <summary>
        /// Fallback name if Git config is not available
        /// </summary>
        public string FallbackName { get; set; } = "MdExplorer User";

        /// <summary>
        /// Fallback email if Git config is not available
        /// </summary>
        public string FallbackEmail { get; set; } = "user@mdexplorer.local";
    }
}