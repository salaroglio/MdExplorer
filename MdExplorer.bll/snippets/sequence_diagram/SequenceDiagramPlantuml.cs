using MdExplorer.Features.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.snippets.sequence_diagram
{
    public class SequenceDiagramPlantuml : ISnippet
    {
        public int Id { get => 1; }
        public string Name { get => "Sequence diagram"; }
        public string Description { get => "Sequence diagram plantuml"; }

        public string GetSnippet()
        {
            return Helper.ExtractResFileString("MdExplorer.Features.snippets.sequence_diagram.plantuml.sequence_diagram.plantuml");
        }

        public void SetAssets(string directoryPath)
        {
            var assetsPath = Path.GetDirectoryName(directoryPath) + Path.DirectorySeparatorChar + "assets";
            if (!Directory.Exists(assetsPath))
            {
                Directory.CreateDirectory(assetsPath);
            }
            var assembly = Assembly.GetExecutingAssembly();
            var arrayOfResources = assembly.GetManifestResourceNames().Where(_ => _.Contains("MdExplorer.Features.snippets.sequence_diagram.assets")).ToList();
            arrayOfResources.ForEach(_ => {
                var sequenceArray = _.Split(".").ToList();
                var tst = sequenceArray.Skip(Math.Max(0, sequenceArray.Count() - 2));
                var fileName = string.Join(".",tst);
                var filePath = assetsPath + Path.DirectorySeparatorChar + fileName;
                Helper.ExtractResFile(_, filePath); });
        }
    }
}
