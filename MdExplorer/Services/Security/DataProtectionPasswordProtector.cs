using System;
using Microsoft.AspNetCore.DataProtection;
using MdExplorer.Features.Services.KnowledgeGraph;

namespace MdExplorer.Services.Security
{
    /// <summary>
    /// Cifratura a riposo dei segreti applicativi (token Atlassian, password Neo4j/Fuseki)
    /// per le piattaforme dove <b>DPAPI non esiste</b>: Linux, macOS, container Docker.
    /// <para>
    /// Perche' serve: <see cref="DpapiPasswordProtector"/> chiama <c>ProtectedData</c> e lancia
    /// <see cref="PlatformNotSupportedException"/> fuori da Windows. Essendo registrato senza
    /// condizione, in Docker faceva fallire sia il salvataggio del token Atlassian sia
    /// <b>ogni</b> lettura successiva (AtlassianController lo usa in entrambe le direzioni):
    /// Jira/Confluence via MCP erano quindi inutilizzabili headless.
    /// </para>
    /// <para>
    /// Il protettore DPAPI resta intatto e resta l'implementazione su Windows di proposito:
    /// sostituirlo renderebbe <b>illeggibili i segreti gia' salvati</b> sulle installazioni
    /// Windows esistenti. Stessa scelta, e stesse ragioni, di
    /// <see cref="Federation.RelayKeyProtector"/>.
    /// </para>
    /// <para>
    /// Onesta' sul livello di protezione: su Linux le chiavi di Data Protection stanno nella
    /// cartella dati di MdExplorer difese dai soli permessi del filesystem — chi legge quella
    /// cartella puo' risalire al segreto. Non e' un caveau, e' un miglioramento onesto rispetto
    /// al testo in chiaro. Le chiavi DEVONO essere persistite (lo fa <c>Startup</c>), altrimenti
    /// a ogni riavvio i token salvati diventerebbero indecifrabili.
    /// </para>
    /// </summary>
    public class DataProtectionPasswordProtector : IPasswordProtector
    {
        /// <summary>Scopo isolato: un testo cifrato qui non e' apribile da altri protettori.</summary>
        public const string Purpose = "MdExplorer.Secrets.PasswordProtector.v1";

        private readonly IDataProtector _protector;

        public DataProtectionPasswordProtector(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector(Purpose);
        }

        public string Protect(string plaintext)
            => string.IsNullOrEmpty(plaintext) ? null : _protector.Protect(plaintext);

        /// <summary>
        /// Decifra; propaga l'eccezione se il testo cifrato non e' apribile — mai un finto
        /// successo, mai un fallback silenzioso: un token illeggibile deve emergere come errore
        /// e non come "credenziali vuote" (che Atlassian riporterebbe come un opaco 401/403).
        /// </summary>
        public string Unprotect(string ciphertext)
            => string.IsNullOrEmpty(ciphertext) ? null : _protector.Unprotect(ciphertext);
    }
}
