---
title: PoR 修改器 Avalonia 重构规划
english_title: PoR Modifier Avalonia Refactor Plan
status: draft
created: 2026-11-11
updated: 2026-11-11
target_stack: Avalonia 12.1.6 / .NET 10 / C#
original_stack: Python 3.13 / PySide6 6.10.1 / dolphin-memory-engine 1.3.0
game: Fire Emblem - Path of Radiance (GameCube)
repo_strategy: same repository, branch `avalonia`, worktree `../PoR-avalonia`
audience:
  - maintainer
  - AI coding agent
decision_status:
  target_stack: working_assumption
  dme_strategy: recommended
  repo_strategy: recommended
  theme_strategy: recommended
  language_theme_entry: agreed_direction
---

# PoR 修改器 Avalonia 重构规划

> 本文档记录当前讨论结论，供后续开发或 AI 接手使用。
>
> **状态说明**
> - `working_assumption`：当前按此方向推进，但尚未最终拍板。
> - `recommended`：已经比较过方案，建议采用；如无特殊理由不要改。
> - `agreed_direction`：讨论中已形成一致方向。
> - `open`：还需要用户决定。
>
> **给 AI 的第一条规则**：先读本文件，再读原 Python 代码。不要凭界面截图猜行为；原来的隐藏交互（行动锁定、EquipLight 联动、动态枚举映射、槽位偏移）必须以本文件为准。

---

## 1. 结论摘要（TL;DR）

1. **目标**：用 **Avalonia 12 + C#** 重构《苍炎的轨迹》动态修改器，功能对齐原 Python/PySide6 版本。
2. **DME 策略**：**C# 移植 DME 的 GameCube/Windows 核心**；C++ DLL + C ABI 作为备选；**不要用 C++/CLI**。
3. **仓库策略**：**在当前仓库开 `avalonia` 分支 + `git worktree`**，不新开仓库；`master` 保留 Python 1.2 稳定版。
4. **目录策略**：第一阶段 Python 原样保留，C# 放在 `dotnet/` 下；功能对齐后再决定是否把 Python 移到 `legacy/`。
5. **UI 入口**：语言/主题/强调色可以用**一个 `DropDownButton` + `Flyout`** 完成，不需要一开始做完整设置页。
6. **主题策略**：**内置 FluentTheme + 自定义语义色 token + `ThemeService`**；第三方主题库可选，但不建议混用。
7. **翻译策略**：**JSON 字典 + `LocalizationService` + 自定义 `TrExtension`**；游戏名词与 UI 文案分两层。
8. **刷新策略**：一次读取 159 个角色槽位快照（约 99KB），不要逐字段读内存。
9. **写入策略**：所有读/写串行化；位域读改写；刷新时抑制写回；失败回滚并进入可重连状态。
10. **安全边界**：行动锁定用 `CancellationToken` 管理；关闭/断线必须停止所有锁。

---

## 2. 项目背景

### 2.1 原项目

- 游戏：火焰纹章 苍炎的轨迹（GameCube）。
- 原实现：Python 3.13 + PySide6 6.10.1 + `dolphin-memory-engine` 1.3.0。
- 功能：人物、能力、技能、道具、支援、金钱等内存修改。
- 打包：PyInstaller 单文件。
- 仓库现状（2026-11-11）：
  - 分支：`master`
  - 工作区：干净
  - 仓库体积：约 5.8MB
  - 跟踪文件：约 644 个
  - 没有 tag
  - 没有 `.gitignore`
  - `.idea/` 被跟踪（包括 `workspace.xml`）

### 2.2 原项目关键代码入口

| 文件 | 作用 |
|---|---|
| `src/main.py` | 启动：`hook()` → 语言弹窗 → QSS/QM → 主窗口 |
| `src/interface/window.py` | 主窗口：左侧角色列表 + 6 个 Tab；关闭时 `un_hook()` |
| `src/interface/status.py` | 状态页 |
| `src/interface/ability.py` | 能力页 |
| `src/interface/skill.py` | 技能页 |
| `src/interface/item.py` | 装备页 |
| `src/interface/support.py` | 支援页 |
| `src/interface/other.py` | 金钱/奖励 EX/帮助文本 |
| `src/widget/slot_list.py` | 角色列表、槽位序列、同行映射 |
| `src/widget/map_combo.py` | 带图标的枚举下拉框 |
| `src/widget/value_spin.py` | 数值输入框，按长度/位域计算范围 |
| `src/widget/bool_check.py` | 复选框；行动锁定使用三态 + QThread |
| `src/widget/support_bar.py` | 支援 C/B/A 进度条 |
| `src/widget/customize.py` | `offset` 父链、`Value` 结构 |
| `src/structure/value.py` | 大端、有符号长度、位域读写 |
| `src/structure/text.py` | 指针递归读取 C 字符串 |
| `src/parameter/data_setting.py` | 201 个字段定义 |
| `src/parameter/enum_data.py` | 枚举、支援、亲密度、动态映射 |
| `resource/` | 600 个图片资源、QSS、`.ts`/`.qm` |
| `src/interface/resource.py` | PySide6 生成的资源注册代码（9MB，不要迁移） |

---

## 3. 已确认 / 建议 / 待定

| 事项 | 状态 | 结论 |
|---|---|---|
| 用 Avalonia 12 重构 | working_assumption | 目标技术栈 |
| DME 处理方式 | recommended | C# 移植；C++ DLL 备选；禁止 C++/CLI |
| 仓库方式 | recommended | 当前仓库开 `avalonia` 分支 + worktree |
| 主题实现 | recommended | FluentTheme + 语义 token + ThemeService |
| 语言/主题入口 | agreed_direction | 一个下拉按钮 + Flyout |
| 完整 UI 日文翻译 | open | 建议先只翻译游戏名词 |
| 自动刷新 | open | 建议默认关闭，可选 1s/2s/5s |
| 人物/道具选择器搜索 | open | 建议做，至少虚拟化 |
| 第三方主题库 | open | 默认不用；需要现成风格时选一个 |
| 原 Python 代码位置 | open | 建议先保留在原路径，后期再移到 `legacy/` |
| 安装包 | open | 建议先单文件 EXE，Inno 后置 |

---

## 4. 不可违反的技术事实

### 4.1 地址类型

**控制台地址和枚举地址必须用 `uint` 或 `long`，不能用 `int`。**

原因：`0x802AF76C` 已经超过 `int.MaxValue`（0x7FFFFFFF）。Python 的 int 无此问题，C# 用 `int` 会溢出/变负数。

- `FieldDefinition.Address`：`uint`
- `MappingItem.Value`：`uint`
- 普通数值字段/UI：`long` 或 `decimal`
- `ValueCodec` 内部按无符号原始字节处理位域

### 4.2 槽位布局

| 常量 | 值 |
|---|---|
| `SlotBase` | `0x802AF5E4` |
| `SlotStep` | `0x280` |
| `SlotCount` | `0x9F`（159） |
| 快照大小 | `159 × 640 = 101,760` 字节 |

- 所有角色字段的 `address - SlotBase` 必须落在 `0 .. 0x280`。
- 所持金：`0x8032E7A1`，3 字节无符号。
- 奖励 EX：`0x8032E7A5`，3 字节无符号。
- 这两个是全局字段，不叠加槽位偏移。

### 4.3 原代码实际使用的 DME API

只用了：

- `hook()`
- `un_hook()`
- `is_hooked()`
- `read_byte()` / `read_word()` / `read_bytes()`
- `write_bytes()`
- 内部通过 `Value` 完成大端/有符号/位域处理

没有使用指针链跟随、Wii MEM2、ARAM。目标游戏是 GameCube，只需要 MEM1。

### 4.4 DME 现状

- 上游：`aldelaro5/dolphin-memory-engine`，MIT。
- 核心 C++ 大小：
  - `WindowsDolphinProcess.cpp`：321 行
  - `DolphinAccessor.cpp`：254 行
  - 合计约 575 行
- DME 没有官方 C# 绑定。
- Python 包 `randovania/py-dolphin-memory-engine` 是 **Cython 扩展**，不是 ctypes；不能直接 P/Invoke `.pyd`。
- DME C++ 是类 API，没有 `extern "C"`；CMake 默认只构建 GUI 可执行文件。
- `Sewer56/Dolphin.Memory.Access`：
  - LGPL-3.0
  - netstandard2.0
  - Windows-only
  - 最后活动 2024-01
  - 只处理 24MB MEM1，使用较老的“扫主模块找指针”方式
  - 不建议作为基础，只可参考
