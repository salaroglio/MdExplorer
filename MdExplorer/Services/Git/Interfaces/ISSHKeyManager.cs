using System.Collections.Generic;
using System.Threading.Tasks;

namespace MdExplorer.Services.Git.Interfaces
{
    /// <summary>
    /// SSH key information
    /// </summary>
    public class SSHKeyInfo
    {
        public string PrivateKeyPath { get; set; }
        public string PublicKeyPath { get; set; }
        public string KeyType { get; set; }
        public bool IsEncrypted { get; set; }
        public bool HasValidPermissions { get; set; }
        public string Fingerprint { get; set; }
    }

    /// <summary>
    /// Interface for managing SSH keys for Git authentication
    /// </summary>
    public interface ISSHKeyManager
    {
        /// <summary>
        /// Finds the best available SSH key for Git authentication
        /// </summary>
        /// <returns>Path to the private key file, or null if none found</returns>
        Task<string> FindSSHKeyAsync();

        /// <summary>
        /// Gets the passphrase for an encrypted SSH key
        /// </summary>
        /// <param name="keyPath">Path to the SSH key</param>
        /// <returns>Passphrase if available, empty string if not encrypted, null if unavailable</returns>
        Task<string> GetPassphraseAsync(string keyPath);

        /// <summary>
        /// Validates if an SSH key is valid and has proper permissions
        /// </summary>
        /// <param name="keyPath">Path to the SSH key</param>
        /// <returns>True if the key is valid and secure</returns>
        Task<bool> IsSSHKeyValidAsync(string keyPath);

        /// <summary>
        /// Gets a list of all available SSH keys
        /// </summary>
        /// <returns>List of SSH key information</returns>
        Task<IEnumerable<SSHKeyInfo>> GetAvailableSSHKeysAsync();

        /// <summary>
        /// Gets the default SSH directory path for the current user
        /// </summary>
        /// <returns>Path to the .ssh directory</returns>
        string GetSSHDirectory();

        /// <summary>
        /// Checks if SSH agent is running and has keys loaded
        /// </summary>
        /// <returns>True if SSH agent is available with keys</returns>
        Task<bool> IsSSHAgentAvailableAsync();

        /// <summary>
        /// Attempts to add a key to the SSH agent
        /// </summary>
        /// <param name="keyPath">Path to the private key</param>
        /// <param name="passphrase">Passphrase for the key (if encrypted)</param>
        /// <returns>True if successfully added to agent</returns>
        Task<bool> AddKeyToSSHAgentAsync(string keyPath, string passphrase = null);
    }
}