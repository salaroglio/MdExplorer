using System.Collections.Generic;

namespace MdExplorer.bll.Services.AI
{
    /// <summary>
    /// Defines AI tool schemas for file operations.
    /// </summary>
    public static class FileOperationTools
    {
        /// <summary>
        /// Gets all file operation tool definitions for AI providers.
        /// </summary>
        public static List<ToolDefinition> GetToolDefinitions()
        {
            return new List<ToolDefinition>
            {
                CreateMarkdownFileTool(),
                ReadMarkdownFileTool(),
                UpdateMarkdownFileTool(),
                CreateSlidePresentationTool(),
                SearchDocumentsTool()
            };
        }

        private static ToolDefinition CreateMarkdownFileTool()
        {
            return new ToolDefinition
            {
                Name = "create_markdown_file",
                Description = "Create a new markdown file with specified content. Use this tool when user asks to: create/write/save documents, reports, notes, diagrams, code examples, or when they say 'put it in a file', 'save it to file.md', 'create file.md with...'. You MUST generate the content yourself before calling this tool. Examples: 'create a diagram and put it in diagram.md' → generate PlantUML/Mermaid diagram content, then call this tool. 'write a summary in summary.md' → generate summary content, then call this tool.",
                Parameters = new ToolParameters
                {
                    Type = "object",
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["file_path"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Relative path from workspace root. Must end with .md. Examples: 'reports/summary.md', 'notes/meeting-2025-10-21.md', 'diagrams/architecture.md', 'ciao-darwin.md'"
                        },
                        ["content"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Complete markdown content to write. YOU must generate this content based on user's request. Can include: markdown formatting (headings, lists, tables), PlantUML diagrams (```plantuml), Mermaid diagrams (```mermaid), code blocks, text, etc. Always include proper frontmatter YAML if needed."
                        },
                        ["overwrite"] = new ToolProperty
                        {
                            Type = "boolean",
                            Description = "If true, overwrite existing file. If false (default), return error if file exists.",
                            Default = false
                        }
                    },
                    Required = new List<string> { "file_path", "content" }
                }
            };
        }

