# KeyClickCounter — GitHub Release 发布文案（中英双语，可直接粘贴）

> 使用说明：在 GitHub → Releases → **Draft a new release** 中填写下方内容。
> - Tag & 版本标题：`v1.0.0`
> - Target：`master`（当前 d1a5880）
> - 附件：上传 `KeyClickCounter-1.0.0-win-x64.zip`

---

## 方案一：简体中文文案

```
# KeyClickCounter v1.0.0 🎉

**首个正式版发布！**

基于 **.NET 10 + WPF** 的 Windows 键盘 / 鼠标统计工具：通过全局低级钩子实时统计
每次按键、鼠标左/右/中/X1/X2、滚轮格数与圈数、光标移动距离（cm/m），
并在 104 键可视化键盘上逐键高亮显示。

> 🔒 **所有数据仅保存在你的电脑本地（%APPDATA%\KeyClickCounter\keycount.json），
> 程序不联网、无遥测、无广告。**

## ✨ 主要特性

- **全局统计**：任意前台程序下均可捕获输入；104 键布局 + 鼠标五键独立计数
- **长按去重**：按住不放只计 1 次；按键触发高亮 200ms 实时反馈
- **滚轮与距离**：滚轮格数 → 圈数（24 格/圈）；光标移动按 px/cm 换算为 cm/m
- **日期维度**：首次启动日期记录，按天分桶持久化，支持日期范围筛选查询
- **主题与托盘**：浅色 / 深色即时切换，系统托盘常驻，窗口置顶，单实例守护
- **多语言界面**：启动自动跟随系统语言（中文 / 英文），工具栏一键中英切换，选择持久化
- **自动保存**：每 30 秒落盘，退出 / 缩到托盘 / 关机时立即保存
- **旧数据迁移**：旧版程序目录的 keycount.json 启动时自动迁移到 %APPDATA%

## 📦 下载

| 文件 | 说明 |
| --- | --- |
| KeyClickCounter-1.0.0-win-x64.zip | 63.6 MB · 单文件自包含 · Windows 10/11 64 位 · **无需安装 .NET** |

SHA-256：
`3B8A1A6E006176792DE6BED22D5D1FF24B8550A5644654D65A66C65AE7E10B29`

## 🚀 快速开始

1. 下载并解压 `KeyClickCounter-1.0.0-win-x64.zip`
2. 双击 `KeyClickCounter.exe` 启动（普通用户权限即可，无需管理员）
3. 最小化 / 关闭窗口会缩到系统托盘，双击托盘图标恢复
4. 托盘右键菜单：显示窗口 / 重置统计 / 退出程序

## 🔒 隐私说明

- 所有统计仅写入 `%APPDATA%\KeyClickCounter\keycount.json`
- **没有任何联网上传、遥测与广告**；程序只“看”输入，不发送、不模拟、不拦截按键
- 彻底清除：卸载后删除 `%APPDATA%\KeyClickCounter` 目录
- 换机备份：复制 `keycount.json` 到新电脑相同路径即 100% 还原

## 🛠 技术栈 & 构建

- .NET 10（net10.0-windows）· WPF（MVVM）· WinForms 托盘 · 全局低级钩子
- 构建：
  ```powershell
  dotnet publish KeyClickCounter -c Release -r win-x64 --self-contained true ^
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
  ```

## 🗺 Roadmap

- CSV 导出 / 统计报表
- 各键“今日”双视图
- 开机自启与更多托盘选项

## 🤖 致谢

本项目由 **DeepSeek V4 Flash** 大模型辅助生成（代码、架构与文档），并经人工审核与实机验证。

完整文档：[README](https://github.com/HaoDaYiGuoFan/KeyMonitor) · [Release Notes](RELEASE.md)
```

---

## 方案二：English 文案

```
# KeyClickCounter v1.0.0 🎉

**First stable release!**

A **.NET 10 + WPF** keyboard & mouse statistics app for Windows: global low-level hooks
count every key press, mouse L/R/M/X1/X2 buttons, wheel notches & rotations, and cursor
travel distance (cm/m) on a visual 104-key layout.

> 🔒 **All data stays on your machine (%APPDATA%\KeyClickCounter\keycount.json).
> No network, no telemetry, no ads.**

## ✨ Highlights

- **Global stats**: captures input in any foreground app; 104-key layout + 5 mouse buttons
- **Hold de-dup**: holding a key counts once; pressed keys highlight for 200ms
- **Wheel & distance**: notches → rotations (24/rev); cursor travel converted to cm/m (px/cm)
- **Time dimension**: first-run date tracked, per-day buckets, date-range filtering
- **Theme & tray**: instant light/dark switch, system-tray resident, always-on-top, single instance
- **Multilingual UI**: auto-matches the system language at launch (Chinese / English), one-click toolbar toggle, persisted with your data
- **Auto-save**: every 30s, plus immediate save on hide / exit / shutdown
- **Legacy migration**: old keycount.json auto-migrated to %APPDATA%

## 📦 Download

| File | Notes |
| --- | --- |
| KeyClickCounter-1.0.0-win-x64.zip | 63.6 MB · single-file self-contained · Windows 10/11 x64 · **no .NET needed** |

SHA-256:
`3B8A1A6E006176792DE6BED22D5D1FF24B8550A5644654D65A66C65AE7E10B29`

## 🚀 Quick Start

1. Download and extract `KeyClickCounter-1.0.0-win-x64.zip`
2. Double-click `KeyClickCounter.exe` (normal user privileges, no admin required)
3. Minimize or close the window to hide to the system tray; double-click the tray icon to restore
4. Tray right-click menu: Show Window / Reset Statistics / Exit

## 🔒 Privacy

- All data is written only to `%APPDATA%\KeyClickCounter\keycount.json`
- **No uploads, no telemetry, no ads** — the app only observes input; it never sends, simulates, or blocks keys
- Full wipe: uninstall and delete `%APPDATA%\KeyClickCounter`
- Migration: copy `keycount.json` to the same path on another machine for a 100% restore

## 🛠 Tech Stack & Build

- .NET 10 (net10.0-windows) · WPF (MVVM) · WinForms tray · global low-level hooks
- Build:
  ```powershell
  dotnet publish KeyClickCounter -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
  ```

## 🗺 Roadmap

- CSV export / reports
- Per-key "today" view
- Auto-start on boot & more tray options

## 🤖 Credits

Generated with the **DeepSeek V4 Flash** LLM (code, architecture & docs), manually reviewed and verified.

Docs: [README](https://github.com/HaoDaYiGuoFan/KeyMonitor) · [Release Notes](RELEASE_EN.md)
```

---

## 发布校验信息（存档）

- 主程序：`KeyClickCounter.exe`（单文件自包含，156,037,207 字节；SHA-256 `99D4430C016A46AAC746DCD2CA4B91F6DFD2017142C3CD8163B97A27AEB6ED36`）
- 发布包：`KeyClickCounter-1.0.0-win-x64.zip`（66,734,276 字节 ≈ 63.6 MB）
- 发布包 SHA-256：`3B8A1A6E006176792DE6BED22D5D1FF24B8550A5644654D65A66C65AE7E10B29`