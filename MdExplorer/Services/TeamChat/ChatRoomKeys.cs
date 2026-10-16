using System.Collections.Concurrent;
using MdExplorer.Features.Federation;

namespace MdExplorer.Services.TeamChat
{
    /// <summary>La chiave di una stanza della chat: il segreto del progetto e il progetto da cui viene.</summary>
    public sealed class ChatRoomKey
    {
        public string RoomId { get; init; }
        public string ProjectPath { get; init; }
        public string RoomSecret { get; init; }

        /// <summary>Ciò che il relay vuole per far entrare nella stanza (lo stesso della città).</summary>
        public string JoinToken => FederationCrypto.DeriveJoinToken(RoomSecret, RoomId);
    }

    /// <summary>
    /// roomId → segreto del progetto. La chat lavora per roomId (hub, relay), ma la chiave sta nel
    /// progetto (<c>agentCity.roomSecret</c> in git): la registra chi conosce il percorso, cioè
    /// <c>GitChatController</c> quando la UI chiede la stanza o apre il progetto.
    /// </summary>
    public interface IChatRoomKeys
    {
        void Register(string roomId, string projectPath, string roomSecret);

        /// <summary>La chiave della stanza, o null se nessun progetto l'ha registrata.</summary>
        ChatRoomKey Find(string roomId);
    }

    public sealed class ChatRoomKeys : IChatRoomKeys
    {
        private readonly ConcurrentDictionary<string, ChatRoomKey> _keys = new();

        public void Register(string roomId, string projectPath, string roomSecret)
            => _keys[roomId] = new ChatRoomKey { RoomId = roomId, ProjectPath = projectPath, RoomSecret = roomSecret };

        public ChatRoomKey Find(string roomId)
            => roomId != null && _keys.TryGetValue(roomId, out var key) ? key : null;
    }
}

namespace MdExplorer.Services.TeamChat
{
    using System.Text.Json;
    using MdExplorer.Hubs;

    /// <summary>
    /// Il messaggio della chat in busta chiusa: testo, nome ed email del mittente cifrati insieme col
    /// segreto del progetto (AES-256-GCM, etichetta <c>mdchat-v1</c>). Il relay salva solo la busta.
    /// </summary>
    public static class ChatEnvelope
    {
        private sealed class Sealed
        {
            public string Content { get; set; }
            public string SenderName { get; set; }
            public string SenderEmail { get; set; }
        }

        private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        /// <summary>Testo che si mostra al posto di una busta che il segreto del progetto non apre.</summary>
        public const string Unreadable = "🔒 Messaggio non leggibile: è stato scritto con un altro segreto del progetto.";

        public static string Seal(ChatRoomKey key, ChatMessageDto message)
            => FederationCrypto.EncryptChat(key.RoomSecret, key.RoomId, JsonSerializer.Serialize(new Sealed
            {
                Content = message.Content,
                SenderName = message.SenderName,
                SenderEmail = message.SenderEmail,
            }, Json));

        private sealed class SealedUser
        {
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string UserEmail { get; set; }
        }

        /// <summary>
        /// Chi è in stanza, come lo vede il relay: nome, email e id veri in una busta in <c>userName</c>,
        /// email vuota, e come <c>userId</c> uno pseudonimo che il relay non sa rovesciare.
        /// </summary>
        public static (string UserId, string UserName, string UserEmail) SealUser(ChatRoomKey key, ChatUserInfo user)
            => (FederationCrypto.ChatPseudonym(key.RoomSecret, key.RoomId, user.UserEmail),
                FederationCrypto.EncryptChat(key.RoomSecret, key.RoomId, JsonSerializer.Serialize(new SealedUser
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    UserEmail = user.UserEmail,
                }, Json)),
                "");

        /// <summary>Apre, sul posto, chi è in stanza come arriva dal relay; i client di prima mandano in chiaro.</summary>
        public static ChatUserInfo OpenUser(ChatRoomKey key, ChatUserInfo user)
        {
            if (user == null || !FederationCrypto.IsChatEnvelope(user.UserName)) return user;
            try
            {
                if (key == null) throw new FederationCryptoException("nessuna chiave per la stanza");
                var plain = JsonSerializer.Deserialize<SealedUser>(FederationCrypto.DecryptChat(key.RoomSecret, key.RoomId, user.UserName), Json);
                user.UserId = plain?.UserId ?? user.UserId;
                user.UserName = plain?.UserName ?? "";
                user.UserEmail = plain?.UserEmail ?? "";
            }
            catch (System.Exception ex) when (ex is FederationCryptoException or JsonException)
            {
                user.UserName = "?";
                user.UserEmail = "";
            }
            return user;
        }

        /// <summary>
        /// Apre il messaggio come è arrivato dal relay, sul posto. Un messaggio di prima della cifratura
        /// (in chiaro) resta com'è; una busta che non si apre diventa <see cref="Unreadable"/>, mai il testo cifrato.
        /// </summary>
        public static ChatMessageDto Open(ChatRoomKey key, ChatMessageDto message)
        {
            if (message == null || !FederationCrypto.IsChatEnvelope(message.Content)) return message;
            try
            {
                if (key == null) throw new FederationCryptoException("nessuna chiave per la stanza");
                var plain = JsonSerializer.Deserialize<Sealed>(FederationCrypto.DecryptChat(key.RoomSecret, key.RoomId, message.Content), Json);
                message.Content = plain?.Content ?? "";
                message.SenderName = plain?.SenderName ?? "";
                message.SenderEmail = plain?.SenderEmail ?? "";
            }
            catch (System.Exception ex) when (ex is FederationCryptoException or JsonException)
            {
                message.Content = Unreadable;
                message.SenderName = "?";
                message.SenderEmail = "";
            }
            return message;
        }
    }
}
