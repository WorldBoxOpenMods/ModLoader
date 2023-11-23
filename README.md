<h1 align="center">
  <img src="https://raw.githubusercontent.com/WorldBoxOpenMods/ModLoader/master/resources/logo.png" alt="logo" width="200">
  <br/>
  NeoModLoader
</h1>

<p align="center">
  <a href="https://github.com/WorldBoxOpenMods/ModLoader/blob/master/README.md"><img alt="zh" src="https://img.shields.io/badge/zh-简体中文-red.svg"></a>
  <a href="https://github.com/WorldBoxOpenMods/ModLoader/blob/master/README.en.md"><img alt="en" src="https://img.shields.io/badge/en-English-green.svg"></a>
</p>

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
4. 从创意工坊订阅该加载器以便自动更新(自动更新只有当该加载器已安装才会生效)

## 如何反馈BUG
[提交issue](https://github.com/WorldBoxOpenMods/ModLoader/issues/new?assignees=&labels=bug&projects=&template=bug-report-zh.yaml&title=%5BBug%5D%3A+)

## 如何编译

简单流程:

1. clone
2. 用Visual Studio/Rider打开NeoModLoader.csproj, 点击编译

用命令行编译:
1. clone
2. 下载[.NET Core SDK](https://dotnet.microsoft.com/download)
3. 到NeoModLoader.csproj所在目录
4. 执行`dotnet build NeoModLoader.csproj`

## 如何贡献代码

对于小修补:

1. Fork
2. 修改代码
3. 提交PR
4. 等待审核

新特性, 重构等:

1. 提交issue
2. 讨论通过后再重复上述步骤