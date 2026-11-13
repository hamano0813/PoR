###  苍炎的轨迹 动态修改器

[![.NET](https://img.shields.io/badge/.NET-10.0-darkcyan?logo=dotnet&style=flat&labelColor=013243)](https://dotnet.microsoft.com/)
[![AvaloniaUI](https://img.shields.io/badge/AvaloniaUI-12.1.6-darkcyan?logo=avaloniaui&style=flat&labelColor=013243)](https://avaloniaui.net/)
[![AvaloniaFluentUI](https://img.shields.io/badge/AvaloniaFluentUI-2.0.6-darkcyan?logo=avaloniaui&style=flat&labelColor=013243)](https://github.com/IzumiPL/Avalonia-Fluent-UI)
[![Dolphin](https://img.shields.io/badge/Dolphin-≥5.0-darkcyan?logo=dolphin&style=flat&labelColor=013243)](https://cn.dolphin-emu.org/)

#### 项目说明

- 修改可能会导致游戏失去乐趣
- 仅支持 Dolphin 5.0 以上版本模拟器，包括稳定版、beta 版及 dev 版等
- 仅支持 64 位 Windows
- 同时支持日文原版及汉化版
- 先打开模拟器开始模拟游戏，再打开修改器，读档进入游戏后点击刷新获取数据
- 修改前和过程中多存档，以免出现不可挽回的损失
- 内置日文和中文两套游戏名词翻译，UI 固定为中文
- 修改完成人物或职业后建议先存档读档一次再进行操作，以防出错
- 将人物改为上级职业时，记得在技能的隐藏特性里勾选“上级职业”
- 可更改人物归属阵营以操作敌方，过关后可能加入，但容易影响后续关卡的强制出击
- 可锁定人物行动状态，重复点击勾选可取消锁定

#### 可修改项目

- [x] 人物
- [x] 能力
- [x] 特技
- [x] 道具
- [x] 支援
- [x] 金钱

#### BUILD本地EXE程序方法

1. 安装 .NET 10 SDK，[.NET 官网下载页面](https://dotnet.microsoft.com/download)
2. 克隆项目：
   ```bash
   git clone https://github.com/hamano0813/PoR.git
   ```
3. 在仓库根目录运行编译脚本：
   ```bat
   build.cmd
   ```
   默认生成自包含单文件版本。需要较小体积时运行：
   ```bat
   build.cmd framework
   ```
   也可以手动执行：
   ```bash
   dotnet publish src/PoR.App/PoR.App.csproj -c Release -p:PublishProfile=win-x64-standalone
   ```
4. 自包含版本在 `publish/win-x64` 目录中；框架依赖版在 `publish/win-x64-framework-dependent` 中
5. 框架依赖版需要安装 .NET 10 Desktop Runtime

#### 运行

```bash
dotnet run --project src/PoR.App/PoR.App.csproj
```

#### 版本

1.9.9

#### License

MIT
