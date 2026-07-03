using NAudio.Vorbis;
using NAudio.Wave;
using System;

namespace UniPlayer.Services
{
    /// <summary>
    /// OGG 播放服务 — 使用 NAudio + NVorbis
    /// </summary>
    public class OggPlayerService
    {
        private WaveOutEvent _outputDevice;
        private VorbisWaveReader _vorbisReader;

        public event Action PlaybackStarted;
        public event Action PlaybackStopped;

        public float Volume
        {
            get => _outputDevice?.Volume ?? 0.5f;
            set
            {
                if (_outputDevice != null)
                    _outputDevice.Volume = Math.Max(0, Math.Min(1, value));
            }
        }

        public bool IsPlaying => _outputDevice != null && _outputDevice.PlaybackState == PlaybackState.Playing;

        public double CurrentPosition
        {
            get
            {
                try { return _vorbisReader?.CurrentTime.TotalSeconds ?? 0; }
                catch { return 0; }
            }
            set
            {
                try { if (_vorbisReader != null) _vorbisReader.CurrentTime = TimeSpan.FromSeconds(value); }
                catch { }
            }
        }

        public double Duration
        {
            get
            {
                try { return _vorbisReader?.TotalTime.TotalSeconds ?? 0; }
                catch { return 0; }
            }
        }

        public void Play(string oggFilePath)
        {
            Stop();

            _vorbisReader = new VorbisWaveReader(oggFilePath);
            _outputDevice = new WaveOutEvent();
            _outputDevice.PlaybackStopped += OnPlaybackStopped;
            _outputDevice.Init(_vorbisReader);
            _outputDevice.Play();

            PlaybackStarted?.Invoke();
        }

        public void Pause()
        {
            _outputDevice?.Pause();
        }

        public void Resume()
        {
            _outputDevice?.Play();
        }

        public void Stop()
        {
            if (_outputDevice != null)
            {
                _outputDevice.PlaybackStopped -= OnPlaybackStopped;
                _outputDevice.Stop();
                _vorbisReader?.Dispose();
                _outputDevice.Dispose();
                _vorbisReader = null;
                _outputDevice = null;
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            _vorbisReader?.Dispose();
            _outputDevice?.Dispose();
            _vorbisReader = null;
            _outputDevice = null;
            PlaybackStopped?.Invoke();
        }
    }
}
