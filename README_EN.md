# KeyClickCounter — Keyboard & Mouse Usage Statistics

> 🌏 **Language: English** ｜ [**简体中文**](README.md)

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=.net&logoColor=white)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/UI-WPF-512BD4?logo=windows&logoColor=white)](https://learn.microsoft.com/windows/apps/desktop/wpf)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D6?logo=windows&logoColor=white)]()
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)
[![Release](https://img.shields.io/badge/Release-v1.0.0-blue)](RELEASE_EN.md)

## 📖 Introduction

**KeyClickCounter** is a **.NET 10 + WPF** desktop app for Windows that live-tracks your daily **keyboard key-press counts** and **mouse activity**: per-key and per-button press counts, mouse-wheel notches and rotations, and cursor travel distance (cm / m).

It captures input events through Windows global low-level hooks (`SetWindowsHookEx` / `WH_KEYBOARD_LL` / `WH_MOUSE_LL`) and visualizes the counters on a realistic **104-key keyboard layout**, highlighting each key as you type. It supports date-range queries, light/dark themes, and a system-tray resident mode. The app is a passive "read-only observer" — it never blocks, filters, or simulates any input.

## 🔒 Privacy Promise: All Data Stays on Your Computer

- All statistics are written only to a local JSON file: `%APPDATA%\KeyClickCounter\keycount.json`
- **No network uploads, no telemetry, no ads** — the app never opens any connection to external servers
- The app only observes keys; it never sends, simulates, or alters any input events
- To fully delete your data: uninstall the app and remove the `%APPDATA%\KeyClickCounter` directory
- To migrate: copy `keycount.json` to the same path on a new machine for a 100% restore

## 🤖 About This Project

This project was generated with the **DeepSeek V4 Flash** large language model: architecture design, the Win32 global-hook implementation, the MVVM view model, the JSON persistence scheme, the UI layout and this documentation — all AI-assisted, then manually reviewed and verified on real hardware.

## 📸 Screenshots

**Main window (light theme):**

![](screenshot-light.png)

**104-key visual keyboard:**

![](screenshot-keyboard.png)

**Dark theme:**

![](screenshot-light-restart.png)

## ✨ Features

### Global Statistics
- **Global low-level hooks** (`SetWindowsHookEx`): captures keyboard and mouse events in any foreground app
- **104-key standard layout** + mouse **Left / Right / Middle / X1 / X2** counted independently; holding a key counts only once (auto de-duplication)
- Pressed keys **highlight for 200ms** with live counter refresh; large numbers auto-format to `k` / `M`
- **Wheel statistics**: cumulative notch count (both vertical and horizontal scrolls, `|Δ| / 120`) converted to rotations (default 24 notches per rotation)
- **Cursor travel distance**: accumulated per-segment with Euclidean distance in the hook, converted to cm / m using the `px/cm` calibration factor (<100cm shown in cm, otherwise m)
- **px/cm calibration**: defaults to system DPI / 2.54, editable in the toolbar
- Rare keys (media keys, etc.) are aggregated into an "other keys" counter

### Time Dimension
- The statistics start date is recorded on first launch; counters are **bucketed per day** and cross-midnight sessions roll into the new day automatically
- **Date filtering**: two toolbar date pickers query any range (default: first launch ~ today); the status bar shows the active range
- **Reset** only clears data in the currently selected range (default: entire range); back up the json first

### Experience
- **Tray resident**: minimizing or closing the window hides it to the system tray; double-click the tray icon to restore
- **Tray menu**: Show Window / Reset Statistics / Exit
- **Theme toggle**: the "Dark skin" switch flips between light and dark instantly; the choice persists with your data (the Win32 title bar follows too)
- **Always-on-top** toggle; the status bar shows total keys / mouse / combined / wheel / distance / uptime
- **Single-instance guard**: launching again just wakes the existing window — no second instance that could overwrite data

### Persistence
- Data file: `%APPDATA%\KeyClickCounter\keycount.json` (legacy files stored next to the old exe are migrated automatically)
- **Auto-save every 30 seconds**, plus an immediate save on hide-to-tray / exit / Windows shutdown; forcibly killing the process loses at most 30 seconds

## 🛠 Tech Stack

| Technology | Purpose |
| ---- | ---- |
| [.NET 10](https://dotnet.microsoft.com/) | Target framework `net10.0-windows` |
| [WPF](https://learn.microsoft.com/windows/apps/desktop/wpf) | UI framework (`UseWPF`), MVVM |
| [WinForms NotifyIcon](https://learn.microsoft.com/windows/win32/windows-forms) | System tray icon & context menu (`UseWindowsForms`) |
| [P/Invoke + SetWindowsHookEx](https://learn.microsoft.com/windows/win32/winmsg/lowlevelkeyboardproc) | Global low-level keyboard / mouse hooks |
| System.Text.Json | JSON persistence & legacy-data migration |

## 📁 Project Structure

```text
KeyMonitor/
├── KeyClickCounter.slnx            solution file
└── KeyClickCounter/
    ├── KeyClickCounter.csproj     net10.0-windows / UseWPF / UseWindowsForms
    ├── App.xaml(.cs)              app entry (single instance + startup init)
    ├── MainWindow.xaml(.cs)       main window: visual keyboard + tray + persistence
    ├── ViewModels/
    │   └── KeyCountViewModel.cs   MVVM core: counting / highlight / totals / reset
    ├── Models/
    │   ├── KeyItem.cs             single key model (INotifyPropertyChanged)
    │   ├── StorageData.cs         persistence model (start date + per-day buckets)
    │   ├── KeyMapping.cs          virtual key code → UI id mapping
    │   ├── KeyboardLayout.cs      104-key + mouse geometry layout
    │   └── RelayCommand.cs        ICommand implementation
    ├── Services/
    │   ├── HookService.cs         Win32 global low-level hooks (install / uninstall / de-dup)
    │   ├── TrayIconService.cs     WinForms NotifyIcon tray
    │   └── StorageService.cs      JSON persistence + legacy migration
    ├── Themes/
    │   ├── Light.xaml             light theme resources
    │   └── Dark.xaml              dark theme resources
    └── Resources/
        └── KeyboardLayout.xaml    key styles & DataTemplate
```

## 🚀 Build & Run

### Requirements

- Windows 10 / 11 (64-bit)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (targets `net10.0-windows`)
- Visual Studio 2022 (17.x+) or the `dotnet` CLI

### Run

```powershell
dotnet run --project KeyClickCounter
```

### Build & Publish

```powershell
# Debug / Release build
dotnet build KeyClickCounter -c Release

# Publish (win-x64, framework-dependent)
dotnet publish KeyClickCounter -c Release -r win-x64 --self-contained false

# Publish (win-x64, single-file self-contained: no .NET required on target machines)
dotnet publish KeyClickCounter -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

You can also open `KeyClickCounter.slnx` in Visual Studio.

## 🗄 Data Storage

- Stats file: `%APPDATA%\KeyClickCounter\keycount.json`
- Contents: first-run date, per-day per-key counters, theme setting, px/cm calibration, etc.
- A legacy `keycount.json` sitting next to the old executable is migrated automatically on first launch
- Restore on a new PC: place a backed-up `keycount.json` at the same path (plain JSON — you can inspect it in any text editor)

## ❓ FAQ

- **Is my data uploaded?** No. The app makes zero network requests; everything stays in `%APPDATA%\KeyClickCounter\keycount.json`.
- Normal-user privileges are enough; some fullscreen games with anti-cheat may prevent low-level hooks from seeing input
- Please exit via the tray menu "Exit" — **avoid force-killing** the process, or up to the last 30 seconds may be lost
- Resetting statistics is irreversible; back up `keycount.json` first
- If the global hook fails to install (blocked by security software), the app shows a warning and keeps running without tracking

## 📄 License

This project is open-sourced under the [MIT License](LICENSE).

## 🤝 Contributing

Issues and PRs are welcome at [Issues](https://github.com/HaoDaYiGuoFan/KeyMonitor/issues) and [Pull Requests](https://github.com/HaoDaYiGuoFan/KeyMonitor/pulls)!

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/xxx`
3. Commit your changes: `git commit -m "feat: xxx"`
4. Push the branch: `git push origin feature/xxx`
5. Open a Pull Request

## 🙏 Acknowledgments

- The [.NET](https://dotnet.microsoft.com/) and [WPF](https://learn.microsoft.com/windows/apps/desktop/wpf) open-source ecosystem
- Microsoft documentation on [low-level keyboard / mouse hooks](https://learn.microsoft.com/windows/win32/winmsg/about-hooks)
- **DeepSeek V4 Flash** LLM: assisted in generating this project's code, architecture, and documentation