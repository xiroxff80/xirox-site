using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XIROX.Models;
using XIROX.Services;

namespace XIROX.Controllers
{
    [ApiController]
    [Route("api/metrics")]
    public class MetricsController : ControllerBase
    {
        private readonly IStatsStore _store;
        public MetricsController(IStatsStore store) => _store = store;

        [HttpGet("state")]
        public async Task<ActionResult<Stats>> State() => Ok(await _store.GetAsync());

        [HttpPost("visit")]
        public async Task<ActionResult<Stats>> Visit() => Ok(await _store.IncrementVisitAsync());

        /// <summary>
        /// prev/next: none | up | down
        /// مثال‌ها:
        /// - none -> up   : (+1,  0)
        /// - up   -> none : (-1,  0)
        /// - down -> up   : (+1, -1)
        /// - up   -> down : (-1, +1)
        /// </summary>
        [HttpPost("vote")]
        public async Task<ActionResult<Stats>> Vote([FromQuery] string prev = "none", [FromQuery] string next = "none")
        {
            int dp = 0, dn = 0;
            string P(string s) => (s ?? "none").ToLowerInvariant();

            var a = P(prev);
            var b = P(next);

            if (a == b) return Ok(await _store.GetAsync());

            if (a == "up") dp -= 1;
            else if (a == "down") dn -= 1;

            if (b == "up") dp += 1;
            else if (b == "down") dn += 1;

            var stats = await _store.ApplyDeltaAsync(dp, dn);
            return Ok(stats);
        }

        // Back-compat: increases positive
        [HttpPost("feedback")]
        public async Task<ActionResult<Stats>> Feedback() => Ok(await _store.ApplyDeltaAsync(+1, 0));
    }
}
