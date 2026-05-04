<p align="center">
  <strong>🌐 Language:</strong>
  <a href="#中文">中文</a> | <a href="#english">English</a>
</p>

---

# AnyDrag

<a id="中文"></a>

## 中文

**按住鼠标组合键，从窗口任意位置拖拽 —— 无需标题栏。**

类似 macOS 三指拖拽，但用鼠标实现。按住自定义按键组合（如左键+右键+侧键1），在窗口任意位置移动鼠标即可拖动窗口。

### 功能

- **任意位置拖拽** — 无需点击标题栏，窗口哪里都能拖
- **自定义组合键** — 左键、右键、中键、侧键1、侧键2，1~5 个按键自由组合
- **全局快捷键** — 键盘快捷键切换开关（默认 Ctrl+Shift+D）
- **开机启动** — 托盘菜单一键设置
- **多语言** — 中文 / English

### 使用

1. 下载解压，运行 `AnyDrag.exe`，程序最小化到系统托盘
2. 默认组合键：**左键 + 右键 + 侧键1**，同时按住即可拖拽
3. 双击托盘图标切换开关，右键菜单进入设置

### 下载

到 [Releases](../../releases) 页面下载 `AnyDrag-v1.0.0-win-x64-self-contained.zip`，解压即用。

> 需要 Windows 10/11。

### 从源码构建

```bash
dotnet build
dotnet run
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

### 开源协议

MIT License

---

# AnyDrag

<a id="english"></a>

## English

**Hold a mouse button combo and drag from anywhere on the window — no title bar needed.**

Like macOS three-finger drag, but for mouse. Hold a custom combo (e.g. Left+Right+Side1) and move the mouse anywhere on a window to drag it.

### Features

- **Drag from anywhere** — No need to grab the title bar, drag from any part of the window
- **Custom combo** — Left, Right, Middle, Side1, Side2 — pick 1 to 5 buttons
- **Global hotkey** — Keyboard shortcut to toggle on/off (default: Ctrl+Shift+D)
- **Auto-start** — One-click startup from the tray menu
- **Multi-language** — Chinese / English

### Usage

1. Download, extract, and run `AnyDrag.exe` — it minimizes to the system tray
2. Default combo: **Left + Right + Side1** — hold all three to drag
3. Double-click tray icon to toggle, right-click for settings

### Download

Download `AnyDrag-v1.0.0-win-x64-self-contained.zip` from the [Releases](../../releases) page, extract and run.

> Requires Windows 10/11.

### Build from source

```bash
dotnet build
dotnet run
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

### License

MIT License