- 因此 C# 侧必须自己补内存层；DME MIT 允许移植/包装。

### 4.5 DME GameCube 检测逻辑（C# 移植要点）

在 Windows 上：

1. 查找进程：`Dolphin.exe`、`DolphinQt2.exe`、`DolphinWx.exe`。
2. `OpenProcess`，权限包含查询/读/写。
3. `VirtualQueryEx` 枚举内存区。
4. 用 `QueryWorkingSetEx` 过滤有效工作集。
5. 对每个候选区读取：
   - `base + 0x18` 的 `SystemInfo`
   - `base + 0x84` 的 `DolphinOSGlobals`
6. 校验 GameCube：
   - disc magic `0xC2339F3D`
   - boot code `0x0D15EA5E` 或 `0xE5207C22`
7. 得到 MEM1 基址与模拟内存大小。
8. 地址转换：`host = Mem1Base + (consoleAddress - 0x80000000)`。
9. 读写：`ReadProcessMemory` / `WriteProcessMemory`。

**只实现 MEM1 即可**；不要为这个项目引入 MEM2/ARAM 复杂度。

### 4.6 动态枚举映射

原 Python 的 `EnumData` 有 4 类动态映射：

| 映射 | 类型 | 参数 |
|---|---|---|
| AID | 指针映射 | `AID_SETTING = (0x802AF760, 0x14)` |
| PID | 指针映射 | `PID_SETTING = (0x802AF76C, 0x64)` |
| JID | 指针映射 | `JID_SETTING = (0x802AF770, 0x64)` |
| IID | 指针映射 | `IID_SETTING = (0x802AF7B0, 0x48)` |
| SEQ | 文本映射 | `SEQ_SETTING = (0x80511258, 0x10)`，地址偏移 `-0x131F` |
| MTYPE | 文本映射 | `MTYPE_SETTING = (0x80511048, 0x10)`，地址偏移 `-0x13DD` |

指针映射算法（必须复刻）：

1. 读 `pointerAddress` 当前指针。
2. 扫描 `pointerAddress + SlotStep * i`，找到非零指针。
3. 读取该指针指向的 C 字符串。
4. 在枚举键中找到匹配项，得到 `index`。
5. 基址 = `currentPointer - index * step`。
6. 返回 `{ base + step * i -> (enumKey, termKey) }`。

文本映射算法：

- `value = read_word(base + step * i) + addressOffset`
- 返回 `{ value -> (enumKey, termKey) }`

**写入前校验**：对指针映射的枚举写入，先读目标地址字符串并与 `Key` 比较；不匹配则拒绝写入并提示刷新。

### 4.7 原数据里的语言耦合

原 Python 的 `AFF_ENUM` 和 `SUPPORT_ENUM` 以**翻译后的名字**为键。

迁移到 C# 时：

- 用 `PID_ENUM` 的“PID 键 → 译名”反查成 `PID_*` 稳定键。
- 不能继续以译文为键，否则切换语言或重复译名会错。
- 支援伙伴名、亲密度图标都改为按 PID 键关联。

### 4.8 资源与翻译规模

资源：

| 目录 | 数量 |
|---|---|
| `resource/PID` | 340 |
| `resource/IID` | 189 |
| `resource/SID` | 46 |
| `resource/AFF` | 8 |
| `resource/WP` | 9 |
| 总计 | 约 600 个文件，约 4.3MB |

其他：

- `src/interface/resource.py`：9MB / 19.2 万行，PySide6 生成，**不要迁移、不要读**。
- `resource/resource.qrc`：不需要迁移。
- qrc 中的 `DLL/mdgcm.dll`：仓库里没有该文件，代码也没有引用，直接丢弃。
- `resource/QSS/custom.qss`：只需迁移样式意图，不迁移文件。
- `resource/QM/*.ts`：1392 条 message，只有 `EnumData` 上下文；中文/日文各 616 条 unfinished，但没有空翻译。
- UI 文案（Tab 名、字段名、按钮、帮助）是硬编码中文，没有进入 `.ts`。

---

## 5. 目标架构

### 5.1 解决方案结构

```text
PoR.sln
├─ src/
│  ├─ PoR.App/                     # Avalonia UI + ViewModel
│  │  ├─ Program.cs
│  │  ├─ App.axaml(.cs)
│  │  ├─ Assets/                   # PID/IID/SID/AFF/WP/icon.ico/blank.png
│  │  ├─ Localization/             # terms.zh.json, terms.jp.json, ui.zh.json, ui.jp.json
│  │  ├─ Views/
│  │  │  ├─ MainWindow.axaml
│  │  │  ├─ Dialogs/{LanguageDialog,MessageDialog}.axaml
│  │  │  └─ Tabs/{StatusView,AbilityView,SkillView,ItemView,SupportView,OtherView}.axaml
│  │  ├─ ViewModels/
│  │  │  ├─ MainWindowViewModel.cs
│  │  │  ├─ CharacterEditorViewModel.cs
│  │  │  ├─ Tabs/...
│  │  │  └─ Fields/{NumericFieldVM,EnumFieldVM,BoolFieldVM,TriStateLockVM}.cs
│  │  ├─ Controls/{IconComboBox,TriStateLockCheckBox,SupportBar,IconImage}.cs
│  │  ├─ Styles/{CompactTheme,Controls,DataTemplates}.axaml
│  │  └─ Services/{AppSettingsService,DialogService,ThemeService}.cs
│  │
│  ├─ PoR.Core/                    # 与 UI/Win32 无关
│  │  ├─ Memory/IDolphinMemory.cs
│  │  ├─ Values/{FieldDefinition,ValueCodec,BitField,FieldKind,FieldScope}.cs
│  │  ├─ Game/{GameProfile,EnumDefinition,MappingDefinitions,IMemoryReader,DolphinMemoryReader,MappingService,SlotSnapshot,GameSnapshot,SupportCatalog}.cs
│  │  ├─ Localization/ILocalizationService.cs
│  │  └─ Assets/IAssetCatalog.cs
│  │
│  ├─ PoR.Game/                    # 苍炎专用数据/编排
│  │  ├─ GameDataCatalog.cs
│  │  ├─ GameSession.cs
│  │  └─ Data/{fields,enums,support,affinity,army,group,weapons,skills,mappings}.json
│  │
│  └─ PoR.Memory.Dolphin/          # DME C# 移植
│     ├─ NativeMethods.cs
│     ├─ DolphinProcessLocator.cs
│     ├─ Mem1Locator.cs
│     └─ WindowsDolphinMemory.cs
│
├─ tests/
│  ├─ PoR.Core.Tests/
│  ├─ PoR.Game.Tests/
│  ├─ PoR.Memory.Tests/
│  └─ PoR.App.Tests/
│
├─ tools/migrate/                  # 一次性迁移脚本
│  ├─ export_fields.py
│  ├─ export_enums.py
│  ├─ convert_ts.py
│  ├─ validate_assets.py
│  └─ compare_state.py
│
├─ Directory.Build.props
├─ global.json
└─ README.md
```

第一阶段 C# 放在当前仓库的 `dotnet/` 下，Python 保持原路径：

```text
/dotnet/PoR.sln
/dotnet/src/...
/src/...          # Python 原样保留
/resource/        # 原样保留
```

功能对齐后再决定是否把 Python 移到 `legacy/python/`。

### 5.2 依赖方向

```text
PoR.App ──> PoR.Game ──> PoR.Core
   └─────> PoR.Memory.Dolphin ──> PoR.Core
```

- `PoR.Core` 不依赖 Avalonia、不依赖 Win32。
- `PoR.Memory.Dolphin` 只实现 `IDolphinMemory`。
- `PoR.Game` 只放苍炎数据、字段定义、映射实现。
- `PoR.App` 只做 UI、VM、设置、主题、本地化。

### 5.3 核心接口

```csharp
public enum DolphinStatus { Hooked, NotRunning, NoEmu, Unhooked, AccessDenied }

public interface IDolphinMemory : IDisposable
{
    DolphinStatus Status { get; }
    event EventHandler<DolphinStatusChangedEventArgs>? StatusChanged;

    bool Hook();
    void Unhook();
    void ReadBytes(uint consoleAddress, Span<byte> buffer);
    void WriteBytes(uint consoleAddress, ReadOnlySpan<byte> data);
}
```

