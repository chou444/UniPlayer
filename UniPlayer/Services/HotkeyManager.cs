using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UniPlayer.Services
{
    /// <summary>
    /// 全局快捷键管理 — 使用 User32.RegisterHotKey
    /// </summary>
    public class HotkeyManager : IDisposable
    {
        // ========== Win32 API ==========
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int WM_HOTKEY = 0x0312;

        private readonly IntPtr _hwnd;
        private readonly Dictionary<int, Action> _hotkeys = new Dictionary<int, Action>();
        private int _nextId = 1;

        public HotkeyManager(IntPtr handle)
        {
            _hwnd = handle;
        }

        /// <summary>
        /// 注册全局快捷键
        /// </summary>
        public bool Register(int id, uint modifiers, Keys key, Action action)
        {
            if (RegisterHotKey(_hwnd, id, modifiers, (uint)key))
            {
                _hotkeys[id] = action;
                return true;
            }
            return false;
        }

        public int Register(uint modifiers, Keys key, Action action)
        {
            int id = _nextId++;
            if (Register(id, modifiers, key, action))
                return id;
            return 0;
        }

        public void Unregister(int id)
        {
            UnregisterHotKey(_hwnd, id);
            _hotkeys.Remove(id);
        }

        /// <summary>
        /// 处理消息 — 在 Form.WndProc 中调用
        /// </summary>
        public bool ProcessHotkey(Message msg)
        {
            if (msg.Msg == WM_HOTKEY)
            {
                int id = msg.WParam.ToInt32();
                if (_hotkeys.TryGetValue(id, out Action action))
                {
                    action();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 注册默认快捷键集
        /// </summary>
        public void RegisterDefaults(Action next, Action prev, Action playPause, Action volumeUp, Action volumeDown)
        {
            const uint MOD_CONTROL_ALT = 0x0002 | 0x0001; // MOD_ALT | MOD_CONTROL

            Register(MOD_CONTROL_ALT, Keys.Right, next);
            Register(MOD_CONTROL_ALT, Keys.Left, prev);
            Register(MOD_CONTROL_ALT, Keys.Space, playPause);
            Register(MOD_CONTROL_ALT, Keys.Up, volumeUp);
            Register(MOD_CONTROL_ALT, Keys.Down, volumeDown);
        }

        public void Dispose()
        {
            foreach (int id in new List<int>(_hotkeys.Keys))
            {
                Unregister(id);
            }
            _hotkeys.Clear();
        }
    }
}
