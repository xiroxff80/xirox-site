using System;

namespace XIROX.Models
{
    public class Stats
    {
        public long TotalVisits { get; set; }
        public long PositiveFeedback { get; set; }
        public long NegativeFeedback { get; set; }
        public DateTime LaunchedAt { get; set; } =
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
