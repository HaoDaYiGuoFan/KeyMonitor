# KeyClickCounter — 键盘鼠标按键统计工具

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=.net&logoColor=white)
![WPF](https://img.shields.io/badge/UI-WPF-512BD4?logo=windows&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D6?logo=windows&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green)

基于 **.NET 10 + WPF** 的桌面应用：通过 Windows 全局低级钩子监听键盘与鼠标输入，在可视化 104 键布局上实时统计按键次数、高亮反馈，并支持按日期范围筛选与本地持久化。

> 单机本地运行，统计过程不打断输入、不拦截按键，数据仅保存在本机，无任何网络上传。

## 📸 界面预览

> TODO：请将截图放入项目根目录并替换下方图片链接（例如 `screenshot-light.png`、`screenshot-keyboard.png`）

| 主界面 | 键盘视图 | 深色皮肤 |
| ------ | -------- | -------- |
| ![](screenshot-light.png) | ![](screenshot-keyboard.png) | ![](screenshot-light-restart.png) |

## ✨ 功能特性

### 全局统计
- **全局钩子**（`SetWindowsHookEx` 低级键盘 / 鼠标钩子）：任意前台程序下均可捕获按键
- **104 键键盘** + 鼠标 **左 / 右 / 中 / X1 / X2** 独立计数，长按只计 1 次
- 按键触发时界面高亮 **200ms**，键帽上实时显示累计次数
- **滚轮统计**：累计格数（上下 / 左右滚均按 `|delta|/120` 计），并换算圈数（默认 24 格 / 圈）
- **光标移动距离**：钩子内逐段欧氏距离累加，按 `px/cm` 换算为 cm / m（<100cm 显示 cm，否则显示 m）
- **px/cm 校准**：默认取系统 DPI / 2.54，工具栏输入框可手动修正

### 时间维度
- 首次启动即记录统计起始日期，计数**按天分桶**持久化，跨天运行自动归入新的一天
- **日期筛选**：工具栏两个日期选择器可按日期范围查看统计（默认：首次启动 ~ 今天），状态栏实时显示当前统计范围
- 重置统计**仅清空当前筛选范围内**的数据（默认全部范围）

### 使用体验
- **托盘常驻**：最小化 / 关闭窗口均缩到托盘，双击托盘图标或右键菜单恢复
- **托盘菜单**：显示窗口 / 重置统计数据 / 退出程序
- **皮肤切换**：工具栏「深色皮肤」开关，浅色 / 深色即时切换，选择随数据持久化（Win32 标题栏跟随）
- **窗口置顶**开关，状态栏显示总按键 / 总鼠标 / 合计 / 运行时长
- **单实例守护**：重复启动只唤醒已运行窗口，不会开第二个实例互相覆盖数据

### 数据持久化
- 数据保存到 `%APPDATA%\KeyClickCounter\keycount.json`（旧版程序目录数据自动迁移）
- 每 **30 秒自动保存** + 缩到托盘 / 退出时立即保存；强杀进程最多丢失 30 秒数据

## 🛠 技术栈

| 技术 | 说明 |
| ---- | ---- |
| [.NET 10](https://dotnet.microsoft.com/) | 目标框架 `net10.0-windows` |
| [WPF](https://learn.microsoft.com/windows/apps/desktop/wpf) | UI 框架（`UseWPF`），MVVM 模式 |
| [WinForms NotifyIcon](https://learn.microsoft.com/windows/win32/windows-forms) | 系统托盘图标与右键菜单（`UseWindowsForms`） |
| [P/Invoke + SetWindowsHookEx](https://learn.microsoft.com/windows/win32/winmsg/lowlevelkeyboardproc) | 全局低级键盘 / 鼠标钩子 |
| System.Text.Json | JSON 持久化与旧数据迁移 |

## 📁 项目结构

```text
KeyMonitor/
├── KeyClickCounter.slnx            解决方案文件
└── KeyClickCounter/
    ├── KeyClickCounter.csproj     net10.0-windows / UseWPF / UseWindowsForms
    ├── App.xaml(.cs)              应用入口（单实例 + 启动初始化）
    ├── MainWindow.xaml(.cs)       主窗口：可视化键盘 + 托盘行为 + 持久化
    ├── ViewModels/
    │   └── KeyCountViewModel.cs   MVVM 核心：计数 / 高亮 / 汇总 / 重置命令
    ├── Models/
    │   ├── KeyItem.cs             单键模型（INotifyPropertyChanged）
    │   ├── StorageData.cs         持久化模型（首次启动日期 + 按天分桶）
    │   ├── KeyMapping.cs          虚拟键码 → 界面 Id 映射
    │   ├── KeyboardLayout.cs      104 键 + 鼠标键几何布局
    │   └── RelayCommand.cs        ICommand 实现
    ├── Services/
    │   ├── HookService.cs         Win32 全局低级钩子（安装 / 卸载 / 去重）
    │   ├── TrayIconService.cs     WinForms NotifyIcon 托盘
    │   └── StorageService.cs      JSON 持久化 + 旧数据自动迁移
    ├── Themes/
    │   ├── Light.xaml             浅色皮肤资源
    │   └── Dark.xaml              深色皮肤资源
    └── Resources/
        └── KeyboardLayout.xaml    按键样式与 DataTemplate
```

## 🚀 编译运行

### 环境要求

- Windows 10 / 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download)（目标 `net10.0-windows`）
- Visual Studio 2022（17.x+，或使用 `dotnet` CLI）

### 运行

```powershell
dotnet run --project KeyClickCounter
```

### 构建

```powershell
# Debug / Release 构建
dotnet build KeyClickCounter -c Release

# 发布（win-x64，依赖框架）
dotnet publish KeyClickCounter -c Release -r win-x64 --self-contained false

# 发布（win-x64，单文件自包含，无需目标机安装 .NET）
dotnet publish KeyClickCounter -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

也可用 Visual Studio 打开 `KeyClickCounter.slnx` 直接运行 / 发布。

## 🗄 数据存储说明

- 统计文件：`%APPDATA%\KeyClickCounter\keycount.json`
- 内容：首次启动日期、每日各键计数分桶、主题设置、px/cm 校准值等
- 旧版本存放在程序目录的 `keycount.json` 会在启动时自动迁移到 `%APPDATA%`
- 迁移 / 重装 / 换机后，将备份的 `keycount.json` 放回上述路径即可恢复统计

## ❓ 常见问题与注意

- 普通用户权限即可运行；部分全屏游戏（反作弊）内低级钩子可能收不到输入
- 程序退出时会正常卸载钩子，**请勿强杀进程**（最多丢失 30 秒统计）
- 重置统计为不可逆操作，重置前建议先备份 `keycount.json`

## 📄 开源协议

本项目基于 [MIT License](LICENSE) 开源。

## 🤝 参与贡献

欢迎提 [Issue](https://github.com/<your-username>/KeyMonitor/issues) 与 [PR](https://github.com/<your-username>/KeyMonitor/pulls)！

1. Fork 本仓库
2. 新建功能分支：`git checkout -b feature/xxx`
3. 提交改动：`git commit -m "feat: xxx"`
4. 推送分支：`git push origin feature/xxx`
5. 发起 Pull Request

## 🙏 致谢

- [.NET](https://dotnet.microsoft.com/) 与 [WPF](https://learn.microsoft.com/windows/apps/desktop/wpf) 开源生态
- Windows 平台 [低级键盘 / 鼠标钩子](https://learn.microsoft.com/windows/win32/winmsg/about-hooks) 文档
