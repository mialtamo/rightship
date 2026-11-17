# Engineering roadmap and safety gates

## Phase 1: implementation in this PR

Validated and canonicalized address uploads, deterministic group allocation preview, Entra-authenticated Function API, Blazor front end, tests. No Azure writes.

## Phase 2: read-only Azure inventory

Use DefaultAzureCredential and ARM with managed identity. Discover policy, rule collection group and managed IP Groups from Azure, never from browser-supplied data. Scope by resource IDs and explicit owner tags. Include policy and capacity validation.

## Phase 3: deployment

Durable orchestration, persistent jobs and audit events. Require separate approval. Create/update IP Groups with Azure long-running operation polling and final re-read. Serialize per-policy writes, honor ETag and throttling, preserve unrelated rules and rule collection priorities. Detect 20,000 network-rule combinations and applicable policy/group limits.

## Phase 4: verification

Verify IP Group entries, provisioning states, rule references and deny action with read-after-write comparisons. Capture partial failure and recovery. Traffic-path testing needed for actual enforcement assertions.

## Known limitations in Phase 1

- CSV inputs are interpreted as comma-separated IP values, not arbitrary multi-column spreadsheets.
- Exact duplicates are removed, but overlapping CIDRs and IPs contained in CIDRs are not yet merged or flagged.
- Existing groups in preview are explicitly hypothetical input, not Azure-sourced.
- Web UI does not yet list actual Azure Firewall resources, persist uploads, audit, remove IPs, or execute deployments.
- Hosting and cross-origin policy need configuration for intended deployment; do not expose functions publicly without authentication.
