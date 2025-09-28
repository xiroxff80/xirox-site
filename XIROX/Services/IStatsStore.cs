using System.Threading.Tasks;
using XIROX.Models;

namespace XIROX.Services
{
    public interface IStatsStore
    {
        Task<Stats> GetAsync();
        Task<Stats> IncrementVisitAsync();

        // تغییر اتمیک رأی‌ها (می‌تواند منفی/مثبت باشد؛ از صفر کمتر نمی‌شود)
        Task<Stats> ApplyDeltaAsync(int deltaPositive, int deltaNegative);

        // سازگاری با نسخه‌های قبلی
        Task<Stats> IncrementPositiveAsync() => ApplyDeltaAsync(+1, 0);
        Task<Stats> IncrementNegativeAsync() => ApplyDeltaAsync(0, +1);
    }
}
