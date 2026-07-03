using LibVLCSharp.Shared;
using UniPlayer.Services;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace UniPlayer
{
    public partial class Form1 : Form
    {
        // ========== 服务层 ==========
        private readonly AudioPlaybackService _audioService;
        private readonly PlaylistManager _playlistManager;
        private readonly OggPlayerService _oggPlayer;
        private readonly StreamingService _streamingService;
        private readonly LibVLC _libVlc;

        // ========== 核心数据（原始结构保留） ==========
        private System.Collections.Generic.List<string> localmusiclist =>
            new System.Collections.Generic.List<string>(_playlistManager.MusicList);

        // ========== 内部状态 ==========
        private bool _isPaused = false;
        private bool _isPlayingOgg = false;
        private bool _isDraggingSeek = false;
        private bool _isUpdatingSelection = false;
        private HotkeyManager _hotkeyManager;

        // ========== 播放历史 ==========
        private readonly System.Collections.Generic.List<string> _playHistory = new System.Collections.Generic.List<string>();
        private Forms.HistoryForm _historyForm;

        // ========== 定时关闭 ==========
        private readonly Timer _sleepTimer = new Timer { Interval = 1000 };
        private int _sleepSecondsRemaining = 0;

        // ========== 倍速 ==========
        private readonly float[] _speedOptions = { 0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 2.0f };
        private int _speedIndex = 2; // 默认 1.0x

        public Form1()
        {
            Core.Initialize();
            InitializeComponent();

            // ---- 初始化右键菜单（避免 linter 清空） ----
            contextMenuPlaylist.Items.Clear();
            contextMenuPlaylist.Items.Add("删除选中");
            contextMenuPlaylist.Items.Add("下一曲播放");
            contextMenuPlaylist.Items.Add("清空列表");
            contextMenuPlaylist.Items.Add(new ToolStripSeparator());
            contextMenuPlaylist.Items.Add("保存歌单");
            contextMenuPlaylist.Items.Add("加载歌单");
            contextMenuPlaylist.ItemClicked += contextMenuPlaylist_ItemClicked;

            // ---- 初始化 LibVLC ----
            _libVlc = new LibVLC("--verbose=2", "--network-caching=300", "--avcodec-hw=d3d11va");
            _mediaPlayer = new MediaPlayer(_libVlc);
            videoView1.MediaPlayer = _mediaPlayer;

            // ---- 初始化服务 ----
            _audioService = new AudioPlaybackService(axWindowsMediaPlayer1);
            _playlistManager = new PlaylistManager();
            _oggPlayer = new OggPlayerService();
            _streamingService = new StreamingService(_libVlc, _mediaPlayer);

            // ---- 订阅事件 ----
            _audioService.PlaybackStarted += OnPlaybackStarted;
            _audioService.PlaybackStopped += OnPlaybackStopped;
            _playlistManager.PlaylistChanged += OnPlaylistChanged;

            // ---- WMP 状态变化（自动下一曲） ----
            axWindowsMediaPlayer1.PlayStateChange += OnWmpPlayStateChange;

            // ---- OGG 播放结束（自动下一曲，来自 NAudio 线程需 BeginInvoke） ----
            _oggPlayer.PlaybackStopped += () =>
            {
                if (_isPlayingOgg)
                    BeginInvoke(new Action(PlayNext));
            };

            // ---- 进度更新定时器 ----
            timerProgress.Interval = 500;
            timerProgress.Tick += TimerProgress_Tick;

            // ---- 音量初始化 ----
            trackVolume.Value = _audioService.GetVolume();
            _oggPlayer.Volume = trackVolume.Value / 100f;

            // ---- 拖拽事件 ----
            listBox1.MouseDown += ListBox1_MouseDown;
            listBox1.MouseMove += ListBox1_MouseMove;
            listBox1.MouseUp += ListBox1_MouseUp;
            seekBar.Scroll += SeekBar_Scroll;
            seekBar.MouseDown += (s, e) => _isDraggingSeek = true;
            seekBar.MouseUp += (s, e) => { _isDraggingSeek = false; ApplySeek(); };

            // ---- 定时关闭 ----
            _sleepTimer.Tick += SleepTimer_Tick;

            // ---- 加载保存的设置 ----
            LoadSettings();

            // ---- 全局快捷键 ----
            _hotkeyManager = new HotkeyManager(Handle);
            _hotkeyManager.RegisterDefaults(
                next: () => Invoke(new Action(PlayNext)),
                prev: () => Invoke(new Action(PlayPrevious)),
                playPause: () => Invoke(new Action(() => btnPlayPause_Click(null, EventArgs.Empty))),
                volumeUp: () => Invoke(new Action(() =>
                {
                    trackVolume.Value = Math.Min(100, trackVolume.Value + 5);
                    trackVolume_Scroll(null, EventArgs.Empty);
                })),
                volumeDown: () => Invoke(new Action(() =>
                {
                    trackVolume.Value = Math.Max(0, trackVolume.Value - 5);
                    trackVolume_Scroll(null, EventArgs.Empty);
                }))
            );
        }

        private void Form1_Load(object sender, EventArgs e) { }

        // ======================================================================
        //  播放事件
        // ======================================================================

        private void OnPlaybackStarted(string songName)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(OnPlaybackStarted), songName);
                return;
            }
            lblSongName.Text = songName;
            _isPaused = false;
            btnPlayPause.Text = "暂停";
            timerProgress.Start();
            UpdateSongInfo();
        }

        private void OnPlaybackStopped()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OnPlaybackStopped));
                return;
            }
            _isPlayingOgg = false;
            btnPlayPause.Text = "播放";
            timerProgress.Stop();
            seekBar.Value = 0;
            lblTimeInfo.Text = "00:00 / 00:00";
        }

        private void OnPlaylistChanged()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OnPlaylistChanged));
                return;
            }
            listBox1.Items.Clear();
            foreach (string path in _playlistManager.MusicList)
            {
                listBox1.Items.Add(Path.GetFileNameWithoutExtension(path));
            }
            UpdateSongCountLabel();
        }

        private void OnWmpPlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (_isPlayingOgg) return; // OGG 不走 WMP 事件
            var state = (WMPLib.WMPPlayState)e.newState;
            if (state == WMPLib.WMPPlayState.wmppsMediaEnded)
            {
                // 自动下一曲 — 用 BeginInvoke 延迟执行，避免在 WMP 事件中直接切换 URL
                BeginInvoke(new Action(PlayNext));
            }
        }

        // ======================================================================
        //  进度条
        // ======================================================================

        private void TimerProgress_Tick(object sender, EventArgs e)
        {
            if (_isDraggingSeek) return;
            double dur, pos;
            if (_isPlayingOgg)
            {
                dur = _oggPlayer.Duration;
                pos = _oggPlayer.CurrentPosition;
            }
            else
            {
                dur = _audioService.Duration;
                pos = _audioService.CurrentPosition;
            }
            if (dur > 0)
            {
                seekBar.Maximum = (int)dur;
                seekBar.Value = Math.Min((int)pos, (int)dur);
                lblTimeInfo.Text = $"{FormatTime(pos)} / {FormatTime(dur)}";
            }
        }

        private void SeekBar_Scroll(object sender, EventArgs e)
        {
            double dur = _isPlayingOgg ? _oggPlayer.Duration : _audioService.Duration;
            double pos = seekBar.Value;
            lblTimeInfo.Text = $"{FormatTime(pos)} / {FormatTime(dur)}";
        }

        private void ApplySeek()
        {
            if (_isPlayingOgg)
                _oggPlayer.CurrentPosition = seekBar.Value;
            else
                _audioService.CurrentPosition = seekBar.Value;
        }

        private string FormatTime(double seconds)
        {
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            return ts.Hours > 0
                ? $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                : $"{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        // ======================================================================
        //  歌曲信息更新
        // ======================================================================

        private void UpdateSongInfo()
        {
            try
            {
                string path = _playlistManager.GetAt(_playlistManager.CurrentIndex);
                if (path != null)
                {
                    var fileInfo = new FileInfo(path);
                    lblFileInfo.Text = $"{fileInfo.Length / 1024 / 1024} MB | {Path.GetExtension(path).ToUpper()}";

                    double dur = _isPlayingOgg ? _oggPlayer.Duration : _audioService.Duration;
                    lblTimeInfo.Text = $"00:00 / {FormatTime(dur)}";
                }
            }
            catch { }
        }

        private void UpdateSongCountLabel()
        {
            string text = $"共 {_playlistManager.Count} 首";
            if (_playlistManager.PlayQueueCount > 0)
                text += $" | 队列: {_playlistManager.PlayQueueCount}";
            lblSongCount.Text = text;
        }

        // ======================================================================
        //  播放控制
        // ======================================================================

        private void RecordHistory(string path)
        {
            _playHistory.Add(path);
            if (_playHistory.Count > 50) _playHistory.RemoveAt(0);
        }

        private void PlayByIndex(int index, bool withFade = false)
        {
            if (index < 0 || index >= _playlistManager.Count) return;
            _playlistManager.CurrentIndex = index;
            string path = _playlistManager.GetAt(index);
            if (path != null)
            {
                bool isOgg = path.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase);

                if (isOgg)
                {
                    // OGG：走 NAudio，不能淡入淡出
                    _audioService.Stop();
                    _oggPlayer.Play(path);
                    _isPlayingOgg = true;
                    _isPaused = false;
                    btnPlayPause.Text = "暂停";
                    timerProgress.Start();
                    string name = Path.GetFileNameWithoutExtension(path);
                    lblSongName.Text = name;
                    RecordHistory(path);
                    HighlightCurrentTrack(index);
                }
                else
                {
                    _oggPlayer.Stop();
                    _isPlayingOgg = false;
                    _isUpdatingSelection = true;

                    if (withFade && _audioService.IsPlaying && !_audioService.IsFading)
                    {
                        _audioService.FadeOut(() =>
                        {
                            _audioService.Play(path);
                            _audioService.FadeIn();
                            RecordHistory(path);
                            HighlightCurrentTrack(index);
                            _isUpdatingSelection = false;
                        });
                    }
                    else
                    {
                        _audioService.Play(path);
                        RecordHistory(path);
                        HighlightCurrentTrack(index);
                        _isUpdatingSelection = false;
                    }
                }
            }
        }

        private void PlayNext()
        {
            int next = _playlistManager.GetNextIndex(_playlistManager.CurrentIndex);
            if (next >= 0) PlayByIndex(next, withFade: true);
        }

        private void PlayPrevious()
        {
            int prev = _playlistManager.GetPreviousIndex(_playlistManager.CurrentIndex);
            if (prev >= 0) PlayByIndex(prev, withFade: true);
        }

        private void HighlightCurrentTrack(int index)
        {
            if (index >= 0 && index < listBox1.Items.Count)
            {
                listBox1.SelectedIndex = index;
            }
        }

        // ======================================================================
        //  控件事件
        // ======================================================================

        // 选择歌曲并播放
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingSelection) return;

            if (_playlistManager.Count > 0 && listBox1.SelectedIndex >= 0)
            {
                string path = _playlistManager.GetAt(listBox1.SelectedIndex);
                if (path != null)
                {
                    bool isOgg = path.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase);

                    if (isOgg)
                    {
                        // OGG：走 NAudio
                        _audioService.Stop();
                        _oggPlayer.Play(path);
                        _isPlayingOgg = true;
                        _isPaused = false;
                        btnPlayPause.Text = "暂停";
                        timerProgress.Start();
                        string name = Path.GetFileNameWithoutExtension(path);
                        lblSongName.Text = name;
                        _playlistManager.CurrentIndex = listBox1.SelectedIndex;
                        RecordHistory(path);
                    }
                    else
                    {
                        // 非 OGG：走 WMP（原始核心逻辑保留）
                        _oggPlayer.Stop();
                        _isPlayingOgg = false;

                        if (_audioService.IsPlaying && !_audioService.IsFading)
                        {
                            _isUpdatingSelection = true;
                            _audioService.FadeOut(() =>
                            {
                                axWindowsMediaPlayer1.URL = path;
                                _audioService.Play(path);
                                _audioService.FadeIn();
                                _playlistManager.CurrentIndex = listBox1.SelectedIndex;
                                RecordHistory(path);
                                _isUpdatingSelection = false;
                            });
                        }
                        else
                        {
                            axWindowsMediaPlayer1.URL = path;
                            _audioService.Play(path);
                            _playlistManager.CurrentIndex = listBox1.SelectedIndex;
                            RecordHistory(path);
                        }
                    }
                }
            }
        }

        // 下一曲
        private void btnNext_Click(object sender, EventArgs e)
        {
            PlayNext();
        }

        // 上一曲
        private void btnPrev_Click(object sender, EventArgs e)
        {
            PlayPrevious();
        }

        // 播放/暂停切换
        private void btnPlayPause_Click(object sender, EventArgs e)
        {
            if (_playlistManager.Count == 0) return;

            bool isActive = _isPlayingOgg ? _oggPlayer.IsPlaying : _audioService.IsPlaying;

            if (isActive)
            {
                if (_isPlayingOgg) _oggPlayer.Pause(); else _audioService.Pause();
                _isPaused = true;
                btnPlayPause.Text = "播放";
            }
            else if (_isPaused)
            {
                if (_isPlayingOgg) _oggPlayer.Resume(); else _audioService.Resume();
                _isPaused = false;
                btnPlayPause.Text = "暂停";
            }
            else
            {
                // 未开始播放，播放当前选中或第一首
                int idx = _playlistManager.CurrentIndex >= 0 ? _playlistManager.CurrentIndex : 0;
                PlayByIndex(idx);
            }
        }

        // 选择歌曲文件
        private void btnOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "音频文件|*.mp3;*.flac;*.wav;*.ogg;*.aac;*.wma;*.m4a";
            openFileDialog1.Multiselect = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _playlistManager.Clear();
                _playlistManager.AddRange(openFileDialog1.FileNames);
            }
        }

        // 打开文件夹
        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择音乐文件夹";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string[] files = PlaylistManager.ScanDirectory(dialog.SelectedPath);
                    if (files.Length > 0)
                    {
                        _playlistManager.Clear();
                        _playlistManager.AddRange(files);
                    }
                    else
                    {
                        MessageBox.Show("该文件夹中没有找到音频文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        // 停止
        private void btnStop_Click(object sender, EventArgs e)
        {
            _audioService.Stop();
            _oggPlayer.Stop();
            _isPlayingOgg = false;
            _isPaused = false;
            btnPlayPause.Text = "播放";
            CancelSleepTimer();
        }

        // 播放模式切换
        private void btnPlayMode_Click(object sender, EventArgs e)
        {
            _playlistManager.NextMode();
            lblPlayMode.Text = _playlistManager.GetModeDisplayText();
        }

        // 音量
        private void trackVolume_Scroll(object sender, EventArgs e)
        {
            _audioService.SetVolume(trackVolume.Value);
            _oggPlayer.Volume = trackVolume.Value / 100f;
            lblVolume.Text = trackVolume.Value.ToString();
        }

        // ======================================================================
        //  倍速播放
        // ======================================================================

        private void lblSpeed_Click(object sender, EventArgs e)
        {
            _speedIndex = (_speedIndex + 1) % _speedOptions.Length;
            float rate = _speedOptions[_speedIndex];
            _audioService.Rate = rate;
            lblSpeed.Text = rate.ToString("0.##") + "x";
        }

        // ======================================================================
        //  定时关闭
        // ======================================================================

        private void btnSleepTimer_Click(object sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("15 分钟", null, (s, ev) => StartSleepTimer(15));
            menu.Items.Add("30 分钟", null, (s, ev) => StartSleepTimer(30));
            menu.Items.Add("45 分钟", null, (s, ev) => StartSleepTimer(45));
            menu.Items.Add("60 分钟", null, (s, ev) => StartSleepTimer(60));
            menu.Items.Add("90 分钟", null, (s, ev) => StartSleepTimer(90));
            menu.Items.Add("120 分钟", null, (s, ev) => StartSleepTimer(120));
            menu.Items.Add("-");
            menu.Items.Add("取消定时", null, (s, ev) => CancelSleepTimer());
            menu.Show(btnSleepTimer, new Point(0, btnSleepTimer.Height));
        }

        private void StartSleepTimer(int minutes)
        {
            _sleepSecondsRemaining = minutes * 60;
            _sleepTimer.Start();
            int min = _sleepSecondsRemaining / 60;
            int sec = _sleepSecondsRemaining % 60;
            lblSleepTimer.Text = $"剩余 {min:D2}:{sec:D2}";
        }

        private void CancelSleepTimer()
        {
            _sleepTimer.Stop();
            _sleepSecondsRemaining = 0;
            lblSleepTimer.Text = "未设置";
        }

        private void SleepTimer_Tick(object sender, EventArgs e)
        {
            _sleepSecondsRemaining--;
            if (_sleepSecondsRemaining <= 0)
            {
                _sleepTimer.Stop();
                _audioService.Stop();
                _oggPlayer.Stop();
                _isPaused = false;
                btnPlayPause.Text = "播放";
                lblSleepTimer.Text = "已停止";
                MessageBox.Show("定时关闭：播放已停止。", "定时关闭", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                int min = _sleepSecondsRemaining / 60;
                int sec = _sleepSecondsRemaining % 60;
                lblSleepTimer.Text = $"剩余 {min:D2}:{sec:D2}";
            }
        }

        // ======================================================================
        //  播放历史
        // ======================================================================

        private void btnHistory_Click(object sender, EventArgs e)
        {
            if (_historyForm == null || _historyForm.IsDisposed)
            {
                _historyForm = new Forms.HistoryForm(_playHistory, (path) =>
                {
                    for (int i = 0; i < _playlistManager.Count; i++)
                    {
                        if (_playlistManager.GetAt(i) == path)
                        {
                            Invoke(new Action(() => PlayByIndex(i)));
                            break;
                        }
                    }
                });
            }
            _historyForm.RefreshList();
            _historyForm.Show(this);
            _historyForm.BringToFront();
        }

        // ======================================================================
        //  OGG / LibVLC（保留）
        // ======================================================================

        private void btnPlayOgg_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "OGG 音频|*.ogg";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _oggPlayer.Play(dialog.FileName);
                    lblSongName.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
                }
            }
        }

        private void btnLibVlc_Click(object sender, EventArgs e)
        {
            try
            {
                _audioService.Stop();
                _oggPlayer.Stop();
                string path = "rtmp://ns8.indexforce.com/home/mystream";
                _streamingService.PlayStream(new Uri(path));
                lblSongName.Text = "网络串流播放中...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"错误: {ex.Message}");
            }
        }

        // ======================================================================
        //  ListBox 拖拽排序
        // ======================================================================

        private int _dragFromIndex = -1;

        private void ListBox1_MouseDown(object sender, MouseEventArgs e)
        {
            _dragFromIndex = listBox1.IndexFromPoint(e.Location);
        }

        private void ListBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _dragFromIndex >= 0)
            {
                int overIndex = listBox1.IndexFromPoint(e.Location);
                if (overIndex >= 0 && overIndex != _dragFromIndex)
                {
                    _playlistManager.Move(_dragFromIndex, overIndex);
                    _dragFromIndex = overIndex;
                }
            }
        }

        private void ListBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _dragFromIndex = -1;
        }

        // ======================================================================
        //  右键菜单（含播放队列）
        // ======================================================================

        private void contextMenuPlaylist_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "删除选中":
                    if (listBox1.SelectedIndex >= 0)
                    {
                        _playlistManager.RemoveAt(listBox1.SelectedIndex);
                    }
                    break;
                case "下一曲播放":
                    if (listBox1.SelectedIndex >= 0)
                    {
                        _playlistManager.EnqueueNext(listBox1.SelectedIndex);
                        UpdateSongCountLabel();
                    }
                    break;
                case "清空列表":
                    _playlistManager.Clear();
                    break;
                case "保存歌单":
                    SavePlaylist();
                    break;
                case "加载歌单":
                    LoadPlaylist();
                    break;
            }
        }

        private void SavePlaylist()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "M3U 歌单|*.m3u";
                dialog.FileName = "playlist.m3u";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _playlistManager.SaveM3u(dialog.FileName);
                }
            }
        }

        private void LoadPlaylist()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "M3U 歌单|*.m3u";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _playlistManager.LoadM3u(dialog.FileName);
                }
            }
        }

        private void btnSavePlaylist_Click(object sender, EventArgs e)
        {
            SavePlaylist();
        }

        // ======================================================================
        //  系统托盘
        // ======================================================================

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(1000, "UniPlayer", "播放器已最小化到托盘", ToolTipIcon.Info);
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowWindow();
        }

        private void trayMenuShow_Click(object sender, EventArgs e)
        {
            ShowWindow();
        }

        private void trayMenuPlayPause_Click(object sender, EventArgs e)
        {
            btnPlayPause_Click(sender, e);
        }

        private void trayMenuNext_Click(object sender, EventArgs e)
        {
            PlayNext();
        }

        private void trayMenuExit_Click(object sender, EventArgs e)
        {
            SaveSettings();
            notifyIcon.Visible = false;
            Application.Exit();
        }

        private void ShowWindow()
        {
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
            notifyIcon.Visible = false;
        }

        // ======================================================================
        //  设置持久化
        // ======================================================================

        private string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UniPlayer", "settings.ini");

        private void SaveSettings()
        {
            try
            {
                string dir = Path.GetDirectoryName(SettingsPath);
                if (dir != null) Directory.CreateDirectory(dir);
                File.WriteAllLines(SettingsPath, new string[]
                {
                    $"Volume={trackVolume.Value}",
                    $"PlayMode={_playlistManager.Mode}",
                    $"WindowWidth={Width}",
                    $"WindowHeight={Height}",
                    $"WindowLeft={Left}",
                    $"WindowTop={Top}",
                    $"PlaybackRate={_audioService.Rate}",
                    $"SleepMinutes={(_sleepTimer.Enabled ? (_sleepSecondsRemaining + 59) / 60 : 0)}"
                });
            }
            catch { }
        }

        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return;
                foreach (string line in File.ReadAllLines(SettingsPath))
                {
                    int eq = line.IndexOf('=');
                    if (eq < 0) continue;
                    string key = line.Substring(0, eq).Trim();
                    string val = line.Substring(eq + 1).Trim();
                    switch (key)
                    {
                        case "Volume":
                            if (int.TryParse(val, out int vol))
                            {
                                trackVolume.Value = vol;
                                _audioService.SetVolume(vol);
                                _oggPlayer.Volume = vol / 100f;
                                lblVolume.Text = vol.ToString();
                            }
                            break;
                        case "PlayMode":
                            if (Enum.TryParse<PlayMode>(val, out PlayMode parsedMode))
                            {
                                _playlistManager.Mode = parsedMode;
                                lblPlayMode.Text = _playlistManager.GetModeDisplayText();
                            }
                            break;
                        case "PlaybackRate":
                            if (float.TryParse(val, out float rate) && rate >= 0.5f && rate <= 2.0f)
                            {
                                _audioService.Rate = rate;
                                // 找到最接近的预设速度
                                for (int i = 0; i < _speedOptions.Length; i++)
                                {
                                    if (Math.Abs(_speedOptions[i] - rate) < 0.01f)
                                    {
                                        _speedIndex = i;
                                        break;
                                    }
                                }
                                lblSpeed.Text = rate.ToString("0.##") + "x";
                            }
                            break;
                        case "SleepMinutes":
                            if (int.TryParse(val, out int sleepMin) && sleepMin > 0)
                            {
                                StartSleepTimer(sleepMin);
                            }
                            break;
                    }
                }
            }
            catch { }
        }

        // ======================================================================
        //  全局快捷键
        // ======================================================================

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            _hotkeyManager?.ProcessHotkey(m);
        }

        // ======================================================================
        //  窗口关闭
        // ======================================================================

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerProgress.Stop();
            _sleepTimer.Stop();
            SaveSettings();
            _oggPlayer.Stop();
            _audioService.Stop();
            _streamingService.Stop();
            _hotkeyManager?.Dispose();
            _libVlc?.Dispose();
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e) { }
        private void videoView1_Click(object sender, EventArgs e) { }
    }
}
