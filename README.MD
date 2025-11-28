###  苍炎的轨迹 动态修改器

[![python](https://img.shields.io/badge/Python-3.13.9-darkcyan?logo=python&style=flat&labelColor=013243)](https://www.python.org/)
[![PySide6](https://img.shields.io/badge/PySide6-6.10.1-darkcyan?logo=qt&style=flat&labelColor=013243)](https://doc.qt.io/qtforpython/)
[![Dolphin](https://img.shields.io/badge/Dolphin-5.0-darkcyan?logo=nintendogamecube&style=flat&labelColor=013243)](https://cn.dolphin-emu.org/)
[![Dolphin-Memory-Engine](https://img.shields.io/badge/Dolphin_Memory_Engine-1.3.0-darkcyan?style=flat&labelColor=013243)](https://pypi.org/project/dolphin-memory-engine/)

#### 项目说明

- 修改可能会导致游戏失去乐趣
- 仅支持Dolphin5.0以上版本模拟器，包括稳定版、beta版及dev版等
- 仅支持64位操作系统，有杀毒软件误报请自行设置
- 同时支持日文原版及汉化版
- 先打开模拟器开始模拟游戏，后打开修改器，读档进入游戏后可刷新获取数据
- 修改前和过程中多存档，以免出现不可挽回的损失
- 内置日文和中文两套翻译方案，仅界面名词上有区别，与日文版或汉化版无关
- 修改完成人物或职业后建议先存档读档一次再进行操作，以防出错
- 将人物改为上级职业时切记在技能的隐藏特性里勾选上级职业
- 可更改人物归属阵营以操作敌方，过关后可加入，但容易影响到后续话数的强制出击
- 可锁定人物行动状态，重复点击勾选可取消锁定

#### 可修改项目

- [x] 人物
- [x] 能力
- [x] 特技
- [x] 道具
- [x] 支援
- [x] 金钱

#### BUILD本地EXE程序方法
1. 安装适合自身操作系统版本的CPython解释器，教程自寻，要求3.8或更高版本。<br>[Python官网下载页面](https://www.python.org/downloads/)
2. 下载并解压项目完整源代码（下方的Source code.zip）
3. 双击build.bat等待构建完成
4. 在dist文件夹内找到修改器，程序入口PoR.exe
5. 程序构建成功后可按需删除源代码和卸载CPython解释器
