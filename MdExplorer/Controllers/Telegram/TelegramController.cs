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

        public TelegramController(TelegramRemoteService remote, TelegramPairingService pairing)
        {
            _remote = remote;
            _pairing = pairing;
        }

        public sealed record PairingRequest(string Action);
        public sealed record EnabledRequest(bool Enabled);
        public sealed record NameRequest(string Name);

        [HttpGet("status")]
        public IActionResult GetStatus() => Ok(_remote.Status);

        /// <summary>Comincia un abbinamento: {action: connect|regenerate} → link da aprire sul telefono.</summary>
        [HttpPost("pairing")]
        public Task<IActionResult> StartPairing([FromBody] PairingRequest request, CancellationToken ct)
            => Run(async () => Ok(await _pairing.StartAsync(request?.Action ?? "connect", ct)));

        /// <summary>Lo stato dell'abbinamento; con <c>ready</c> mostra chi si è abbinato e aspetta la conferma.</summary>
        [HttpGet("pairing")]
        public Task<IActionResult> PollPairing(CancellationToken ct)
            => Run(async () => (IActionResult)Ok(await _pairing.PollAsync(ct) ?? (object)new { state = "none" }));

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
