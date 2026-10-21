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
}
