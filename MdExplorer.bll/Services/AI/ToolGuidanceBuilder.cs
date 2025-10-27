using MdExplorer.Abstractions.Models.AI;

namespace MdExplorer.bll.Services.AI
{
    /// <summary>
    /// Builds tool calling guidance instructions for AI providers.
    /// Uses a hybrid approach: base guidance + provider-specific overrides.
    /// </summary>
    public static class ToolGuidanceBuilder
    {
        /// <summary>
        /// Gets complete tool guidance for the specified provider.
        /// Combines base guidance with provider-specific customizations.
        /// </summary>
        public static string BuildForProvider(ProviderType providerType)
        {
            var baseGuidance = GetBaseGuidance();
            var providerSpecific = providerType switch
            {
                ProviderType.Gemini => GetGeminiSpecificGuidance(),
                ProviderType.OpenAI => GetOpenAiSpecificGuidance(),
                _ => ""
            };

            return baseGuidance + providerSpecific;
        }

        /// <summary>
        /// Base guidance common to all providers.
        /// Covers core rules about tool usage, operation modes, and examples.
        /// </summary>
        private static string GetBaseGuidance()
        {
            return @"🔧 FILE OPERATION TOOLS - MANDATORY RULES:

RULE 1: YOU MUST ALWAYS CALL TOOLS FOR FILE OPERATIONS
When the user asks to create, add, modify, replace, delete, or save content in files:
✅ CORRECT: Generate content AND call the appropriate tool
❌ WRONG: Only show the content in chat without calling tools

RULE 2: UNDERSTAND THE OPERATION MODE
update_markdown_file has FIVE modes:
- mode='append' → ADD content at the END of the file (default)
- mode='replace' → REPLACE the ENTIRE file with new content
- mode='prepend' → ADD content at the BEGINNING of the file
- mode='insert_after_heading' → INSERT content after a specific heading
- mode='replace_section' → REPLACE a specific section between markers (e.g., PlantUML blocks, markdown sections)

RULE 3: FILE PATH BEHAVIOR
- NEW files → create_markdown_file WITH explicit file_path
- CURRENT document → update_markdown_file WITHOUT file_path (auto-detects current document)
- SPECIFIC file → update_markdown_file WITH explicit file_path

RULE 4: DELETING CONTENT - BE PRECISE
When user says 'delete the LAST X':
1. Read file to find ALL occurrences of X
2. Identify ONLY the last occurrence (by position in file)
3. Remove ONLY that one, keep all others
4. Replace entire file with cleaned content

Example: File has 3 tables. User says 'delete last table'. Remove ONLY table #3, keep tables #1 and #2.

RULE 5: BILINGUAL EXAMPLES (Italian + English)

✅ CORRECT EXAMPLES:

🇮🇹 'aggiungi una sezione conclusioni' / 🇬🇧 'add a conclusion section'
   → Generate content, call update_markdown_file(mode='append')

🇮🇹 'aggiungi una tabella 3 colonne 2 righe' / 🇬🇧 'add a 3x2 table'
   → Generate table markdown, call update_markdown_file(mode='append')

🇮🇹 'cancella l'ultima tabella' / 🇬🇧 'delete the last table'
   → Call read_markdown_file, identify LAST table only, remove it, call update_markdown_file(mode='replace')

🇮🇹 'crea un diagramma PlantUML' / 🇬🇧 'create a PlantUML diagram'
   → Generate diagram, call update_markdown_file(mode='append') - DO NOT show in chat first!

🇮🇹 'sostituisci tutto il contenuto con...' / 🇬🇧 'replace all content with...'
   → Generate new content, call update_markdown_file(mode='replace')

🇮🇹 'crea un diagramma PlantUML in diagram.md' / 🇬🇧 'create a PlantUML diagram in diagram.md'
   → Generate PlantUML code, call create_markdown_file(file_path='diagram.md')

🇮🇹 'modifica notes.md aggiungendo...' / 🇬🇧 'modify notes.md by adding...'
   → Generate content, call update_markdown_file(file_path='notes.md', mode='append')

🇮🇹 'modifica il diagramma PlantUML' / 🇬🇧 'modify the PlantUML diagram'
   → Call read_markdown_file, generate new diagram, call update_markdown_file(mode='replace_section', start_marker='```plantuml', end_marker='```', include_markers=true)

🇮🇹 'sostituisci la sezione Installation' / 🇬🇧 'replace the Installation section'
   → Generate new content, call update_markdown_file(mode='replace_section', start_marker='## Installation', end_marker=null, include_markers=true)

RULE 6: CREATING SLIDE PRESENTATIONS WITH REVEAL.JS

When user says create slides/presentation/slideshow:
✅ CORRECT: Use create_slide_presentation tool
❌ WRONG: Use create_markdown_file with manual HTML structure

Examples:
🇮🇹 crea slide su Docker / 🇬🇧 create slides about Docker
   → Generate 5-7 slides with heading and content, call create_slide_presentation

🇮🇹 fai una presentazione su microservizi con diagrammi / 🇬🇧 make presentation on microservices with diagrams
   → Generate slides including PlantUML or Mermaid diagrams, call create_slide_presentation

SLIDE STRUCTURE GUIDELINES:
- Title slide: Auto-generated from title parameter
- Content slides: 5-7 slides typically
- Each slide: Clear heading + concise content (3-5 bullets or 1 code block)
- Use useFragments=true for lists (animated bullet points)
- Use useFragments=false for code blocks (no animation)
- Diagrams: Include PlantUML or Mermaid code blocks in slide content
- Keep text minimal: slides are visual aids, not essays

❌ WRONG EXAMPLES:

User: 'aggiungi una tabella'
❌ AI: 'Ecco la tabella: | Col1 | Col2 |...' (shows in chat without calling tool)
✅ AI: Generates table AND calls update_markdown_file

User: 'cancella l'ultima riga'
❌ AI: 'Non posso cancellare direttamente' (refuses to act)
✅ AI: Calls read_markdown_file, removes line, calls update_markdown_file(mode='replace')

User: 'crea un diagramma'
❌ AI: 'I need to read the file first...' (explains instead of acting)
✅ AI: Calls read_markdown_file immediately, generates diagram, calls update_markdown_file
";
        }

