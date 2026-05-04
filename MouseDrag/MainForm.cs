namespace MouseDrag;

public class MainForm : Form
{
    private NotifyIcon _trayIcon = null!;
    private ContextMenuStrip _trayMenu = null!;
    private ToolStripMenuItem _toggleItem = null!;
    private ToolStripMenuItem _startupItem = null!;
    private MouseHook _hook = null!;
    private WindowDragger _dragger = null!;
    private AppSettings _settings = null!;
    private bool _isEnabled;

    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_ID = 1;
    private const int MOD_ALT = 0x0001;
    private const int MOD_CTRL = 0x0002;
    private const int MOD_SHIFT = 0x0004;
    private const int MOD_WIN = 0x0008;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public MainForm()
    {
        InitializeComponent();
        InitializeTray();
        InitializeHook();
        RegisterToggleHotkey();
    }

    private void InitializeComponent()
    {
        Text = "MouseDrag";
        ShowInTaskbar = false;
        WindowState = FormWindowState.Minimized;
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        Size = new Size(0, 0);
        StartPosition = FormStartPosition.Manual;
        Location = new Point(-10000, -10000);
        Opacity = 0;
    }

    private void InitializeTray()
    {
        _settings = AppSettings.Load();
        Lang.SetLanguage(_settings.Language);
        _isEnabled = _settings.Enabled;

        _toggleItem = new ToolStripMenuItem(Lang.Get("TrayEnabled"))
        {
            CheckOnClick = true,
            Checked = _isEnabled
        };
        _toggleItem.Click += (s, e) => ToggleEnabled();

        _startupItem = new ToolStripMenuItem(Lang.Get("TrayStartup"))
        {
            CheckOnClick = true,
            Checked = Startup.IsEnabled()
        };
        _startupItem.Click += (s, e) =>
        {
            Startup.SetEnabled(_startupItem.Checked);
        };

        var settingsItem = new ToolStripMenuItem(Lang.Get("TraySettings"));
        settingsItem.Click += (s, e) => ShowSettings();

        var exitItem = new ToolStripMenuItem(Lang.Get("TrayExit"));
        exitItem.Click += (s, e) => ExitApp();

        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add(_toggleItem);
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add(_startupItem);
        _trayMenu.Items.Add(settingsItem);
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add(exitItem);

        _trayIcon = new NotifyIcon
        {
            Text = $"MouseDrag - {Lang.FormatCombo(_settings.ButtonCombo)}",
            ContextMenuStrip = _trayMenu,
            Visible = true
        };

        UpdateIcon();
        _trayIcon.DoubleClick += (s, e) => ToggleEnabled();
    }

    private void InitializeHook()
    {
        _dragger = new WindowDragger();
        _hook = new MouseHook();
        _hook.SetRequiredButtons(_settings.ButtonCombo);
        _hook.Enabled = _isEnabled;

        _hook.DragStart += () =>
        {
            GetCursorPos(out var pt);
            _dragger.BeginDrag(pt.x, pt.y);
        };

        _hook.DragEnd += () => _dragger.EndDrag();

        _hook.DragMove += (x, y) => _dragger.UpdateDrag(x, y);

        _hook.Install();
    }

    private void RegisterToggleHotkey()
    {
        UnregisterHotKey(Handle, HOTKEY_ID);
        RegisterHotKey(Handle, HOTKEY_ID, (uint)_settings.HotkeyModifiers, (uint)_settings.HotkeyKey);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
        {
            ToggleEnabled();
            return;
        }
        base.WndProc(ref m);
    }

    private void ToggleEnabled()
    {
        _isEnabled = !_isEnabled;
        _toggleItem.Checked = _isEnabled;
        _hook.Enabled = _isEnabled;
        _settings.Enabled = _isEnabled;
        _settings.Save();
        UpdateIcon();
        _trayIcon.ShowBalloonTip(2000, "MouseDrag",
            _isEnabled ? Lang.Get("TrayEnabledTip") : Lang.Get("TrayDisabledTip"),
            ToolTipIcon.Info);
    }

    private void ShowSettings()
    {
        using var form = new SettingsForm(_settings);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _settings = form.Settings;
            Lang.SetLanguage(_settings.Language);
            _hook.SetRequiredButtons(_settings.ButtonCombo);
            _trayIcon.Text = $"MouseDrag - {Lang.FormatCombo(_settings.ButtonCombo)}";
            _settings.Save();
            RegisterToggleHotkey();
            RefreshTrayMenu();
        }
    }

    private void RefreshTrayMenu()
    {
        _toggleItem.Text = Lang.Get("TrayEnabled");
        _startupItem.Text = Lang.Get("TrayStartup");
        if (_trayMenu.Items.Count >= 6)
        {
            _trayMenu.Items[3].Text = Lang.Get("TraySettings");
            _trayMenu.Items[5].Text = Lang.Get("TrayExit");
        }
    }

    private void ExitApp()
    {
        UnregisterHotKey(Handle, HOTKEY_ID);
        _hook.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Exit();
    }

    private void UpdateIcon()
    {
        _trayIcon.Icon = _isEnabled ? CreateIcon(Color.LimeGreen) : CreateIcon(Color.Gray);
    }

    private static Icon CreateIcon(Color color)
    {
        var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);
        using var brush = new SolidBrush(color);
        g.FillEllipse(brush, 2, 2, 12, 12);
        return Icon.FromHandle(bmp.GetHicon());
    }

    protected override void SetVisibleCore(bool value)
    {
        base.SetVisibleCore(false);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
        }
        else
        {
            base.OnFormClosing(e);
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }
}
