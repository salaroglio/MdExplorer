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
    /// Credential resolver that uses Windows Credential Manager for secure storage
    /// </summary>
    public class WindowsCredentialStoreResolver : ICredentialResolver
    {
        private readonly ILogger<WindowsCredentialStoreResolver> _logger;
        private const string CredentialTargetPrefix = "git:";

        public WindowsCredentialStoreResolver(ILogger<WindowsCredentialStoreResolver> logger)
        {
            _logger = logger;
        }

        public AuthenticationMethod GetAuthenticationMethod() => AuthenticationMethod.SystemCredentialStore;

        public int GetPriority() => 30;

        public bool CanResolveCredentials(string url, SupportedCredentialTypes types)
        {
            // Windows Credential Store supports username/password credentials
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
                var targetName = GetTargetName(url);
                _logger.LogDebug("Looking for credentials in Windows Credential Manager for: {TargetName}", targetName);

                var credential = await Task.Run(() => ReadCredential(targetName));
                
                if (credential != null)
                {
                    _logger.LogInformation("Successfully retrieved credentials from Windows Credential Manager for: {TargetName}", targetName);
                    return new UsernamePasswordCredentials
                    {
                        Username = credential.Username,
                        Password = credential.Password
                    };
                }

                // Try without port as fallback (for backwards compatibility)
                try 
                {
                    var uri = new Uri(url);
                    var targetWithoutPort = $"{CredentialTargetPrefix}{uri.Scheme}://{uri.Host}";
                    if (targetWithoutPort != targetName)
                    {
                        _logger.LogDebug("Trying fallback target without port: {TargetName}", targetWithoutPort);
                        credential = await Task.Run(() => ReadCredential(targetWithoutPort));
                        
                        if (credential != null)
                        {
                            _logger.LogInformation("Successfully retrieved credentials from Windows Credential Manager for fallback target: {TargetName}", targetWithoutPort);
                            return new UsernamePasswordCredentials
                            {
                                Username = credential.Username,
                                Password = credential.Password
                            };
                        }
                    }
                }
                catch 
                {
                    // Ignore fallback errors
                }

                // Try generic Git credential as last resort
                var genericTarget = "git:https://github.com";
                _logger.LogDebug("Trying generic Git credentials: {TargetName}", genericTarget);
                credential = await Task.Run(() => ReadCredential(genericTarget));
                
                if (credential != null)
                {
                    _logger.LogInformation("Using generic Git credentials from Windows Credential Manager");
                    return new UsernamePasswordCredentials
                    {
                        Username = credential.Username,
                        Password = credential.Password
                    };
                }

                _logger.LogDebug("No credentials found in Windows Credential Manager");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing Windows Credential Manager");
                return null;
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
            // Convert URL to a credential target name
            // Example: https://dbs-svn.dedagroup.it:8443/repo -> git:https://dbs-svn.dedagroup.it:8443
            try
            {
                var uri = new Uri(url);
                var hostWithPort = uri.Port != -1 && uri.Port != 80 && uri.Port != 443 
                    ? $"{uri.Host}:{uri.Port}" 
                    : uri.Host;
                return $"{CredentialTargetPrefix}{uri.Scheme}://{hostWithPort}";
            }
            catch
            {
                // Fallback for malformed URLs
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