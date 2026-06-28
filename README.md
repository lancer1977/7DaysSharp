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
