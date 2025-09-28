using Microsoft.AspNetCore.Mvc;
using XIROX.Models;
using XIROX.Services;

namespace XIROX.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly IEmailSender _email;
        private readonly ILogger<TicketController> _logger;

        public TicketController(IEmailSender email, ILogger<TicketController> logger)
        {
            _email = email;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] TicketRequest dto, CancellationToken ct)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(new { ok = false, error = "empty_message" });

            var subj = string.IsNullOrWhiteSpace(dto.Subject) ? $"New ticket from {dto.Name ?? "Anonymous"}" : dto.Subject!;
            var body = $"Name: {dto.Name}\nEmail: {dto.Email}\nTime(UTC): {DateTime.UtcNow:u}\n\n{dto.Message}";

            try
            {
                await _email.SendAsync(subj, body, dto.Name, dto.Email, ct);
                return Ok(new { ok = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket send failed");
                return StatusCode(500, new { ok = false, error = "send_failed" });
            }
        }
    }
}