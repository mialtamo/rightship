# Run RightShip without Entra sign-in

The Phase 1 UI is open for local development. You do **not** need an Entra app registration or tenant values to use the offline IP Group allocation preview.

1. Install .NET 8 SDK and Azure Functions Core Tools v4.
2. Copy `src/RightShip.Functions/local.settings.sample.json` to `src/RightShip.Functions/local.settings.json`.
3. Keep `AZURE_FUNCTIONS_ENVIRONMENT=Development` and `RIGHTSHIP_ALLOW_LOCAL_PREVIEW=true` **only on your development computer**.
4. Run `func start` from `src/RightShip.Functions` and `dotnet run` from `src/RightShip.Web`.
5. Open the Blazor URL printed by `dotnet run`. Go to Settings to set IPBAN naming and target resource group.

The preview API skips Entra roles **only when both flags are explicitly enabled**. Never put these flags in an Azure-hosted Function App or expose the local development server to untrusted networks.

Production API requests still require a validated Entra token with the appropriate app role. The UI has no production login flow yet. Production hosting must reinstate MSAL and server-side role authorization before rollout. The deployment endpoint is deliberately not implemented and returns HTTP 501. No Azure resources can be changed by this code.
