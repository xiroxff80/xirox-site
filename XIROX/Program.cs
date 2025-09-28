// Program.cs
using System.IO.Compression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetEscapades.AspNetCore.SecurityHeaders;
using WebOptimizer;

// سرویس‌ها/مدل‌های پروژه
using XIROX.Services;   // IEmailSender, SmtpEmailSender, IStatsStore, FileStatsStore, SiteStatsOptions

var builder = WebApplication.CreateBuilder(args);

// ========== MVC / Razor ==========
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ========== Response Compression ==========
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Optimal);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Optimal);

// ========== WebOptimizer (Bundle/Minify) ==========
builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.MinifyCssFiles("css/*.css");
    pipeline.MinifyJsFiles("js/*.js");
    // اگر SCSS داری:
    // pipeline.CompileScssFiles("css/**/*.scss");
});

// ========== SMTP & (optional) Site Stats ==========
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

// اگر از آمار/متریکس استفاده می‌کنی این دو خط را نگه دار
builder.Services.Configure<SiteStatsOptions>(builder.Configuration.GetSection("SiteStats"));
builder.Services.AddSingleton<IStatsStore, FileStatsStore>();

var app = builder.Build();

// ========== Errors / HSTS ==========
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

// اگر پشت Nginx/Cloudflare/Load Balancer هستی، Scheme/ClientIP درست می‌شود
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();

// Security Headers
app.UseSecurityHeaders(policies =>
    policies
        .AddDefaultSecurityHeaders()
        .AddReferrerPolicyNoReferrer()
        .AddContentSecurityPolicy(csp =>
        {
            csp.AddDefaultSrc().Self();

            // اسکریپت‌ها: چون inline در Views داری فعلاً UnsafeInline
            csp.AddScriptSrc().Self().UnsafeInline();

            // استایل‌ها: لوکال + Google Fonts + inline
            csp.AddStyleSrc().Self().UnsafeInline().From("https://fonts.googleapis.com");

            // فونت‌ها: لوکال + gstatic + data:
            csp.AddFontSrc().Self().From("https://fonts.gstatic.com").From("data:");

            // تصاویر: لوکال + data:
            csp.AddImgSrc().Self().From("data:");

            // برای preconnect فونت‌ها
            csp.AddConnectSrc().Self().From("https://fonts.gstatic.com");

            csp.AddBaseUri().Self();
            csp.AddFrameAncestors().Self();
            csp.AddObjectSrc().None();
            csp.AddUpgradeInsecureRequests();
        })
);

app.UseResponseCompression();
app.UseWebOptimizer();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int days = 30;
        ctx.Context.Response.Headers["Cache-Control"] = $"public, max-age={days * 86400}";
    }
});

app.UseRouting();
app.UseAuthorization();

// APIهایی که Attribute Routing دارند (مثل MetricsController)
app.MapControllers();

// مسیر پیش‌فرض MVC + Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
