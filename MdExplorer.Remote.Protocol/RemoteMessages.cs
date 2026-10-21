using System.Collections.Generic;

namespace MdExplorer.RemoteProtocol
{
    /// <summary>
    /// Una richiesta. I percorsi sono relativi alla radice con <c>/</c> come separatore; <c>""</c> è la radice.
    /// Ogni operazione usa solo i suoi campi (vedi il piano, «F1 — il serverino»).
    /// </summary>
    public sealed class RemoteRequest
    {
        public int Id { get; set; }
        public string Op { get; set; } = "";
        public string? Path { get; set; }
        /// <summary><c>move</c>: la destinazione.</summary>
        public string? To { get; set; }
        /// <summary><c>write</c>, <c>create</c>: il contenuto.</summary>
        public string? Text { get; set; }
        /// <summary><c>write</c>: l'impronta letta; se il file è cambiato nel frattempo la scrittura è rifiutata.</summary>
        public string? Stamp { get; set; }
        /// <summary><c>delete</c>: una cartella non vuota si cancella solo se richiesto.</summary>
        public bool Recursive { get; set; }
        /// <summary><c>grep</c>: espressione regolare .NET.</summary>
        public string? Pattern { get; set; }
        public bool IgnoreCase { get; set; }
        /// <summary><c>grep</c>: al più tante righe; 0 = il default del serverino.</summary>
        public int MaxHits { get; set; }
        /// <summary><c>grep</c>: nomi di cartelle da saltare (il backend manda <c>.git</c>).</summary>
        public List<string>? Exclude { get; set; }
    }

    public sealed class RemoteResponse
    {
        public int Id { get; set; }
        public bool Ok { get; set; }
        public RemoteError? Error { get; set; }
        public RemoteHello? Hello { get; set; }
        public List<RemoteEntry>? Entries { get; set; }
        public RemoteEntry? Entry { get; set; }
        public string? Text { get; set; }
        public string? Stamp { get; set; }
        /// <summary><c>read</c>: il file comincia con un BOM (che <c>write</c> conserva).</summary>
        public bool? Bom { get; set; }
        /// <summary><c>read</c>: la codifica riconosciuta (<see cref="TextDecoding"/>), in cui <c>write</c> riscrive.</summary>
        public string? Encoding { get; set; }
        public List<RemoteHit>? Hits { get; set; }
        /// <summary><c>grep</c>: c'erano altre righe oltre <see cref="RemoteRequest.MaxHits"/>.</summary>
        public bool? Truncated { get; set; }
        public RemoteGitInfo? Git { get; set; }
    }

    public sealed class RemoteError
    {
        public string Code { get; set; } = "";
        public string Message { get; set; } = "";
    }

    /// <summary>Il saluto (id 0) con cui il serverino dice che è pronto.</summary>
    public sealed class RemoteHello
    {
        public int Protocol { get; set; }
        public string Version { get; set; } = "";
        /// <summary>La radice assoluta sul server, normalizzata.</summary>
        public string Root { get; set; } = "";
    }

    public sealed class RemoteEntry
    {
        public string Name { get; set; } = "";
        /// <summary>Relativo alla radice, con <c>/</c>.</summary>
        public string Path { get; set; } = "";
        public bool IsFolder { get; set; }
        public long Size { get; set; }
        /// <summary>Un file senza byte NUL nei primi 8 KB; le cartelle no.</summary>
        public bool IsText { get; set; }
        /// <summary>Perché non si è potuto leggere (il file resta nell'elenco, non sparisce).</summary>
        public string? Unreadable { get; set; }
        /// <summary>Solo nei file di <c>stat</c>.</summary>
        public string? Stamp { get; set; }
    }

    public sealed class RemoteHit
    {
        public string Path { get; set; } = "";
        public int Line { get; set; }
        public string Text { get; set; } = "";
    }

    /// <summary><c>gitInfo</c>: il repository che contiene la radice, letto dai file di <c>.git</c> (niente <c>git</c> sul server).</summary>
    public sealed class RemoteGitInfo
    {
        /// <summary>La cartella del repository (quella con <c>.git</c>), assoluta sul server.</summary>
        public string Top { get; set; } = "";
        /// <summary>Gli URL dei remote, «origin» per primo, come <c>CodeRootService</c> in locale.</summary>
        public List<RemoteGitRemote> Remotes { get; set; } = new();
    }

    public sealed class RemoteGitRemote
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
    }
}
