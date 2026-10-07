using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace MdExplorer.Features.Git
{
    /// <summary>
    /// L'unione a tre vie di <c>.development.yml</c>, voce per voce. Il file lo scrive MdExplorer, e lo cambiano sia la sorgente
    /// (una configurazione nuova del progetto) sia l'app sul computer della persona (la città accesa, la chiave della stanza):
    /// git vede due inserimenti alle stesse righe e si ferma, anche quando non c'è niente in contraddizione. Qui si confrontano
    /// le voci, non le righe: una voce cambiata da un lato solo si prende da quel lato; la stessa voce cambiata dai due lati in
    /// modo diverso è un conflitto vero, e si dice quale.
    /// </summary>
    public static class DevelopmentYamlMerge
    {
        /// <param name="baseText">L'antenato comune; null se il file non c'era.</param>
        /// <param name="conflict">La voce (per esempio <c>agentCity.ownershipDoc</c>) cambiata in modo diverso dai due lati.</param>
        /// <returns>Il file unito; null se c'è un conflitto.</returns>
        public static string Merge(string baseText, string ours, string theirs, out string conflict)
        {
            conflict = null;
            var b = Load(baseText);
            var o = Load(ours) ?? throw new InvalidOperationException("La tua versione di .development.yml non è un documento YAML con delle voci.");
            var t = Load(theirs) ?? throw new InvalidOperationException("La versione della sorgente di .development.yml non è un documento YAML con delle voci.");
            var merged = MergeMapping(b, o, t, "", ref conflict);
            if (conflict != null) return null;

            var stream = new YamlStream(new YamlDocument(merged));
            using var writer = new StringWriter();
            stream.Save(writer, assignAnchors: false);
            var text = writer.ToString().Replace("\r\n", "\n");
            // YamlDotNet chiude il documento con «...»: il file di MdExplorer non lo ha.
            if (text.EndsWith("...\n")) text = text.Substring(0, text.Length - 4);
            return ours.Contains("\r\n") ? text.Replace("\n", "\r\n") : text;
        }

        private static YamlMappingNode Load(string text)
        {
            if (text == null) return null;
            var stream = new YamlStream();
            stream.Load(new StringReader(text));
            if (stream.Documents.Count == 0) return new YamlMappingNode();
            return stream.Documents[0].RootNode as YamlMappingNode;
        }

        private static YamlMappingNode MergeMapping(YamlMappingNode b, YamlMappingNode o, YamlMappingNode t, string path, ref string conflict)
        {
            var result = new YamlMappingNode();
            var keys = o.Children.Keys.Select(Key)
                .Concat(t.Children.Keys.Select(Key).Where(k => !o.Children.Keys.Select(Key).Contains(k)))
                .ToList();
            foreach (var key in keys)
            {
                var bv = Get(b, key);
                var ov = Get(o, key);
                var tv = Get(t, key);
                var where = path.Length == 0 ? key : path + "." + key;
                YamlNode chosen;
                if (Same(ov, tv)) chosen = ov;
                else if (Same(bv, ov)) chosen = tv;          // da te non è cambiata: vale la sorgente (anche se l'ha tolta)
                else if (Same(bv, tv)) chosen = ov;          // dalla sorgente non è cambiata: vale la tua
                else if (ov is YamlMappingNode om && tv is YamlMappingNode tm)
                {
                    chosen = MergeMapping(bv as YamlMappingNode, om, tm, where, ref conflict);
                    if (conflict != null) return result;
                }
                else
                {
                    conflict = where;
                    return result;
                }
                if (chosen != null) result.Add(new YamlScalarNode(key), chosen);
            }
            return result;
        }

        private static string Key(YamlNode node) => (node as YamlScalarNode)?.Value ?? node.ToString();

        private static YamlNode Get(YamlMappingNode map, string key)
            => map == null ? null : map.Children.FirstOrDefault(kv => Key(kv.Key) == key).Value;

        private static bool Same(YamlNode a, YamlNode b)
        {
            if (a == null || b == null) return a == null && b == null;
            switch (a)
            {
                case YamlScalarNode sa when b is YamlScalarNode sb:
                    return sa.Value == sb.Value;
                case YamlSequenceNode qa when b is YamlSequenceNode qb:
                    return qa.Children.Count == qb.Children.Count && qa.Children.Zip(qb.Children, Same).All(x => x);
                case YamlMappingNode ma when b is YamlMappingNode mb:
                    return ma.Children.Count == mb.Children.Count
                           && ma.Children.All(kv => Same(kv.Value, Get(mb, Key(kv.Key))));
                default:
                    return false;
            }
        }
    }
}
