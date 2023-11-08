#!/bin/bash

case "$OSTYPE" in
  darwin*)
    export GMF=$HOME/Library/Application\ Support/Steam/steamapps/common/worldbox/worldbox_Data/StreamingAssets/Mods
    export BDF=$(pwd)/bin/Debug/net48
    ;; 
  linux*)
    export GMF=$HOME/Games/steam/worldbox/worldbox_Data/StreamingAssets/Mods
    export BDF=$(pwd)/bin/Debug/net48
    ;;
esac

if [ ! -e $GMF/NeoModLoader.dll ]; then
  ln -s $BDF/NeoModLoader.dll $GMF/NeoModLoader.dll
fi

if [ ! -e $GMF/NeoModLoader.pdb ]; then
  ln -s $BDF/NeoModLoader.pdb $GMF/NeoModLoader.pdb
fi

if [ ! -e $GMF/NeoModLoader.xml ]; then
  ln -s $BDF/NeoModLoader.xml $GMF/NeoModLoader.xml
fi

if [ -e $GMF/NML/mod_compile_records.json ]; then
  rm $GMF/NML/mod_compile_records.json
fi