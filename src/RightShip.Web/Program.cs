using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RightShip.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<RightShip.Web.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:7071";
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBase) });
builder.Services.AddScoped<SettingsStore>();
await builder.Build().RunAsync();
