using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Services;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI;

namespace MdExplorer.Test
{
    /// <summary>
    /// Programma di test per verificare i provider AI
    /// </summary>
    public class TestAiProviders
    {
        public static async Task RunTests()
        {
            Console.WriteLine("=== TEST AI MULTI-PROVIDER SYSTEM ===\n");

            // Setup DI container
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Add HttpClient
            services.AddHttpClient();

            // Add existing services
            services.AddSingleton<IGeminiApiService, GeminiApiService>();

            // Add multi-provider AI system
            services.AddSingleton<IAiProvider, OpenAiProvider>();
            services.AddSingleton<IAiProvider, GeminiProvider>();
            services.AddSingleton<IModelDiscoveryProvider, OpenAiModelDiscovery>();
            services.AddSingleton<IModelDiscoveryProvider, GeminiModelDiscovery>();

            var serviceProvider = services.BuildServiceProvider();

            // Get all providers
            var providers = serviceProvider.GetServices<IAiProvider>().ToList();
            var discoveryProviders = serviceProvider.GetServices<IModelDiscoveryProvider>().ToList();

            Console.WriteLine($"✅ Found {providers.Count} AI providers");
            Console.WriteLine($"✅ Found {discoveryProviders.Count} discovery providers\n");

            // Test 1: List all providers
            Console.WriteLine("--- TEST 1: Lista Provider ---");
            foreach (var provider in providers)
            {
                Console.WriteLine($"  Provider: {provider.GetName()}");
                Console.WriteLine($"    Type: {provider.GetProviderType()}");
                Console.WriteLine($"    Available: {provider.IsAvailable()}");

                var capabilities = provider.GetCapabilities();
                Console.WriteLine($"    Streaming: {capabilities.SupportsStreaming}");
                Console.WriteLine($"    Function Calling: {capabilities.SupportsFunctionCalling}");
                Console.WriteLine($"    Vision: {capabilities.SupportsVision}");
                Console.WriteLine($"    Max Input Tokens: {capabilities.MaxInputTokens}");
                Console.WriteLine();
            }

            // Test 2: Model Discovery
            Console.WriteLine("\n--- TEST 2: Model Discovery ---");
            foreach (var discoveryProvider in discoveryProviders)
            {
                Console.WriteLine($"\nProvider: {discoveryProvider.ProviderType}");
                Console.WriteLine($"  Supports Discovery: {discoveryProvider.SupportsDiscovery()}");

                try
                {
                    var models = await discoveryProvider.GetModelsAsync();
                    Console.WriteLine($"  Found {models.Count} models:");

                    foreach (var model in models.Take(5)) // Show first 5
                    {
                        Console.WriteLine($"    - {model.Name} ({model.Id})");
                        Console.WriteLine($"      Tokens: {model.InputTokenLimit} in / {model.OutputTokenLimit} out");
                        if (model.IsDeprecated)
                            Console.WriteLine($"      ⚠️ DEPRECATED");
                    }

                    if (models.Count > 5)
                        Console.WriteLine($"    ... and {models.Count - 5} more");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ Error: {ex.Message}");
                }
            }

            // Test 3: Test Gemini if available
            Console.WriteLine("\n--- TEST 3: Test Chat con Gemini ---");
            var geminiProvider = providers.FirstOrDefault(p => p.GetProviderType() == ProviderType.Gemini);

            if (geminiProvider == null)
            {
                Console.WriteLine("❌ Gemini provider not found");
            }
            else if (!geminiProvider.IsAvailable())
            {
                Console.WriteLine("⚠️ Gemini is not available (API key not configured)");
            }
            else
            {
                Console.WriteLine("✅ Gemini is available! Testing chat...");
                Console.WriteLine("Sending message: 'Ciao! Rispondi brevemente in italiano.'");

                try
                {
                    var response = await geminiProvider.ChatAsync("Ciao! Rispondi brevemente in italiano dicendo solo 'Test OK'.", "gemini-1.5-flash");
                    Console.WriteLine($"\n📨 Risposta da Gemini:");
                    Console.WriteLine($"{response}\n");
                    Console.WriteLine("✅ Test completato con successo!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error: {ex.Message}");
                }
            }

            Console.WriteLine("\n=== TEST COMPLETATI ===");
        }
    }
}
