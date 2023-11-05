# NeoModLoader

[![zh](https://img.shields.io/badge/zh-简体中文-red.svg)](README.md)
[![en](https://img.shields.io/badge/en-English-green.svg)](README.en.md)

![Icon](resources/logo.png)

为[Worldbox](http://www.superworldbox.com/)提供继[NCMS](https://denq04.github.io/ncms/)后的新模组加载器

## 目前支持的模组加载直接相关功能

1. 在GAMEPATH/Mods搜索mod.json并加载模组
2. 模组的简单依赖管理(部分边界情况尚未处理, 环依赖未处理)
3. 模组编译结果缓存
4. 支持NCMS的模组(部分)
5. 上传模组到Steam创意工坊(仍在实验测试阶段)
6. 浏览订阅的模组(仍在实验测试阶段)

## 近期计划
1. 完成Steam创意工坊模组管理
2. 完成NCMS模组的兼容层
3. 添加BepInEx支持
4. 根据mod.json直接加载Mods文件夹下的已编译模组
5. 加载zip等压缩包中的模组

## 安装方法
1. 下载NeoModLoader.dll(和NeoModLoader.pdb)放入GAMEPATH/worldbox_Data/StreamingAssets/mods文件夹
2. 在开启实验模式的情况下启动游戏