# RightShip settings

The Phase 1 Settings page is at `/settings`.

Fields include Azure cloud, subscription ID, IP Group resource group and location, Firewall Policy resource ID, rule collection group, deny rule collection and deny network rule. Allocation settings include group prefix (default `IPBAN`), starting sequence (default `0`), per-group entry capacity (default 5,000), and an approval preference.

Names use literal `prefix + sequential integer`: `IPBAN0`, `IPBAN1`, `IPBAN2`. The planner skips names already in its **provided preview inventory**.

Settings are saved to the current browser's localStorage solely for Phase 1 convenience. This is not a secure centralized configuration store, and does not grant access to Azure. Do not place secrets in these fields.

**Before production:** move settings to an administrator-restricted persistent backend, validate Azure ARM IDs and permissions, load actual IP Groups from Azure, enforce resource group and ownership boundaries, perform policy read/verify, and add audited change approval with durable orchestration.