```csharp
public enum FieldScope { Character, Global }

public sealed record FieldDefinition(
    string Id,              // 稳定英文 ID，如 "person"
    string LegacyKey,       // 原中文键，如 "人物"
    uint Address,           // 槽位 0 的绝对地址
    int Length,             // 正数无符号，负数有符号
    BitField? Bit,          // null = 整字段
    FieldScope Scope,
    string DisplayKey,      // UiText 键
    string? EnumId = null,
    bool RefreshAfterWrite = false);

public readonly record struct BitField(int Start, int End); // End 独占

public static class ValueCodec
{
    public static long Read(ReadOnlySpan<byte> buffer, int length, BitField? bit = null);
    public static void Write(Span<byte> buffer, int length, long value, BitField? bit = null);
}
```

```csharp
public sealed record MappingItem(
    uint Value,        // 写回内存的值（地址或数字）
    string Key,        // 稳定键，如 PID_IKE
    string TermKey,    // 翻译键
    string? IconKey);  // 图标键
```

```csharp
public sealed class GameSession
{
    public Task<GameSnapshot> RefreshAsync(CancellationToken ct);
    public Task WriteAsync(FieldDefinition field, int? slotIndex, long value, CancellationToken ct);
    public LockManager Locks { get; }
    public DolphinStatus Status { get; }
}
```

```csharp
public sealed class LockManager : IAsyncDisposable
{
    public bool IsLocked(uint address);
    public Task StartAsync(uint address, long value, TimeSpan interval, CancellationToken ct);
    public Task StopAsync(uint address, long? finalValue = null);
    public Task StopAllAsync();
    public event EventHandler<LockStateChangedEventArgs>? Changed;
}
```

### 5.4 数据流

```text
AXAML View
  ↕ compiled bindings
ViewModel (CommunityToolkit.Mvvm)
  ↕
CharacterEditorViewModel / GameSession
  ↕                         ↕
MappingService           LockManager
  ↕                         ↕
CachedMemoryReader ← IDolphinMemory ← WindowsDolphinMemory ← Win32
  ↑
GameDataCatalog(JSON) / LocalizationService / AssetCatalog
```

刷新流程：

1. `GameSession.RefreshAsync`。
2. 若未 hook，先尝试 hook。
3. 读取 159 × 0x280 槽位快照。
4. 构建 `RefreshContext`，按需计算 PID/JID/IID/AID/SEQ/MTYPE/Slot 映射，并缓存。
5. 解析每个槽位的 PID/JID、空槽/未知槽。
6. 生成 `GameSnapshot`。
7. UI 线程更新 ViewModel；刷新时抑制写回。

写入流程：

1. ViewModel 校验值。
2. `GameSession.WriteAsync` 获取串行锁。
3. 计算绝对地址；位域先读后改再写。
4. 写失败：`Unhook`、抛 `MemoryAccessException`、UI 回滚。
5. `RefreshAfterWrite` 字段防抖刷新；行动锁定由 `LockManager` 持续写。

---

## 6. DME 决策：C# 移植 vs C++ DLL

### 6.1 结论

**推荐 C# 移植。**

理由：

- 实际只用到 5 个 API，且只读 GC MEM1。
- DME Windows 核心约 575 行，GameCube 路径更少。
- 纯托管：单文件发布、调试、AOT、分发都简单。
- 不需要 CMake/MSVC/原生 DLL/VC runtime/混合调试。

### 6.2 备选：C++ DLL + C ABI

只在以下情况使用：

- 要支持 Wii MEM2/ARAM；
- 要跨 Linux/macOS；
- 希望自动跟随 DME 上游更新；
- 不想维护 C# 检测逻辑，且能接受原生构建。

实现方式：

- 用 `py-dolphin-memory-engine` 的 CMake 静态库源码；
- 加一层 `extern "C"` 包装：

```cpp
extern "C" {
  __declspec(dllexport) int  dme_hook();
  __declspec(dllexport) int  dme_status();
  __declspec(dllexport) int  dme_read(uint32_t console_addr, void* buffer, size_t size);
  __declspec(dllexport) int  dme_write(uint32_t console_addr, const void* buffer, size_t size);
}
```

- C# `DllImport`/`LibraryImport`，`CallingConvention.Cdecl`；
- 用 `int` 返回码，不要传 C++ `bool`；
- 捕获所有 C++ 异常，不让异常穿过 ABI；
- x64；`/MT` 或分发 VC++ runtime；保留 PDB；
- DLL 与 exe 同目录，或 `IncludeNativeLibrariesForSelfExtract`。

### 6.3 禁止

- 不要用 C++/CLI。
- 不要直接 P/Invoke 现有 DME exe 或 Python `.pyd`；它们没有稳定 C ABI。

---

## 7. UI/交互

### 7.1 主窗口

```text
┌────────────────────────────────────────────────────────────────────────────┐
│ [ICO] 苍炎的轨迹 动态修改器  v2.0              ● 已连接   [刷新] [设置]    │
├──────────────────┬─────────────────────────────────────────────────────────┤
│ 角色              │  状态   能力   技能   装备   支援   其他                │
│ ┌──────────────┐ │ ┌─────────────────────────────────────────────────────┐ │
│ │ 🔍 搜索角色   │ │ │ [头像 56] 艾克                                      │ │
│ └──────────────┘ │ │ 神将 · 古雷尔佣兵团 · Lv 20 · HP 41/41              │ │
│ ┌──────────────┐ │ │ 行动：未行动                    [ 锁定 ]            │ │
│ │[头像] 艾克    │ │ └─────────────────────────────────────────────────────┘ │
│ │ 神将 · Lv20  │ │ ┌─ 身份 ──────────────────────────────────────────┐   │
│ └──────────────┘ │ │ 人物 [艾克 ▾]          职业 [神将 ▾]            │   │
│ ┌──────────────┐ │ │ 模型 [领主 ▾]          阵营 [己方 ▾]            │   │
│ │[头像] 迪亚…   │ │ │ 部队 [古雷尔佣兵团 ▾]  同行 [—— ▾]             │   │
│ │ 圣骑士·Lv18  │ │ └─────────────────────────────────────────────────┘   │
│ └──────────────┘ │ ┌─ 状态 ──────────────────────────────────────────┐   │
│       …          │ │ 等级 [20] 经验 [45] HP [41] 化身 [0]            │   │
│                  │ │ 救出 ☐ 被救 ☐ 首领 ☐                            │   │
│ [ ] 显示空槽     │ │ 中毒 [0] 睡眠 [0] 沉默 [0] 狂暴 [0] 麻痹 [0]    │   │
│ [ 刷新列表 ]     │ └─────────────────────────────────────────────────┘   │
│                  │ ...                                                  │
├──────────────────┴─────────────────────────────────────────────────────────┤
│ ● 已连接 · Dolphin 5.0 · MEM1 0x… · 上次刷新 12:34:56        [诊断]       │
└────────────────────────────────────────────────────────────────────────────┘
```

布局：

- 左侧 300px：搜索框、角色列表、显示空槽、刷新按钮。
- 右侧：顶部当前角色摘要卡，下方 TabControl。
- 底部 28px 状态栏：连接状态、Dolphin 版本、MEM1 基址、上次刷新、诊断入口。
- 窗口最小约 980×620，默认约 1200×780。

### 7.2 控件映射

| Qt | Avalonia | 备注 |
|---|---|---|
| `QMainWindow` | `Window` | 保留最小高度 |
| `QTabWidget` | `TabControl` | 紧凑 Tab |
| `QListView` + model | `ListBox` + `ItemsSource/ItemTemplate` | 虚拟化 |
| `QComboBox` | `ComboBox` + `ItemTemplate` | 大列表用可搜索 `IconComboBox` |
| `QSpinBox` | `NumericUpDown` | `decimal` 绑定，`FormatString="0"` |
| `QCheckBox` | `CheckBox` | 位域写 0/1 |
| 三态 `QCheckBox` | 自定义 `TriStateLockCheckBox` | 未行动/锁定/行动完毕 |
| `QProgressBar` | 自定义 `SupportBar` | C/B/A 阈值 |
| `QLabel` + `QPixmap` | `Image` + `TextBlock` | 图标 |
| `QTextEdit` 只读 | `SelectableTextBlock` | 帮助文本 |
| `QMessageBox` | 自定义 `DialogWindow` | 语言/错误/确认 |
| QSS | `Styles` / `ControlTheme` | 紧凑 Fluent |
| `.qm`/`.ts` | JSON 本地化 | 游戏名词 |
| `qrc`/`resource.py` | `AvaloniaResource` | 直接复制图片 |

