using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using RightShip.Core;
namespace RightShip.Functions;
public sealed class BlocklistFunctions(EntraValidator auth)
{
 public sealed record PreviewRequest(string Content,List<ManagedIpGroup>? ExistingGroups,string? GroupPrefix,int StartingNumber=0,int Capacity=5000);
 [Function("Health")]
 public async Task<HttpResponseData> Health([HttpTrigger(AuthorizationLevel.Anonymous,"get",Route="health")]HttpRequestData req){
  var r=req.CreateResponse(HttpStatusCode.OK);await r.WriteAsJsonAsync(new{status="ok",mode="preview-only"});return r;
 }
 [Function("Preview")]
 public async Task<HttpResponseData> Preview([HttpTrigger(AuthorizationLevel.Anonymous,"post",Route="blocklists/preview")]HttpRequestData req){
  if(!auth.AllowLocalPreview && !await auth.HasRoleAsync(req,"RightShip.Operator","RightShip.Administrator"))return await Failure(req,HttpStatusCode.Forbidden,"Operator or Administrator Entra role required");
  try{
   var payload=await JsonSerializer.DeserializeAsync<PreviewRequest>(req.Body,new JsonSerializerOptions{PropertyNameCaseInsensitive=true});
   if(payload?.Content is null||payload.Content.Length>2_000_000)return await Failure(req,HttpStatusCode.BadRequest,"Missing or oversized content (2 MB max)");
   // Caller-provided groups are for offline preview ONLY. Deploy must use Azure inventory.
   var plan=IpAllocator.BuildPlan(payload.Content,payload.ExistingGroups??[],payload.GroupPrefix??"IPBAN",payload.Capacity,payload.StartingNumber);
   var r=req.CreateResponse(HttpStatusCode.OK);await r.WriteAsJsonAsync(plan);return r;
  }catch(JsonException){return await Failure(req,HttpStatusCode.BadRequest,"Invalid JSON");}
  catch(ArgumentException e){return await Failure(req,HttpStatusCode.BadRequest,e.Message);}
 }
 [Function("Deploy")]
 public async Task<HttpResponseData> Deploy([HttpTrigger(AuthorizationLevel.Anonymous,"post",Route="blocklists/deploy")]HttpRequestData req){
  if(!await auth.HasRoleAsync(req,"RightShip.Administrator"))return await Failure(req,HttpStatusCode.Forbidden,"Administrator role required");
  return await Failure(req,HttpStatusCode.NotImplemented,"Azure writes are not enabled in Phase 1");
 }
 static async Task<HttpResponseData> Failure(HttpRequestData req,HttpStatusCode status,string message){var r=req.CreateResponse(status);await r.WriteAsJsonAsync(new{error=message});return r;}
}