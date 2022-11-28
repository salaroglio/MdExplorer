using MdExplorer.Features.Yaml.Interfaces;
using MdExplorer.Features.Yaml.Models;
using Remotion.Linq.Clauses.ResultOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Features.Yaml
{
    public class YamlDocumentDescriptorParser : IYamlParser<MdExplorerDocumentDescriptor>
    {
        public YamlDocumentDescriptorParser()
        {

        }

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"^-{3}(.*?)-{3}", // missing 
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public MdExplorerDocumentDescriptor GetDescriptor(string markdown)
        {
            try
            {
                var matches = GetMatches(markdown);
                var yml = matches.Count() > 0 ? matches[0].Groups[1].Value : null;
                if (yml == null)
                    return null;

                var deserializer = new DeserializerBuilder()
                                .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                                
                                .Build();
                var yamlDescriptor = deserializer.Deserialize<MdExplorerDocumentDescriptor>(yml);
                return yamlDescriptor;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return null;
            }
        }

        public string SetDescriptor(MdExplorerDocumentDescriptor descriptor, string markdown) {
            var matches = GetMatches(markdown);
            var yamlOld = matches.Count() > 0 ? matches[0].Groups[1].Value : null;

            var yamlNew = string.Concat("---",System.Environment.NewLine,SerializeDescriptor(descriptor),"---");

            markdown = markdown.Replace(matches[0].Groups[0].Value, yamlNew);
            return markdown;
        }



        public string SerializeDescriptor(MdExplorerDocumentDescriptor descriptor)
        {
            var serializer = new SerializerBuilder()
                            .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                            .Build();
            var toReturn = serializer.Serialize(descriptor);
            return toReturn;
        }
    }


}
