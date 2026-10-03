using MdExplorer.Features.Yaml.Interfaces;
using MdExplorer.Features.Yaml.Models;
using Remotion.Linq.Clauses.ResultOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using YamlDotNet.RepresentationModel;
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

                // The front matter also holds keys the descriptor does not model (the slides'
                // reveal: block): they must not hide the ones it does.
                var deserializer = new DeserializerBuilder()
                                .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                                .IgnoreUnmatchedProperties()
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

            var serialized = SerializeDescriptor(descriptor);
            var yamlNew = string.Concat("---", System.Environment.NewLine, serialized,
                UnknownKeysAsWritten(yamlOld, TopLevelKeys(serialized)), "---");

            markdown = markdown.Replace(matches[0].Groups[0].Value, yamlNew);
            return markdown;
        }

        /// <summary>
        /// The text of the top-level keys of <paramref name="yaml"/> that the descriptor does not
        /// write, exactly as the user wrote them (comments and quoting included): a key's text runs
        /// from its line to the line of the next top-level key.
        /// </summary>
        private static string UnknownKeysAsWritten(string yaml, ISet<string> knownKeys)
        {
            var root = RootMapping(yaml);
            if (root == null)
                return string.Empty;

            var lines = yaml.Split('\n');
            // Mark.Line counts from 1.
            var keys = root.Children.Keys.OfType<YamlScalarNode>()
                .Select(k => (Name: k.Value, Line: (int)k.Start.Line - 1))
                .OrderBy(k => k.Line)
                .ToList();

            var kept = new StringBuilder();
            for (var i = 0; i < keys.Count; i++)
            {
                if (knownKeys.Contains(keys[i].Name))
                    continue;
                var end = i + 1 < keys.Count ? keys[i + 1].Line : lines.Length;
                var text = string.Join("\n", lines.Skip(keys[i].Line).Take(end - keys[i].Line)).TrimEnd();
                kept.Append(text).Append('\n');
            }
            return kept.ToString();
        }

        private static ISet<string> TopLevelKeys(string yaml)
            => new HashSet<string>(RootMapping(yaml)?.Children.Keys.OfType<YamlScalarNode>().Select(k => k.Value)
                                   ?? Enumerable.Empty<string>());

        private static YamlMappingNode RootMapping(string yaml)
        {
            if (string.IsNullOrWhiteSpace(yaml))
                return null;
            var stream = new YamlStream();
            stream.Load(new StringReader(yaml));
            return stream.Documents.Count > 0 ? stream.Documents[0].RootNode as YamlMappingNode : null;
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