### 7.3 各 Tab 行为（必须保留）

| Tab | 字段/行为 |
|---|---|
| 状态 | PID/JID/AID/GROUP/ARMY/ALLY 下拉；首领/救出/被救；行动三态锁定；等级/经验/HP/化身/状态/坐标/战斗/胜利；MTYPE/SEQ1-3 |
| 能力 | 10 个 `S_*`、8 个 `E_*`、8 种武器熟练；`S_` 有符号 1 字节，`E_` 无符号 1 字节 |
| 技能 | 32 个技能、14 个奥义、47 个隐藏特性；每个是 4 字节 bit 0 |
| 装备 | 8 行：道具枚举、耐久、装备 bit7、掉落 bit6；空道具建议禁用耐久/开关 |
| 支援 | 当前角色亲密度/名字；7 行伙伴、等级、C/B/A 三条进度条；无支援隐藏 |
| 其他 | 所持金 3 字节、奖励 EX 3 字节、帮助文本 |

### 7.4 必须预先设计的隐藏行为

| 行为 | 原实现 | 新设计 |
|---|---|---|
| 行动锁定 | 三态复选框 + 10ms `QThread` + `terminate()` | `LockManager` 按绝对地址，`PeriodicTimer` + `CancellationToken`；断线/关闭停止 |
| EquipLight 联动 | `Window.charge_light()` 遍历视觉树 | `CharacterEditorViewModel` 共享状态/事件，更新能力页 `StaffIcon` |
| 动态枚举映射 | 每次刷新扫描槽位/读字符串 | `MappingService` 每刷新计算一次并缓存；空值回退；写前校验 |
| 槽位偏移 | `Customize.offset` 父链 | `SlotInfo { Index, Offset, SlotAddress }`，过滤后保留原始 offset |
| 支援条 | 3 个 `QProgressBar` 区间 | `SupportBar` 控件 + 阈值区间 |
| 语言 | 每次启动弹窗 + `.qm` | `LocalizationService`，持久化，GameTerms/UiText 分层 |
| 大列表 | 340/189 项 Qt 下拉 | 可搜索/虚拟化 `IconComboBox` + 图标缓存 |
| 未连接 | hook 失败直接退出 | 主窗口先启动，状态栏提示 + 重连 |
| 写入失败 | 未捕获异常 | `MemoryAccessException`，回滚 + 错误横幅 |
| 刷新性能 | 每字段一次读 | 一次 99KB 槽位快照 + 映射缓存 |

### 7.5 写入语义

- 所有写操作串行化；刷新和锁定不能并发写。
- 位域必须读整字段 → 改位 → 写回。
- 刷新/加载值时必须设置 `_loading` 标志，禁止触发写回。
- 数字输入建议 100–200ms 防抖；复选框/下拉立即写。
- 写失败：恢复旧值、状态栏报错、红色边框；不崩溃。
- 枚举下拉按 `Value/Key` 选中，不能按显示文本选中。
- 指针映射的枚举写入前校验目标字符串。
- 切换人物/职业/模型/道具等 `RefreshAfterWrite` 字段后防抖刷新，避免映射过期。

### 7.6 语言/主题切换入口

一个 `DropDownButton` + `Flyout` 足够，不需要完整设置页：

```text
● 已连接     [🎨 中文 · 浅色 ▾]     [↻]     [⚙]
```

弹出：

```text
┌─ 外观与语言 ───────────────────┐
│ 语言                            │
│  [ 中文 ▾ ]                     │
│                                 │
│ 主题                            │
│  [ 跟随系统 ▾ ]                 │
│                                 │
│ 强调色                          │
│  ●  ●  ●  ●  ●  ●              │
│  青 蓝 紫 红 琥珀 金             │
│                                 │
│ [更多设置…]                     │
└─────────────────────────────────┘
```

- 选择即时生效、即时保存，不需要“应用”按钮。
- 当前状态显示在按钮上。
- “更多设置…”再打开完整设置窗口，放自动刷新、锁定间隔、诊断。
- 语言选项建议用原生名称：`中文` / `日本語`。

---

## 8. 动态翻译实现

### 8.1 方案

**JSON 字典 + `LocalizationService` + 自定义 `TrExtension`**。

两层：

- `ui.zh.json` / `ui.jp.json`：Tab、字段名、按钮、提示、帮助文本。
- `terms.zh.json` / `terms.jp.json`：`PID_IKE`、`SID_HERO` 等游戏名词，来自原 `.ts`。

不要用 `.resx` + `CultureInfo` 做运行时切换（不会自动通知）；也不建议把 1392 条游戏名词拆成 ResourceDictionary。

### 8.2 服务核心

```csharp
public sealed class LocalizationService : INotifyPropertyChanged
{
    public static LocalizationService Instance { get; } = new();

    private IReadOnlyDictionary<string, string> _ui = Empty;
    private IReadOnlyDictionary<string, string> _terms = Empty;

    public string Language { get; private set; } = "zh";

    public string this[string key]
    {
        get
        {
            if (key.StartsWith("Term.", StringComparison.Ordinal))
            {
                var k = key[5..];
                return _terms.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v) ? v : k;
            }

            return _ui.TryGetValue(key, out var ui) && !string.IsNullOrWhiteSpace(ui) ? ui : key;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? LanguageChanged;

    public void SetLanguage(string language)
    {
        Language = language;
        _ui = LoadDictionary($"ui.{language}.json");
        _terms = LoadDictionary($"terms.{language}.json");

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
```

### 8.3 XAML 标记扩展

```csharp
public sealed class TrExtension : MarkupExtension
{
    public TrExtension() { }
    public TrExtension(string key) => Key = key;

    public string Key { get; set; } = "Ui.Missing";

    public override object ProvideValue(IServiceProvider serviceProvider)
        => new Binding
        {
            Source = LocalizationService.Instance,
            Path = $"[{Key}]",
            Mode = BindingMode.OneWay
        };
}
```

```xml
xmlns:loc="clr-namespace:PoR.App.Localization"

<TextBlock Text="{loc:Tr Ui.Field.Person}" />
<TabItem Header="{loc:Tr Ui.Tab.Status}" />
<Button Content="{loc:Tr Ui.Action.Refresh}" />
<Window Title="{loc:Tr Ui.App.Title}" />
```

### 8.4 游戏名词

枚举项 ViewModel 持有 `TermKey`：

```csharp
public string DisplayName =>
    LocalizationService.Instance[$"Term.{TermKey}"];
```

订阅 `LanguageChanged` 后 `OnPropertyChanged(nameof(DisplayName))`；重建集合时取消订阅。

### 8.5 规则

- 语言切换保留当前选中项的 `Value/Key`。
- 缺翻译回退 key。
- 启动时先 `SetLanguage` 再创建窗口，避免闪烁。
- 扫描 XAML `{loc:Tr ...}` 键做测试；扫描 `Term.*` 做覆盖率报告。

---

## 9. 动态主题实现

### 9.1 方案

- 明暗：`Application.Current.RequestedThemeVariant = ThemeVariant.Light / Dark / Default`。
- 配色：`ThemeService` 生成/替换语义色 `ResourceDictionary`。
- 所有样式用 `DynamicResource`，View 里不写死颜色。
- 强调色生成一组：`Accent/Hover/Pressed/Subtle/Border/OnAccent`。

### 9.2 语义 token

`AppBackgroundBrush`、`SurfaceBrush`、`SurfaceAltBrush`、`SurfaceHoverBrush`、`SurfaceSelectedBrush`、`BorderBrush`、`DividerBrush`、`TextPrimaryBrush`、`TextSecondaryBrush`、`TextDisabledBrush`、`AccentBrush`、`AccentHoverBrush`、`AccentPressedBrush`、`AccentSubtleBrush`、`AccentBorderBrush`、`OnAccentBrush`、`SupportBrush`、`SupportTrackBrush`、`SuccessBrush`、`WarningBrush`、`DangerBrush`、`FocusBrush`。

### 9.3 ThemeService

