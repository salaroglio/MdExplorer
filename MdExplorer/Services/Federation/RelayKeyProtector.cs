using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Federation
{
    /// <summary>
    /// Cifratura a riposo della API key del relay.
    /// <para>
    /// NON riusa <c>IPasswordProtector</c> (Neo4j/Fuseki/Jira): quello è basato su <b>DPAPI</b> e
    /// lancia <see cref="PlatformNotSupportedException"/> fuori da Windows — verificato eseguendo
    /// il codice, non leggendolo. Siccome la città degli agenti gira anche su Linux, qui si usa
    /// <b>Data Protection</b> di ASP.NET Core, che è cross-platform. Il protettore esistente resta
    /// intatto di proposito: cambiarlo renderebbe illeggibili i segreti già salvati su Windows.
    /// </para>
    /// <para>
    /// Onestà sul livello di protezione: su Windows le chiavi di Data Protection sono a loro volta
    /// protette con DPAPI; su Linux stanno nel profilo utente difese dai permessi del filesystem.
    /// Su Linux, quindi, chi legge la home dell'utente può risalire alla chiave — esattamente come
    /// poteva leggerla in <c>appsettings.Development.json</c>, che è ciò che questo sostituisce.
    /// Non è un caveau, è un miglioramento onesto rispetto al testo in chiaro.
    /// </para>
    /// </summary>
    public interface IRelayKeyProtector
    {
        string Protect(string plaintext);

        /// <summary>Decifra; solleva se il testo cifrato non è apribile (mai un finto successo).</summary>
        string Unprotect(string ciphertext);
    }

    public class RelayKeyProtector : IRelayKeyProtector
    {
        /// <summary>Scopo isolato: un testo cifrato per il relay non è apribile da altri protettori.</summary>
        public const string Purpose = "MdExplorer.Federation.RelayApiKey.v1";

        private readonly IDataProtector _protector;

        public RelayKeyProtector(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector(Purpose);
        }

        public string Protect(string plaintext)
            => string.IsNullOrEmpty(plaintext) ? null : _protector.Protect(plaintext);

        public string Unprotect(string ciphertext)
            => string.IsNullOrEmpty(ciphertext) ? null : _protector.Unprotect(ciphertext);
    }
}
