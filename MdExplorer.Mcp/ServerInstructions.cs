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
        "Code root files may not be UTF-8 (e.g. CP-850): check the encoding with code_roots before an edit, keep it, " +
        "never convert to UTF-8. " +
        "Details: a MdExplorer project can link external code roots (@alias/...), often CP-850 or CP-1252 legacy code, and a root can assign an encoding per extension " +
        "(e.g. .cbl, .cpy -> ibm850): code_roots lists each root's `encodings`, and remote_read states the encoding of a " +
        "file that is not UTF-8. When the remote_* tools are available, edit through them: they keep the file's encoding. " +
        "With your own tools (Edit, Write, sed, perl...), write the new text in the file's own encoding. Never insert " +
        "UTF-8 text into a non-UTF-8 file: it breaks the file for its compiler. If unsure, ask the user.";

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

    /// <summary>Le istruzioni per i gruppi accesi, o null se nessuno ne ha.</summary>
    public static string? For(bool code, bool remote)
        => code ? CodeRootEncodings + " " + CodeGraphFirst
         : remote ? CodeRootEncodings
         : null;
}
