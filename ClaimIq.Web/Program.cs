using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ClaimIq.Web;
using MudBlazor.Services;
using ClaimIq.Web.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<AuthenticatedHttpService>();
builder.Services.AddScoped<ClaimsService>();
builder.Services.AddScoped<FeatureFlagService>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
