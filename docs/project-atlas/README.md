# 7DaysSharp Project Atlas

`Api.7DaysSharp` is the shared C# wrapper for 7 Days to Die Web API and console
command integration.

## Owned Surfaces

- `PolyhydraGames.7DaysSharp/SdtdApiClient.cs`: typed Web API request client.
- `PolyhydraGames.7DaysSharp/SdtdBridge.cs`: legacy console command bridge.
- `PolyhydraGames.7DaysSharp/Models/`: response/config DTOs.
- `Tests/`: NUnit validation for deterministic client behavior plus legacy
  bridge smoke coverage.

## Boundaries

- This repo does not own live 7 Days server deployment.
- This repo does not own game-server secrets.
- This repo does not own stream/operator UI.
- Runtime deployment and live smokes belong in paired support/server repos.

## Validation

```sh
scripts/validate.sh
scripts/smoke-package.sh
devstudio validate --repo /mnt/data/lancer1977/code/Api.7DaysSharp
```

## Related Docs

- [Release Runbook](../release-runbook.md)
- [Modding Capability Profile](../features/modding-capability-profile.md)
- [Implementation Epics](../roadmaps/implementation-epics.md)
