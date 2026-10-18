using System;
using System.Linq;
using System.Text.Json;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Features.Services.AI.ClaudeCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.ClaudeCode
{
    /// <summary>
    /// L'elenco dei modelli di Claude Code viene dalla risposta a <c>initialize</c> sullo stream-json.
    /// La risposta qui sotto ha la forma catturata il 13/09/2026 da claude 2.1.270, senza il blocco
    /// <c>account</c> (contiene l'email). Test puri: nessun processo, nessun token.
    /// </summary>
    [TestClass]
    public class ClaudeCodeModelSource_Should
    {
        private const string Initialize = @"{""type"":""control_response"",""response"":{""subtype"":""success"",""request_id"":""mde-list-models"",""response"":{""commands"":[],""models"":[
            {""value"":""default"",""resolvedModel"":""claude-opus-5[1m]"",""displayName"":""Default (recommended)"",""description"":""Opus 5 with 1M context · Best for everyday, complex tasks"",""supportsEffort"":true},
            {""value"":""opus[1m]"",""resolvedModel"":""claude-opus-5[1m]"",""displayName"":""Opus (1M context)"",""description"":""Opus 5 with 1M context · Best for everyday, complex tasks""},
            {""value"":""claude-fable-5-1[1m]"",""resolvedModel"":""claude-fable-5-1"",""displayName"":""Fable"",""description"":""Fable 5.1 · Most capable for your hardest and longest-running tasks""},
            {""value"":""sonnet"",""resolvedModel"":""claude-sonnet-5"",""displayName"":""Sonnet"",""description"":""Sonnet 5 · Efficient for routine tasks""},
            {""value"":""haiku"",""resolvedModel"":""claude-haiku-4-5-20251001"",""displayName"":""Haiku"",""description"":""Haiku 4.5 · Fastest for quick answers""}
        ]},""pending_permission_requests"":[]}}";

        [TestMethod]
        public void Leggere_i_modelli_nellordine_del_cli()
        {
            var models = ClaudeCodeModelSource.ParseInitializeResponse(Initialize);

            CollectionAssert.AreEqual(
                new[] { "default", "opus[1m]", "claude-fable-5-1[1m]", "sonnet", "haiku" },
                models.Select(m => m.Id).ToArray(),
                "l'id è `value`: quello che si passa a --model e a set_model");
            Assert.AreEqual("Opus (1M context)", models[1].Name);
            Assert.IsTrue(models.All(m => m.Provider == ProviderType.ClaudeCode));
        }

        [TestMethod]
        public void Dire_nella_descrizione_quale_modello_ce_dietro_lalias()
        {
            var sonnet = ClaudeCodeModelSource.ParseInitializeResponse(Initialize).Single(m => m.Id == "sonnet");

            Assert.AreEqual("Sonnet 5 · Efficient for routine tasks (claude-sonnet-5)", sonnet.Description);
        }

        [TestMethod]
        public void Prendere_il_contesto_dal_suffisso_che_mette_il_cli()
        {
            var models = ClaudeCodeModelSource.ParseInitializeResponse(Initialize).ToDictionary(m => m.Id);

            Assert.AreEqual(ClaudeCodeModelSource.OneMillionContext, models["opus[1m]"].InputTokenLimit);
            Assert.AreEqual(ClaudeCodeModelSource.OneMillionContext, models["default"].InputTokenLimit, "l'alias porta il suffisso nel resolvedModel");
            Assert.AreEqual(ClaudeCodeModelSource.StandardContext, models["haiku"].InputTokenLimit);
        }

        [TestMethod]
        public void Fallire_col_motivo_quando_il_cli_rifiuta_la_richiesta()
        {
            const string error = @"{""type"":""control_response"",""response"":{""subtype"":""error"",""request_id"":""mde-list-models"",""error"":""Unsupported control request subtype: initialize""}}";

            var ex = Assert.ThrowsException<InvalidOperationException>(() => ClaudeCodeModelSource.ParseInitializeResponse(error));

            StringAssert.Contains(ex.Message, "Unsupported control request subtype: initialize");
        }

        [TestMethod]
        public void Fallire_quando_la_risposta_non_ha_lelenco_invece_di_restituire_niente()
        {
            const string noModels = @"{""type"":""control_response"",""response"":{""subtype"":""success"",""request_id"":""mde-list-models"",""response"":{""commands"":[]}}}";
            const string emptyModels = @"{""type"":""control_response"",""response"":{""subtype"":""success"",""request_id"":""mde-list-models"",""response"":{""models"":[{""displayName"":""senza id""}]}}}";

            StringAssert.Contains(
                Assert.ThrowsException<InvalidOperationException>(() => ClaudeCodeModelSource.ParseInitializeResponse(noModels)).Message,
                "claude update");
            Assert.ThrowsException<InvalidOperationException>(() => ClaudeCodeModelSource.ParseInitializeResponse(emptyModels),
                "una voce senza value non è selezionabile, e un elenco senza voci non è un elenco");
        }

        [TestMethod]
        public void Riconoscere_solo_la_propria_risposta()
        {
            Assert.IsTrue(ClaudeCodeModelSource.IsControlResponseFor(Initialize, ClaudeCodeModelSource.RequestId));
            Assert.IsFalse(ClaudeCodeModelSource.IsControlResponseFor(Initialize, "altra-richiesta"));
            Assert.IsFalse(ClaudeCodeModelSource.IsControlResponseFor(@"{""type"":""system"",""subtype"":""init""}", ClaudeCodeModelSource.RequestId));
            Assert.IsFalse(ClaudeCodeModelSource.IsControlResponseFor("non è json", ClaudeCodeModelSource.RequestId));
            Assert.IsFalse(ClaudeCodeModelSource.IsControlResponseFor("", ClaudeCodeModelSource.RequestId));
        }

        [TestMethod]
        public void Chiedere_initialize_senza_aprire_un_turno()
        {
            using var doc = JsonDocument.Parse(ClaudeCodeModelSource.BuildInitializeRequest());
            var root = doc.RootElement;

            Assert.AreEqual("control_request", root.GetProperty("type").GetString());
            Assert.AreEqual(ClaudeCodeModelSource.RequestId, root.GetProperty("request_id").GetString());
            Assert.AreEqual("initialize", root.GetProperty("request").GetProperty("subtype").GetString());
            Assert.IsFalse(ClaudeCodeModelSource.Arguments.Contains("--model"), "l'elenco non dipende da un modello");
        }
    }
}
