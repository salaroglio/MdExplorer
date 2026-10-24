using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.Controllers.GitChat;
using MdExplorer.Hubs;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.Git;
using MdExplorer.Services.TeamChat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il segreto della stanza si distribuisce, non si ricrea (23/10/2026: una copia indietro ne ha creato uno suo e al pull
    /// è finito in conflitto). Aprendo la chat, una copia senza segreto prima chiede all'origin; una copia con un segreto
    /// diverso da quello pubblicato entra nella stanza di quello pubblicato, dove sono i colleghi.
    /// </summary>
    [TestClass]
    public class ChatRoomSecretFromOrigin_Should
    {
        private const string Config = "harness:\n  target: copilot\n";

        [TestMethod]
        public async Task Use_the_secret_already_on_the_origin_instead_of_creating_one()
        {
            using var ctx = new AgentCityContext();
            var (origin, anna) = Publish(ctx);
            var marco = Clone(ctx, origin, "marco");
            File.WriteAllText(Path.Combine(anna, ".development.yml"), Config + "agentCity:\n  enabled: false\n  roomSecret: PUBBLICATO\n");
            Git(anna, "commit", "-am", "segreto");
            Git(anna, "push", "origin", "main");

            var (info, key) = await RoomInfo(ctx, marco);

            Assert.AreEqual("notPulled", info.GetProperty("keyState").GetString(), info.ToString());
            Assert.AreEqual("PUBBLICATO", key, "si entra nella stanza dei colleghi");
            Assert.AreEqual(Config, File.ReadAllText(Path.Combine(marco, ".development.yml")), "niente segreto nuovo qui: arriva con lo scaricamento");
        }

        [TestMethod]
        public async Task Enter_the_room_of_the_published_secret_when_this_copy_has_another_one()
        {
            using var ctx = new AgentCityContext();
            var (origin, anna) = Publish(ctx);
            var marco = Clone(ctx, origin, "marco");
            File.WriteAllText(Path.Combine(anna, ".development.yml"), Config + "agentCity:\n  enabled: false\n  roomSecret: PUBBLICATO\n");
            Git(anna, "commit", "-am", "segreto");
            Git(anna, "push", "origin", "main");
            File.WriteAllText(Path.Combine(marco, ".development.yml"), Config + "agentCity:\n  enabled: false\n  roomSecret: CREATO-QUI\n");
            Git(marco, "commit", "-am", "segreto creato qui");
            Git(marco, "fetch", "origin");

            var (info, key) = await RoomInfo(ctx, marco);

            Assert.AreEqual("differs", info.GetProperty("keyState").GetString(), info.ToString());
            Assert.AreEqual("PUBBLICATO", key);
        }

        [TestMethod]
        public async Task Create_the_secret_only_when_the_origin_has_none()
        {
            using var ctx = new AgentCityContext();
            var (origin, _) = Publish(ctx);
            var marco = Clone(ctx, origin, "marco");

            var (info, key) = await RoomInfo(ctx, marco);

            Assert.AreEqual("notCommitted", info.GetProperty("keyState").GetString(), info.ToString());
            Assert.IsFalse(string.IsNullOrWhiteSpace(key));
            StringAssert.Contains(File.ReadAllText(Path.Combine(marco, ".development.yml")), "roomSecret: " + key);
        }

        [TestMethod]
        public async Task Refuse_to_open_the_room_when_the_file_has_a_git_conflict()
        {
            using var ctx = new AgentCityContext();
            var (origin, _) = Publish(ctx);
            var marco = Clone(ctx, origin, "marco");
            var conflicted = Config + "agentCity:\n<<<<<<< HEAD\n  roomSecret: A\n=======\n  roomSecret: B\n>>>>>>> 20def39\n";
            File.WriteAllText(Path.Combine(marco, ".development.yml"), conflicted);

            var (info, key) = await RoomInfo(ctx, marco);

            Assert.IsFalse(info.GetProperty("hasRemote").GetBoolean());
            StringAssert.Contains(info.GetProperty("error").GetString(), ".development.yml");
            Assert.IsNull(key);
            Assert.AreEqual(conflicted, File.ReadAllText(Path.Combine(marco, ".development.yml")), "il conflitto resta da risolvere, non si riscrive");
        }

        // ------------------------------------------------------------------

        /// <summary>Un origin bare e la copia di chi lo ha creato, con un .development.yml senza segreto già pubblicato.</summary>
        private static (string Origin, string Anna) Publish(AgentCityContext ctx)
        {
            var origin = Path.Combine(ctx.Factory.DataDir, "origins", Guid.NewGuid().ToString("N") + ".git");
            Directory.CreateDirectory(origin);
            Git(origin, "init", "--bare", "-b", "main");
            var anna = Path.Combine(ctx.Factory.DataDir, "anna-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(anna);
            Git(anna, "init", "-b", "main");
            Git(anna, "config", "user.email", "anna@example.com");
            Git(anna, "config", "user.name", "anna");
            File.WriteAllText(Path.Combine(anna, ".development.yml"), Config);
            Git(anna, "add", ".");
            Git(anna, "commit", "-m", "inizio");
            Git(anna, "remote", "add", "origin", origin);
            Git(anna, "push", "-u", "origin", "main");
            return (origin, anna);
        }

        private static string Clone(AgentCityContext ctx, string origin, string who)
        {
            var path = Path.Combine(ctx.Factory.DataDir, who + "-" + Guid.NewGuid().ToString("N"));
            Git(ctx.Factory.DataDir, "clone", origin, path);
            Git(path, "config", "user.email", who + "@example.com");
            Git(path, "config", "user.name", who);
            return path;
        }

        /// <summary>room-info com'è chiamato dalla chat; il relay è finto (sempre ok). Dà la risposta e la chiave registrata.</summary>
        private static async Task<(JsonElement Info, string Key)> RoomInfo(AgentCityContext ctx, string path)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            var sp = scope.ServiceProvider;
            var keys = new ChatRoomKeys();
            var controller = new GitChatController(NullLogger<GitChatController>.Instance, new RelayAlwaysOk(),
                sp.GetRequiredService<IHubContext<TeamChatHub>>(), sp.GetRequiredService<IProjectMetadataService>(),
                keys, sp.GetRequiredService<INativeGitTransport>());

            var result = (OkObjectResult)await controller.GetRoomInfo(path);
            var info = JsonSerializer.SerializeToElement(result.Value);
            var key = info.TryGetProperty("roomId", out var roomId) ? keys.Find(roomId.GetString())?.RoomSecret : null;
            return (info, key);
        }

        private static string Git(string cwd, params string[] args)
        {
            var res = GitCli.Run(cwd, args);
            if (res.Code != 0) throw new InvalidOperationException($"git {string.Join(" ", args)}: {res.Err}");
            return res.Out;
        }

        private sealed class RelayAlwaysOk : ITeamChatService
        {
            public Task<ChatRoomCheck> CheckRoomAsync(string roomId) => Task.FromResult(new ChatRoomCheck { Ok = true });
            public Task JoinRoom(string roomId, string connectionId, ChatUserInfo user) => throw new NotSupportedException();
            public Task LeaveRoom(string roomId, string connectionId) => throw new NotSupportedException();
            public Task<ChatMessageDto> SendMessage(string roomId, ChatMessageDto message) => throw new NotSupportedException();
            public Task<List<ChatMessageDto>> GetRecentMessages(string roomId, int limit) => throw new NotSupportedException();
            public PresenceInfoDto GetPresence(string roomId) => throw new NotSupportedException();
            public Task RegisterProjectOpen(string roomId, string oderId, ChatUserInfo user) => throw new NotSupportedException();
            public Task UnregisterProjectOpen(string roomId, string oderId) => throw new NotSupportedException();
            public int GetProjectUsersCount(string roomId) => throw new NotSupportedException();
        }
    }
}
