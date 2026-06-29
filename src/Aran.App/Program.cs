using Aran.App.Components;
using Aran.App.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Session-per-connection state
builder.Services.AddScoped<AppSession>();
builder.Services.AddScoped<EngineRunner>();
builder.Services.AddScoped<ReportBuilder>();

// 10 MB upload limit
builder.WebHost.ConfigureKestrel(k =>
    k.Limits.MaxRequestBodySize = 10L * 1024 * 1024);

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