        private static ToolDefinition ReadMarkdownFileTool()
        {
            return new ToolDefinition
            {
                Name = "read_markdown_file",
                Description = "Read the content of an existing markdown file. Use this to analyze, summarize, or extract information from documents.",
                Parameters = new ToolParameters
                {
                    Type = "object",
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["file_path"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Relative path to the markdown file to read. Example: 'docs/architecture.md'"
                        }
                    },
                    Required = new List<string> { "file_path" }
                }
            };
        }

        private static ToolDefinition UpdateMarkdownFileTool()
        {
            return new ToolDefinition
            {
                Name = "update_markdown_file",
                Description = "Update an existing markdown file by appending content, replacing sections, or inserting after specific headings. Can work on the CURRENT DOCUMENT (the file user is viewing) or a specific file. Use cases: 'add a conclusion section' → updates current document. 'modify this file' → updates current document. 'update report.md with...' → updates specific file.",
                Parameters = new ToolParameters
                {
                    Type = "object",
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["file_path"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Relative path to the markdown file to update. OPTIONAL: If omitted, updates the CURRENT DOCUMENT (the file user is viewing). Provide this ONLY when user explicitly mentions a different file name. Examples: omit for 'add a section here', provide 'notes.md' for 'update notes.md'."
                        },
                        ["content"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Markdown content to add or replace"
                        },
                        ["mode"] = new ToolProperty
                        {
                            Type = "string",
                            Enum = new List<string> { "append", "prepend", "replace", "insert_after_heading", "replace_section" },
                            Description = "How to update the file. 'append': add at end. 'prepend': add at start. 'replace': replace entire content. 'insert_after_heading': insert after specific heading. 'replace_section': replace content between start_marker and end_marker."
                        },
                        ["heading"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Required if mode='insert_after_heading'. The heading text after which to insert content. Example: '## Installation'"
                        },
                        ["start_marker"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Required if mode='replace_section'. The text marking the start of the section to replace. Examples: '```plantuml' for code blocks, '## Section Title' for headings."
                        },
                        ["end_marker"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Optional if mode='replace_section'. The text marking the end of the section to replace. If omitted, auto-detects end based on start_marker type (e.g., closing ``` for fenced code blocks, next heading for markdown sections)."
                        },
                        ["occurrence"] = new ToolProperty
                        {
                            Type = "integer",
                            Description = "Optional if mode='replace_section'. Which occurrence of start_marker to replace. Default: 1 (first occurrence). Use -1 for last occurrence.",
                            Default = 1
                        },
                        ["include_markers"] = new ToolProperty
                        {
                            Type = "boolean",
                            Description = "Optional if mode='replace_section'. Whether to include the markers in the replacement. Default: true. If false, preserves the original start and end marker lines.",
                            Default = true
                        }
                    },
                    Required = new List<string> { "content", "mode" }
                }
            };
        }

        private static ToolDefinition SearchDocumentsTool()
        {
            return new ToolDefinition
            {
                Name = "search_documents",
                Description = "Search documents semantically in the current project using RAG (Retrieval Augmented Generation). Returns the most relevant document chunks matching the query. Results include related content from the same document sections (e.g., if a text chunk matches, associated PlantUML diagrams are automatically included, and vice versa). Use this when the user asks questions about their documents, wants to find information across files, or needs context from the project.",
                Parameters = new ToolParameters
                {
                    Type = "object",
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["query"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "The search query. Use natural language to describe what you're looking for. Examples: 'authentication flow', 'database schema', 'deployment instructions'"
                        },
                        ["topK"] = new ToolProperty
                        {
                            Type = "integer",
                            Description = "Maximum number of results to return. Default: 5",
                            Default = 5
                        }
                    },
                    Required = new List<string> { "query" }
                }
            };
        }

        private static ToolDefinition CreateSlidePresentationTool()
        {
            return new ToolDefinition
            {
                Name = "create_slide_presentation",
                Description = "Create a reveal.js slide presentation from markdown. Use this when user asks to: 'create slides', 'make a presentation', 'create slideshow', 'make slides about...'. YOU MUST generate all slide content yourself. Examples: 'create slides about Docker' → generate 5-7 slides with titles and content, then call this tool. 'make a presentation on AI' → generate structured slides with introduction, main points, examples, conclusion.",
                Parameters = new ToolParameters
                {
                    Type = "object",
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["file_path"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Relative path from workspace root. Must end with .md. Examples: 'presentations/docker-intro.md', 'slides/ai-overview.md'"
                        },
                        ["title"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Main title of the presentation"
                        },
                        ["author"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Author name (optional)"
                        },
                        ["email"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Author email address (optional)"
                        },
                        ["slides"] = new ToolProperty
                        {
                            Type = "array",
                            Description = "Array of slide objects. Each slide should have: heading (string), content (string markdown), type ('horizontal' or 'vertical', default: 'horizontal'), useFragments (boolean, default: false for code slides, true for lists)",
                            Items = new
                            {
                                type = "object",
                                properties = new Dictionary<string, object>
                                {
                                    ["heading"] = new { type = "string", description = "Slide title/heading" },
                                    ["content"] = new { type = "string", description = "Markdown content of the slide (bullets, code, text)" },
                                    ["type"] = new { type = "string", description = "Slide type: 'horizontal' or 'vertical'", @default = "horizontal" },
                                    ["useFragments"] = new { type = "boolean", description = "Enable fragment animations for list items", @default = false }
                                },
                                required = new[] { "heading", "content" }
                            }
                        },
                        ["theme"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Reveal.js theme: 'black', 'white', 'league', 'beige', 'sky', 'night', 'serif', 'simple', 'solarized', 'blood', 'moon'. Default: 'black'",
                            Default = "black"
                        }
                    },
                    Required = new List<string> { "file_path", "title", "slides" }
                }
            };
        }
    }

    /// <summary>
    /// Tool definition model compatible with OpenAI/Gemini function calling.
    /// </summary>
    public class ToolDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ToolParameters Parameters { get; set; }
    }

    public class ToolParameters
    {
        public string Type { get; set; }
        public Dictionary<string, ToolProperty> Properties { get; set; }
        public List<string> Required { get; set; }
    }

    public class ToolProperty
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public List<string> Enum { get; set; }
        public object Default { get; set; }
        public object Items { get; set; } // For array types: defines the structure of array elements
    }
}
