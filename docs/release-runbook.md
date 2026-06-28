# 7DaysSharp Release Runbook

`Api.7DaysSharp` is a shared NuGet/API package. It does not own a live runtime
deployment, game server process, Portainer stack, or public web route. Runtime
deployment stays in the paired 7 Days to Die support/server repositories.

## Artifact

- Artifact: `PolyhydraGames.SdtdSharp.<version>.nupkg`
- Producer: `.github/workflows/ci.yml` for build/test confidence, plus local
  package smoke with `scripts/smoke-package.sh`
- Destination: package publishing lane, not a live service rollout
- Rollback: consumers pin the previous package version and redeploy from their
  owning support/runtime repo

## Required Configuration

No repository secret is required to build or smoke the package.

Consumer/runtime code needs these values when it connects to a server:

- 7 Days to Die Web API host/IP
- 7 Days to Die Web API port
- `adminuser`
- `admintoken`
- optional HTTPS override

Keep real values in the consumer repo, secret store, or runtime environment.
Do not commit them here.

## Commands

```sh
scripts/validate.sh
scripts/smoke-package.sh
```

`scripts/validate.sh` restores, builds, tests, and packs the package into
`artifacts/package-smoke`.

`scripts/smoke-package.sh` is the deploy smoke gate for this package repo: it
proves the package artifact can be produced locally without publishing or
touching a live game server.

## Delegated Runtime Deployment

If a support repo needs the current package:

1. Consume the published package or local package artifact.
2. Run that support repo's own deploy command.
3. Run that support repo's own live smoke against the game server.

This repo should not run `deploy/game-server/deploy-game-stack.sh` directly for
production. That folder is retained as a copied template/reference only.
