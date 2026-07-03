# UniPlayer - 音乐播放器

基于 .NET Framework 4.7.2 的 Windows 桌面音乐播放器。
核心播放引擎使用 Windows Media Player COM 控件，并集成 NAudio（OGG）和 LibVLC（网络串流）作为扩展。

---

## 使用的技术与方法

### 开发框架
- **.NET Framework 4.7.2 / C# 8+** — 项目运行时，支持模式匹配、switch 表达式等现代语法
- **Windows Forms** — UI 框架，使用 `Form`、`Button`、`ListBox`、`TrackBar`、`Label`、`Timer`、`NotifyIcon`、`ContextMenuStrip`、`OpenFileDialog` 等控件
- **SDK 风格 .csproj** — 迁移后的项目格式，支持 `dotnet build` 命令行编译

### 音频播放引擎

| 引擎 | 应用场景 | 技术实现 |
|------|---------|---------|
| **Windows Media Player COM** | MP3 / FLAC / WAV | `AxInterop.WMPLib.AxWindowsMediaPlayer` 控件，通过 `URL = filename` 加载音频，`Ctlcontrols.play()` / `pause()` / `stop()` 控制播放 |
| **NAudio 2.3.0 + NVorbis** | OGG Vorbis | `WaveOutEvent` 输出设备 + `VorbisWaveReader` 解码，独立于 WMP 运行。OGG 文件自动走此引擎 |
| **LibVLC 3.0.23 + LibVLCSharp 3.9.7** | 网络串流 (RTMP/HLS) | `LibVLC` 实例 + `MediaPlayer`，`VideoView` 控件作为视频输出占位 |

### 软件架构

- **服务层抽取** — 将播放逻辑从 Form1.cs 抽入独立服务类：`AudioPlaybackService`、`PlaylistManager`、`OggPlayerService`、`StreamingService`、`HotkeyManager`
- **双引擎路由** — `PlayByIndex()` 和 `listBox1_SelectedIndexChanged` 根据 `.ogg` 扩展名自动选择 WMP 或 NAudio 引擎
- **事件驱动** — 播放状态变化通过 C# `event` 通知 UI 更新，`BeginInvoke` 处理跨线程访问
- **原始代码保留** — 核心 `musicplay(string filename)` 方法原封不动保留在 `AudioPlaybackService` 中

### 关键技术细节

| 技术 | 方法 |
|------|------|
| 淡入淡出 | `Timer` 每 30ms 步进调整 `_wmp.settings.volume`（每次 ±4），`_preFadeVolume` 保存原音量 |
| 倍速播放 | 直接设置 `_wmp.settings.rate`（0.5~2.0），界面标签循环切换 |
| 自动下一曲（WMP） | 监听 `PlayStateChange` 事件，`wmppsMediaEnded` 触发 `BeginInvoke(PlayNext)` |
| 自动下一曲（OGG） | 订阅 `_oggPlayer.PlaybackStopped` 事件，NAudio 线程用 `BeginInvoke` 回 UI 线程 |
| 进度追踪 | `Timer` 每 500ms 查询引擎的 `CurrentPosition` / `Duration`，OGG 时查 NAudio |
| 播放队列 | `PlaylistManager._playQueue`（`List<int>`），`GetNextIndex()` 优先出队 |
| 播放模式 | 枚举 `Sequential` / `RepeatOne` / `Shuffle`，`GetNextIndex()` 根据模式计算下一曲索引 |
| 全局快捷键 | `User32.RegisterHotKey` / `UnregisterHotKey` Win32 API，Form 重写 `WndProc` 处理 `WM_HOTKEY` 消息 |
| M3U 歌单 | 标准 `#EXTM3U` 格式，每行一条文件绝对路径 |
| 拖拽排序 | ListBox 的 `MouseDown` / `MouseMove` / `MouseUp` 事件检测拖拽并调用 `PlaylistManager.Move()` |
| 系统托盘 | `NotifyIcon` 控件 + `ContextMenuStrip`，`Form.Resize` 事件在最小化时隐藏窗口 |
| 定时关闭 | 独立 `Timer`（Interval=1000ms）倒计时，到期调 `_audioService.Stop()` + `_oggPlayer.Stop()` |
| 播放历史 | `List<string>` 上限 50 条，`HistoryForm` 弹窗显示，双击回播 |
| 设置持久化 | 退出时写入 `%LOCALAPPDATA%/UniPlayer/settings.ini`，启动时读取恢复（含倍速、定时器状态） |

### 构建工具

- **MSBuild / dotnet CLI** — .NET SDK 9.0 编译，目标框架 `net472`
- **NuGet 包管理** — 依赖包：`NAudio 2.3.0`、`NAudio.Vorbis 1.5.0`、`LibVLCSharp 3.9.7`、`VideoLAN.LibVLC.Windows 3.0.23`
- **COM 互操作** — WMP 控件的 COM 引用使用预生成的 `AxInterop.WMPLib.dll` / `Interop.WMPLib.dll` 避免运行时 COM 注册问题

---

## 功能清单

### 播放控制

