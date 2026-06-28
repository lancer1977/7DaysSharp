# 7DaysSharp Modding Capability Profile

Hermes Kanban: pending promotion

This profile maps `Api.7DaysSharp` to the shared game-server modding playbook in
`../Api.GameServerInterop`. It is an API package inventory, not a production
runtime plan.

## Ownership Boundary

- This repo owns the C# wrapper contract for the 7 Days to Die Web API and
  console command bridge.
- Runtime servers, live secrets, game saves, Docker stacks, and operator action
  approval belong in the paired support/runtime repos.
- Any mutating command remains approval-required until a support repo adds
  audit logging, dry-run behavior, operator identity, and rollback guidance.

## Capability Families

| Family | Status | Current surface | Notes |
| --- | --- | --- | --- |
| Identity | observed | `GetPlayerListAsync`, `GetOnlinePlayersAsync`, `OnlinePlayerResponse`, `PlayerListResponse` | Player identity is readable through Web API models. |
| Health | read-only incomplete | `GetServerInfoAsync`, `GetStatsAsync` | Package can query server info/stats; no dedicated health helper exists yet. |
| Snapshot | observed | `GetStatsAsync`, `GetServerInfoAsync`, `GetWebUIUpdatesAsync`, `GetPlayersLocationAsync` | Read-only snapshot data is represented by typed DTOs. |
| Activity events | observed | `GetLogAsync`, `GetWebUIUpdatesAsync`, `LogLine` | Log/update polling exists; event normalization is not implemented. |
| Communication | approval-required | `SdtdBridge.Say` via `executeconsolecommand` | Chat output mutates the live server and needs operator approval/audit in consumers. |
| Player effects | approval-required | `KickPlayer`, `BanPlayer`, `UnbanPlayer`, `Buff`, `Debuff` | Moderation and buff/debuff commands are mutating and must stay gated. |
| Movement | approval-required | `Teleport` | Live teleport is powerful and needs explicit approval plus audit trail. |
| Entity/event spawns | approval-required | `SpawnEntity`, `SpawnScouts`, `SpawnHorde` | Spawn commands should not be exposed directly to AI or stream chat. |
| World/environment | approval-required | `SetTime`, `Weather`, `Shutdown` | World and lifecycle commands require operator-only controls. |
| Inventory/progression | read-only incomplete | `GetPlayerInventoryAsync`, `GetPlayerInventoriesAsync`, `GiveQuest` | Inventory read models exist; quest granting is mutating and approval-required. |
| Presentation | deferred | none in this package | Presentation belongs in support overlays, dashboards, or stream surfaces. |
| Custom events | deferred | none in this package | Would require consumer-side event normalization. |
| AI analysis | dry-run only | read-only snapshot/log methods | AI can summarize read-only snapshots; it must not issue mutating commands from this package directly. |
| Stream/operator surfaces | read-only incomplete | bridge methods plus typed API client | Operator UI and stream triggers belong in paired support repos with approvals. |

## Concrete Hooks

Read-only Web API endpoints wrapped today:

- `/api/getstats`
- `/api/getplayersonline`
- `/api/getallowedcommands`
- `/api/getanimalslocation`
- `/api/gethostilelocation`
- `/api/getlandclaims`
- `/api/getplayerinventory`
- `/api/getplayerinventories`
- `/api/getplayerlist`
- `/api/getplayerslocation`
- `/api/getserverinfo`
- `/api/getwebuiupdates`
- `/api/getlog`

Mutating command bridge:

- `/api/executeconsolecommand`
- wrapped commands: `listplayers`, `kick`, `ban add`, `ban remove`, `say`,
  `gettime`, `settime`, `weather`, `teleport`, `spawnentity`, `givequest`,
  `shutdown`, `buff`, `debuff`, `spawnscouts`, `spawnhorde`

## Unsupported Or Deferred Ideas

- Native game hooks, Harmony patches, or DLL mods are not implemented here.
- Direct save mutation is not implemented here.
- Stream-chat controlled player effects are not production-enabled.
- AI-command execution is not production-enabled.

## Follow-Up Implementation Epics

Generated from this profile by `#2`:

- `#7` Deterministic HTTP fixture tests for API client requests and DTO parsing.
- `#8` Diagnostics/health helper for read-only server readiness.
- `#9` Command approval/audit boundary before exposing mutating bridge methods.
- `#10` Event normalization for log/Web UI update streams.
- `#11` Native-hook research for capabilities the Web API cannot support safely.
