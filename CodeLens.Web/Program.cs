using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CodeLens.Web;
using CodeLens.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── API base address (configured via wwwroot/appsettings.json) ────────────────
var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "https://localhost:7001";

// ── Blazored Local Storage ────────────────────────────────────────────────────
builder.Services.AddBlazoredLocalStorage();

// ── Auth state ────────────────────────────────────────────────────────────────
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthStateProvider>());

// ── Named HTTP client with Bearer-token injection ─────────────────────────────
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddHttpClient("CodeLensApi", client =>
    client.BaseAddress = new Uri(apiBaseAddress))
    .AddHttpMessageHandler<AuthHeaderHandler>();

// Provide HttpClient (used by AuthService / ReviewService) from the named client
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("CodeLensApi"));

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

await builder.Build().RunAsync();
