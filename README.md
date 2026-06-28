# 7DaysSharp

Shared API package and legacy wrapper for 7 Days to Die integrations.

First read: [docs/README.md](./docs/README.md).

A simple C# wrapper based on work by CatalysmsServerManager. Keep active runtime and support-home ownership in the paired game-server repos.
## Tags

- dotnet
- api-7-days-sharp
- api
- server

## Documentation

- [Docs Index](./docs/README.md)
- [Feature Index](./docs/features/README.md)
- [Roadmap Index](./docs/roadmaps/README.md)
- [Release Runbook](./docs/release-runbook.md)

## Validation

```sh
scripts/validate.sh
scripts/smoke-package.sh
```

`scripts/validate.sh` restores, builds, tests, and creates a local package smoke
artifact. `scripts/smoke-package.sh` is the release/deploy smoke gate for this
package repo.

## Read-Only Diagnostics

```csharp
var config = new SdtdServerConfig("127.0.0.1", 8082, false, "admin", "token");
var api = new SdtdApiClient(config, new HttpClient());
var report = await new SdtdDiagnosticsProbe(api).CheckReadinessAsync();
```

The diagnostics probe calls only read-only endpoints: server info, stats, and
allowed commands. It reports auth, HTTP, parse, timeout, and connection failures
without issuing console commands.

## Command Approval Boundary

```csharp
var decision = SdtdCommandApprovalPolicy.Evaluate(new SdtdCommandApprovalRequest(
    "spawnentity 171 zombieBoe",
    Approved: false,
    DryRun: true,
    Actor: "stream"));
```

`SdtdBridge` wraps mutating console commands. Chat, AI, stream, and unattended
operator surfaces should evaluate commands with `SdtdCommandApprovalPolicy`
before calling `ExecuteConsoleCommandAsync` or bridge helpers. Unknown and
lifecycle commands are denied by default; moderation, spawn, world, movement,
progression, player-effect, and communication commands require approval unless
the caller is only doing a dry run.

## Activity Events

```csharp
var events = SdtdActivityEventNormalizer.FromLog(logResponse);
var update = SdtdActivityEventNormalizer.FromWebUiUpdates(updateResponse);
```

The normalizer maps player join/leave, chat, command output, and Web UI update
snapshots into stable event kinds. Unsupported log rows are preserved as
`unknown` events so consumers can inspect them later.
