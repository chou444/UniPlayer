using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UniPlayer.Services
{
    /// <summary>
    /// 播放模式
    /// </summary>
    public enum PlayMode
    {
        Sequential,  // 顺序播放
        RepeatOne,   // 单曲循环
        Shuffle      // 随机播放
    }

    /// <summary>
    /// 歌单管理器：管理 localmusiclist 的数据操作
    /// </summary>
    public class PlaylistManager
    {
        private readonly List<string> _musicList = new List<string>();
        private readonly Random _random = new Random();
        private int _currentIndex = -1;

        public PlayMode Mode { get; set; } = PlayMode.Sequential;
        public IReadOnlyList<string> MusicList => _musicList.AsReadOnly();
        public int Count => _musicList.Count;
        public int CurrentIndex
        {
            get => _currentIndex;
            set => _currentIndex = value;
        }

        public event Action PlaylistChanged;

        // ========== 基本操作 ==========

        public void Add(string path)
        {
            _musicList.Add(path);
            PlaylistChanged?.Invoke();
        }

        public void AddRange(IEnumerable<string> paths)
        {
            _musicList.AddRange(paths);
            PlaylistChanged?.Invoke();
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < _musicList.Count)
            {
                _musicList.RemoveAt(index);
                if (_currentIndex >= _musicList.Count)
                    _currentIndex = _musicList.Count - 1;
                CleanQueue();
                PlaylistChanged?.Invoke();
            }
        }

        public void Clear()
        {
            _musicList.Clear();
            _playQueue.Clear();
            _currentIndex = -1;
            PlaylistChanged?.Invoke();
        }

        public void Move(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= _musicList.Count) return;
            if (toIndex < 0 || toIndex >= _musicList.Count) return;
            if (fromIndex == toIndex) return;

            string item = _musicList[fromIndex];
            _musicList.RemoveAt(fromIndex);
            _musicList.Insert(toIndex, item);

            if (_currentIndex == fromIndex)
                _currentIndex = toIndex;
            else if (_currentIndex > fromIndex && _currentIndex <= toIndex)
                _currentIndex--;
            else if (_currentIndex < fromIndex && _currentIndex >= toIndex)
                _currentIndex++;

            PlaylistChanged?.Invoke();
        }

        public string GetAt(int index)
        {
            if (index >= 0 && index < _musicList.Count)
                return _musicList[index];
            return null;
        }

        // ========== 播放队列 ==========

        private readonly List<int> _playQueue = new List<int>();

        public IReadOnlyList<int> PlayQueue => _playQueue.AsReadOnly();
        public int PlayQueueCount => _playQueue.Count;
        public bool HasQueuedNext => _playQueue.Count > 0;

        /// <summary>
        /// 将指定索引加入播放队列（下一曲播放）
        /// </summary>
        public void EnqueueNext(int index)
        {
            if (index >= 0 && index < _musicList.Count && !_playQueue.Contains(index))
            {
                _playQueue.Add(index);
            }
        }

        /// <summary>
        /// 取出队列首项索引，如果没有则返回 -1
        /// </summary>
        public int DequeueNext()
        {
            if (_playQueue.Count == 0) return -1;
            int idx = _playQueue[0];
            _playQueue.RemoveAt(0);
            return idx;
        }

        /// <summary>
        /// 清空播放队列
        /// </summary>
        public void ClearQueue()
        {
            _playQueue.Clear();
        }

        /// <summary>
        /// 清理队列中失效的索引（列表项被删除后调用）
        /// </summary>
        private void CleanQueue()
        {
            _playQueue.RemoveAll(i => i < 0 || i >= _musicList.Count);
        }

        // ========== 播放顺序 ==========

        public int GetNextIndex(int currentIndex)
        {
            // 播放队列优先
            if (_playQueue.Count > 0)
            {
                return DequeueNext();
            }

            if (_musicList.Count == 0) return -1;

            switch (Mode)
            {
                case PlayMode.RepeatOne:
                    return currentIndex >= 0 && currentIndex < _musicList.Count ? currentIndex : 0;

                case PlayMode.Shuffle:
                    if (_musicList.Count == 1) return 0;
                    int next;
                    do
                    {
                        next = _random.Next(_musicList.Count);
                    } while (next == currentIndex && _musicList.Count > 1);
                    return next;

                case PlayMode.Sequential:
                default:
                    int idx = currentIndex + 1;
                    if (idx >= _musicList.Count) idx = 0;
                    return idx;
            }
        }

        public int GetPreviousIndex(int currentIndex)
        {
            if (_musicList.Count == 0) return -1;

            switch (Mode)
            {
                case PlayMode.RepeatOne:
                    return currentIndex >= 0 && currentIndex < _musicList.Count ? currentIndex : 0;

                case PlayMode.Shuffle:
                    return GetNextIndex(currentIndex); // 随机模式下一曲 = 上一曲

                case PlayMode.Sequential:
                default:
                    int idx = currentIndex - 1;
                    if (idx < 0) idx = _musicList.Count - 1;
                    return idx;
            }
        }

        // ========== M3U 读写 ==========

        public void SaveM3u(string path)
        {
            var lines = _musicList.Select(f => $"#EXTINF:-1,{Path.GetFileNameWithoutExtension(f)}{Environment.NewLine}{f}");
            File.WriteAllLines(path, new[] { "#EXTM3U" }.Concat(lines));
        }

        public void LoadM3u(string path)
        {
            if (!File.Exists(path)) return;

            _musicList.Clear();
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                if (File.Exists(line))
                {
                    _musicList.Add(line);
                }
            }

            _currentIndex = -1;
            PlaylistChanged?.Invoke();
        }

        // ========== 扫描目录 ==========

        public static string[] ScanDirectory(string dirPath)
        {
            string[] extensions = { "*.mp3", "*.flac", "*.wav", "*.ogg", "*.aac", "*.wma", "*.m4a" };
            var files = new List<string>();
            foreach (string ext in extensions)
            {
                try { files.AddRange(Directory.GetFiles(dirPath, ext, SearchOption.AllDirectories)); }
                catch { }
            }
            return files.ToArray();
        }

        /// <summary>
        /// 获取播放模式的中文名称
        /// </summary>
        public string GetModeDisplayText()
        {
            return Mode switch
            {
                PlayMode.Sequential => "顺序播放",
                PlayMode.RepeatOne => "单曲循环",
                PlayMode.Shuffle => "随机播放",
                _ => "顺序播放"
            };
        }

        /// <summary>
        /// 切换到下一个播放模式
        /// </summary>
        public PlayMode NextMode()
        {
            Mode = Mode switch
            {
                PlayMode.Sequential => PlayMode.RepeatOne,
                PlayMode.RepeatOne => PlayMode.Shuffle,
                PlayMode.Shuffle => PlayMode.Sequential,
                _ => PlayMode.Sequential
            };
            return Mode;
        }
    }
}
