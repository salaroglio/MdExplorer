using System.Threading.Tasks;
using MdExplorer.AI.Abstractions.Models;

namespace MdExplorer.AI.Abstractions.Services
{
    /// <summary>
    /// Validatore licenze per AI features.
    /// </summary>
    public interface ILicenseValidator
    {
        /// <summary>
        /// Valida la licenza corrente (online + fallback offline).
        /// </summary>
        Task<LicenseStatus> ValidateAsync();

        /// <summary>
        /// Attiva una nuova licenza con la chiave fornita.
        /// </summary>
        Task<bool> ActivateLicenseAsync(string licenseKey);

        /// <summary>
        /// Disattiva la licenza corrente.
        /// </summary>
        Task<bool> DeactivateLicenseAsync();

        /// <summary>
        /// Ottiene informazioni sulla licenza senza validazione online.
        /// </summary>
        Task<LicenseStatus> GetCachedLicenseStatusAsync();
    }
}
