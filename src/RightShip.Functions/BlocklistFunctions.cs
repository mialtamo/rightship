using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using RightShip.Core;

namespace RightShip.Functions;

public sealed class BlocklistFunctions(EntraValidator auth)
{
    public sealed record PreviewRequest(string Content, List<ManagedIpGroup>? ExistingGroups, string? GroupPrefix);

    [Function("Health")]
    public async Task<HttpResponseData> Health([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { status = "ok", mode = "preview-only" });
        return response;
    }

    [Function("Preview")]
    public async Task<HttpResponseData> Preview([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "blocklists/preview")] HttpRequestData req)
    {
        if (!await auth.HasRoleAsync(req, "RightShip.Operator", "RightShip.Administrator"))
        {
            var unauthorized = req.CreateResponse(HttpStatusCode.Forbidden);
            await unauthorized.WriteStringAsync("Entra authentication with Operator or Administrator role is required.");
            return unauthorized;
        }
        try
        {
            if (req.Body.CanSeek && req.Body.Length > 2_000_000)
                return await Failure(req, HttpStatusCode.RequestEntityTooLarge, "Maximum upload is 2 MB.");
            var payload = await JsonSerializer.DeserializeAsync<PreviewRequest>(req.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (payload?.Content is null || payload.Content.Length > 2_000_000)
                return await Failure(req, HttpStatusCode.BadRequest, "Missing content or upload exceeds 2 MB.");
            // The caller-provided inventory is untrusted and only supports offline planning.
            // Future deployment MUST discover real managed groups via Azure, never trust this list.
            var plan = IpAllocator.BuildPlan(payload.Content, payload.ExistingGroups ?? [], payload.GroupPrefix ?? "rightship-block-");
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(plan);
            return response;
        }
        catch (JsonException) { return await Failure(req, HttpStatusCode.BadRequest, "Invalid JSON."); }
        catch (ArgumentException e) { return await Failure(req, HttpStatusCode.BadRequest, e.Message); }
    }

    [Function("Deploy")]
    public async Task<HttpResponseData> Deploy([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "blocklists/deploy")] HttpRequestData req)
    {
        if (!await auth.HasRoleAsync(req, "RightShip.Administrator"))
            return await Failure(req, HttpStatusCode.Forbidden, "Administrator role required.");
        return await Failure(req, HttpStatusCode.NotImplemented, "Azure writes are not enabled in Phase 1. Preview only.");
    }

    private static async Task<HttpResponseData> Failure(HttpRequestData req, HttpStatusCode status, string message)
    {
        var response = req.CreateResponse(status);
        await response.WriteAsJsonAsync(new { error = message });
        return response;
    }
}
