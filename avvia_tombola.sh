#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

if ! command -v dotnet >/dev/null 2>&1; then
  echo ".NET SDK non trovato nel PATH."
  echo "Installa .NET e riprova."
  exit 1
fi

dotnet run --project "$SCRIPT_DIR/Tombola.csproj"
