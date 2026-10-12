using System;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Le regole pure delle radici di codice esterne (sprint 2026-10-12-Radici-Di-Codice-Esterne, R1, R2, R7): l'alias, il
    /// percorso logico <c>@alias/percorso</c> — uguale su ogni macchina, è quello che va negli script e in Fuseki — e il
    /// confronto dei <c>remote</c> git, che decide se la cartella scelta è davvero quella dichiarata.
    /// </summary>
    public static class CodeRootPaths
    {
        public const char Marker = '@';

        private static readonly Regex AliasRule = new(@"^[a-z0-9][a-z0-9._-]{0,62}$", RegexOptions.CultureInvariant);

        /// <summary>Minuscole, cifre, <c>. _ -</c>, al massimo 63 caratteri, inizia con lettera o cifra: va in un percorso e in un IRI.</summary>
        public static bool IsValidAlias(string alias) => alias != null && AliasRule.IsMatch(alias);

        public static string RequireAlias(string alias)
            => IsValidAlias(alias) ? alias
             : throw new ArgumentException($"Alias '{alias}' non valido: minuscole, cifre, «.», «_» o «-», inizia con lettera o cifra, al massimo 63 caratteri (es. «backend»).");

        /// <summary>Vero per un percorso logico di una radice esterna (<c>@backend/src/Foo.cs</c>, anche solo <c>@backend</c>).</summary>
        public static bool IsLogical(string path) => !string.IsNullOrEmpty(path) && path[0] == Marker;

        /// <summary><c>@backend/src/Foo.cs</c> → (<c>backend</c>, <c>src/Foo.cs</c>); <c>@backend</c> → (<c>backend</c>, "").</summary>
        public static (string Alias, string Remainder) Parse(string logicalPath)
        {
            if (!IsLogical(logicalPath)) throw new FormatException($"'{logicalPath}' non è un percorso di una radice esterna (deve cominciare con «@alias»).");
            var body = logicalPath.Substring(1).Replace('\\', '/');
            var slash = body.IndexOf('/');
            var alias = slash < 0 ? body : body.Substring(0, slash);
            var rest = slash < 0 ? string.Empty : body.Substring(slash + 1).Trim('/');
            if (!IsValidAlias(alias)) throw new FormatException($"'{logicalPath}': alias «{alias}» non valido.");
            return (alias, rest);
        }

        /// <summary>(<c>backend</c>, <c>src\Foo.cs</c>) → <c>@backend/src/Foo.cs</c>.</summary>
        public static string Format(string alias, string rest)
        {
            RequireAlias(alias);
            var r = (rest ?? string.Empty).Replace('\\', '/').Trim('/');
            return r.Length == 0 ? Marker + alias : Marker + alias + "/" + r;
        }

        /// <summary>
        /// Un <c>remote</c> git ridotto a <c>host/percorso</c> minuscolo, senza utente, porta, schema né <c>.git</c>:
        /// <c>https://user@github.com/Acme/Backend.git</c>, <c>git@github.com:acme/backend</c> e
        /// <c>ssh://git@github.com:22/acme/backend.git</c> diventano tutti <c>github.com/acme/backend</c>. Un percorso
        /// locale resta un percorso (normalizzato). Null se vuoto.
        /// </summary>
        public static string NormalizeRemote(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            var u = url.Trim();
            string host, path;
            var scheme = Regex.Match(u, @"^[a-zA-Z][a-zA-Z0-9+.-]*://");
            if (scheme.Success)
            {
                var rest = u.Substring(scheme.Length);
                if (scheme.Value.StartsWith("file", StringComparison.OrdinalIgnoreCase)) return NormalizeLocal(rest);
                var slash = rest.IndexOf('/');
                var authority = slash < 0 ? rest : rest.Substring(0, slash);
                path = slash < 0 ? string.Empty : rest.Substring(slash + 1);
                var at = authority.LastIndexOf('@');
                if (at >= 0) authority = authority.Substring(at + 1);
                var colon = authority.IndexOf(':');
                host = colon >= 0 ? authority.Substring(0, colon) : authority;
            }
            else
            {
                // scp: git@github.com:acme/backend.git — ma non C:\percorso né /percorso
                var scp = Regex.Match(u, @"^(?:[^@/\\]+@)?([^:/\\]{2,}):(?!\\)(.+)$");
                if (!scp.Success) return NormalizeLocal(u);
                host = scp.Groups[1].Value;
                path = scp.Groups[2].Value;
            }
            path = path.Replace('\\', '/').Trim('/');
            if (path.EndsWith(".git", StringComparison.OrdinalIgnoreCase)) path = path.Substring(0, path.Length - 4);
            return (host + "/" + path).ToLowerInvariant();
        }

        /// <summary>Stesso repository: i due <c>remote</c> normalizzati coincidono.</summary>
        public static bool SameRepository(string a, string b)
        {
            var na = NormalizeRemote(a);
            var nb = NormalizeRemote(b);
            return na != null && string.Equals(na, nb, StringComparison.Ordinal);
        }

        private static string NormalizeLocal(string path)
            => "file:" + path.Replace('\\', '/').TrimEnd('/').ToLowerInvariant();
    }
}
