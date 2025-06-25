using System;
using System.Diagnostics;
using LibGit2Sharp;
using MdExplorer.Services.Git.CredentialStores;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Testing
{
    /// <summary>
    /// Test class to compare LibGit2Sharp credential resolution with native Git
    /// </summary>
    public class GitCredentialTest
    {
        private readonly ILogger<GitCredentialTest> _logger;
        private readonly string _testUrl = "https://dbs-svn.dedagroup.it:8443/scm/repo/BCCSi/bccsi-e2e-test";
        private readonly string _repositoryPath = @"C:\sviluppo\dedagroup\BCCEnd2End";

        public GitCredentialTest(ILogger<GitCredentialTest> logger)
        {
            _logger = logger;
        }

        public void RunComparisonTest()
        {
            _logger.LogInformation("Starting Git credential comparison test");
            _logger.LogInformation("Test URL: {Url}", _testUrl);
            _logger.LogInformation("Repository Path: {Path}", _repositoryPath);

            Console.WriteLine("==============================================");
            Console.WriteLine("Git Credential Resolution Comparison Test");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            // Test 1: Native Git credential helper
            TestNativeGitCredential();
            Console.WriteLine();

            // Test 2: Our WindowsCredentialStoreResolver
            TestWindowsCredentialStoreResolver();
            Console.WriteLine();

            // Test 3: LibGit2Sharp direct credential callback
            TestLibGit2SharpCredentialCallback();
            Console.WriteLine();

            // Test 4: Different target name formats
            TestDifferentTargetFormats();
        }

        private void TestNativeGitCredential()
        {
            Console.WriteLine("--- Test 1: Native Git Credential Helper ---");
            
            try {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "credential fill",
                    WorkingDirectory = _repositoryPath,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    // Send the URL to git credential fill
                    process.StandardInput.WriteLine($"url={_testUrl}");
                    process.StandardInput.Close();

                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    
                    process.WaitForExit();

                    Console.WriteLine($"Exit Code: {process.ExitCode}");
                    Console.WriteLine($"Output: {output}");
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error: {error}");
                    }

                    if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine("✓ Native Git credential helper SUCCESS");
                    }
                    else
                    {
                        Console.WriteLine("✗ Native Git credential helper FAILED");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error running native Git credential helper: {ex.Message}");
            }
        }

        private void TestWindowsCredentialStoreResolver()
        {
            Console.WriteLine("--- Test 2: Our WindowsCredentialStoreResolver ---");
            
            try 
            {
                var logger = LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<WindowsCredentialStoreResolver>();
                
                var resolver = new WindowsCredentialStoreResolver(logger);
                
                Console.WriteLine($"Can resolve credentials: {resolver.CanResolveCredentials(_testUrl, SupportedCredentialTypes.UsernamePassword)}");
                
                var credentials = resolver.ResolveCredentialsAsync(_testUrl, null, SupportedCredentialTypes.UsernamePassword).Result;
                
                if (credentials != null)
                {
                    Console.WriteLine("✓ WindowsCredentialStoreResolver SUCCESS");
                    Console.WriteLine($"Credential type: {credentials.GetType().Name}");
                    
                    if (credentials is UsernamePasswordCredentials upCreds)
                    {
                        Console.WriteLine($"Username: {upCreds.Username}");
                        Console.WriteLine($"Password: {new string('*', upCreds.Password?.Length ?? 0)}");
                    }
                }
                else
                {
                    Console.WriteLine("✗ WindowsCredentialStoreResolver FAILED - No credentials returned");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error in WindowsCredentialStoreResolver: {ex.Message}");
            }
        }

        private void TestLibGit2SharpCredentialCallback()
        {
            Console.WriteLine("--- Test 3: LibGit2Sharp Direct Credential Callback ---");
            
            try
            {
                using (var repo = new Repository(_repositoryPath))
                {
                    var callbackInvoked = false;
                    string callbackUrl = null;
                    string callbackUser = null;
                    SupportedCredentialTypes callbackTypes = SupportedCredentialTypes.Default;

                    var fetchOptions = new FetchOptions
                    {
                        CredentialsProvider = (url, usernameFromUrl, types) =>
                        {
                            callbackInvoked = true;
                            callbackUrl = url;
                            callbackUser = usernameFromUrl;
                            callbackTypes = types;

                            Console.WriteLine($"Callback invoked:");
                            Console.WriteLine($"  URL: {url}");
                            Console.WriteLine($"  Username from URL: {usernameFromUrl}");
                            Console.WriteLine($"  Supported types: {types}");

                            // Try to resolve using our resolver
                            var logger = LoggerFactory.Create(builder => builder.AddConsole())
                                .CreateLogger<WindowsCredentialStoreResolver>();
                            var resolver = new WindowsCredentialStoreResolver(logger);
                            
                            var creds = resolver.ResolveCredentialsAsync(url, usernameFromUrl, types).Result;
                            
                            if (creds != null)
                            {
                                Console.WriteLine("  ✓ Credentials resolved in callback");
                                return creds;
                            }
                            else
                            {
                                Console.WriteLine("  ✗ No credentials resolved in callback");
                                return null;
                            }
                        }
                    };

                    try
                    {
                        // Try a simple fetch to trigger the credential callback
                        var remote = repo.Network.Remotes["origin"];
                        if (remote != null)
                        {
                            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                            Commands.Fetch(repo, remote.Name, refSpecs, fetchOptions, "Test fetch");
                            Console.WriteLine("✓ LibGit2Sharp fetch SUCCESS");
                        }
                        else
                        {
                            Console.WriteLine("✗ No 'origin' remote found");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ LibGit2Sharp fetch FAILED: {ex.Message}");
                        
                        if (callbackInvoked)
                        {
                            Console.WriteLine("But credential callback was invoked, indicating LibGit2Sharp tried to authenticate");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error in LibGit2Sharp test: {ex.Message}");
            }
        }

        private void TestDifferentTargetFormats()
        {
            Console.WriteLine("--- Test 4: Different Target Name Formats ---");
            
            var targetFormats = new[]
            {
                "git:https://dbs-svn.dedagroup.it",
                "git:https://dbs-svn.dedagroup.it:8443", 
                "https://dbs-svn.dedagroup.it",
                "https://dbs-svn.dedagroup.it:8443",
                "dbs-svn.dedagroup.it",
                "dbs-svn.dedagroup.it:8443",
                "git:https://github.com"
            };

            foreach (var target in targetFormats)
            {
                Console.WriteLine($"Testing target format: {target}");
                
                // Test if this target exists in Windows Credential Manager
                var processInfo = new ProcessStartInfo
                {
                    FileName = "cmdkey",
                    Arguments = $"/list:\"{target}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                try
                {
                    using (var process = Process.Start(processInfo))
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        var error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (process.ExitCode == 0)
                        {
                            Console.WriteLine($"  ✓ FOUND: {target}");
                            Console.WriteLine($"  Details: {output.Trim()}");
                        }
                        else
                        {
                            Console.WriteLine($"  ✗ NOT FOUND: {target}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ ERROR testing {target}: {ex.Message}");
                }
            }
        }
    }
}