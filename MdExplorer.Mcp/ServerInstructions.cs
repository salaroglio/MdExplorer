namespace MdExplorer.Mcp;

/// <summary>
/// Il testo delle istruzioni del server MCP (<c>ServerInstructions</c>): regole generali, statiche, che devono stare nel
/// contesto del modello prima di qualunque chiamata. I dati del progetto (quali estensioni, quale codifica) li dà
/// <c>code_roots</c>.
/// </summary>
public static class ServerInstructions
{
    // La PRIMA frase deve bastare da sola: Copilot CLI (1.0.101, provato il 21/10) non mette le istruzioni nel prompt,
    // le mostra come presentazione del server nell'elenco degli strumenti differiti, tagliate a ~155 caratteri. Claude
    // Code le mette intere nel prompt di sistema.
    public const string CodeRootEncodings =
        "Change code root files (@alias/...) ONLY with remote_edit/remote_write: they keep the encoding (e.g. CP-850) and " +
        "the user reviews each change. " +
        "Details: a MdExplorer project can link external code roots (@alias/...), local folders or folders on an SSH server, " +
        "often CP-850 or CP-1252 legacy code, and a root can assign an encoding per extension (e.g. .cbl, .cpy -> ibm850): " +
        "code_roots lists each root's `encodings`, and remote_read states the encoding of a file that is not UTF-8. On EVERY " +
        "code root, local or remote, files are created, changed, moved and deleted only with the remote_* tools: MDE keeps the " +
        "original of each file, so the user reviews your change piece by piece and can undo it. Your own Edit/Write are refused " +
        "there; never write them from the shell either (sed -i, perl -i, echo >, ssh, scp, sftp, rsync): that skips the review " +
        "and the encoding. Reading with your own tools is fine for local roots. Never convert a file to UTF-8 and never insert " +
        "UTF-8 text into a non-UTF-8 file: it breaks the file for its compiler. After a change, tell the user which files are " +
        "waiting for review. If unsure, ask the user.";

    /// <summary>
    /// Solo con il gruppo <c>code</c>, DOPO le codifiche (la cui prima frase deve restare la prima, vedi sopra): il grafo
    /// prima della ricerca a mano (26/10/2026). Il caso: MarkAgent aveva scritto l'analisi di una modifica su una radice
    /// <c>@alias</c> cercando nei file, senza il grafo, perché nulla nella richiesta faceva scattare la skill mde-code. Il 28/10
    /// verificando un documento scritto a mano non sapeva a che cosa serve il grafo: lo cercava come una cosa da completare.
    /// </summary>
    public const string CodeGraphFirst =
        "Before searching source code by hand to answer, plan or analyse a change (which files and methods to touch, " +
        "what depends on them) or check a document against the code, ask the code graph: it is the map of the code, " +
        "built so that you do not read the files, every answer with path:line. code_find_symbol to locate, code_describe " +
        "for what it does, code_impact and code_neighbors for what depends on it, code_roots for where @alias/... is on " +
        "this machine; the mde-code skill has the details. It is a tool to read, not something to complete: if the " +
        "graph has nothing for that code, say so before searching the files.";

    /// <summary>
    /// Le istruzioni per i gruppi accesi, o null se nessuno ne ha. La prima frase (le radici si cambiano solo coi remote_*) deve
    /// bastare da sola: Copilot ne mostra ~155 caratteri (27/11/2026, sprint Modifiche-Da-Approvare-Sulle-Radici).
    /// </summary>
    public static string? For(bool code, bool remote)
        => code ? CodeRootEncodings + " " + CodeGraphFirst
         : remote ? CodeRootEncodings
         : null;
}