```csharp
public enum ThemeMode { System, Light, Dark }
public enum AccentPreset { Teal, Azure, Violet, Crimson, Amber, Gold }

public sealed class ThemeService
{
    private ResourceDictionary? _palette;
    private bool _applying;

    public ThemeMode Mode { get; private set; } = ThemeMode.System;
    public AccentPreset Accent { get; private set; } = AccentPreset.Teal;

    public void Initialize()
    {
        Application.Current!.ActualThemeVariantChanged += (_, _) =>
        {
            if (Mode == ThemeMode.System && !_applying)
                Apply(Mode, Accent);
        };
    }

    public void Apply(ThemeMode mode, AccentPreset accent)
    {
        Mode = mode;
        Accent = accent;

        var app = Application.Current!;
        _applying = true;
        app.RequestedThemeVariant = mode switch
        {
            ThemeMode.Light => ThemeVariant.Light,
            ThemeMode.Dark  => ThemeVariant.Dark,
            _               => ThemeVariant.Default
        };
        _applying = false;

        var variant = app.ActualThemeVariant == ThemeVariant.Dark
            ? ThemeVariant.Dark
            : ThemeVariant.Light;

        var palette = PaletteFactory.Create(variant, accent);

        if (_palette is not null)
            app.Resources.MergedDictionaries.Remove(_palette);

        _palette = palette;
        app.Resources.MergedDictionaries.Insert(0, palette);
    }
}
```

### 9.4 色板

基础色：

| Token | Light | Dark |
|---|---|---|
| AppBackground | `#F4F6F8` | `#0E131A` |
| Surface | `#FFFFFF` | `#151C25` |
| SurfaceAlt | `#F8FAFC` | `#1A222D` |
| Border | `#D7DEE7` | `#2B3745` |
| Divider | `#E5EAF0` | `#222C38` |
| TextPrimary | `#17202B` | `#E6EDF5` |
| TextSecondary | `#5A6878` | `#9AA8B8` |
| TextDisabled | `#98A4B3` | `#667385` |
| Accent (Teal) | `#0E8FA3` | `#35C7D8` |
| AccentSubtle | `#DFF5F8` | `#103239` |
| OnAccent | `#FFFFFF` | `#06272C` |
| Support | `#05B8CC` | `#22C7D6` |
| Success | `#1E8E4E` | `#3FB950` |
| Warning | `#B7791F` | `#D29922` |
| Danger | `#C53030` | `#F85149` |

强调色预设：

| 名称 | Light | Dark |
|---|---|---|
| 青（默认） | `#0E8FA3` | `#35C7D8` |
| 蓝 | `#2563EB` | `#60A5FA` |
| 紫 | `#7C3AED` | `#A78BFA` |
| 红 | `#DC2626` | `#F87171` |
| 琥珀 | `#B45309` | `#FBBF24` |
| 金 | `#A16207` | `#EAB308` |

`OnAccent` 按亮度自动选黑/白。

### 9.5 规则

- View 中不出现硬编码颜色；颜色只在 `PaletteFactory`。
- 不用 `StaticResource` 引用主题色。
- 切换主题不重建 ViewModel。
- 系统主题变化时重新生成 palette。
- 关键控件（Button/CheckBox/ComboBox/NumericUpDown/TabItem/ListBoxItem/ScrollBar）自定义紧凑样式，颜色走 token。
- 不要把 Fluent 内部资源键当稳定 API。

### 9.6 第三方主题库

截至 2026-11，Avalonia 12 可用的主要库：

| 库 | 最新稳定版 | 依赖 Avalonia | 风格 |
|---|---|---|---|
| FluentAvaloniaUI | 3.1.0 | 12.1.0 | WinUI/Fluent，带 NavigationView/InfoBar |
| Semi.Avalonia | 12.1.0.1 | 12.1.0 | Semi Design，表单密集 |
| SukiUI | 7.0.1 | 12.0.3 | 扁平/深色开发者工具风 |
| Material.Avalonia | 3.21.0 | 12.1.1 | Material 3 |
| 内置 Fluent/Simple | 12.1.6 | 无额外依赖 | 官方 |

建议：

- 本项目默认**内置 FluentTheme + 自定义 token**；紧凑 30px 布局更容易控制。
- 想快速出效果可选 **Semi.Avalonia**（表单）或 **SukiUI**（开发者工具）。
- 需要 WinUI 控件再选 **FluentAvalonia**；它自带 Fluent base，不要再加 FluentTheme。
- 不要同时引入多个主题库。
- 第三方库不解决翻译；动态强调色能力也各不相同。

---

## 10. UI 设计规范

### 10.1 字体与尺寸

| 角色 | 字号/字重 |
|---|---|
| 窗口标题 | 14 SemiBold |
| 区块标题 | 13 SemiBold |
| Tab | 13 Medium |
| 字段标签 | 12 Regular |
| 字段值 | 13 Regular |
| 状态栏 | 11 Regular |
| 帮助文本 | 12 Regular，行高 1.5 |

- 字体族：`Microsoft YaHei UI` / `Segoe UI Variable` / `Yu Gothic UI` 回退。
- 控件高度 30；卡片内边距 12；卡片间距 12；行间距 6；圆角 4/6。
- 图标：列表 40–50、人物选择器 56–100、道具 24、技能 28–36、亲和 30。

### 10.2 组件

- 按钮：主按钮（Accent 填充）、次按钮（Surface + Border）、幽灵按钮、危险按钮。
- 输入框/数字框：30 高，焦点 2px Accent 环；错误红框；禁用 45% 透明。
- Tab：38 高，选中底部 2px Accent 下划线。
- 角色列表项：56 高，选中 AccentSubtle + 3px 左 Accent 条；头像 40；锁定图标琥珀色。
- 支援条：22–24 高，圆角 3，轨道 SurfaceAlt，填充 Support/Accent。
- 行动三态：分段选择器 `未行动 / 锁定未行动 / 行动完毕`。
- 错误横幅：DangerSubtle + 图标 + 重连按钮。
- 空状态：未连接时提示“请先启动 Dolphin 并进入游戏”，保留重连。
- 加载状态：状态栏小型进度条，不遮挡内容。
- 可访问性：键盘可到达；焦点环清晰；颜色不作为唯一状态标识。

---

## 11. 数据迁移与验证

### 11.1 一次性 Python 脚本

| 脚本 | 输出 |
|---|---|
| `export_fields.py` | `fields.json`：稳定 ID、legacyKey、地址、长度、bit、scope |
| `export_enums.py` | `enums.json`、`support.json`、`affinity.json`、`army.json`、`group.json`、`skills.json`、`mappings.json` |
| `convert_ts.py` | `terms.zh.json`、`terms.jp.json` |
| `validate_assets.py` | 资源/键/字段/支援覆盖报告 |
| `compare_state.py` | Python/C# 状态对比 |

### 11.2 数据格式示例

```json
{
  "id": "person",
  "legacyKey": "人物",
  "address": "0x802AF76C",
  "length": 4,
  "bit": null,
  "scope": "character",
  "kind": "enum",
  "enumId": "pid",
  "displayKey": "Field.Person",
  "refreshAfterWrite": true
}
```

```json
{
  "pid": {
    "type": "pointer",
    "pointerAddress": "0x802AF76C",
    "step": 100,
    "enum": "pid"
  }
}
```

地址用十六进制字符串，避免 JSON 精度/可读性问题。

### 11.3 校验

- 每个 PID/IID/SID 键是否有图片（或 allowlist）。
- 支援伙伴是否能反查到 PID 键。
- `AFF_ENUM` 是否能映射到 PID 键。
- 每个角色字段是否在 `0x280` 内；bit 是否合法。
- 所有 VM 引用的字段/枚举 ID 是否存在。
- 翻译键覆盖报告。
- `PoR.Tools.Probe` 输出状态 JSON，与 Python 版 diff。

---

## 12. 测试

### 12.1 单元测试

- `ValueCodec`：1/2/3/4 字节、有符号/无符号、负值、边界、单 bit、多 bit、读改写不破坏邻位。
- `FieldDefinition`：槽位偏移、范围、bit 宽度、全局字段。
- `MappingService`：FakeMemory 指针表/字符串；PID/JID/IID/AID、SEQ/MTYPE、未知/空值、重复译名。
- `Support`：7 行、阈值区间、隐藏行、缺失头像。
- `LockManager`：启动/停止/最终值/失败/多地址；可注入时间源。
- `Localization`：缺失回退、语言切换、unfinished。
- `AssetCatalog`：键→URI、缺失回退。

