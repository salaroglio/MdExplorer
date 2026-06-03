using System.Text.Json.Nodes;

namespace MdExplorer.Features.Services.Atlassian
{
    /// <summary>
    /// Builds a minimal Atlassian Document Format (ADF) document from plain text,
    /// for write operations (e.g. issue description on create). Each non-empty
    /// line becomes a paragraph. Pure and dependency-free so it is unit-testable;
    /// the round-trip AdfBuilder.FromPlainText → AdfRenderer.ToText recovers the
    /// original lines.
    /// </summary>
    public static class AdfBuilder
    {
        public static string FromPlainText(string text)
        {
            var content = new JsonArray();
            var normalized = (text ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n");
            var any = false;
            foreach (var line in normalized.Split('\n'))
            {
                if (line.Length == 0) continue; // blank lines are paragraph breaks
                content.Add(new JsonObject
                {
                    ["type"] = "paragraph",
                    ["content"] = new JsonArray { new JsonObject { ["type"] = "text", ["text"] = line } }
                });
                any = true;
            }
            if (!any)
                content.Add(new JsonObject { ["type"] = "paragraph", ["content"] = new JsonArray() });

            var doc = new JsonObject
            {
                ["type"] = "doc",
                ["version"] = 1,
                ["content"] = content
            };
            return doc.ToJsonString();
        }
    }
}
