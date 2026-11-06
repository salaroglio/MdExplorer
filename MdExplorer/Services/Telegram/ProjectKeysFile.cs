using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services;
using YamlDotNet.RepresentationModel;

namespace MdExplorer.Services.Telegram
{
    /// <summary>Il bot di progetto come sta in <c>.mde-keys.yml</c>: token, username e canale privato dove scrive.</summary>
    public sealed record ProjectBotKeys(string Token, string BotUsername, long ChannelId, string ChannelTitle);

    /// <summary>
    /// <c>.mde-keys.yml</c>, i segreti condivisi del progetto (docs-internal/Sprints/2026-10-31-Telecomando-Telegram.md, D7-D8).
    /// Regole: <b>questo è l'unico scrittore</b>; il file nasce solo col primo segreto (mai vuoto, mai un segnaposto); la
    /// lettura è rigorosa (un file illeggibile è un errore, non «non c'è»); le voci che non conosce si conservano, perché il
    /// file può venire da un MDE più nuovo: si lavora sul documento YAML, non su un modello che le scarterebbe.
    /// </summary>
    public static class ProjectKeysFile
    {
        public const string FileName = ".mde-keys.yml";

        private const string Header =
            "# Segreti condivisi del progetto, scritti da MdExplorer. Non modificarli a mano e non copiarli in altri progetti.\n" +
            "# Shared project secrets, written by MdExplorer. Do not edit them by hand or copy them to other projects.\n";

        public static string PathOf(string projectRoot) => Path.Combine(projectRoot, FileName);

        /// <summary>Il bot di progetto, o <c>null</c> se il file o la voce non ci sono (funzione spenta).</summary>
        public static ProjectBotKeys ReadProjectBot(string projectRoot)
        {
            var file = PathOf(projectRoot);
            return File.Exists(file) ? ParseProjectBot(File.ReadAllText(file), file) : null;
        }

        /// <summary>Come <see cref="ReadProjectBot"/>, dal testo (per esempio la versione dell'origin letta con <c>git show</c>).</summary>
        public static ProjectBotKeys ParseProjectBot(string text, string where)
        {
            var root = Load(text, where);
            if (Child(root, "telegram") is not YamlMappingNode telegram) return null;
            if (Child(telegram, "projectBot") is not YamlMappingNode bot) return null;
            var token = Scalar(bot, "token");
            var username = Scalar(bot, "botUsername");
            var channel = Scalar(bot, "channelId");
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(username) || !long.TryParse(channel, out var channelId))
                throw new InvalidOperationException(UiText.Say(
                    $"{where}: la voce telegram.projectBot è incompleta (servono token, botUsername e channelId).",
                    $"{where}: the telegram.projectBot entry is incomplete (token, botUsername and channelId are required)."));
            return new ProjectBotKeys(token, username, channelId, Scalar(bot, "channelTitle"));
        }

        /// <summary>Scrive (o sostituisce) la voce del bot di progetto; tutto il resto del file resta com'è.</summary>
        public static void WriteProjectBot(string projectRoot, ProjectBotKeys keys)
        {
            var file = PathOf(projectRoot);
            var root = File.Exists(file) ? Load(File.ReadAllText(file), file) : new YamlMappingNode();
            if (Child(root, "telegram") is not YamlMappingNode telegram)
            {
                telegram = new YamlMappingNode();
                root.Children[new YamlScalarNode("telegram")] = telegram;
            }
            telegram.Children[new YamlScalarNode("projectBot")] = new YamlMappingNode(
                new YamlScalarNode("token"), Quoted(keys.Token),
                new YamlScalarNode("botUsername"), new YamlScalarNode(keys.BotUsername),
                new YamlScalarNode("channelId"), new YamlScalarNode(keys.ChannelId.ToString()),
                new YamlScalarNode("channelTitle"), Quoted(keys.ChannelTitle ?? ""));

            using var writer = new StringWriter();
            new YamlStream(new YamlDocument(root)).Save(writer, assignAnchors: false);
            var text = writer.ToString().Replace("\r\n", "\n");
            // YamlDotNet chiude il documento con «...»: non serve.
            if (text.EndsWith("...\n")) text = text.Substring(0, text.Length - 4);
            File.WriteAllText(file, Header + text);
        }

        private static YamlMappingNode Load(string text, string where)
        {
            var stream = new YamlStream();
            try { stream.Load(new StringReader(text ?? "")); }
            catch (YamlDotNet.Core.YamlException ex)
            {
                throw new InvalidOperationException(UiText.Say(
                    $"{where} non è YAML valido ({ex.Message}): correggilo o recuperalo da git, MDE non lo riscrive da solo.",
                    $"{where} is not valid YAML ({ex.Message}): fix it or restore it from git, MDE will not rewrite it by itself."), ex);
            }
            if (stream.Documents.Count == 0) return new YamlMappingNode();
            return stream.Documents[0].RootNode as YamlMappingNode
                ?? throw new InvalidOperationException(UiText.Say(
                    $"{where}: il file deve essere un elenco di voci YAML.",
                    $"{where}: the file must be a YAML mapping."));
        }

        private static YamlNode Child(YamlMappingNode node, string key)
            => node.Children.FirstOrDefault(kv => (kv.Key as YamlScalarNode)?.Value == key).Value;

        private static string Scalar(YamlMappingNode node, string key) => (Child(node, key) as YamlScalarNode)?.Value;

        private static YamlScalarNode Quoted(string value)
            => new(value) { Style = YamlDotNet.Core.ScalarStyle.DoubleQuoted };
    }
}
