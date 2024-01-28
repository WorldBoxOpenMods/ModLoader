<h1 align="center">
  <img src="https://raw.githubusercontent.com/WorldBoxOpenMods/ModLoader/master/resources/logo.png" alt="logo" width="200">
  <br/>
  NeoModLoader
</h1>

<p align="center">
  <a href="https://github.com/WorldBoxOpenMods/ModLoader/blob/master/README.cz.md"><img alt="zh" src="https://img.shields.io/badge/切换语言-简体中文-red.svg"></a>
  <a href="https://github.com/WorldBoxOpenMods/ModLoader/blob/master/README.md"><img alt="en" src="https://img.shields.io/badge/Change Language-English-green.svg"></a>
<br/>
    <small><small>图标由微软Image Creator于2023/10/13生成, 如有侵权联系更换</small></small>
</p>

为[Worldbox](http://www.superworldbox.com/)提供继[NCMS](https://denq04.github.io/ncms/)后的新模组加载器, 简称NML.
详细介绍见[Gitbook](https://worldboxopenmods.gitbook.io/mod-tutorial-zh/)

<h2 align="center"> 预览 </h2>


<h2 align="center">
    安装方法
</h2>

### 一键安装(仅限Windows)

链接: https://pan.baidu.com/s/17YB5QRHPsDdQrbMwh908uA?pwd=e9cn 提取码: e9cn 仅Windows可用

### 手动安装

1. 下载NeoModLoader.dll(和NeoModLoader.pdb)放入GAMEPATH/worldbox_Data/StreamingAssets/mods文件夹
2. 删除NCMS_memload.dll (它们可以同时安装, 但不推荐)
3. 在开启实验模式的情况下启动游戏

推荐在创意工坊订阅该加载器以便自动更新(自动更新只有当该加载器已安装才会生效)

<h2 align="center">其他相关</h2>
<h3 align="center">
<a href="https://github.com/WorldBoxOpenMods/ModLoader/issues/new?assignees=&labels=bug&projects=&template=bug-report-zh.yaml&title=%5BBug%5D%3A+">
反馈BUG</a>
</h3>
<h3 align="center">
<a href="https://worldboxopenmods.gitbook.io/mod-tutorial-zh/mo-zu-zhi-zuo-jiao-cheng/start">制作模组</a>
</h3>
<h3 align="center">
自行编译NML
</h3>

#### 简单流程

1. clone
2. 用Visual Studio/Rider打开NeoModLoader.csproj, 点击编译

#### 用命令行编译

1. clone
2. 下载[.NET Core SDK](https://dotnet.microsoft.com/download)
3. 到NeoModLoader.csproj所在目录
4. 执行`dotnet build NeoModLoader.csproj`

<h3 align="center">
    贡献代码
</h3>

对于小修补:

1. Fork
2. 修改代码
3. 提交PR
4. 等待审核

新特性, 重构等:

1. 提交issue
2. 讨论通过后再重复上述步骤

<h3 align="center">
    后续开发路线
</h3>

1. 自动布局窗口
2. 多网站的一键模组安装支持
3. 管理原版tab栏中的按钮
4. 支持Assembly-CSharp mod(动态修改游戏源码)