using System.Text.Json;
using Microsoft.JSInterop;
namespace RightShip.Web.Services;
public sealed class SettingsStore(IJSRuntime js)
{
 const string Key="rightship.preview.settings.v1";
 public async Task<PlannerSettings> LoadAsync()
 {
   try { var json=await js.InvokeAsync<string?>("localStorage.getItem",Key); return string.IsNullOrWhiteSpace(json)?new PlannerSettings():JsonSerializer.Deserialize<PlannerSettings>(json)??new PlannerSettings(); }
   catch(JSException){return new PlannerSettings();}
 }
 public async Task SaveAsync(PlannerSettings settings) => await js.InvokeVoidAsync("localStorage.setItem",Key,JsonSerializer.Serialize(settings));
}