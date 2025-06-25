using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Services.Git.CredentialStores
{
    /// <summary>
    /// ENHANCED VERSION: Tests multiple target formats to match Visual Studio Code behavior
    /// </summary>
    public class WindowsCredentialStoreResolver_Enhanced : ICredentialResolver
    {
        private readonly ILogger<WindowsCredentialStoreResolver> _logger;
        private const string CredentialTargetPrefix = "git:";

        public WindowsCredentialStoreResolver_Enhanced(ILogger<WindowsCredentialStoreResolver> logger)
        {
            _logger = logger;
        }

        public AuthenticationMethod GetAuthenticationMethod() => AuthenticationMethod.SystemCredentialStore;

        public int GetPriority() => 30;

        public bool CanResolveCredentials(string url, SupportedCredentialTypes types)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                   types.HasFlag(SupportedCredentialTypes.UsernamePassword);
        }

        public async Task<Credentials> ResolveCredentialsAsync(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _logger.LogWarning("Windows Credential Store resolver called on non-Windows platform");
                return null;
            }

            if (!types.HasFlag(SupportedCredentialTypes.UsernamePassword))
            {
                _logger.LogDebug("Windows Credential Store does not support the requested credential type");
                return null;
            }

            try
            {
                // Test multiple target name formats to find the one that works
                var targetFormats = GetAllPossibleTargetFormats(url);
                
                foreach (var targetName in targetFormats)
                {
                    _logger.LogDebug("Trying credential target: {TargetName}", targetName);
                    
                    var credential = await Task.Run(() => ReadCredential(targetName));
                    
                    if (credential != null)
                    {
                        _logger.LogInformation("✓ Successfully found credentials for target: {TargetName}", targetName);
                        return new UsernamePasswordCredentials
                        {
                            Username = credential.Username,
                            Password = credential.Password
                        };
                    }
                    else
                    {
                        _logger.LogDebug("✗ No credentials found for target: {TargetName}", targetName);
                    }
                }

                _logger.LogWarning("No credentials found for any target format for URL: {Url}", url);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing Windows Credential Manager");
                return null;
            }
        }

        private string[] GetAllPossibleTargetFormats(string url)
        {
            try
            {
                var uri = new Uri(url);
                var host = uri.Host;
                var hostWithPort = uri.Port != -1 && uri.Port != 80 && uri.Port != 443 
                    ? $"{uri.Host}:{uri.Port}" 
                    : uri.Host;
                
                // Return all possible formats that Visual Studio Code or Git might use
                return new[]
                {
                    // Our current format (without port)
                    $"{CredentialTargetPrefix}{uri.Scheme}://{host}",
                    
                    // With port included
                    $"{CredentialTargetPrefix}{uri.Scheme}://{hostWithPort}",
                    
                    // Without git prefix
                    $"{uri.Scheme}://{host}",
                    $"{uri.Scheme}://{hostWithPort}",
                    
                    // Host only variations
                    host,
                    hostWithPort,
                    
                    // Full URL as target
                    url,
                    $"{CredentialTargetPrefix}{url}",
                    
                    // Common Git credential manager formats
                    $"git:{uri.Scheme}://{host}",
                    $"git:{uri.Scheme}://{hostWithPort}",
                    
                    // Generic fallback (our current fallback)
                    "git:https://github.com"
                };
            }
            catch
            {
                // Fallback for malformed URLs
                return new[] 
                { 
                    $"{CredentialTargetPrefix}{url}",
                    url,
                    "git:https://github.com"
                };
            }
        }

        public async Task<bool> StoreCredentialsAsync(string url, string username, string password)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }

            try
            {
                var targetName = GetTargetName(url);
                var result = await Task.Run(() => WriteCredential(targetName, username, password));
                
                if (result)
                {
                    _logger.LogInformation("Successfully stored credentials in Windows Credential Manager");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing credentials in Windows Credential Manager");
                return false;
            }
        }

        private string GetTargetName(string url)
        {
            try
            {
                var uri = new Uri(url);
                return $"{CredentialTargetPrefix}{uri.Scheme}://{uri.Host}";
            }
            catch
            {
                return $"{CredentialTargetPrefix}{url}";
            }
        }

        private CredentialData ReadCredential(string targetName)
        {
            IntPtr credentialPtr = IntPtr.Zero;
            try
            {
                if (CredRead(targetName, CRED_TYPE.GENERIC, 0, out credentialPtr))
                {
                    var credential = Marshal.PtrToStructure<CREDENTIAL>(credentialPtr);
                    
                    var username = credential.UserName;
                    var password = Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / 2);
                    
                    return new CredentialData
                    {
                        Username = username,
                        Password = password
                    };
                }
                
                return null;
            }
            finally
            {
                if (credentialPtr != IntPtr.Zero)
                {
                    CredFree(credentialPtr);
                }
            }
        }

        private bool WriteCredential(string targetName, string username, string password)
        {
            var passwordBytes = Encoding.Unicode.GetBytes(password);
            
            var credential = new CREDENTIAL
            {
                Type = CRED_TYPE.GENERIC,
                TargetName = targetName,
                Comment = "Git credentials managed by MdExplorer",
                CredentialBlobSize = (uint)passwordBytes.Length,
                CredentialBlob = Marshal.AllocHGlobal(passwordBytes.Length),
                Persist = CRED_PERSIST.LOCAL_MACHINE,
                UserName = username
            };

            try
            {
                Marshal.Copy(passwordBytes, 0, credential.CredentialBlob, passwordBytes.Length);
                return CredWrite(ref credential, 0);
            }
            finally
            {
                if (credential.CredentialBlob != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(credential.CredentialBlob);
                }
            }
        }

        private class CredentialData
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        #region Windows Credential Manager P/Invoke
        // Same P/Invoke declarations as original...
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredRead(string target, CRED_TYPE type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredWrite([In] ref CREDENTIAL credential, [In] uint flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CredFree([In] IntPtr credential);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
        {
            public uint Flags;
            public CRED_TYPE Type;
            public string TargetName;
            public string Comment;
            public FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public CRED_PERSIST Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        private enum CRED_TYPE : uint
        {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            GENERIC_CERTIFICATE = 5,
            DOMAIN_EXTENDED = 6,
            MAXIMUM = 7,
            MAXIMUM_EX = 1007
        }

        private enum CRED_PERSIST : uint
        {
            SESSION = 1,
            LOCAL_MACHINE = 2,
            ENTERPRISE = 3
        }
        #endregion
    }
}