### 12.2 手动回归

- Dolphin 稳定版/beta/dev：启动、读档、刷新、断开、重连。
- 6 个 Tab 所有字段类型各改一次，与 Python 版对比。
- 切换角色、刷新、空槽、未知职业/人物。
- 行动锁定：锁定后切角色、等 1 秒、切回；关闭程序确认停止。
- EquipLight 勾选后能力页武器图标变化。
- 支援等级改变，C/B/A 条同步。
- 语言/主题/强调色切换；重启保留。
- 单文件发布在干净 Windows 上运行。

---

## 13. 实施阶段

| 阶段 | 交付物 | 验收 |
|---|---|---|
| P0 数据迁移 | JSON + 资源清单 + 校验报告 | 关键资源/键零缺失；地址/bit 校验通过 |
| P1 内存 PoC | C# 控制台读取人物/金钱；槽位快照 | 与 Python 版一致 |
| P2 Core 领域 | `ValueCodec`、`FieldCatalog`、`MappingService`、FakeMemory 测试 | 单元测试通过；映射算法对齐 |
| P3 App 骨架 | Avalonia 主窗口 + 角色列表 + 状态页 + 语言 | 能读、写、刷新、切换角色 |
| P4 全页面 | 能力/技能/装备/支援/其他 + 自定义控件 | 功能对齐；EquipLight/支援条正确 |
| P5 行为完善 | 行动锁定、断线重连、设置、搜索、诊断 | 无崩溃、无锁泄漏 |
| P6 发布 | 单文件 EXE + 版本信息 + 可选安装包 | 干净机器可运行；回归通过 |

---

## 14. 环境与命令

### 14.1 当前环境（2026-11-11）

- Windows 11 Pro x64。
- .NET 10.0.14 **运行时已装**；**没有 .NET SDK**。
- VS Code 1.143.1；未装 C# / Avalonia 扩展。
- Visual Studio 只有 Build Tools 2022 17.7.6 + MSVC 14.37 + Windows SDK 10.0.22621；没有 IDE。
- Git、CMake、PowerShell 5.1、Inno Setup 已装。
- 磁盘约 176GB 可用；长路径已启用。
- NuGet 可用（`api.nuget.org` 会 302 到 `nuget.azure.cn`）。

### 14.2 需要安装

```powershell
# .NET 10 SDK（当前最新 10.0.403）
winget install --id Microsoft.DotNet.SDK.10 -e --source winget ^
  --accept-package-agreements --accept-source-agreements

# 验证
dotnet --list-sdks
```

```bash
# Avalonia 模板（当前 12.1.6）
dotnet new install Avalonia.Templates::12.1.6

# VS Code 扩展
code --install-extension ms-dotnettools.csharp
code --install-extension AvaloniaTeam.vscode-avalonia
```

### 14.3 创建项目

```bash
dotnet new avalonia.mvvm -o PoR.App
dotnet run
```

### 14.4 发布

```bash
dotnet publish -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true
```

可选：

- `-p:PublishReadyToRun=true`
- `-p:PublishTrimmed=true`（需测试 Avalonia 绑定）
- `-p:PublishAot=true`（需要较新的 MSVC/SDK）

---

## 15. 仓库策略

### 15.1 推荐

在当前仓库开新分支 + worktree，不新开仓库：

```bash
cd C:\iCode\PoR
git tag -a v1.2.0 -m "Python 1.2.0 baseline"
git push origin master --tags

git worktree add ..\PoR-avalonia -b avalonia
cd ..\PoR-avalonia
```

- `master`：Python 1.2 稳定版。
- `avalonia`：C# 重构。
- 两个目录共享 `.git`，可同时运行/对比。
- C# 放 `dotnet/`；Python 先保持原路径。
- 功能对齐后再决定 `legacy/python/` 或合并回 `master`。

### 15.2 先补 `.gitignore`

```gitignore
[Bb]in/
[Oo]bj/
.vs/
*.user
TestResults/
artifacts/
publish/
*.nupkg

venv/
dist/
build/
__pycache__/

.idea/workspace.xml
```

### 15.3 新开仓库的条件

- v2 变成独立产品/名称/许可证/维护者；
- 需要完全干净的公开历史；
- 两个版本独立发版/issue。

否则不要分仓。

---

## 16. 待确认

| 问题 | 建议默认 |
|---|---|
| UI 文案是否也做日文翻译 | 先只翻译游戏名词；UI 文案先中文资源化 |
| 人物/道具选择器是否加搜索 | 建议加，至少虚拟化 |
| 是否自动刷新 | 默认关闭，可选 1s/2s/5s |
| 主题风格 | 紧凑 Fluent + 原灰边框/青色支援条；深色可选 |
| 原 Python 代码位置 | 先原路径；后期移到 `legacy/` |
| 发布形式 | 先单文件 EXE；Inno 安装包后置 |
| 危险修改确认框 | 默认 inline 警告，不默认弹窗 |

---

## 17. 风险

| 风险 | 影响 | 缓解 |
|---|---|---|
| 动态映射依赖当前指针 | 枚举/道具列表空或错位 | 每刷新重算；空值回退；写前校验 |
| Dolphin 版本变化 | 无法 hook | 移植当前 DME 算法；日志；保留 DLL/sidecar 接口 |
| Python→C# 数据迁移错漏 | 数值/列表不一致 | 自动导出 + Probe 对比 + 数据完整性测试 |
| 翻译/图片缺失 | 空白或键名 | 校验脚本 + 回退 + 报告 |
| 大组合框卡顿 | UI 卡顿 | 虚拟化/搜索/图标缓存 |
| 锁定线程与 UI 写冲突 | 值被覆盖/锁泄漏 | 串行化 + CancellationToken + StopAll |
| Avalonia 12 较新 | 资料少/API 变化 | 只用核心控件；编译绑定；不依赖冷门库 |
| 游戏内回归不足 | 写入错误地址 | 回归清单 + Probe 对比 + 危险操作提示 |

---

## 18. AI 接手检查清单

### 开始前

1. 确认在 `avalonia` 分支/worktree，不要在 `master` 上直接重写。
2. 先读本文件，再读原 Python 代码。
3. 先做 P0/P1：数据导出 + 内存 PoC，不要先写 6 个页面。
4. 任何不确定的行为，以原 Python 版为准；先对齐，再现代化。

### 绝对不要

- 不要读 `src/interface/resource.py`（9MB 生成文件）。
- 不要把控制台地址存成 `int`。
- 不要按显示文本选 ComboBox；按 `Value/Key`。
- 不要在刷新加载值时触发写回（`_loading` 守卫）。
- 不要用 `QThread.terminate` 的等价物；用取消令牌。
- 不要用视觉树索引实现 EquipLight 联动。
- 不要同时引入多个主题库。
- 不要在 View 里硬编码颜色/文案。
- 不要直接把 `resource.py`/qrc 迁移到 Avalonia。
- 不要用 `Sewer56/Dolphin.Memory.Access` 作为基础（LGPL + 老旧）。
- 不要用 C++/CLI。
- 不要在没有游戏回归的情况下宣布完成。

### 每次提交前

- `dotnet build` 通过。
- 单元测试通过。
- 新增字段/枚举/资源通过完整性检查。
- 语言/主题切换手动验证：zh/jp × light/dark。
- 行动锁定启动/停止无泄漏。
- 刷新不冻结 UI。

---

## 19. 外部参考

- DME 上游：<https://github.com/aldelaro5/dolphin-memory-engine>
- Python DME：<https://github.com/randovania/py-dolphin-memory-engine>
- Sewer56 C# 库（仅参考）：<https://github.com/Sewer56/Dolphin.Memory.Access>
- Avalonia：<https://avaloniaui.net/>
- Avalonia 12 发布公告：<https://avaloniaui.net/blog/avalonia-12/>
- FluentAvalonia：<https://github.com/amwx/FluentAvalonia>
- Semi.Avalonia：<https://github.com/irihitech/Semi.Avalonia>
- SukiUI：<https://github.com/kikipoulet/SukiUI>
- Material.Avalonia：<https://github.com/AvaloniaCommunity/Material.Avalonia>

---

## 20. 文档维护

