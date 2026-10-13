using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace SnapLine.Services
{
    public class HotKeyManager : IDisposable
    {
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private const int VK_SNAPSHOT = 0x2C; // PrintScreen (Impr Pant)
        private const int VK_S = 0x53;        // Tecla 'S'

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookId = IntPtr.Zero;
        private long _lastSnapshotTick = 0;

        public event Action HotKeyPressed;

        public void Start()
        {
            if (_hookId != IntPtr.Zero) return;

            _proc = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                bool isKeyDown = (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN);
                bool isKeyUp = (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP);

                // 1. Interceptar PrintScreen tanto en WM_KEYUP como en WM_KEYDOWN
                if (vkCode == VK_SNAPSHOT)
                {
                    // En Windows, VK_SNAPSHOT envia tipicamente solo WM_KEYUP (o a veces ambos).
                    // Disparamos con debounce para responder de inmediato sin duplicar.
                    long now = Environment.TickCount;
                    if (now - _lastSnapshotTick > 350)
                    {
                        _lastSnapshotTick = now;
                        var handler = HotKeyPressed;
                        if (handler != null)
                        {
                            ThreadPool.QueueUserWorkItem(delegate { handler(); });
                        }
                    }
                    // Consumir el evento para que Windows no abra la herramienta de recortes
                    return (IntPtr)1;
                }

                // 2. Interceptar Ctrl + Shift + S
                if (vkCode == VK_S && isKeyDown)
                {
                    bool ctrl = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                    bool shift = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
                    if (ctrl && shift)
                    {
                        long now = Environment.TickCount;
                        if (now - _lastSnapshotTick > 350)
                        {
                            _lastSnapshotTick = now;
                            var handler = HotKeyPressed;
                            if (handler != null)
                            {
                                ThreadPool.QueueUserWorkItem(delegate { handler(); });
                            }
                        }
                        return (IntPtr)1;
                    }
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Stop()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
