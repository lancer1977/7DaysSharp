# Script Template: .NET Coverage

This folder captures a reusable shell-script baseline for .NET coverage workflows.

Copy the script into a product repo when you want a simple local command that:

- runs the target test project or solution
- collects `XPlat Code Coverage`
- writes artifacts to a predictable `TestResults/` location
- optionally generates a human-readable HTML report when ReportGenerator is available

## Expected inputs

- `COVERAGE_TARGET` or the first positional argument: the solution or test project path
- `CONFIGURATION` (optional): defaults to `Release`
- `COVERAGE_RESULTS_DIR` (optional): defaults to `./TestResults/coverage`

## Notes

- The script is intentionally lightweight so repos can copy it and adapt the target path.
- If ReportGenerator is installed, the script will emit an HTML summary under the report directory.
- Repos with more advanced needs can wrap this command in CI or a repo-local task file.

## Repo Commands

```sh
scripts/validate.sh
scripts/smoke-package.sh
```

- `validate.sh` restores, builds, tests, and packs the package smoke artifact.
- `smoke-package.sh` rebuilds the package artifact and fails if no `.nupkg` is
  produced.
