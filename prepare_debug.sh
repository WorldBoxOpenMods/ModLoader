#!/bin/bash

case "$OSTYPE" in
  darwin*)
    export GMF=$HOME/Library/Application\ Support/Steam/steamapps/common/worldbox/worldbox_Data/StreamingAssets/Mods
    export BDF=$(pwd)/bin/Debug/net48
    ;; 
  linux*)
    export GMF=$HOME/.local/share/Steam/steamapps/common/worldbox/worldbox_Data/StreamingAssets/mods/
    export BDF=$(pwd)/bin/Debug/net48
    ;;
esac

cp -f "$BDF"/NeoModLoader.dll "$GMF"/NeoModLoader.dll
cp -f "$BDF"/NeoModLoader.pdb "$GMF"/NeoModLoader.pdb
cp -f "$BDF"/NeoModLoader.xml "$GMF"/NeoModLoader.xml

#if [ -e "$GMF"/NML/mod_compile_records.json ]; then
#  rm "$GMF"/NML/mod_compile_records.json
#fi