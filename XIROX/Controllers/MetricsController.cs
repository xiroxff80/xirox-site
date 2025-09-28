using Microsoft.AspNetCore.Mvc;
using XIROX.Models;
using XIROX.Services;

namespace XIROX.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController : ControllerBase
    {
        private readonly MetricsService _svc;
        public MetricsController(MetricsService svc) { _svc = svc; }

        [HttpPost("visit")] public ActionResult<MetricsState> Visit() { _svc.Visit(); return Ok(_svc.Get()); }
        [HttpGet("state")] public ActionResult<MetricsState> State() => Ok(_svc.Get());
        [HttpPost("vote")] public ActionResult<MetricsState> Vote([FromQuery] string? prev="none",[FromQuery] string? next="none"){ _svc.Vote(prev,next); return Ok(_svc.Get()); }
    }
}