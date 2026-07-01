#!/bin/bash

set -euo pipefail

CONFIGURATION="${1:-Debug}"
PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
BDF="$PROJECT_DIR/bin/$CONFIGURATION/net48"

case "$OSTYPE" in
  darwin*)
    GMF="$HOME/Library/Application Support/Steam/steamapps/common/worldbox/worldbox_Data/StreamingAssets/Mods"
    ;;
  linux*)
    GMF="$HOME/.local/share/Steam/steamapps/common/worldbox/worldbox_Data/StreamingAssets/mods"
    ;;
  *)
    echo "Unsupported OSTYPE: $OSTYPE" >&2
    exit 1
    ;;
esac

mkdir -p "$GMF"
cp -f "$BDF/NeoModLoader.dll" "$GMF/NeoModLoader.dll"
cp -f "$BDF/NeoModLoader.pdb" "$GMF/NeoModLoader.pdb"
cp -f "$BDF/NeoModLoader.xml" "$GMF/NeoModLoader.xml"

#if [ -e "$GMF"/NML/mod_compile_records.json ]; then
#  rm "$GMF"/NML/mod_compile_records.json
#fi
