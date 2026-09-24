# Release Notes — KeyClickCounter v1.0.1

<p align="center">
  <a href="RELEASE.md"><img src="https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-7d8590?style=for-the-badge" alt="简体中文版本"/></a>
  <a href="RELEASE_EN.md"><img src="https://img.shields.io/badge/Language-English-2fbf71?style=for-the-badge" alt="English (Current)"/></a>
</p>

## 📦 Release Info

| Item | Value |
| --- | --- |
| Version | **v1.0.1** (patch release) |
| Release date | 2026-09-24 |
| Target platform | Windows 10 / 11 (64-bit) · `win-x64` |
| Runtime | **No .NET required** (single-file self-contained) |
| Repository | [https://github.com/HaoDaYiGuoFan/KeyMonitor](https://github.com/HaoDaYiGuoFan/KeyMonitor) |
| Tech stack | .NET 10 (`net10.0-windows`) · WPF · WinForms tray |
| AI assist | Project generated with the **DeepSeek V4 Flash** LLM |

## ✨ What's New in v1.0.1

**New in v1.0.1 (vs v1.0.0):**
- Multilingual UI: auto-matches the system language at launch (Chinese / English), one-click toolbar toggle, persisted with your data
- The package now bundles a per-file `SHA256SUMS.txt` checksum manifest; release docs include the full verification info

**Full feature overview (core):**
- Global low-level keyboard / mouse hooks (`WH_KEYBOARD_LL` / `WH_MOUSE_LL`) — works in any foreground app
- 104-key visual keyboard layout, pressed keys highlight for 200ms with live counters
- Mouse Left / Right / Middle / X1 / X2 counted independently; key-hold de-duplication (one press per physical press)
- Wheel notch & rotation statistics (24 notches per revolution); cursor travel distance in cm / m
- First-run date tracking + per-day persistence; automatic rollover into a new day across midnight
- Date-range filtering & range reset; toolbar px/cm calibration
- Light / dark theme instant switching, persisted with your data
- System-tray resident (show / reset stats / exit), always-on-top, single-instance guard
- Multilingual UI: auto-matches the system language at launch (Chinese / English), one-click toolbar toggle, persisted with your data

**Data & privacy:**
- All data is stored only on your machine at `%APPDATA%\KeyClickCounter\keycount.json`
- **Zero network uploads**; auto-save every 30 seconds, plus immediate save on exit / hide-to-tray / shutdown
- Legacy `keycount.json` next to the old exe is migrated to `%APPDATA%` automatically

## 📁 Package Contents

| File | Description |
| --- | --- |
| `KeyClickCounter.exe` | Single-file main program (self-contained, double-click to run) |
| `zh-Hans/KeyClickCounter.resources.dll` | Chinese localization resources (if present) |
| `README.md` / `README_EN.md` | Bilingual documentation |
| `RELEASE.md` / `RELEASE_EN.md` | These release notes |
| `LICENSE` | MIT license |
| `screenshot-*.png` | Screenshots |

## 🚀 Installation & Usage

1. Download `KeyClickCounter-1.0.1-win-x64.zip` and extract it anywhere
2. Double-click `KeyClickCounter.exe` to start (normal user privileges — no admin required)
3. Minimizing or closing the window hides the app to the **system tray**; double-click the tray icon to restore
4. Tray right-click menu: Show Window / Reset Statistics / Exit
5. Date range, theme, px/cm calibration are saved automatically

## 🔒 Data Storage & Privacy

- Data file: `%APPDATA%\KeyClickCounter\keycount.json`
- Contents: first-run date, per-day per-key counters, theme setting, px/cm calibration
- **Privacy**: everything stays local — the app makes no network requests, no telemetry, no ads
- Backup / migration: copy the json to the same path on another machine for a full restore

## 🔐 Checksums

The SHA-256 value of the release archive is in the bundled `SHA256SUMS.txt`. Verify with:

```powershell
Get-FileHash .\KeyClickCounter-1.0.1-win-x64.zip -Algorithm SHA256
```

## 🛠 Build from Source

```powershell
git clone https://github.com/HaoDaYiGuoFan/KeyMonitor.git
cd KeyMonitor

# Single-file self-contained publish (identical to the release package)
dotnet publish KeyClickCounter -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) on your machine.

## ⚠️ Known Issues & Notes

- Low-level hooks may not receive input inside some fullscreen games with anti-cheat (OS limitation, not a defect)
- Exit via the tray menu "Exit"; force-killing the process can lose up to the last 30 seconds
- Reset is irreversible — back up `keycount.json` first
- If security software blocks hook installation, the app warns you and keeps running (without tracking)

## 🗺 Roadmap

- [ ] CSV export / shareable statistics reports
- [ ] Per-key "today" view (all-time vs today)
- [ ] Auto-start on boot and more tray personalization options

Feedback is welcome via [Issues](https://github.com/HaoDaYiGuoFan/KeyMonitor/issues).