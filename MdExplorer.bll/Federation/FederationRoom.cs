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
        /// <c>.git</c>, protocollo, credenziali, slash finali): SSH e HTTPS dello stesso
        /// repo collassano sulla stessa stringa.
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
            normalized = Regex.Replace(normalized, @"^[^@/]+@", "");   // via user:pass@ (prima del primo slash)

            // Trim degli slash PRIMA di togliere .git, così anche 'repo.git/' collassa su 'repo'.
            normalized = normalized.TrimEnd('/');
            if (normalized.EndsWith(".git"))
                normalized = normalized.Substring(0, normalized.Length - 4);
            return normalized.TrimEnd('/');
        }

        /// <summary>Stanza = primi 16 hex di SHA256(origin normalizzato).</summary>
        public static string ComputeRoomId(string origin)
            => Hash(NormalizeGitOrigin(origin), 16);

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
