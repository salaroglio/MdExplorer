using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MdExplorer.Mcp;

/// <summary>
/// Gruppo <c>plantuml</c>: la verifica di un diagramma prima di scriverlo in un documento.
/// Un tool solo, ~268 token, ed è quello su cui si appoggia la verifica dei diagrammi di MarkAgent.
/// </summary>
[McpServerToolType]
public sealed class PlantUmlTools : McpToolsBase
{
    public PlantUmlTools(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

    [McpServerTool, Description(
        "Check a PlantUML diagram BEFORE writing it into a document, and get back what to fix. " +
        "Call this every time you produce or edit a PlantUML diagram: it is cheap, it does not render anything, " +
        "and it catches mistakes you cannot see otherwise. " +
        "It reports three kinds of finding. 'error' means the diagram will NOT be displayed at all — fix it and check again. " +
        "'warning' means it will be displayed but wrong inside MdExplorer, typically a colour that vanishes in dark theme. " +
        "'hint' is style only, and is reported just when there are no errors. " +
        "Each finding carries the line, the line itself, what it means and ONE concrete fix: apply the fix, then call this again. " +
        "If the answer contains 'toolUnavailable', the diagram was NOT judged — something is missing on the machine, " +
        "so do NOT change your diagram: report what is missing to the user.")]
    public async Task<string> CheckPlantuml(
        [Description("The diagram source, from @startuml to @enduml, without the markdown fence.")] string source)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");

        try
        {
            var payload = new StringContent(
                JsonSerializer.Serialize(new { source }),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/Plantuml/Check", payload);

            if (!response.IsSuccessStatusCode)
            {
                return $"Error: MdExplorer API returned {response.StatusCode}. The diagram was NOT checked — do not change it because of this.";
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}. The diagram was NOT checked — do not change it because of this.";
        }
    }
}
