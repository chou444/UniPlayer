using AxWMPLib;
using System;
using System.Windows.Forms;

namespace UniPlayer.Services
{
    /// <summary>
    /// 音频播放服务：包装 axWindowsMediaPlayer1 的播放操作
    /// 核心 musicplay 方法原封保留
    /// </summary>
    public class AudioPlaybackService
    {
        private readonly AxWindowsMediaPlayer _wmp;
        private readonly Timer _fadeTimer = new Timer { Interval = 30 };
        private int _preFadeVolume = 50;
        private bool _isFading = false;

        public event Action<string> PlaybackStarted;
        public event Action PlaybackStopped;

        public AudioPlaybackService(AxWindowsMediaPlayer wmp)
        {
            _wmp = wmp;

            // 监听播放状态变化 (自动下一曲)
            _wmp.PlayStateChange += OnPlayStateChange;
            _fadeTimer.Tick += FadeTimer_Tick;
        }

        // ========== 淡入淡出 ==========

        public bool IsFading => _isFading;

        /// <summary>
        /// 淡出：音量逐步降到 0，完成后调用回调
        /// </summary>
        public void FadeOut(Action onComplete)
        {
            if (_isFading) return;
            _isFading = true;
            _preFadeVolume = GetVolume();
            _fadeTag = onComplete;
            _fadeDirection = -1; // 下降
            _fadeTimer.Start();
        }

        /// <summary>
        /// 淡入：音量从当前逐步恢复到淡出前的值
        /// （必须在 FadeOut 之后调用，_preFadeVolume 由 FadeOut 保存）
        /// </summary>
        public void FadeIn()
        {
            if (_isFading) return;
            _isFading = true;
            // 不覆盖 _preFadeVolume — FadeOut 已经保存了要恢复的音量
            _fadeTag = null;
            _fadeDirection = 1; // 上升
            _fadeTimer.Start();
        }

        private Action _fadeTag;
        private int _fadeDirection; // -1: fade out, 1: fade in

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            int current = GetVolume();
            int next = current + (_fadeDirection * 4);

            if (_fadeDirection < 0 && next <= 0)
            {
                // 淡出完成
                SetVolume(0);
                _fadeTimer.Stop();
                _isFading = false;
                _fadeTag?.Invoke();
            }
            else if (_fadeDirection > 0 && next >= _preFadeVolume)
            {
                // 淡入完成
                SetVolume(_preFadeVolume);
                _fadeTimer.Stop();
                _isFading = false;
            }
            else
            {
                SetVolume(next);
            }
        }

        // ========== 倍速播放 ==========

        public float Rate
        {
            get
            {
                try { return (float)_wmp.settings.rate; }
                catch { return 1.0f; }
            }
            set
            {
                try { _wmp.settings.rate = (double)Math.Max(0.5f, Math.Min(2.0f, value)); }
                catch { }
            }
        }

        // ========== 原始核心方法 ==========

        /// <summary>
        /// 原始核心播放方法 — 代码原封不动
        /// </summary>
        private void musicplay(string filename)
        {
            _wmp.URL = filename;
            string extension = System.IO.Path.GetExtension(filename);

            if (extension == ".ogg") { Console.WriteLine("这是ogg文件。"); }
            else { _wmp.Ctlcontrols.play(); }
        }

        public void Play(string path)
        {
            musicplay(path);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            PlaybackStarted?.Invoke(name);
        }

        public void Stop()
        {
            _fadeTimer.Stop();
            _isFading = false;
            _wmp.Ctlcontrols.stop();
            PlaybackStopped?.Invoke();
        }

        public void Pause()
        {
            _wmp.Ctlcontrols.pause();
        }

        public void Resume()
        {
            _wmp.Ctlcontrols.play();
        }

        public bool IsPlaying
        {
            get
            {
                try { return _wmp.playState == WMPLib.WMPPlayState.wmppsPlaying; }
                catch { return false; }
            }
        }

        public double CurrentPosition
        {
            get
            {
                try { return _wmp.Ctlcontrols.currentPosition; }
                catch { return 0; }
            }
            set
            {
                try { _wmp.Ctlcontrols.currentPosition = value; }
                catch { }
            }
        }

        public double Duration
        {
            get
            {
                try { return _wmp.currentMedia.duration; }
                catch { return 0; }
            }
        }

        public void SetVolume(int volume)
        {
            try { _wmp.settings.volume = Math.Max(0, Math.Min(100, volume)); }
            catch { }
        }

        public int GetVolume()
        {
            try { return _wmp.settings.volume; }
            catch { return 50; }
        }

        private void OnPlayStateChange(object sender, _WMPOCXEvents_PlayStateChangeEvent e)
        {
            WMPLib.WMPPlayState state = (WMPLib.WMPPlayState)e.newState;

            if (state == WMPLib.WMPPlayState.wmppsStopped)
            {
                PlaybackStopped?.Invoke();
            }
        }
    }
}
