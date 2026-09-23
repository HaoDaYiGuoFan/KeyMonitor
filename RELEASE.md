# Release Notes — KeyClickCounter v1.0.0

<p align="center">
  <a href="RELEASE.md"><img src="https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-2e7dff?style=for-the-badge" alt="简体中文（当前语言）"/></a>
  <a href="RELEASE_EN.md"><img src="https://img.shields.io/badge/Language-English-7d8590?style=for-the-badge" alt="English"/></a>
</p>

## 📦 版本信息

| 项 | 值 |
| --- | --- |
| 版本号 | **v1.0.0**（首个正式版） |
| 发布日期 | 2026-09-23 |
| 目标平台 | Windows 10 / 11（64 位）· `win-x64` |
| 运行时 | **无需安装 .NET**（单文件自包含 Self-contained） |
| 源码仓库 | [https://github.com/HaoDaYiGuoFan/KeyMonitor](https://github.com/HaoDaYiGuoFan/KeyMonitor) |
| 技术栈 | .NET 10（`net10.0-windows`）· WPF · WinForms 托盘 |
| AI 辅助 | 项目由 **DeepSeek V4 Flash** 大模型辅助生成 |

## ✨ 版本更新内容（v1.0.0）

**核心功能：**
- 全局低级键盘 / 鼠标钩子（`WH_KEYBOARD_LL` / `WH_MOUSE_LL`），任意前台程序下均可统计
- 104 键标准键盘可视化布局，按键按下高亮 200ms 并实时显示累计次数
- 鼠标左 / 右 / 中 / X1 / X2 独立计数；长按去重，每键只计一次
- 滚轮格数与圈数统计（默认 24 格 / 圈），光标移动距离（cm / m）统计
- 首次启动日期记录 + 按天分桶持久化，跨天自动切换统计日
- 日期范围筛选查询、范围重置；工具栏 px/cm 校准
- 浅色 / 深色主题即时切换，选择随数据持久化
- 系统托盘常驻（显示 / 重置 / 退出）、窗口置顶、单实例守护

**数据与隐私：**
- 所有数据仅保存在本机 `%APPDATA%\KeyClickCounter\keycount.json`
- **无任何网络上传**；每 30 秒自动保存，退出 / 缩到托盘 / 关机时立即保存
- 旧版存于程序目录的 `keycount.json` 自动迁移到 `%APPDATA%`

## 📁 发布包内容

| 文件 | 说明 |
| --- | --- |
| `KeyClickCounter.exe` | 主程序单文件（自包含，双击即用） |
| `zh-Hans/KeyClickCounter.resources.dll` | 中文本地化资源（如发布包含） |
| `README.md` / `README_EN.md` | 中 / 英双语文档 |
| `RELEASE.md` / `RELEASE_EN.md` | 本发布说明 |
| `LICENSE` | MIT 开源协议 |
| `screenshot-*.png` | 界面预览截图 |

## 🚀 安装与使用

1. 下载 `KeyClickCounter-1.0.0-win-x64.zip` 并解压到任意目录
2. 双击 `KeyClickCounter.exe` 启动（普通用户权限即可，无需管理员）
3. 最小化或关闭窗口会缩到**系统托盘**，双击托盘图标恢复
4. 托盘右键菜单：显示窗口 / 重置统计数据 / 退出程序
5. 统计范围、主题、px/cm 等设置自动保存，无需手动配置

## 🔒 数据存储与隐私

- 数据文件：`%APPDATA%\KeyClickCounter\keycount.json`
- 内容：首次统计日期、按天分桶的按键 / 鼠标计数、主题设置、px/cm 校准值
- **隐私**：所有数据仅存在于本机，程序不联网、无遥测、无广告
- 备份 / 换机：复制该 json 到新电脑相同路径即完整还原

## 🔐 校验信息（Checksums）

发布包 SHA-256 值请见随包附带的 `SHA256SUMS.txt`，可用以下命令核验：

```powershell
Get-FileHash .\KeyClickCounter-1.0.0-win-x64.zip -Algorithm SHA256
```

## 🛠 从源码构建

```powershell
git clone https://github.com/HaoDaYiGuoFan/KeyMonitor.git
cd KeyMonitor

# 单文件自包含发布（与发布包一致）
dotnet publish KeyClickCounter -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

需要本机安装 [.NET 10 SDK](https://dotnet.microsoft.com/download)。

## ⚠️ 已知问题与注意事项

- 部分带反作弊的全屏游戏中，低级钩子可能收不到输入（系统限制，非程序缺陷）
- 请通过托盘菜单「退出程序」正常退出；强杀进程最多丢失最后 30 秒统计
- 重置统计为不可逆操作，重置前建议先备份 `keycount.json`
- 若被安全软件拦截钩子安装，程序会提示并继续运行（此时不统计）

## 🗺 后续规划（Roadmap）

- [ ] 导出 CSV / 分享统计报表
- [ ] 各键「今日」双视图（总量 vs 今日）
- [ ] 开机自启与更多托盘个性化选项

欢迎通过 [Issue](https://github.com/HaoDaYiGuoFan/KeyMonitor/issues) 反馈建议。