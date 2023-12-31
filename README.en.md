﻿<h1 align="center">
  <img src="https://raw.githubusercontent.com/WorldBoxOpenMods/ModLoader/master/resources/logo.png" alt="logo" width="200">
  <br/>
  NeoModLoader
</h1>

<p align="center">
  <a href="https://github.com/WorldBoxOpenMods/ModLoader/blob/master/README.md"><img alt="zh" src="https://img.shields.io/badge/zh-简体中文-red.svg"></a>
  <a href="https://github.com/WorldBoxOpenMods/ModLoader/blob/master/README.en.md"><img alt="en" src="https://img.shields.io/badge/en-English-green.svg"></a>
</p>

Provide a new mod loader for [Worldbox](http://www.superworldbox.com/) after [NCMS](https://denq04.github.io/ncms/).

## Features of mod loading

1. Search mod.json and load mod under GAMEPATH/Mods
2. Simple dependency management for mods (some boundary conditions are not handled, circular dependency is not handled)
3. Cache of mod compilation results
4. Support for NCMS mods (nearly all)
5. Recognize BepInEx mods loaded (You need to install BepInEx manually)
6. Upload mods to and Order mods from Steam Workshop (still in experimental testing phase)
7. Give up compiling and loading mods under GAMEPATH/Mods when NCMS existed
8. Auto-update based on Steam Workshop (still in experimental testing phase)

## Recent plans

1. ~~Complete Steam Workshop mod upload authentication~~
2. ~~Load compiled mods under Mods folder directly according to mod.json~~
3. ~~Load mods in zip and other compressed packages~~
4. Complete simple mod tutorial

## How to install

1. Download NeoModLoader.dll (and NeoModLoader.pdb) and put it in GAMEPATH/worldbox_Data/StreamingAssets/mods folder
2. Delete NCMS_memload.dll (They can be installed together but not suggested)
3. Start the game with experimental mode enabled

## How to report bugs

[Submit issue](https://github.com/WorldBoxOpenMods/ModLoader/issues/new?assignees=&labels=bug&projects=&template=bug-report-en.yaml&title=%5BBug%5D%3A+)