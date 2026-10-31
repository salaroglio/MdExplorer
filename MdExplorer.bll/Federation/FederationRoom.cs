using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Federation
{
    /// <summary>
    /// Identità deterministiche della federazione (§12.2 / Fase 6b): la <b>stanza</b> di un
    /// repo e l'<b>id del padrone</b>, ricavati con lo stesso schema della Team Chat
    /// (<c>GitChatController.ComputeRoomId</c>/<c>ComputeUserId</c>) così due città sullo
    /// stesso repo finiscono nella stessa stanza. Logica pura → testabile e condivisa,
    /// unico punto di verità dell'algoritmo.
    /// </summary>
    public static class FederationRoom
    {
        /// <summary>
        /// Normalizza un URL git a una forma canonica (host/percorso, minuscolo, senza
        /// <c>.git</c>, protocollo, credenziali, slash finali): SSH e HTTPS dello stesso repo
        /// collassano sulla stessa stringa. <b>Unico punto di verità</b> — identico all'algoritmo
        /// della Team Chat (<c>GitChatController.NormalizeGitUrl</c> delega qui), così le due
        /// città sullo stesso repo finiscono nella STESSA stanza (l'ordine dei passi è quello
        /// storico della chat: <c>.git</c> tolto PRIMA del trim slash, auth <c>^[^@]+@</c>).
        /// </summary>
        public static string NormalizeGitOrigin(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            var normalized = url.Trim().ToLowerInvariant();

            // git@host:user/repo -> host/user/repo
            var ssh = Regex.Match(normalized, @"^git@([^:]+):(.+)$");
            if (ssh.Success)
                normalized = $"{ssh.Groups[1].Value}/{ssh.Groups[2].Value}";

            normalized = Regex.Replace(normalized, @"^(https?|ssh|git)://", "");
            normalized = Regex.Replace(normalized, @"^[^@]+@", "");   // via user:pass@
            // Trim slash PRIMA e DOPO lo strip di .git, così sia 'repo.git' sia 'repo.git/'
            // (slash dopo .git) collassano su 'repo' — più robusto del solo pre-strip.
            normalized = normalized.TrimEnd('/');
            if (normalized.EndsWith(".git"))
                normalized = normalized.Substring(0, normalized.Length - 4);
            return normalized.TrimEnd('/');
        }

        /// <summary>Prefisso delle stanze ricavate dal segreto: il relay le lascia registrare senza chiave master.</summary>
        public const string RoomIdPrefix = "r2-";

        /// <summary>
        /// Stanza = <c>r2-</c> + primi 32 hex di HMAC-SHA256(chiave = room secret, "mdroom-v2|" + origin normalizzato).
        /// Senza il segreto non si calcola: nessuno può occupare la stanza di un progetto prima di chi ha il repo
        /// (sprint Telecomando Telegram, C1). L'origin resta dentro, così un fork con lo stesso segreto ha la sua stanza.
        /// </summary>
        public static string ComputeRoomId(string origin, string roomSecret)
            => RoomIdFromNormalized(NormalizeGitOrigin(origin), roomSecret);

        /// <summary>Primitiva condivisa: stanza da una stringa GIÀ normalizzata (la chat la usa così).</summary>
        public static string RoomIdFromNormalized(string normalizedOrigin, string roomSecret)
        {
            if (string.IsNullOrWhiteSpace(roomSecret))
                throw new ArgumentException(MdExplorer.Features.Services.UiText.Say(
                    "La stanza si ricava dal segreto del progetto: il segreto manca.",
                    "The room is derived from the project secret: the secret is missing."), nameof(roomSecret));
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(roomSecret.Trim()));
            var mac = hmac.ComputeHash(Encoding.UTF8.GetBytes("mdroom-v2|" + (normalizedOrigin ?? string.Empty)));
            return RoomIdPrefix + Convert.ToHexString(mac).ToLowerInvariant().Substring(0, 32);
        }

        /// <summary>Id del padrone = primi 12 hex di SHA256(email lowercased).</summary>
        public static string ComputeUserId(string email)
            => Hash((email ?? string.Empty).ToLowerInvariant().Trim(), 12);

        private static string Hash(string input, int hexLen)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            return hex.Substring(0, hexLen);
        }
    }
}
