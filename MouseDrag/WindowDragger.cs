using System.Runtime.InteropServices;

namespace MouseDrag;

public class WindowDragger
{
    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(POINT point);

    [DllImport("user32.dll")]
    private static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    private const uint GA_ROOT = 2;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;

    private IntPtr _targetHwnd;
    private POINT _startCursorPos;
    private RECT _startWindowRect;
    private bool _isDragging;

    public bool IsDragging => _isDragging;

    public void BeginDrag(int cursorX, int cursorY)
    {
        var point = new POINT { x = cursorX, y = cursorY };
        IntPtr hwnd = WindowFromPoint(point);
        if (hwnd == IntPtr.Zero) return;

        // Get the top-level window
        IntPtr rootHwnd = GetAncestor(hwnd, GA_ROOT);
        if (rootHwnd == IntPtr.Zero) rootHwnd = hwnd;

        if (!GetWindowRect(rootHwnd, out var rect)) return;

        _targetHwnd = rootHwnd;
        _startCursorPos = new POINT { x = cursorX, y = cursorY };
        _startWindowRect = rect;
        _isDragging = true;
    }

    public void UpdateDrag(int cursorX, int cursorY)
    {
        if (!_isDragging) return;
        if (!IsWindow(_targetHwnd))
        {
            EndDrag();
            return;
        }

        int dx = cursorX - _startCursorPos.x;
        int dy = cursorY - _startCursorPos.y;

        int newX = _startWindowRect.left + dx;
        int newY = _startWindowRect.top + dy;

        SetWindowPos(_targetHwnd, IntPtr.Zero, newX, newY, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
    }

    public void EndDrag()
    {
        _isDragging = false;
        _targetHwnd = IntPtr.Zero;
    }
}
