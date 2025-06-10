using MdExplorer.Features.Yaml.Models;

namespace MdExplorer.Features.Yaml.Interfaces
{
    public interface IYamlDefaultGenerator
    {
        /// <summary>
        /// Genera una stringa YAML di default con i metadati del documento
        /// </summary>
        /// <param name="projectPath">Path del progetto per recuperare le informazioni Git</param>
        /// <returns>Stringa YAML completa con separatori ---</returns>
        string GenerateDefaultYaml(string projectPath = null);
        
        /// <summary>
        /// Genera un descriptor di default per il documento
        /// </summary>
        /// <param name="projectPath">Path del progetto per recuperare le informazioni Git</param>
        /// <returns>Descriptor con valori di default</returns>
        MdExplorerDocumentDescriptor GenerateDefaultDescriptor(string projectPath = null);
    }
}