- 如果用户确认了“待确认”项，把对应状态改为 `confirmed` 并更新决策表。
- 如果 DME 策略改变，更新第 6 节和 `IDolphinMemory` 实现说明。
- 如果主题库选定，更新第 9.6 节和 `ThemeService` 包装方式。
- 如果仓库策略执行，更新第 15 节为实际分支/路径。
- 版本号、Avalonia/.NET 版本变化时更新 frontmatter 和第 14 节。

---

## 21. 实施进度

- 2026-11-11：P0 数据迁移完成。
  - `.NET 10 SDK 10.0.404` 已安装。
  - 生成 `dotnet/src/PoR.Game/Data/*.json` 和 `dotnet/src/PoR.App/Localization/terms.*.json`。
  - 校验工具报告：0 错误；PID/IID/SID/AFF/WP 图标 0 缺失。
- 2026-11-11：P1 内存 PoC 完成并验证。
  - `PoR.Core`：`IDolphinMemory` / `DolphinStatus` / `MemoryAccessException`。
  - `PoR.Memory.Dolphin`：Windows/GameCube MEM1 探测、读/写、状态机。
  - `PoR.Tools.Probe`：读取人物/职业/等级/金钱和 159 个槽位。
  - 构建通过；未启动游戏时正确返回 `NoEmu`。
  - 启动 Dolphin 2609b +《苍炎》后返回 `Hooked`，MEM1 大小 `0x1800000`。
  - C# 与 Python DME 的 person/class/level/money/visibleSlots/10 个槽位样本完全一致。
  - 同值写回自测（金钱 3 字节 + 等级 1 字节）通过，说明 `WriteProcessMemory` 正常。
  - 新增 `tools/legacy/probe_python.py` 和 `tools/verify_parity.py`，可重复做 parity 检查。
- 2026-11-11：P2 Core 领域层完成。
  - `ValueCodec`：大端、有符号、1–4 字节、位域读改写。
  - `FieldDefinition` / `FieldScope` / `FieldKind` / `BitField`。
  - `GameDataCatalog`：从嵌入 JSON 加载字段、枚举、支援、亲密度、分组/部队、映射。
  - `MappingService`：指针映射（PID/JID/AID/IID）、文本映射（SEQ/MTYPE）、分组/部队。
  - `SlotSnapshot` / `SlotInfo` / `GameSnapshot` / `GameSession`。
  - `GameSession` 刷新后映射数量：aid=508、pid=340、jid=115、iid=189、seq=75、mtype=33、group=8、army=30、ally=42。
  - 槽位 0 识别为 `PID_IKE` / `JID_HERO`，与 Python 版一致。
  - `--session-write-self-test` 对等级（字节）、行动（位域）、金钱（全局 3 字节）做同值写回，通过。
  - 45 个单元测试通过（ValueCodec、Catalog、MappingService、SlotSnapshot）。
- 2026-11-12：P3 Avalonia 壳完成（待人工 UI 测试）。
  - `PoR.App`：Avalonia 12.1.6 + CommunityToolkit.Mvvm。
  - 主窗口：左侧角色列表（头像/名字/职业）+ 右侧 Tab + 底部状态栏。
  - 状态页：身份/阵营/部队/同行、标志位、等级/经验/HP、状态、坐标、MTYPE/SEQ、战斗/胜利。
  - 语言切换（游戏名词 zh/jp）、浅色/深色切换、刷新/重连。
  - 其他 5 个 Tab 为占位页，P4 实现。
  - 资源图标已复制到 `dotnet/src/PoR.App/Assets`。
  - 构建 0 警告 0 错误；应用冒烟运行 15 秒无崩溃。
  - 启动命令：`dotnet run --project src/PoR.App/PoR.App.csproj`（在 `dotnet/` 下）。
  - 注意：Avalonia 原生依赖较大，当前机器通过 `.nuget-local` 本地包源完成 restore；该目录已忽略，不提交。
- 2026-11-12：根据人工 UI 测试修复。
  - 翻译资源 `.zh.json` 被 MSBuild 识别为 culture，生成了 `zh/PoR.App.resources.dll`；改为显式 `LogicalName` + `WithCulture=false`，并把资源名改为 `terms-zh.json` / `ui-zh.json`。
  - 新增 `ui.zh.json` / `ui.jp.json`，覆盖标题、Tab、按钮、连接状态、状态字段标签、行动选项。
  - 新增 `TrExtension`（`{loc:Tr Ui_App_Title}`），语言切换时刷新所有索引器绑定。
  - 语言 ComboBox、主题 ToggleButton 改为显式 `Mode=TwoWay`。
  - 主题增加 Mica、圆角控件样式、卡片背景、青绿色 Accent。
  - 仍需人工验证：Mica 在目标机器上的效果、浅色/深色切换、zh/jp 所有标签与游戏名词切换。
- 2026-11-12：根据截图反馈重构主窗口布局。
  - 删除顶部标题行，避免与系统标题栏重复；连接状态/语言/主题/刷新/重连接到底部状态栏。
  - 左侧头像 40 → 56；刷新列表按钮加宽至 220 并居中。
  - 状态页改为 `UniformGrid Columns=2`，所有行固定“100px 标签 + 拉伸控件”，解决控件宽度不一造成的杂乱。
  - NumericUpDown 隐藏 spinner，减少视觉噪音。
  - 移除 Mica/透明窗口，改用主题字典 `AppBackgroundBrush` / `AppSurfaceBrush` / `AppSurfaceAltBrush` / `AppBorderBrush`：暗色为 #1F1F1F/#2B2B2B，浅色为 #F3F3F3/#FFFFFF。
  - 语言改为按钮切换（中文/日本語），主题按钮带太阳/月亮图标，刷新/重连带 Fluent 图标。
  - 修复初始主题状态：根据 `ActualThemeVariant` 初始化 `IsDarkTheme`，避免系统已暗色时按钮仍显示“深色”。
  - 已用 UI Automation 实测：语言切换为日文后标题/Tab/字段/游戏名词全部变化；主题切换后浅色/暗色背景与控件颜色变化。
- 2026-11-12：第二轮 UI 反馈修复（紧凑布局 / 头像 / 行动锁定）。
  - 状态页 ViewModel 从动态 `Fields` 集合改为命名字段属性，视图按原版 6 列紧凑网格重建：66px 标签 + 110px 控件；人物/职业/模型/模式/SEQ 跨 5 列。
  - 数值字段恢复可见 spinner（`NumericUpDown` 宽 110，值区可显示 3 位数）；下拉框使用 12px 紧凑样式。
  - 行动字段改为自定义三态复选框 `TriStateLockCheckBox`（`StyleKeyOverride => typeof(CheckBox)` 复用 Fluent 主题），循环顺序 未行动 → 锁定未行动 → 行动完毕；中间态由 `GameSession` 每 10ms 写 0，切回时写 0/1 并停止轮询。
  - 实测：中间态下用 Python DME 外部写 action 位为 1，200ms 后 C# 锁把 action 位恢复为 0；切到“行动完毕”读回 1，再切回“未行动”读回 0。
  - 新增 2 个 `GameSession` 锁定单元测试；目前 47 个测试通过。
  - 删除左侧重复“刷新列表”按钮，只保留底部刷新；底部语言/主题/刷新/重连按钮统一 96px，文字切换不再改变布局。
  - 新建 `tools/process_avatars.py`：把原版 GIF 头像外圈的白色抗锯齿蒙版转换为黑色 + alpha，输出 340 张 `PID/*.png`；`AssetService` 优先加载 PNG、回退 GIF。深色/浅色背景下边缘均无白边。
  - 人物下拉框选项和选中项都显示头像；头像/图标行使用 32px 图片。
- 2026-11-12：第三轮 UI 微调。
  - 字段名称标签水平居中；标签字号 12px。
  - 人物下拉框恢复原版 100px 头像，下拉列表最大高度 540，人物行控件高度 112px。
  - 其他下拉框与微调框统一为 32px 高；数值文本右对齐。
  - 行列间距放宽（控件上下 3px、左侧 2px），复选框左侧额外 10px 缩进。
  - Tab 字号从默认值降至 13px，内边距 12,5。
  - 人物标签改为顶部对齐，并通过 `StatusView.axaml` 中的 `PersonLabelMargin` 资源暴露上边距微调接口。
