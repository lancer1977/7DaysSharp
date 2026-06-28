#!/usr/bin/env sh
set -eu

ROOT="$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"
CONFIGURATION="${CONFIGURATION:-Release}"
OUTPUT_DIR="${OUTPUT_DIR:-$ROOT/artifacts/package-smoke}"

cd "$ROOT"
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

dotnet pack PolyhydraGames.7DaysSharp/PolyhydraGames.SdtdSharp.csproj \
  --configuration "$CONFIGURATION" \
  --output "$OUTPUT_DIR"

set -- "$OUTPUT_DIR"/*.nupkg
[ -f "$1" ] || {
  echo "No NuGet package was produced in $OUTPUT_DIR" >&2
  exit 1
}

echo "Package smoke produced: $1"
