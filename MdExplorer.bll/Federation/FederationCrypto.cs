using System;
using System.Security.Cryptography;
using System.Text;

namespace MdExplorer.Features.Federation
{
    /// <summary>
    /// Sollevata quando una busta federata non può essere aperta: versione ignota, formato
    /// corrotto, o <b>tag GCM non valido</b> (manomissione / secret sbagliato). Fail-loud:
    /// non si ritorna mai un testo "quasi giusto".
    /// </summary>
    public sealed class FederationCryptoException : Exception
    {
        public FederationCryptoException(string message) : base(message) { }
        public FederationCryptoException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Cifratura end-to-end del payload federato (§12.6 / Fase 6b, R15). Il relay vede solo
    /// <b>buste chiuse</b>: la chiave non lascia mai le città. Schema (versionato <c>v1</c>):
    /// <list type="bullet">
    /// <item>chiave = <b>HKDF-SHA256</b>(ikm = room secret, salt = roomId, info = "mdfed-v1") → 32 byte;</item>
    /// <item><b>AES-256-GCM</b>, <b>nonce 96-bit casuale per messaggio</b>, tag 128-bit;</item>
    /// <item>busta = <c>mdfed.v1.</c> + base64(nonce ‖ tag ‖ ciphertext).</item>
    /// </list>
    /// Interoperabilità: stesso room secret (condiviso via git) + stesso roomId → stessa
    /// chiave derivata su tutte le città, quindi ognuna decifra ciò che le altre cifrano.
    /// Deterministica solo nella chiave: il nonce casuale rende ogni busta diversa.
    /// </summary>
    public static class FederationCrypto
    {
        public const string Version = "v1";
        private const string Prefix = "mdfed.v1.";
        private static readonly byte[] Info = Encoding.UTF8.GetBytes("mdfed-v1");

        private const int KeyLen = 32;    // AES-256
        private const int NonceLen = 12;  // 96-bit, raccomandato per GCM
        private const int TagLen = 16;    // 128-bit

        /// <summary>
        /// Deriva la chiave simmetrica dal room secret e dal roomId (salt). Punto unico di
        /// verità: la stessa derivazione DEVE valere su tutte le città per interoperare.
        /// </summary>
        public static byte[] DeriveKey(string roomSecret, string roomId)
        {
            if (string.IsNullOrEmpty(roomSecret))
                throw new ArgumentException("room secret assente: senza segreto non si può derivare la chiave.", nameof(roomSecret));
            if (string.IsNullOrEmpty(roomId))
                throw new ArgumentException("roomId assente: serve come salt della derivazione.", nameof(roomId));

            var ikm = Encoding.UTF8.GetBytes(roomSecret);
            var salt = Encoding.UTF8.GetBytes(roomId);
            return HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, KeyLen, salt, Info);
        }

        /// <summary>Cifra <paramref name="plaintext"/> in una busta <c>mdfed.v1....</c>.</summary>
        public static string Encrypt(string roomSecret, string roomId, string plaintext)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));

            var key = DeriveKey(roomSecret, roomId);
            var plainBytes = Encoding.UTF8.GetBytes(plaintext);

            var nonce = RandomNumberGenerator.GetBytes(NonceLen);
            var cipher = new byte[plainBytes.Length];
            var tag = new byte[TagLen];

            using (var gcm = new AesGcm(key, TagLen))
                gcm.Encrypt(nonce, plainBytes, cipher, tag);

            // busta = nonce ‖ tag ‖ ciphertext
            var envelope = new byte[NonceLen + TagLen + cipher.Length];
            Buffer.BlockCopy(nonce, 0, envelope, 0, NonceLen);
            Buffer.BlockCopy(tag, 0, envelope, NonceLen, TagLen);
            Buffer.BlockCopy(cipher, 0, envelope, NonceLen + TagLen, cipher.Length);

            return Prefix + Convert.ToBase64String(envelope);
        }

        /// <summary>
        /// Apre una busta <c>mdfed.v1....</c>. Fail-loud: versione ignota, formato corrotto o
        /// tag non valido (manomissione / secret errato) → <see cref="FederationCryptoException"/>.
        /// </summary>
        public static string Decrypt(string roomSecret, string roomId, string envelope)
        {
            if (string.IsNullOrEmpty(envelope))
                throw new FederationCryptoException("Busta vuota.");
            if (!envelope.StartsWith(Prefix, StringComparison.Ordinal))
                throw new FederationCryptoException($"Versione della busta non riconosciuta: attesa '{Version}'. Aggiornare la città più vecchia.");

            byte[] raw;
            try { raw = Convert.FromBase64String(envelope.Substring(Prefix.Length)); }
            catch (FormatException ex) { throw new FederationCryptoException("Busta corrotta: base64 non valido.", ex); }

            if (raw.Length < NonceLen + TagLen)
                throw new FederationCryptoException("Busta troppo corta: nonce/tag mancanti.");

            var nonce = new byte[NonceLen];
            var tag = new byte[TagLen];
            var cipher = new byte[raw.Length - NonceLen - TagLen];
            Buffer.BlockCopy(raw, 0, nonce, 0, NonceLen);
            Buffer.BlockCopy(raw, NonceLen, tag, 0, TagLen);
            Buffer.BlockCopy(raw, NonceLen + TagLen, cipher, 0, cipher.Length);

            var key = DeriveKey(roomSecret, roomId);
            var plain = new byte[cipher.Length];
            try
            {
                using var gcm = new AesGcm(key, TagLen);
                gcm.Decrypt(nonce, cipher, tag, plain);
            }
            catch (AuthenticationTagMismatchException ex)
            {
                // Tag non valido: la busta è stata manomessa OPPURE il room secret è sbagliato.
                // In entrambi i casi non c'è un plaintext fidato da restituire.
                throw new FederationCryptoException("Tag di autenticazione non valido: busta manomessa o room secret errato.", ex);
            }

            return Encoding.UTF8.GetString(plain);
        }
    }
}
