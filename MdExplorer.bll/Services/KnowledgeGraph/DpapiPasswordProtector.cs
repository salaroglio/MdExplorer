using System;
using System.Security.Cryptography;
using System.Text;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    public class DpapiPasswordProtector : IPasswordProtector
    {
        public string Protect(string plaintext)
        {
            EnsureWindows();
            if (string.IsNullOrEmpty(plaintext)) return null;
            var data = Encoding.UTF8.GetBytes(plaintext);
#pragma warning disable CA1416 // protected by EnsureWindows() above
            var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
#pragma warning restore CA1416
            return Convert.ToBase64String(encrypted);
        }

        public string Unprotect(string ciphertext)
        {
            EnsureWindows();
            if (string.IsNullOrEmpty(ciphertext)) return null;
            var data = Convert.FromBase64String(ciphertext);
#pragma warning disable CA1416 // protected by EnsureWindows() above
            var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
#pragma warning restore CA1416
            return Encoding.UTF8.GetString(decrypted);
        }

        private static void EnsureWindows()
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException(
                    "DPAPI-based password protection is only available on Windows. " +
                    "Neo4j credentials persistence is not supported on this platform; " +
                    "the Knowledge Graph feature requires Windows for now.");
            }
        }
    }
}
