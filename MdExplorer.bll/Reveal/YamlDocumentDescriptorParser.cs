using MdExplorer.Features.Reveal.Interfaces;
using MdExplorer.Features.Reveal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Features.Reveal
{
    public class YamlDocumentDescriptorParser: IYamlParser<YamlDocumentDescriptor>
    {
        public YamlDocumentDescriptorParser()
        {

        }

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"-{3}([^-{3}]*)",
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public YamlDocumentDescriptor GetDescriptor(string markdown)
        {
            var matches = GetMatches(markdown);
            var yml = matches.Count() > 0 ? matches[0].Groups[1].Value : null;
            if (yml == null)
                return null;

            var deserializer = new DeserializerBuilder()
                            .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                            .Build();
            var yamlDescriptor = deserializer.Deserialize<YamlDocumentDescriptor>(yml);
            return yamlDescriptor;
        }
    }


}
