# NeoModLoader

[![zh](https://img.shields.io/badge/zh-简体中文-red.svg)](README.md)
[![en](https://img.shields.io/badge/en-English-green.svg)](README.en.md)

![Icon](resources/logo.png)

为[Worldbox](http://www.superworldbox.com/)提供继[NCMS](https://denq04.github.io/ncms/)后的新模组加载器

## 目前支持的模组加载直接相关功能

1. 在GAMEPATH/Mods搜索mod.json并加载模组
2. 模组的简单依赖管理(部分边界情况尚未处理, 环依赖未处理)
3. 模组编译结果缓存
4. 支持NCMS的模组(几乎所有)
5. 支持识别BepInEx模组(仍需手动安装BepInEx)
6. 上传, 订阅Steam创意工坊模组(仍在实验测试阶段)
7. NCMS存在时放弃编译加载游戏路径下Mods中的模组
8. 基于创意工坊的加载器自动更新(仍在实验测试阶段)

## 近期计划

1. ~~较为完善的上传验证机制~~
2. ~~根据mod.json直接加载Mods文件夹下的已编译模组~~
3. ~~加载zip等压缩包中的模组~~
4. 完成简单的[模组教程](https://github.com/WorldBoxOpenMods/ModLoader/wiki/Home(简体中文))

## 安装方法

1. 下载NeoModLoader.dll(和NeoModLoader.pdb)放入GAMEPATH/worldbox_Data/StreamingAssets/mods文件夹
2. 删除NCMS_memload.dll (它们可以同时安装, 但不推荐)
3. 在开启实验模式的情况下启动游戏
