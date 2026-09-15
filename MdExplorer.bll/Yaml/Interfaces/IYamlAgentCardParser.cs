using MdExplorer.Features.Yaml.Models;

namespace MdExplorer.Features.Yaml.Interfaces
{
    /// <summary>
    /// Parser del blocco <c>a2a:</c> del frontmatter di un <c>.agent.md</c>.
    /// Fotocopia del pattern di <see cref="IYamlParser{T}"/> ma con esito fail-loud
    /// (§5 del design doc): niente <c>null</c> ambiguo, ma un
    /// <see cref="AgentCardParseResult"/> che distingue "non cittadino" da
    /// "cittadino con blocco malformato/invalido".
    /// </summary>
    public interface IYamlAgentCardParser
    {
        AgentCardParseResult GetDescriptor(string markdown);
    }
}
