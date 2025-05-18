#!/usr/bin/env sh
set -euo pipefail
IFS=' \t\n'

# -----------------------------------
# Bite Build Engine Helper Script
# -----------------------------------

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"
PROG="$(basename "$0")"
LOCAL_DOTNET="$SCRIPT_DIR/.dotnet"
INSTALL_SCRIPT_URL="https://dot.net/v1/dotnet-install.sh"

# Defaults
LOG_FILE="${LOG_FILE:-$SCRIPT_DIR/${PROG%.sh}.log}"
VERBOSE=0
DRY_RUN=0

# -----------------
# Utility Functions
# -----------------
echo_and_log() {
  echo "$*" | tee -a "$LOG_FILE"
}

log() {
  echo "[$(date +'%Y-%m-%dT%H:%M:%S%z')] $*" >> "$LOG_FILE"
  [ "$VERBOSE" -eq 1 ] && echo "$*"
}

debug_setup() {
  touch "$LOG_FILE"
  [ "$VERBOSE" -eq 1 ] && set -x
  trap 'err=$?; echo_and_log "[ERROR] line $LINENO exit $err"; exit $err' ERR
}

usage() {
  cat <<EOF
Usage: $PROG [options] <command> [additional dotnet args]

Options:
  -v, --verbose       Enable verbose tracing
  -n, --dry-run       Preview commands without executing
  --log-file <path>   Write logs to specified file (default: $LOG_FILE)

Commands:
  build               Build the solution (default)
  test                Run unit tests in the solution
  restore             Restore NuGet packages for the solution
  clean               Clean outputs for the solution
  bite [target]       Invoke bite target
  help                Show this message and exit

Example:
  $PROG build -c Release
EOF
  exit 0
}

parse_global_flags() {
  while [ $# -gt 0 ]; do
    case "$1" in
      -v|--verbose)
        VERBOSE=1; shift ;;
      -n|--dry-run)
        DRY_RUN=1; shift ;;
      --log-file)
        LOG_FILE="$2"; shift 2 ;;
      --)
        shift; break ;;
      -h|--help)
        usage ;;
      -*?)
        echo "Unknown option: $1" >&2; usage ;;
      *)
        break ;;
    esac
  done
}

run_cmd() {
  cmd_str="$*"
  log "CMD: $cmd_str"
  if [ "$DRY_RUN" -eq 1 ]; then
    echo "[DRY-RUN] $cmd_str"
  else
    eval "$cmd_str"
  fi
}

# -------------------
# SDK Installation
# -------------------
install_local_sdk() {
  version="$1"
  log "Installing SDK $version locally"
  mkdir -p "$LOCAL_DOTNET"
  curl -sSL "$INSTALL_SCRIPT_URL" -o "$LOCAL_DOTNET/dotnet-install.sh"
  run_cmd sh "$LOCAL_DOTNET/dotnet-install.sh" --version "$version" --install-dir "$LOCAL_DOTNET"
  DOTNET_CMD="$LOCAL_DOTNET/dotnet"
  export DOTNET_ROOT="$LOCAL_DOTNET"
  export PATH="$DOTNET_ROOT:$PATH"
}

get_required_sdk() {
  if [ -f global.json ]; then
    grep -m1 '"version"' global.json | sed -E 's/.*"([0-9]+\.[0-9]+\.[0-9]+)".*/\1/'
  else
    echo "latest"
  fi
}

ensure_dotnet() {
  required_sdk=$(get_required_sdk)
  if command -v dotnet >/dev/null 2>&1; then
    DOTNET_CMD="dotnet"
    log "Found system dotnet"
    if [ "$required_sdk" != "latest" ] && ! "$DOTNET_CMD" --list-sdks | awk '{print \$1}' | grep -q "^${required_sdk}"; then
      install_local_sdk "$required_sdk"
    fi
  else
    log "System dotnet not found, installing $required_sdk"
    install_local_sdk "$required_sdk"
  fi
}

# -------------------
# Solution Detection
# -------------------
detect_solution() {
  mapfile -t slns < <(find . -maxdepth 1 -type f -name '*.sln')
  case "${#slns[@]}" in
    1) SOLUTION="${slns[0]}" ;;
    0) echo_and_log "ERROR: no .sln file found"; exit 1 ;;
    *) echo_and_log "ERROR: multiple .sln files found: ${slns[*]}"; exit 1 ;;
  esac
  log "Using solution: $SOLUTION"
}

# -------------------
# Main Dispatch
# -------------------
main() {
  parse_global_flags "$@"
  debug_setup
  log "Started $PROG with args: $*"

  ensure_dotnet
  detect_solution

  if [ $# -eq 0 ]; then
    set -- build -- "$SOLUTION"
  fi

  command="$1"; shift

  case "$command" in
    restore|clean|build|test)
      run_cmd "$DOTNET_CMD" "$command" "$SOLUTION" "$@" ;;
    bite)
      if [ $# -gt 0 ] && case "$1" in -*) false;; /*) false;; *) true;; esac; then
        target="$1"; shift
      else
        target=help
      fi
      run_cmd "$DOTNET_CMD" msbuild -t:"$target" "bite.proj" "$@" ;;
    help|-h|--help)
      usage ;;
    *)
      echo "Unknown command: $command" >&2; usage ;;
  esac
}

main "$@"
