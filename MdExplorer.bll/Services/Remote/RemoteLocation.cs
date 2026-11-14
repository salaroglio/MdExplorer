using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.RemoteFolders
{
    /// <summary>
    /// Una cartella su un server raggiunto via SSH (sprint 2026-10-21-Cartella-Remota-Via-SSH, P7): <c>ssh://host/percorso</c>,
    /// dove <c>host</c> è quello che si scriverebbe dopo <c>ssh</c> — un alias di <c>~/.ssh/config</c>, <c>utente@server</c>,
    /// con <c>:porta</c> se serve — e il percorso è assoluto sul server (Linux, separatore <c>/</c>). Sta nello UserDB al
    /// posto del percorso locale di una radice esterna.
    /// </summary>
    public sealed class RemoteLocation : IEquatable<RemoteLocation>
    {
        public const string Scheme = "ssh://";

        // Niente spazi, virgolette o caratteri di shell; mai un trattino iniziale (diventerebbe un'opzione di ssh).
        private static readonly Regex HostPattern = new(@"^([A-Za-z0-9_][A-Za-z0-9._\-]*@)?[A-Za-z0-9_][A-Za-z0-9._\-]*(:[0-9]{1,5})?$", RegexOptions.CultureInvariant);

        public string Host { get; }
        /// <summary>Assoluto sul server, normalizzato, senza <c>/</c> finale (tranne la radice <c>/</c>).</summary>
        public string Path { get; }

        private RemoteLocation(string host, string path) { Host = host; Path = path; }

        public static bool IsRemote(string location) => location != null && location.StartsWith(Scheme, StringComparison.Ordinal);

        public static RemoteLocation Parse(string location)
        {
            if (!IsRemote(location))
                throw new FormatException(UiText.Say($"'{location}' non è una cartella remota (deve cominciare con {Scheme}).", $"'{location}' is not a remote folder (it must start with {Scheme})."));
            var rest = location.Substring(Scheme.Length);
            var slash = rest.IndexOf('/');
            var host = slash < 0 ? rest : rest.Substring(0, slash);
            var path = slash < 0 ? "/" : rest.Substring(slash);
            if (!HostPattern.IsMatch(host))
                throw new FormatException(UiText.Say($"'{location}': il server «{host}» non è valido (alias di ~/.ssh/config, utente@server, eventualmente :porta).", $"'{location}': the server «{host}» is not valid (an ~/.ssh/config alias, user@server, optionally :port)."));
            return new RemoteLocation(host, Normalize(path, location));
        }

        public static RemoteLocation Of(string host, string absolutePath) => Parse(Scheme + host + (absolutePath.StartsWith("/", StringComparison.Ordinal) ? absolutePath : "/" + absolutePath));

        /// <summary>
        /// Il destinatario da passare a <c>ssh</c>: l'host così com'è; con una porta la forma URI (<c>ssh://utente@server:porta</c>),
        /// che OpenSSH capisce dalla 7.7.
        /// </summary>
        public string Destination => Host.Contains(':') ? Scheme + Host : Host;

        /// <summary>Una cartella o un file dentro questa, con un percorso relativo (<c>/</c>); un <c>..</c> che esce è rifiutato.</summary>
        public RemoteLocation Combine(string relative)
        {
            if (string.IsNullOrEmpty(relative)) return this;
            if (relative.StartsWith("/", StringComparison.Ordinal))
                throw new FormatException(UiText.Say($"'{relative}' deve essere relativo.", $"'{relative}' must be relative."));
            var combined = new RemoteLocation(Host, Normalize(Path == "/" ? "/" + relative : Path + "/" + relative, relative));
            if (combined.RelativeTo(this) == null)
                throw new InvalidOperationException(UiText.Say($"'{relative}' esce da {this}.", $"'{relative}' goes outside {this}."));
            return combined;
        }

        /// <summary>Il percorso relativo (<c>/</c>, <c>""</c> se coincidono) se questa sta dentro <paramref name="outer"/>, altrimenti null.</summary>
        public string RelativeTo(RemoteLocation outer)
        {
            if (outer == null || !string.Equals(Host, outer.Host, StringComparison.Ordinal)) return null;
            if (Path == outer.Path) return string.Empty;
            var prefix = outer.Path == "/" ? "/" : outer.Path + "/";
            return Path.StartsWith(prefix, StringComparison.Ordinal) ? Path.Substring(prefix.Length) : null;
        }

        public override string ToString() => Scheme + Host + Path;
        public bool Equals(RemoteLocation other) => other != null && Host == other.Host && Path == other.Path;
        public override bool Equals(object obj) => Equals(obj as RemoteLocation);
        public override int GetHashCode() => HashCode.Combine(Host, Path);

        /// <summary>Toglie <c>.</c>, <c>//</c> e il <c>/</c> finale, risolve <c>..</c>; rifiuta un <c>..</c> sopra la radice del server.</summary>
        private static string Normalize(string path, string forMessage)
        {
            var parts = new List<string>();
            foreach (var part in path.Split('/'))
            {
                if (part.Length == 0 || part == ".") continue;
                if (part == "..")
                {
                    if (parts.Count == 0) throw new FormatException(UiText.Say($"'{forMessage}' sale sopra la radice del server.", $"'{forMessage}' goes above the server root."));
                    parts.RemoveAt(parts.Count - 1);
                    continue;
                }
                if (part.IndexOf('\0') >= 0) throw new FormatException(UiText.Say($"'{forMessage}' contiene un carattere non valido.", $"'{forMessage}' contains an invalid character."));
                parts.Add(part);
            }
            return "/" + string.Join("/", parts);
        }
    }
}
