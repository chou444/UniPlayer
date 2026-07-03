using LibVLCSharp.Shared;
using System;

namespace UniPlayer.Services
{
    /// <summary>
    /// 网络串流播放服务 — 使用 LibVLC 播放 RTMP/HLS 等网络流
    /// </summary>
    public class StreamingService
    {
        private readonly LibVLC _libVlc;
        private readonly MediaPlayer _mediaPlayer;

        public MediaPlayer MediaPlayer => _mediaPlayer;
        public LibVLC LibVlc => _libVlc;

        public event Action Playing;
        public event Action Stopped;
        public event Action<string> Error;

        public StreamingService(LibVLC libVlc, MediaPlayer mediaPlayer)
        {
            _libVlc = libVlc;
            _mediaPlayer = mediaPlayer;

            _mediaPlayer.Playing += (s, e) => Playing?.Invoke();
            _mediaPlayer.Stopped += (s, e) => Stopped?.Invoke();
            _mediaPlayer.EncounteredError += (s, e) => Error?.Invoke("播放错误");
        }

        public void PlayStream(Uri uri, string networkCaching = "300")
        {
            _mediaPlayer.Stop();
            var media = new Media(_libVlc, uri);
            media.AddOption($":network-caching={networkCaching}");
            _mediaPlayer.Play(media);
        }

        public void Stop()
        {
            _mediaPlayer.Stop();
        }
    }
}