- 2026-11-12：固定窗口尺寸并加宽编辑控件。
  - 窗口从 1180×760 调整为 960×760，左侧角色列表从 300px 收窄到 280px，禁用拖拽缩放和最大化，启动时居中。
  - 状态页列宽从 66/110 调整为 72/124，扩大数值编辑框和整体表单宽度，避免最大化后控件群显得过窄。
- 2026-11-12：P4 全部编辑页完成。
  - 将 Numeric/Enum/Bool/TriState 编辑器 DataTemplate 上移到 `App.axaml`，六个页面共用。
  - 能力页：10 项能力值、8 项成长率、8 种武器熟练，使用 WP 图标。
  - 技能页：32 个技能 + 14 个奥义、51 个隐藏特性，双列滚动列表，SID 图标和即时读写。
  - 装备页：8 组道具/耐久/装备/掉落，道具下拉使用 IID 图标并提供空道具选项。
  - 支援页：当前角色属性图标/姓名、7 名支援对象、支援值与 C/B/A 三段进度条；修改支援值时进度条即时更新。
  - 其他页：全局所持金、奖励 EXP 和原版修改提示。
  - `GameSnapshot` 新增 `GlobalValues`，刷新时同时读取全局字段，其他页直接使用同一快照。
  - 新增目录完整性与全局快照单元测试；目前 49 个测试通过。
  - 新增 `CompactNumericUpDown`：运行时把默认横向大号 spinner 重排为 18px 宽的上下箭头，数值区更宽，切换主题后仍保持紧凑样式。
- 2026-11-12：`avalonia-fluent-ui` 分支采用 FluentAvaloniaUI。
  - 引入 `FluentAvaloniaUI` 3.1.0，移除内置 `Avalonia.Themes.Fluent`，改用 `FluentAvaloniaTheme`（Accent `#0E8FA3`）。
  - 主窗口改为 `FAAppWindow`，其他页面的警告改用 `FAInfoBar`。
  - 保持固定 960×800、禁止缩放/最大化、左侧列表 280px、紧凑表单和固定宽度底部按钮。
  - 该分支使用自定义数值编辑器：左右各一个接近正方形的 Fluent 风格按钮（左减右加），中间为居中的 TextBox；加减通过 ViewModel 命令执行并在 `NumericFieldViewModel` 中统一做范围钳制。
  - 所有页面 ViewModel、本地化、内存读写逻辑与 `avalonia` 分支共用。
- 2026-11-12：FluentAvalonia 界面细节调整。
  - 数值框改为自定义编辑器：TextBox 占满整个控件，左右各一个 26px 接近正方形的 Fluent 风格按钮叠加在上面；按钮默认无边框、透明底色，仅在鼠标悬浮时显示 Fluent 悬浮底色。文本左右预留按钮宽度并居中显示，加减通过 ViewModel 命令执行并做范围钳制。
  - 底部按钮顺序改为刷新（强调色）、重连、主题（中性色）、语言；主题按钮不再使用强调色。
  - 状态页标签宽度 72→80；行动复选框前增加“行动状态”标签。
  - 救出/被救放入同一横向 StackPanel，缩小间距并保证被救文字完整显示。
  - 微调按钮内容改为居中对齐，修正取消边框后箭头偏移 1px 的问题。
  - 主题切换从 ToggleButton 改为与语言相同的普通 Button + `ToggleThemeCommand`，两者视觉一致。
  - 新增 `CompactTabControl`，在模板应用后移除 FluentAvalonia `PART_SelectedContentHost` 的 12px 外边距，并将 TabControl 外边距从 8 缩小到 4；右侧页面内容更贴近边缘。
  - 微调按钮右侧箭头增加 1px 修正量，解决视觉上仍略偏左的问题。
  - 所有复选框文本统一为 12px，与下拉框/数值框/标签一致。
  - 左侧角色列表头像去掉方框底色，改为透明背景。
  - 复选框文本改为垂直居中并通过 `TranslateTransform Y=-2` 再上移 2px；微调箭头再次下移 1px。
  - 禁用状态的微调按钮显式移除背景和边框，只保留禁用箭头。
- 2026-11-12：状态页新网格布局。
  - 窗口宽度 960→1220px，高度保持 800px，仍禁止缩放/最大化，左侧列表 280px。
  - 状态页改为 10 列网格，每列 90px，总内容宽 900px；每行高 40px。
  - 人物下拉框占 3 行 × 4 列（360px）；职业、3D建模下拉框各占 4 列；同行下拉框占 2 列（180px）。
  - 移动AI 下拉框占 3 列（270px）；行为AI 行使用 1 个 90px 标签和 3 个 270px 下拉框。
  - 其余数值/复选框为 1 列（90px）；每行最多 5 组“标签 + 控件”。
  - 未连接模拟器时错误提示缩短为单行，并限制在状态栏剩余宽度内，超出时省略。
- 2026-11-12：状态页并入能力与装备组。
  - 状态组下方新增能力（5 列）与装备（5 列）两组，左右并排；窗口宽度由 1220px 调整为 1260px，高度保持 800px。
  - 能力组每行 5 个 90px 单元格：属性标签/数值、成长数值、武器熟练标签/数值；体格与移动没有成长和熟练数据，后三格留空。成长列不再单独显示“经验”标签。
  - 装备组复用现有 8 个 item 槽：武器栏 4 行、道具栏 4 行；每行下拉框跨 2 格，数量/装备/掉落各 1 格；两个栏目标题各跨 5 格。
  - 页面内容超出窗口高度时显示自适应垂直滚动条，滚动条在需要时保持可见。
  - 能力/装备标签页与状态页共享同一 ViewModel 实例，任意一处修改都会反映到另一处。
- 2026-11-12：状态页单元格加宽到 100px。
  - 状态组 10 列 × 100px（1000px）；能力组与装备组各 5 列 × 100px（500px），间隔 10px。
  - 人物/职业/3D建模下拉框 400px，同行 200px，移动AI 300px，行为AI 为 1 个 100px 标签和 3 个 300px 下拉框。
  - 装备下拉框 200px，数量/装备/掉落各 100px；窗口宽度调整为 1360px，仍保留自动垂直滚动条。
- 2026-11-12：窗口宽度调整为 1440px。
  - 状态页单元格、组宽度和垂直滚动条保持不变；加宽部分作为左右留白，内容仍居中。
- 2026-11-12：状态页拆分为三个独立面板。
  - 状态、能力、装备分别使用独立的 `Border.card` 面板；能力与装备面板并排，间隔 10px。
  - 页面级 `ScrollViewer` 包裹三个面板，保留自动垂直滚动条。
  - 面板内容宽度保持：状态 1000px，能力/装备各 500px；窗口保持 1440×800。
- 2026-11-12：装备面板微调复选框列。
  - 仅将“装备”复选框列从 100px 缩到 64px，“掉落”仍为 100px。
  - 64px = 100px −（10px 面板间距 + 两侧各 12px padding + 1px 边框）= 100 − 36；用于抵消面板内边距和间距造成的视觉宽度差。
  - 装备行内容宽变为 464px（200 + 100 + 64 + 100）；装备卡片外宽相应调整为 490px（464 + 12×2 padding + 1×2 边框）。
  - 下层总宽为 526 + 10 + 490 = 1026px，与上方状态卡外宽 1026px 完全一致；三层内容从左边缘到右边缘对齐。
  - 装备复选框改为局部紧凑 CheckBox，去掉模板的 10px 左边距，使 64px 内仍完整显示“装备”文本。
- 2026-11-12：标准单元格宽度加大到 108px。
  - 状态组改为 10 × 108px（1080px）；人物/职业/3D建模下拉框 432px，同行 216px，移动AI 324px，行为AI 为 108px 标签 + 3 个 324px 下拉框。
  - 阵营下拉框改为标准 1 格（108px），部队下拉框改为标准 2 格（216px），首领仍为 1 格（108px）。
  - 能力组 5 × 108px（540px），面板外宽 566px。
  - 装备行：下拉框 2 格 216px、数量 108px、装备列 72px、掉落 108px；装备内容宽 504px，面板外宽 530px。
  - 下层总宽 566 + 10 + 530 = 1106px，与状态面板外宽 1106px 对齐；窗口保持 1440×800。
- 2026-11-12：同行人物下拉框增加空选项。
  - 动态同行列表最前面加入值为 0、显示为“——”的空项；选择后写回 0，表示没有同行人物。
