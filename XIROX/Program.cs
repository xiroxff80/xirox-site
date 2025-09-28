using Microsoft.AspNetCore.HttpOverrides;
using XIROX.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // app.UseHsts(); // TLS بیرون کانتینر انجام میشه
}

// Forwarded headers از رندر
var fwd = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
fwd.KnownNetworks.Clear();
fwd.KnownProxies.Clear();
app.UseForwardedHeaders(fwd);

// داخل کانتینر HTTPS ردایرکت نکن؛ Render خودش TLS میده
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Health check برای پروب و تست
app.MapGet("/healthz", () => Results.Ok("OK"));

app.Run();