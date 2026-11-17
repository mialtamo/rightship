# RightShip | Azure Firewall IP Blocklist Manager

RightShip is an Entra-protected Azure Functions + Blazor application for planning and managing Azure Firewall IP blocklists.

## Current implementation (Phase 1)

- .NET 8 isolated-worker Azure Functions API with Entra JWT validation.
- Blazor WebAssembly dashboard with MSAL sign-in, upload/preview, and capacity planning.
- IPv4/IPv6 literal and CIDR parsing, canonicalization, invalid-row reporting, and duplicate removal.
- Idempotent allocation preview that fills existing managed groups before opening new 5,000-entry groups.
- User role enforcement for read vs upload actions.
- Unit tests for parsing, duplicate handling, and group capacity.
- Azure writes intentionally **not yet implemented**. The API returns previews only. Never interpret a preview as a deployed firewall change.

## Repository layout

`src/RightShip.Core` contains independent, testable validation and allocation logic.
`src/RightShip.Functions` contains the authenticated HTTP API.
`src/RightShip.Web` contains the Blazor dashboard.
`tests/RightShip.Core.Tests` contains xUnit tests.

## Local setup

Install .NET 8 SDK, Azure Functions Core Tools v4, PowerShell 7, and Az PowerShell modules.

Set the Function App settings shown in `src/RightShip.Functions/local.settings.sample.json` (copy to `local.settings.json` and fill in values). Register an Entra API app with an exposed application ID URI and app roles `RightShip.Reader`, `RightShip.Operator`, `RightShip.Administrator`. Configure a separate single-page application registration with redirect URIs for your web origin and delegated access to the API scope.

Set the Blazor values in `src/RightShip.Web/wwwroot/appsettings.json`: Authority (Commercial: `https://login.microsoftonline.com/{tenantId}`; Government: `https://login.microsoftonline.us/{tenantId}`), SPA ClientId, ApiBaseUrl, and API scope.

Run:
```powershell
dotnet test tests/RightShip.Core.Tests/RightShip.Core.Tests.csproj
Set-Location src/RightShip.Functions
func start
```
In a second PowerShell terminal:
```powershell
Set-Location src/RightShip.Web
dotnet run
```

Only browser users with configured Entra app roles may use the API. The API checks JWT signatures and issuer keys using the tenant's OIDC metadata. Do not set secrets in source control.

## Upcoming phases

1. Read-only Azure Firewall Policy and IP Group discovery through managed identity.
2. Durable Functions deployment jobs with table/blob persistence and audit trails.
3. Safe, approved IP Group creation and updates with read-after-write verification.
4. Firewall Rule Collection Group integration with ETag-safe updates and policy/limit checks.
5. Removal, reconciliation, expiry, monitoring and production hardening.

**Design constraint:** Azure Firewall Policy deny rules require careful source/destination selection. DNAT traffic may not be blocked by the same network-rule path. Never assume a management-plane update proves packet-level enforcement.
