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
                UpdateMarkdownFileTool()
                // search_documents tool will be added in Phase 7 (RAG)
            };
        }

        private static ToolDefinition CreateMarkdownFileTool()
        {
            return new ToolDefinition
            {
                Name = "create_markdown_file",
                Description = "Create a new markdown file with specified content. Use this when user asks to create documents, reports, summaries, or any new markdown file.",
                Parameters = new ToolParameters
                {
                    Type = "object",
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["file_path"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Relative path from workspace root. Must end with .md. Example: 'reports/summary.md' or 'notes/meeting-2025-10-21.md'"
                        },
                        ["content"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Complete markdown content to write. Include proper markdown formatting with headings, lists, tables as appropriate."
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
                Description = "Update an existing markdown file by appending content, replacing sections, or inserting after specific headings.",
                Parameters = new ToolParameters
                {
                    Type = "object",
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["file_path"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Relative path to the markdown file to update"
                        },
                        ["content"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Markdown content to add or replace"
                        },
                        ["mode"] = new ToolProperty
                        {
                            Type = "string",
                            Enum = new List<string> { "append", "prepend", "replace", "insert_after_heading" },
                            Description = "How to update the file. 'append': add at end. 'prepend': add at start. 'replace': replace entire content. 'insert_after_heading': insert after specific heading."
                        },
                        ["heading"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "Required if mode='insert_after_heading'. The heading text after which to insert content. Example: '## Installation'"
                        }
                    },
                    Required = new List<string> { "file_path", "content", "mode" }
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
    }
}
