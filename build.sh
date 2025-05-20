#!/usr/bin/env bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

CACHE_DIR="$SCRIPT_DIR/__pycache__"
mkdir -p "$CACHE_DIR"

# Tell Python to use that as the pycache prefix
export PYTHONPYCACHEPREFIX="$CACHE_DIR"

exec python3 "$SCRIPT_DIR/build.py" "$@"