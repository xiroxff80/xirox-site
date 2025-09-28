using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using XIROX.Models;

namespace XIROX.Services
{
    public sealed class FileStatsStore : IStatsStore
    {
        private readonly string _path;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly JsonSerializerOptions _json = new() { WriteIndented = true };

        public FileStatsStore(
            IWebHostEnvironment env,
            IOptions<SiteStatsOptions> optsAccessor,
            IConfiguration cfg)
        {
            var opts = optsAccessor?.Value ?? new SiteStatsOptions();
            var dataFile = string.IsNullOrWhiteSpace(opts.DataFile) ? "App_Data/stats.json" : opts.DataFile;
            if (!Path.IsPathRooted(dataFile)) dataFile = Path.Combine(env.ContentRootPath, dataFile);

            Directory.CreateDirectory(Path.GetDirectoryName(dataFile)!);
            _path = dataFile;

            if (!File.Exists(_path))
            {
                var s = new Stats();

                // خواندن تاریخ لانچ به صورت رشته (با UTC)
                var siteBirthday = cfg["SiteStats:SiteBirthday"];
                var xiroxLaunch = cfg["Xirox:LaunchDateUtc"];
                if (DateTime.TryParse(siteBirthday, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed1))
                    s.LaunchedAt = DateTime.SpecifyKind(parsed1, DateTimeKind.Utc);
                else if (DateTime.TryParse(xiroxLaunch, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed2))
                    s.LaunchedAt = DateTime.SpecifyKind(parsed2, DateTimeKind.Utc);

                SaveUnlockedAsync(s).GetAwaiter().GetResult();
            }
        }

        public async Task<Stats> GetAsync()
        {
            await _lock.WaitAsync();
            try { return await LoadUnlockedAsync() ?? new Stats(); }
            finally { _lock.Release(); }
        }

        public async Task<Stats> IncrementVisitAsync()
        {
            await _lock.WaitAsync();
            try
            {
                var s = await LoadUnlockedAsync() ?? new Stats();
                s.TotalVisits++;
                await SaveUnlockedAsync(s);
                return s;
            }
            finally { _lock.Release(); }
        }

        public async Task<Stats> ApplyDeltaAsync(int deltaPositive, int deltaNegative)
        {
            await _lock.WaitAsync();
            try
            {
                var s = await LoadUnlockedAsync() ?? new Stats();
                // clamp >= 0
                long p = s.PositiveFeedback + deltaPositive;
                long n = s.NegativeFeedback + deltaNegative;
                s.PositiveFeedback = p < 0 ? 0 : p;
                s.NegativeFeedback = n < 0 ? 0 : n;

                await SaveUnlockedAsync(s);
                return s;
            }
            finally { _lock.Release(); }
        }

        // ---------- helpers ----------
        private async Task<Stats?> LoadUnlockedAsync()
        {
            if (!File.Exists(_path)) return new Stats();
            await using var fs = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            try { return await JsonSerializer.DeserializeAsync<Stats>(fs) ?? new Stats(); }
            catch { return new Stats(); }
        }

        private async Task SaveUnlockedAsync(Stats s)
        {
            await using var fs = File.Open(_path, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(fs, s, _json);
        }
    }
}