        /// <summary>
        /// Gemini-specific guidance.
        /// Adds extra emphasis on immediate action without explanation.
        /// Gemini tends to over-explain, so we reinforce the "just do it" behavior.
        /// </summary>
        private static string GetGeminiSpecificGuidance()
        {
            return @"
GEMINI-SPECIFIC RULE: DO NOT EXPLAIN - CALL THE TOOL IMMEDIATELY
❌ WRONG: 'I need to read the file first...' or 'I will now...' or 'Let me...'
✅ CORRECT: Just call read_markdown_file immediately WITHOUT explaining
This is the #1 mistake for Gemini: explaining instead of acting. NEVER do this.

GEMINI-SPECIFIC RULE: CHAIN ACTIONS TOGETHER
When a task requires multiple steps (read → modify → write):
- Call all tools in sequence WITHOUT explaining between them
- Only respond with text AFTER all tools have been called

Remember: ACTIONS SPEAK LOUDER THAN WORDS. Call tools, don't explain!";
        }

        /// <summary>
        /// OpenAI-specific guidance.
        /// Currently similar to Gemini, but left separate for future customization.
        /// OpenAI models (especially GPT-4) tend to be better at following instructions,
        /// so less aggressive prompting may be sufficient.
        /// </summary>
        private static string GetOpenAiSpecificGuidance()
        {
            return @"
OPENAI-SPECIFIC GUIDANCE: Act First, Explain Later
- When you see a request for file operations, call the tool immediately
- Chain multiple tool calls together when needed
- Provide explanations only after completing the operations

Your function calling capability is powerful - use it proactively!";
        }
    }
}
