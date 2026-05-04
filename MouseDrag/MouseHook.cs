using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MouseDrag;

public enum MouseButton
{
    Left,
    Right,
    Middle,
    XButton1,
    XButton2
}

public class MouseHook : IDisposable
{
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_RBUTTONUP = 0x0205;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_MBUTTONUP = 0x0208;
    private const int WM_XBUTTONDOWN = 0x020B;
    private const int WM_XBUTTONUP = 0x020C;
    private const int WM_MOUSEMOVE = 0x0200;
    private const int XBUTTON1 = 0x0001;
    private const int XBUTTON2 = 0x0002;

    private delegate IntPtr LowMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    private IntPtr _hookId = IntPtr.Zero;
    private LowMouseProc? _proc;
    private readonly HashSet<MouseButton> _pressedButtons = new();
    private HashSet<MouseButton> _requiredButtons = new();
    private System.Threading.Timer? _blockTimer;

    public event Action? DragStart;
    public event Action? DragEnd;
    public event Action<int, int>? DragMove;

    public bool Enabled { get; set; } = true;

    public void SetRequiredButtons(IEnumerable<MouseButton> buttons)
    {
        _requiredButtons = new HashSet<MouseButton>(buttons);
    }

    private void ResetBlocking()
    {
        _blocking = false;
        _blockTimer?.Dispose();
        _blockTimer = null;
    }

    public void Install()
    {
        if (_hookId != IntPtr.Zero) return;
        _proc = HookCallback;
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule!;
        _hookId = SetWindowsHookEx(WH_MOUSE_LL, _proc, GetModuleHandle(module.ModuleName), 0);
    }

    public void Uninstall()
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }

    private bool _dragging;
    private bool _blocking;

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && Enabled)
        {
            var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            int msg = (int)wParam;

            MouseButton? button = msg switch
            {
                WM_LBUTTONDOWN => MouseButton.Left,
                WM_LBUTTONUP => MouseButton.Left,
                WM_RBUTTONDOWN => MouseButton.Right,
                WM_RBUTTONUP => MouseButton.Right,
                WM_MBUTTONDOWN => MouseButton.Middle,
                WM_MBUTTONUP => MouseButton.Middle,
                WM_XBUTTONDOWN when (hookStruct.mouseData >> 16) == XBUTTON1 => MouseButton.XButton1,
                WM_XBUTTONUP when (hookStruct.mouseData >> 16) == XBUTTON1 => MouseButton.XButton1,
                WM_XBUTTONDOWN when (hookStruct.mouseData >> 16) == XBUTTON2 => MouseButton.XButton2,
                WM_XBUTTONUP when (hookStruct.mouseData >> 16) == XBUTTON2 => MouseButton.XButton2,
                _ => null
            };

            bool isDown = button.HasValue && msg is WM_LBUTTONDOWN or WM_RBUTTONDOWN or WM_MBUTTONDOWN or WM_XBUTTONDOWN;
            bool isUp = button.HasValue && !isDown;
            bool isComboButton = button.HasValue && _requiredButtons.Contains(button.Value);

            // When we have all but one required button, start blocking
            // to prevent system menus/selection from triggering
            if (isComboButton && !_blocking && !_dragging)
            {
                int matchCount = _requiredButtons.Count(b => _pressedButtons.Contains(b));
                if (matchCount >= _requiredButtons.Count - 1)
                {
                    _blocking = true;
                    // Safety: auto-reset blocking after 2s if combo doesn't complete
                    _blockTimer?.Dispose();
                    _blockTimer = new System.Threading.Timer(_ => ResetBlocking(), null, 2000, System.Threading.Timeout.Infinite);
                }
            }

            // Update internal state
            if (button.HasValue && isDown)
            {
                _pressedButtons.Add(button.Value);
                if (!_dragging && _requiredButtons.Count > 0 && _requiredButtons.IsSubsetOf(_pressedButtons))
                {
                    _dragging = true;
                    DragStart?.Invoke();
                }
            }
            else if (button.HasValue && isUp)
            {
                _pressedButtons.Remove(button.Value);
                if (_dragging)
                {
                    _dragging = false;
                    DragEnd?.Invoke();
                }
                if (_blocking && !_dragging)
                {
                    ResetBlocking();
                }
            }
            else if (msg == WM_MOUSEMOVE && _dragging)
            {
                DragMove?.Invoke(hookStruct.pt.x, hookStruct.pt.y);
            }

            // Block combo button events to prevent system behavior
            if ((_dragging || _blocking) && isComboButton)
            {
                return (IntPtr)1;
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        _blockTimer?.Dispose();
        Uninstall();
    }
}
