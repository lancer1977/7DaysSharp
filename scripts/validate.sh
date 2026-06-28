#!/usr/bin/env sh
set -eu

ROOT="$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"
CONFIGURATION="${CONFIGURATION:-Release}"
SOLUTION="${SOLUTION:-RconSharp.slnx}"

cd "$ROOT"

dotnet restore "$SOLUTION"
dotnet build "$SOLUTION" --configuration "$CONFIGURATION" --no-restore
dotnet test "$SOLUTION" --configuration "$CONFIGURATION" --no-restore --no-build --verbosity normal
dotnet pack PolyhydraGames.7DaysSharp/PolyhydraGames.SdtdSharp.csproj \
  --configuration "$CONFIGURATION" \
  --no-restore \
  --no-build \
  --output artifacts/package-smoke
