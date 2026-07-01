# Version Coverage Map

This page maps `Api.7DaysSharp` to the shared game-server V-layer ladder as a
shared API package. It does not make this repo a support home or runtime lane.

```yaml
established_versions: [V2, V3]
modeled_versions: [V4]
blocked_versions: [V5]
not_applicable_versions: [V0, V1]
```

## Coverage Summary

| Layer | Tag | Current coverage | Evidence | Next proof |
| --- | --- | --- | --- | --- |
| `V0` infra baseline | `not-applicable` | Runtime boot belongs in support/runtime repos | `7Days-Support`, `Docker-7DaysToDie` | Keep this package out of server ownership |
| `V1` support-home boundary | `not-applicable` | Support-home ownership belongs in `7Days-Support` | Workspace boundary docs | Keep this repo focused on API contracts/helpers |
| `V2` read-only support proof | `established` | Read-only diagnostics, snapshot DTOs, and activity normalization are implemented and tested | `SdtdDiagnosticsProbe`, `SdtdActivityEventNormalizer`, API DTO tests | Support repos should consume these helpers in live read-only smoke |
| `V3` control truth | `established` | Command classification, approval-required defaults, dry-run behavior, and deny-by-default unknown/lifecycle behavior are tested | `SdtdCommandApprovalPolicy` and tests | Support repos should wrap this with operator identity and audit logs |
| `V4` public/operator projection | `modeled` | Package exposes data needed for projections, but does not own redaction/UI policy | DTOs, activity events, capability profile | Define public projection contracts in support/overlay repos |
| `V5` approval-gated gameplay proof | `blocked` | Package can classify and gate commands, but cannot prove live gameplay safety | Native-hook research and approval policy | Requires support/runtime repo, dev server, dry-run, approval, audit, and rollback |

## Established V2 Evidence

The package has read-only coverage for diagnostics and observation:

- server info, stats, and allowed commands checks
- typed Web API DTOs for player, inventory, location, log, and Web UI update
  readback
- activity event normalization that preserves unknown rows for inspection
- failure classification for auth, connection, timeout, parse, and HTTP errors

## Established V3 Evidence

The package has tested control-truth classification:

- unknown commands are denied
- lifecycle shutdown is denied by default
- mutating command families are approval-required
- dry-run never executes a mutating command
- approved communication commands can be allowed by a caller that owns audit and
  operator context

## V4 And V5 Boundary

`Api.7DaysSharp` can help feed V4/V5, but cannot establish them alone.

- V4 public/operator projection belongs in `7Days-Support`, `cc-sidecar`, or an
  operator UI repo that owns redaction and display policy.
- V5 approval-gated gameplay proof belongs in a support/runtime/mod repo with a
  real dev server, rollback plan, audit output, and production mutation blocked
  by default.

## Validation

Current validation anchors:

```bash
scripts/validate.sh
scripts/smoke-package.sh
```
