# Native-Hook Capability Research

Hermes Kanban: pending promotion

Issue: `lancer1977/7DaysSharp#11`

This research maps capabilities that cannot be safely owned by the
`Api.7DaysSharp` Web API wrapper alone. It uses current public 7 Days to Die
modding references and keeps implementation ownership out of this package unless
the work is a pure contract/helper surface.

## Source Notes

- Official/community modding resources list XML/XPath modding and Harmony docs
  as core 7 Days to Die modding paths:
  <https://7daystodie.wiki.gg/wiki/Modding_Resources>
- The 7D2D Harmony kata describes C# code mods as DLLs with an `IModApi` entry
  point, Harmony patches, and references to game assemblies from the managed
  game folder:
  <https://wiki.7d2d.net/mods/modding-kata/07-code-mods-harmony/>
- DMT Harmony docs describe `Harmony/` scripts compiled into standalone
  libraries and loaded from the Mods folder:
  <https://7d2dmods.github.io/HarmonyDocs/FileandFolderStructureSupport.html>
- Server mod guides distinguish XML modlets, Harmony mods, and overhaul mods as
  separate deployment shapes:
  <https://xgamingserver.com/docs/7-days-to-die/mod-setup>
- The existing game Web API and console-command bridge remain useful for
  sidecar/read-only work, but they cannot prove native gameplay hooks or
  low-level event interception.

## Ownership Rules

- `Api.7DaysSharp` may own DTOs, policy models, event normalization, and helper
  abstractions.
- Paired support/runtime repos own live server installation, Mods folder
  deployment, config/secrets, EAC policy, smoke servers, and rollback.
- Native DLL/Harmony implementation belongs in a dedicated mod/runtime repo, not
  in this API package, unless the output is only a shared contract assembly.
- AI/stream callers must go through approval and audit layers before mutating a
  server.

## Capability Map

| Capability | Current package path | Native-hook disposition | Runtime owner | Validation before production |
| --- | --- | --- | --- | --- |
| Identity | Web API DTOs and `SdtdApiClient` | Web API sufficient for current needs | this package plus support consumers | fixture tests plus live read-only smoke |
| Health/readiness | `SdtdDiagnosticsProbe` | Web API sufficient | this package plus support consumers | fixture tests plus live read-only smoke |
| Snapshot | Web API stats, server info, location, inventory | Web API sufficient for polling snapshots | this package plus support consumers | fixture tests plus live read-only smoke |
| Activity events | `SdtdActivityEventNormalizer` over logs/Web UI updates | Native hook only if exact event timing or richer payloads are required | support/runtime or dedicated mod repo | fixture tests, dedicated test server, event replay evidence |
| Chat/communication | console command bridge plus approval policy | Web API/console sufficient for basic server say; native hook only for richer chat interception | support/runtime repo | approval audit tests plus live smoke |
| Player effects | console commands for buff/debuff/kick/ban | Native hook required for custom effects beyond exposed commands | dedicated mod/runtime repo | test server, explicit approvals, rollback |
| Movement | console teleport | Native hook required for custom movement constraints/triggers | support/runtime or mod repo | test server with operator audit |
| Entity/event spawns | console spawn commands | Native hook required for custom spawn lifecycle or event injection | mod/runtime repo | test server, spawn cleanup plan, rollback |
| World/environment | console time/weather/shutdown | Native hook required for custom weather/world mechanics; shutdown stays denied by default | support/runtime or mod repo | operator-only test server validation |
| Inventory/progression | inventory reads plus `givequest` command | Native hook required for direct inventory mutation or custom progression hooks | mod/runtime repo | save backup, test world, rollback |
| Presentation | none | XUi/XML modlets or external overlays, not this package | support/overlay repo | screenshot/browser or in-game proof |
| Custom events | normalized log/Web UI events | Native hook required for first-class custom game events | mod/runtime repo plus this package for contracts | event fixture replay and live test server proof |
| AI analysis | read-only snapshots/events | no native hook needed for analysis; mutation remains approval-required | support/AI consumer | read-only fixture and live smoke |
| Stream/operator controls | approval policy and normalized events | native hook only for in-game control surfaces | support/runtime or overlay repo | approval tests, audit log, live smoke |

## Recommended Follow-Ups

- Keep native hook implementation out of `Api.7DaysSharp` until there is a
  dedicated mod repo with a test server and rollback plan.
- If a native mod is needed, start with a minimal `IModApi`/Harmony proof repo
  that emits read-only custom events, then consume those events through this
  package as DTOs.
- Treat XML/XPath modlets as content/config deployment, not package code.
- Treat Harmony/C# patches as high-risk runtime code that needs version pinning
  against game assemblies and explicit server compatibility notes.

## Validation Used For This Research

- Reviewed the local capability profile in
  `docs/features/modding-capability-profile.md`.
- Checked public modding references for XML/XPath, Harmony/C# mods, DMT folder
  structure, and server mod deployment shapes.
- No live server or native mod was deployed; this issue is a research and
  ownership-boundary slice.
