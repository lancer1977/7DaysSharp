# 7DaysSharp Implementation Epics

Hermes Kanban: pending promotion

This page mirrors the GitHub issues generated from the capability profile. Keep
execution in GitHub issues and use this file only as a repo-local index.

| Area | GitHub issue | Validation surface |
| --- | --- | --- |
| HTTP fixture coverage | `#7` HTTP fixture coverage for 7DaysSharp API client | `dotnet test RconSharp.slnx --configuration Release` |
| Diagnostics/health helper | `#8` Add read-only diagnostics helper for 7DaysSharp | fixture-backed unit tests plus read-only smoke guidance |
| Command approval boundary | `#9` Add approval boundary for 7DaysSharp mutating bridge commands | unit tests for deny/dry-run/approval-required paths |
| Log/update event normalization | `#10` Normalize 7DaysSharp log and Web UI update events | fixture-backed parser tests |
| Native-hook research | `#11` Research native-hook-only 7DaysSharp modding capabilities | docs-only research output with support-repo boundary |