| 功能 | 说明 |
|------|------|
| 播放/暂停 | 同一按钮切换，播放中显示"暂停"，否则显示"播放"，OGG/非 OGG 均支持 |
| 上一曲 | ⏮ 按钮，切换时含淡入淡出效果 |
| 下一曲 | ⏭ 按钮，切换时含淡入淡出效果 |
| 停止播放 | ⏹ 按钮，停止后进度归零，同时取消定时关闭 |
| 播放模式切换 | 🔁 按钮循环切换：顺序播放 → 单曲循环 → 随机播放 |
| 自动下一曲 | 当前曲目播放完毕后自动播放下一条，支持 WMP + OGG 双引擎 |
| 进度拖动 Seek | 拖动进度条可跳转到任意位置，OGG 时写 `VorbisWaveReader.CurrentTime` |
| 实时进度显示 | 每 500ms 更新进度条和时间（`当前时间 / 总时长`） |

### 高级播放功能

| 功能 | 说明 | 实现 |
|------|------|------|
| 淡入淡出 | 切换曲目时音量平滑过渡（约 0.6s） | `AudioPlaybackService.FadeOut()` + `FadeIn()`，Timer 步进调 `settings.volume` |
| 倍速播放 | 点击 `1.0x` 标签循环切换速度 | `_wmp.settings.rate` 在 0.5x / 0.75x / 1.0x / 1.25x / 1.5x / 2.0x 间循环 |
| 播放队列 | 右键"下一曲播放"插入临时队列 | `PlaylistManager.EnqueueNext(index)`，`GetNextIndex()` 优先返回队列项 |
| 定时关闭 | 设定 N 分钟后自动停止播放 | `Timer` 倒计时，结束调 `Stop()`，界面显示剩余时间，支持 15/30/45/60/90/120 分钟 |
| 播放历史 | 记录最近 50 首，可查看和双击回播 | `List<string>` + `HistoryForm` 弹窗 |

### 音量控制

| 功能 | 说明 |
|------|------|
| 音量滑块 | TrackBar 0-100，同时控制 WMP 的 `settings.volume` 和 NAudio 的 `WaveOutEvent.Volume` |
| 音量数值 | 滑块右侧实时显示当前音量值 |

### 歌单管理

| 功能 | 说明 |
|------|------|
| 选择歌曲 | 打开文件对话框，支持多选 mp3/flac/wav/ogg/aac/wma/m4a |
| 扫描文件夹 | 选择目录后递归扫描所有子文件夹中的音频文件 |
| 下一曲播放 | 右键菜单 → 将选中曲目插入播放队列 |
| 删除曲目 | 右键菜单 → 删除选中 |
| 清空列表 | 右键菜单 → 清空列表 |
| 保存歌单 | 导出为 .m3u 格式文件 |
| 加载歌单 | 从 .m3u 文件恢复歌单 |
| 拖拽排序 | 在列表中直接拖动调整曲目顺序 |

### 信息显示

| 功能 | 说明 |
|------|------|
| 当前曲目名 | 顶部粗体显示 |
| 文件信息 | 显示文件大小和格式（如 "5 MB \| .MP3"） |
| 歌单数量 | 右上角显示"共 N 首"，有队列时显示"共 N 首 \| 队列: M" |
| 播放时长 | 进度条右侧实时显示 |

### 系统托盘

| 功能 | 说明 |
|------|------|
| 最小化到托盘 | 窗口最小化时自动隐藏到系统托盘 |
| 托盘菜单 | 右键托盘图标可：显示主窗口、播放/暂停、下一曲、退出 |

### 全局快捷键

| 快捷键 | 功能 |
|--------|------|
| Ctrl + Alt + ← | 上一曲 |
| Ctrl + Alt + → | 下一曲 |
| Ctrl + Alt + Space | 播放/暂停 |
| Ctrl + Alt + ↑ | 音量 +5 |
| Ctrl + Alt + ↓ | 音量 -5 |

### OGG 播放

.ogg 文件在歌单中可直接播放，与 MP3/WAV/FLAC 统一操作体验。NAudio 引擎独立运行，进度条、seek、暂停/继续全程支持。

### 网络串流

通过 LibVLCSharp 支持 RTMP / HLS 等网络流媒体播放（测试按钮）。

### 状态持久化

退出时自动保存到 `%LOCALAPPDATA%/UniPlayer/settings.ini`，启动自动恢复。
保存内容：音量、播放模式、倍速、定时关闭剩余时间、窗口位置。

---

## 项目结构

```
UniPlayer/
├── Form1.cs                        主窗口逻辑（播放控制、拖拽、托盘、设置）
├── Form1.Designer.cs               控件布局和声明
├── Program.cs                      入口点
├── Forms/
│   └── HistoryForm.cs             播放历史查看窗口
├── Services/
│   ├── AudioPlaybackService.cs     WMP 播放核心封装（musicplay、淡入淡出、倍速）
│   ├── PlaylistManager.cs          歌单管理、播放模式、播放队列
│   ├── OggPlayerService.cs         NAudio OGG 播放（含 Pause/Seek/进度）
│   ├── StreamingService.cs         LibVLC 网络串流
│   └── HotkeyManager.cs            全局快捷键注册
├── Lib/                            COM 互操作 DLL
│   ├── AxInterop.WMPLib.dll
│   └── Interop.WMPLib.dll
└── bin/Debug/net472/UniPlayer.exe  编译输出
```

---

## 构建

```bash
cd UniPlayer
dotnet build
```

输出：`bin/Debug/net472/UniPlayer.exe`
