using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Azure.Functions.Worker.Http;

namespace RightShip.Functions;

public sealed class EntraValidator
{
    private readonly ConfigurationManager<OpenIdConnectConfiguration>? _metadata;
    private readonly string? _audience;

    public EntraValidator()
    {
        var tenant = Environment.GetEnvironmentVariable("ENTRA_TENANT_ID");
        _audience = Environment.GetEnvironmentVariable("ENTRA_API_AUDIENCE");
        var loginHost = Environment.GetEnvironmentVariable("ENTRA_LOGIN_HOST") ?? "login.microsoftonline.com";
        if (loginHost is not ("login.microsoftonline.com" or "login.microsoftonline.us"))
            throw new InvalidOperationException("Unsupported Entra login host");
        if (string.IsNullOrWhiteSpace(tenant) || string.IsNullOrWhiteSpace(_audience)) return;
        var authority = $"https://{loginHost}/{tenant}/v2.0";
        _metadata = new ConfigurationManager<OpenIdConnectConfiguration>(
            authority + "/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever { RequireHttps = true });
    }

    public bool AllowLocalPreview =>
        string.Equals(Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase)
        && string.Equals(Environment.GetEnvironmentVariable("RIGHTSHIP_ALLOW_LOCAL_PREVIEW"), "true", StringComparison.OrdinalIgnoreCase);

    public async Task<bool> HasRoleAsync(HttpRequestData request, params string[] roles)
    {
        if (_metadata is null || string.IsNullOrWhiteSpace(_audience)) return false;
        if (!request.Headers.TryGetValues("Authorization", out var headers)) return false;
        var header = headers.FirstOrDefault();
        if (header is null || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return false;
        var token = header["Bearer ".Length..].Trim();
        try
        {
            var configuration = await _metadata.GetConfigurationAsync(CancellationToken.None);
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = configuration.SigningKeys,
                ValidateIssuer = true,
                ValidIssuer = configuration.Issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            }, out _);
            return principal.Claims.Where(c => c.Type is "roles" or "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                .Any(c => roles.Contains(c.Value, StringComparer.OrdinalIgnoreCase));
        }
        catch (SecurityTokenException) { return false; }
        catch (ArgumentException) { return false; }
    }
}
