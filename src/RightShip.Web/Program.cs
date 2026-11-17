using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using RightShip.Web.Services;
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<RightShip.Web.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
var apiBase=builder.Configuration["ApiBaseUrl"]??"http://localhost:7071";
var scope=builder.Configuration["ApiScope"]??throw new InvalidOperationException("ApiScope missing");
builder.Services.AddHttpClient("RightShip.Api",client=>client.BaseAddress=new Uri(apiBase))
 .AddHttpMessageHandler(sp=>sp.GetRequiredService<AuthorizationMessageHandler>().ConfigureHandler(authorizedUrls:[apiBase],scopes:[scope]));
builder.Services.AddScoped(sp=>sp.GetRequiredService<IHttpClientFactory>().CreateClient("RightShip.Api"));
builder.Services.AddScoped<SettingsStore>();
builder.Services.AddMsalAuthentication(options=>{builder.Configuration.Bind("AzureAd",options.ProviderOptions.Authentication);options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);});
await builder.Build().RunAsync();