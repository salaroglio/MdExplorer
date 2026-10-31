using System;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Services.Telegram;
using Microsoft.AspNetCore.Mvc;

namespace MdExplorer.Service.Controllers.Telegram
{
    /// <summary>
    /// Telecomando Telegram di questa installazione (docs-internal/Sprints/2026-10-31-Telecomando-Telegram.md, F2): stato,
    /// abbinamento col bot gestore (con conferma di chi si è abbinato), interruttore, «Prendi il controllo», scollega, nome.
    /// Gli errori tornano col loro testo: l'interfaccia li mostra così come sono.
    /// </summary>
    [Route("api/telegram")]
    [ApiController]
    public class TelegramController : ControllerBase
    {
        private readonly TelegramRemoteService _remote;
        private readonly TelegramPairingService _pairing;
        private readonly ProjectBotService _projectBot;

        public TelegramController(TelegramRemoteService remote, TelegramPairingService pairing, ProjectBotService projectBot)
        {
            _remote = remote;
            _pairing = pairing;
            _projectBot = projectBot;
        }

        /// <param name="Action">connect | regenerate | project</param>
        /// <param name="ProjectPath">Solo per <c>project</c>: la cartella del progetto.</param>
        public sealed record PairingRequest(string Action, string ProjectPath);
        public sealed record ProjectRequest(string ProjectPath);
        public sealed record EnabledRequest(bool Enabled);
        public sealed record NameRequest(string Name);

        [HttpGet("status")]
        public IActionResult GetStatus() => Ok(_remote.Status);

        /// <summary>Comincia un abbinamento: {action: connect|regenerate} → link da aprire sul telefono.</summary>
        [HttpPost("pairing")]
        public Task<IActionResult> StartPairing([FromBody] PairingRequest request, CancellationToken ct)
            => Run(async () => Ok(await _pairing.StartAsync(request?.Action ?? "connect", request?.ProjectPath, ct)));

        /// <summary>Lo stato dell'abbinamento; con <c>ready</c> mostra chi si è abbinato e aspetta la conferma.</summary>
        [HttpGet("pairing")]
        public Task<IActionResult> PollPairing(CancellationToken ct)
            => Run(async () => (IActionResult)Ok(await _pairing.PollAsync(ct) ?? (object)new { state = "none" }));

        /// <summary>Il link dell'abbinamento in corso come QR (SVG), da inquadrare col telefono.</summary>
        [HttpGet("pairing/qr")]
        public IActionResult PairingQr()
        {
            var link = _pairing.CurrentLink;
            if (link == null) return NotFound();
            using var data = new QRCoder.QRCodeGenerator().CreateQrCode(link, QRCoder.QRCodeGenerator.ECCLevel.M);
            var svg = new QRCoder.SvgQRCode(data).GetGraphic(6);
            return Content(svg, "image/svg+xml");
        }

        [HttpPost("pairing/confirm")]
        public Task<IActionResult> ConfirmPairing(CancellationToken ct)
            => Run(async () => { await _pairing.ConfirmAsync(ct); return Ok(_remote.Status); });

        [HttpDelete("pairing")]
        public Task<IActionResult> CancelPairing(CancellationToken ct)
            => Run(async () => { await _pairing.CancelAsync(ct); return Ok(new { state = "none" }); });

        [HttpPost("enabled")]
        public Task<IActionResult> SetEnabled([FromBody] EnabledRequest request, CancellationToken ct)
            => Run(async () => { await _remote.SetEnabledAsync(request.Enabled, ct); return Ok(_remote.Status); });

        [HttpPost("take-control")]
        public Task<IActionResult> TakeControl(CancellationToken ct)
            => Run(async () => { await _remote.TakeControlAsync(ct); return Ok(_remote.Status); });

        [HttpPost("disconnect")]
        public Task<IActionResult> Disconnect(CancellationToken ct)
            => Run(async () => { await _remote.DisconnectAsync(ct); return Ok(_remote.Status); });

        [HttpPut("installation-name")]
        public Task<IActionResult> SetInstallationName([FromBody] NameRequest request)
            => Run(() => { _remote.SetInstallationName(request?.Name); return Task.FromResult<IActionResult>(Ok(_remote.Status)); });

        // ------------------------------------------------------------------ bot di progetto (F4)

        [HttpGet("project")]
        public Task<IActionResult> ProjectStatus([FromQuery] string path, CancellationToken ct)
            => Run(async () => Ok(await _projectBot.StatusAsync(path, ct)));

        /// <summary>Prima di creare: il progetto non ha già un bot (qui o sull'origin); il repository è pubblico?</summary>
        [HttpGet("project/preflight")]
        public Task<IActionResult> ProjectPreflight([FromQuery] string path, CancellationToken ct)
            => Run(async () => Ok(await _projectBot.PreflightAsync(path, ct)));

        [HttpDelete("project/setup")]
        public IActionResult CancelProjectSetup([FromQuery] string path)
        {
            _projectBot.CancelSetup(path);
            return Ok(new { state = "none" });
        }

        [HttpPost("project/invite")]
        public Task<IActionResult> ProjectInvite([FromBody] ProjectRequest request, CancellationToken ct)
            => Run(async () => Ok(new { link = await _projectBot.InviteAsync(request?.ProjectPath, ct) }));

        [HttpPost("project/test")]
        public Task<IActionResult> ProjectTest([FromBody] ProjectRequest request, CancellationToken ct)
            => Run(async () => { await _projectBot.SendTestAsync(request?.ProjectPath, ct); return Ok(new { ok = true }); });

        /// <summary>Un link Telegram come QR (invito al canale, aggiunta del bot al canale). Solo link t.me.</summary>
        [HttpGet("qr")]
        public IActionResult Qr([FromQuery] string text)
        {
            if (string.IsNullOrEmpty(text) || !text.StartsWith("https://t.me/", StringComparison.Ordinal) || text.Length > 512)
                return BadRequest(new { error = "solo link https://t.me/" });
            using var data = new QRCoder.QRCodeGenerator().CreateQrCode(text, QRCoder.QRCodeGenerator.ECCLevel.M);
            return Content(new QRCoder.SvgQRCode(data).GetGraphic(6), "image/svg+xml");
        }

        /// <summary>Errori della persona (precondizioni, gestore, Telegram) → 400 col testo; il resto risale.</summary>
        private async Task<IActionResult> Run(Func<Task<IActionResult>> action)
        {
            try { return await action(); }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or TelegramApiException
                                           or System.Net.Http.HttpRequestException)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
