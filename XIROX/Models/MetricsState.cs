namespace XIROX.Models
{
    public class MetricsState
    {
        public long TotalVisits { get; set; }
        public long PositiveFeedback { get; set; }
        public long NegativeFeedback { get; set; }
        public DateTime LaunchedAt { get; set; }
    }
}