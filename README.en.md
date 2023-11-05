# NeoModLoader

[![zh](https://img.shields.io/badge/zh-简体中文-red.svg)](README.md)
[![en](https://img.shields.io/badge/en-English-green.svg)](README.en.md)

![Icon](resources/logo.png)

Provide a new mod loader for [Worldbox](http://www.superworldbox.com/) after [NCMS](https://denq04.github.io/ncms/).

## Features of mod loading

1. Search mod.json and load mod under GAMEPATH/Mods
2. Simple dependency management for mods (some boundary conditions are not handled, circular dependency is not handled)
3. Cache of mod compilation results
4. Support for NCMS mods (partially)
5. Upload mods to Steam Workshop (still in experimental testing phase)
6. Browse subscribed mods (still in experimental testing phase)

## Recent plans
1. Complete Steam Workshop mod management
2. Complete compatibility layer for NCMS mods
3. Add BepInEx support
4. Load compiled mods under Mods folder directly according to mod.json
5. Load mods in zip and other compressed packages