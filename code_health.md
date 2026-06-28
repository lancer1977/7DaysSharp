# 7DaysSharp Code Health

Last reviewed: 2026-06-28

## Status

- Repo type: shared .NET API package.
- Runtime ownership: delegated to paired 7 Days to Die support/server repos.
- Validation command: `scripts/validate.sh`.
- Package smoke: `scripts/smoke-package.sh`.
- CI: `.github/workflows/ci.yml` runs the repo validation command and uploads
  the local package smoke artifact.

## Current Health Notes

- Deterministic HTTP fixture tests cover auth headers, endpoint shape, and
  encoded console command queries.
- `SdtdDiagnosticsProbe` provides a read-only readiness helper around server
  info, stats, and allowed commands.
- `SdtdCommandApprovalPolicy` classifies raw console commands and provides a
  deny/dry-run/approval-required/approved decision before callers execute
  mutating bridge commands.
- The legacy `ListPlayers` integration-style test is marked explicit because it
  requires a reachable LAN 7 Days to Die Web API. Normal validation uses
  deterministic HTTP fixtures.
- `dotnet list RconSharp.slnx package --outdated` reports no package updates
  from the configured sources.
- DevStudio validation finds required repo-shape items; recommended project
  atlas docs live in `docs/project-atlas/`.

## Follow-Up Themes

- Replace the legacy live-server-tolerant test with explicit fixture and
  opt-in live smoke coverage.
- Add consumer-side audit logging around approved mutating command execution.
