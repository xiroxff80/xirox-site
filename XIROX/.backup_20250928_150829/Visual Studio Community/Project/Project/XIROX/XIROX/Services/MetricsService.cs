using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XIROX.Models;

namespace XIROX.Services
{
    public class MetricsService
    {
        private readonly string _filePath;
        private readonly ILogger<MetricsService> _logger;
        private readonly object _gate = new();
        private MetricsState _state = new();

        public MetricsService(IHostEnvironment env, ILogger<MetricsService> logger)
        {
            _logger = logger;
            _filePath = Path.Combine(env.ContentRootPath, "App_Data", "metrics.json");
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            Load();
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    var s = JsonSerializer.Deserialize<MetricsState>(json);
                    if (s != null) { _state = s; return; }
                }
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to load metrics; using defaults"); }

            _state = new MetricsState
            {
                TotalVisits = 30,
                PositiveFeedback = 0,
                NegativeFeedback = 0,
                LaunchedAt = DateTime.UtcNow.AddYears(-1)
            };
            Save();
        }

        private void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_state, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to save metrics"); }
        }

        public MetricsState Get()
        {
            lock (_gate) { return new MetricsState
            {
                TotalVisits = _state.TotalVisits,
                PositiveFeedback = _state.PositiveFeedback,
                NegativeFeedback = _state.NegativeFeedback,
                LaunchedAt = _state.LaunchedAt
            }; }
        }

        public void Visit()
        {
            lock (_gate) { _state.TotalVisits++; Save(); }
        }

        public void Vote(string? prev, string? next)
        {
            static int V(string? v) => string.Equals(v, "up", StringComparison.OrdinalIgnoreCase) ? 1 :
                                       string.Equals(v, "down", StringComparison.OrdinalIgnoreCase) ? -1 : 0;
            lock (_gate)
            {
                var p = V(prev); var n = V(next);
                if (p == 1) _state.PositiveFeedback = Math.Max(0, _state.PositiveFeedback - 1);
                if (p == -1) _state.NegativeFeedback = Math.Max(0, _state.NegativeFeedback - 1);
                if (n == 1) _state.PositiveFeedback++;
                if (n == -1) _state.NegativeFeedback++;
                Save();
            }
        }
    }
}