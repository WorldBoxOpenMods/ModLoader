#!/bin/bash

export GAMEMODFOLDER=/home/inmny/Games/steam/worldbox/worldbox_Data/StreamingAssets/Mods

export BUILDFOLDER=/home/inmny/source/repo/NeoModLoader/NeoModLoader/bin/Debug/net48

if [ -e $GAMEMODFOLDER/NeoModLoader.dll ]; then
  echo "Skip"
else
  ln -s $BUILDFOLDER/NeoModLoader.dll $GAMEMODFOLDER/NeoModLoader.dll
fi

if [ -e $GAMEMODFOLDER/NeoModLoader.pdb ]; then
  echo "Skip"
else
  ln -s $BUILDFOLDER/NeoModLoader.pdb $GAMEMODFOLDER/NeoModLoader.pdb
fi

if [ -e $GAMEMODFOLDER/NeoModLoader.xml ]; then
  echo "Skip"
else
  ln -s $BUILDFOLDER/NeoModLoader.xml $GAMEMODFOLDER/NeoModLoader.xml
fi

rm $GAMEMODFOLDER/NML/mod_compile_records.json