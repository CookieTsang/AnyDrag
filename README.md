<p align="center">
  <strong>🌐 Language:</strong>
  <a href="#中文">中文</a> | <a href="#english">English</a>
</p>

---

# MouseDrag

<a id="中文"></a>

## 中文

Windows 鼠标组合键拖拽窗口工具。类似 macOS 三指拖拽，但用鼠标实现。

### 功能

- **组合键拖拽** — 按住自定义鼠标按键组合，拖动鼠标即可移动任意窗口，无需点击标题栏
- **自定义按键** — 支持左键、右键、中键、侧键1、侧键2 的任意组合
- **可调按键数量** — 1~5 个按键自由选择
- **全局快捷键** — 可配置键盘快捷键切换功能开关（默认 Ctrl+Shift+D）
- **开机启动** — 托盘菜单一键设置开机自启
- **多语言** — 支持中文 / English
- **事件拦截** — 拖拽时自动屏蔽系统右键菜单和左键框选

### 使用

1. 运行 `MouseDrag.exe`，程序最小化到系统托盘（绿色圆点）
2. 默认组合键：**左键 + 右键 + 侧键1**，同时按住即可拖拽窗口
3. 双击托盘图标切换开关，右键菜单可进入设置

### 设置项

| 设置 | 说明 |
|------|------|
| 按键数量 | 选择组合键包含几个按键（1~5） |
| 录制新组合 | 按下想要的按键组合自动录制 |
| 切换快捷键 | 录制键盘快捷键来切换功能开关 |
| 语言 | 切换中文 / English |
| 开机启动 | 托盘右键菜单中设置 |

### 下载

到 [Releases](../../releases) 页面下载最新版。

| 版本 | 文件 | 大小 | 说明 |
|------|------|------|------|
| 自包含版 | `MouseDrag-v1.0.0-win-x64-self-contained.zip` | ~63 MB | 无需安装任何运行时，解压即用 |
| 框架依赖版 | `MouseDrag-v1.0.0-win-x64-framework-dependent.zip` | ~90 KB | 需先安装 [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)，体积小 |

> 推荐大多数用户使用**自包含版**，开箱即用。如果你已安装 .NET 8 运行时，可选框架依赖版节省空间。
>
> 需要 Windows 10/11。

### 从源码构建

```bash
# 需要 .NET 8 SDK
dotnet build

# 运行
dotnet run

# 发布独立 EXE
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

### 开源协议

MIT License

---

# MouseDrag

<a id="english"></a>

## English

A Windows tool for dragging windows with mouse button combos — like macOS three-finger drag, but for mouse.

### Features

- **Combo drag** — Hold a custom mouse button combo and move the mouse to drag any window, no need to click the title bar
- **Custom buttons** — Any combination of Left, Right, Middle, Side1, Side2
- **Adjustable count** — Choose 1 to 5 buttons for the combo
- **Global hotkey** — Configurable keyboard shortcut to toggle on/off (default: Ctrl+Shift+D)
- **Auto-start** — One-click startup toggle from the tray menu
- **Multi-language** — Chinese / English
- **Event blocking** — Suppresses right-click menus and left-click selection during drag

### Usage

1. Run `MouseDrag.exe` — the app minimizes to the system tray (green dot)
2. Default combo: **Left + Right + Side1** — hold all three and move the mouse to drag a window
3. Double-click the tray icon to toggle on/off, right-click for settings

### Settings

| Setting | Description |
|---------|-------------|
| Button count | Number of buttons in the combo (1-5) |
| Record combo | Press the desired buttons to record |
| Toggle hotkey | Record a keyboard shortcut to toggle the feature |
| Language | Switch Chinese / English |
| Auto-start | Set in the tray right-click menu |

### Download

Download the latest version from the [Releases](../../releases) page.

| Version | File | Size | Description |
|---------|------|------|-------------|
| Self-contained | `MouseDrag-v1.0.0-win-x64-self-contained.zip` | ~63 MB | No runtime needed, extract and run |
| Framework-dependent | `MouseDrag-v1.0.0-win-x64-framework-dependent.zip` | ~90 KB | Requires [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), smaller download |

> Most users should use the **self-contained** version. Choose framework-dependent if you already have .NET 8 installed.
>
> Requires Windows 10/11.

### Build from source

```bash
# Requires .NET 8 SDK
dotnet build

# Run
dotnet run

# Publish standalone EXE
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

### License

MIT License
