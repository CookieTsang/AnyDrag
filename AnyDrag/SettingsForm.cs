namespace AnyDrag;

public class SettingsForm : Form
{
    private Label _comboLabel = null!;
    private ComboBox _countCombo = null!;
    private ComboBox _langCombo = null!;
    private Button _recordButton = null!;
    private Button _resetButton = null!;
    private Button _okButton = null!;
    private Button _cancelButton = null!;
    private Label _hotkeyLabel = null!;
    private Button _hotkeyRecordButton = null!;
    private Label _countLabel = null!;
    private Label _comboTitleLabel = null!;
    private Label _hotkeyTitleLabel = null!;
    private Label _langLabel = null!;
    private HashSet<MouseButton> _recordedButtons = new();
    private bool _isRecording;
    private bool _isRecordingHotkey;
    private int _recordedHotkeyModifiers;
    private int _recordedHotkeyKey;
    private System.Windows.Forms.Timer _recordTimer = null!;

    public AppSettings Settings { get; private set; }

    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_RBUTTONUP = 0x0205;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_MBUTTONUP = 0x0208;
    private const int WM_XBUTTONDOWN = 0x020B;
    private const int WM_XBUTTONUP = 0x020C;
    private const int XBUTTON1 = 0x0001;
    private const int XBUTTON2 = 0x0002;

    private delegate IntPtr LowMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    private IntPtr _hookId = IntPtr.Zero;
    private LowMouseProc? _hookProc;

    public SettingsForm(AppSettings settings)
    {
        Settings = new AppSettings
        {
            Enabled = settings.Enabled,
            ButtonCount = settings.ButtonCount,
            HotkeyModifiers = settings.HotkeyModifiers,
            HotkeyKey = settings.HotkeyKey,
            Language = settings.Language,
            ButtonCombo = new List<MouseButton>(settings.ButtonCombo)
        };
        _recordedHotkeyModifiers = settings.HotkeyModifiers;
        _recordedHotkeyKey = settings.HotkeyKey;
        Lang.SetLanguage(settings.Language);
        InitializeComponent();
        UpdateComboText();
        UpdateHotkeyText();
    }

    private void InitializeComponent()
    {
        Text = Lang.Get("SettingsTitle");
        Size = new Size(620, 620);
        BackColor = Color.FromArgb(245, 245, 245);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        TopMost = true;
        KeyPreview = true;

        // Language panel
        var langPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(24, 16, 24, 0)
        };

        _langLabel = new Label
        {
            Text = Lang.Get("Language"),
            Location = new Point(24, 20),
            AutoSize = true,
            Font = new Font("Segoe UI", 10)
        };

        _langCombo = new ComboBox
        {
            Location = new Point(110, 17),
            Size = new Size(150, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10)
        };
        _langCombo.Items.AddRange(new object[] { "zh-CN", "en-US" });
        _langCombo.SelectedItem = Settings.Language;
        _langCombo.SelectedIndexChanged += OnLanguageChanged;

        langPanel.Controls.Add(_langLabel);
        langPanel.Controls.Add(_langCombo);

        // Combo panel
        var comboPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 160,
            Padding = new Padding(24, 12, 24, 0)
        };

        _countLabel = new Label
        {
            Text = Lang.Get("ButtonCount"),
            Location = new Point(24, 18),
            AutoSize = true,
            Font = new Font("Segoe UI", 10)
        };

        _countCombo = new ComboBox
        {
            Location = new Point(140, 15),
            Size = new Size(70, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10)
        };
        _countCombo.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
        _countCombo.SelectedItem = Settings.ButtonCount.ToString();

        _comboTitleLabel = new Label
        {
            Text = Lang.Get("CurrentCombo"),
            Location = new Point(24, 58),
            AutoSize = true,
            Font = new Font("Segoe UI", 10)
        };

        _comboLabel = new Label
        {
            Text = "",
            Location = new Point(24, 88),
            AutoSize = true,
            Font = new Font("Segoe UI", 14, FontStyle.Bold)
        };

        comboPanel.Controls.Add(_countLabel);
        comboPanel.Controls.Add(_countCombo);
        comboPanel.Controls.Add(_comboTitleLabel);
        comboPanel.Controls.Add(_comboLabel);

        // Button panel
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 60,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(20, 8, 20, 8)
        };

        _recordButton = new Button
        {
            Text = Lang.Get("RecordCombo"),
            Size = new Size(140, 40),
            Margin = new Padding(4),
            Font = new Font("Segoe UI", 10),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.White
        };
        _recordButton.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
        _recordButton.Click += StartRecording;

        _resetButton = new Button
        {
            Text = Lang.Get("ResetDefault"),
            Size = new Size(140, 40),
            Margin = new Padding(4),
            Font = new Font("Segoe UI", 10),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.White
        };
        _resetButton.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
        _resetButton.Click += ResetToDefault;

        buttonPanel.Controls.Add(_recordButton);
        buttonPanel.Controls.Add(_resetButton);

        // Hotkey panel
        var hotkeyPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            Padding = new Padding(24, 12, 24, 0)
        };

        _hotkeyTitleLabel = new Label
        {
            Text = Lang.Get("ToggleHotkey"),
            Location = new Point(24, 12),
            AutoSize = true,
            Font = new Font("Segoe UI", 10)
        };

        _hotkeyLabel = new Label
        {
            Text = "",
            Location = new Point(24, 42),
            AutoSize = true,
            Font = new Font("Segoe UI", 13, FontStyle.Bold)
        };

        _hotkeyRecordButton = new Button
        {
            Text = Lang.Get("RecordHotkey"),
            Size = new Size(140, 35),
            Location = new Point(400, 40),
            Font = new Font("Segoe UI", 10),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.White
        };
        _hotkeyRecordButton.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
        _hotkeyRecordButton.Click += StartHotkeyRecording;

        hotkeyPanel.Controls.Add(_hotkeyTitleLabel);
        hotkeyPanel.Controls.Add(_hotkeyLabel);
        hotkeyPanel.Controls.Add(_hotkeyRecordButton);

        // Bottom panel
        var bottomPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 65,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(20, 10, 20, 10)
        };

        _cancelButton = new Button
        {
            Text = Lang.Get("Cancel"),
            Size = new Size(100, 42),
            DialogResult = DialogResult.Cancel,
            Margin = new Padding(4),
            Font = new Font("Segoe UI", 10),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(180, 180, 180),
            ForeColor = Color.FromArgb(50, 50, 50)
        };
        _cancelButton.FlatAppearance.BorderColor = Color.FromArgb(160, 160, 160);

        _okButton = new Button
        {
            Text = Lang.Get("Ok"),
            Size = new Size(100, 42),
            DialogResult = DialogResult.OK,
            Margin = new Padding(4),
            Font = new Font("Segoe UI", 10),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 120, 212),
            ForeColor = Color.White
        };
        _okButton.FlatAppearance.BorderColor = Color.FromArgb(0, 100, 180);
        _okButton.Click += (s, e) =>
        {
            Settings.ButtonCombo = new List<MouseButton>(_recordedButtons);
            Settings.ButtonCount = int.Parse(_countCombo.SelectedItem!.ToString()!);
            Settings.HotkeyModifiers = _recordedHotkeyModifiers;
            Settings.HotkeyKey = _recordedHotkeyKey;
            Settings.Language = _langCombo.SelectedItem!.ToString()!;
        };

        bottomPanel.Controls.Add(_cancelButton);
        bottomPanel.Controls.Add(_okButton);

        Controls.Add(bottomPanel);
        Controls.Add(hotkeyPanel);
        Controls.Add(CreateSeparator());
        Controls.Add(buttonPanel);
        Controls.Add(comboPanel);
        Controls.Add(CreateSeparator());
        Controls.Add(langPanel);

        AcceptButton = _okButton;
        CancelButton = _cancelButton;

        _recordedButtons = new HashSet<MouseButton>(Settings.ButtonCombo);

        _recordTimer = new System.Windows.Forms.Timer { Interval = 3000 };
        _recordTimer.Tick += (s, e) => StopRecording();
    }

    private static Panel CreateSeparator()
    {
        return new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = Color.FromArgb(220, 220, 220),
            Margin = new Padding(24, 0, 24, 0)
        };
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        var lang = _langCombo.SelectedItem!.ToString()!;
        Lang.SetLanguage(lang);
        RefreshLabels();
    }

    private void RefreshLabels()
    {
        Text = Lang.Get("SettingsTitle");
        _langLabel.Text = Lang.Get("Language");
        _countLabel.Text = Lang.Get("ButtonCount");
        _comboTitleLabel.Text = Lang.Get("CurrentCombo");
        _recordButton.Text = _isRecording ? Lang.Get("PressCombo") : Lang.Get("RecordCombo");
        _resetButton.Text = Lang.Get("ResetDefault");
        _hotkeyTitleLabel.Text = Lang.Get("ToggleHotkey");
        _hotkeyRecordButton.Text = _isRecordingHotkey ? Lang.Get("PressHotkey") : Lang.Get("RecordHotkey");
        _okButton.Text = Lang.Get("Ok");
        _cancelButton.Text = Lang.Get("Cancel");
        UpdateComboText();
        UpdateHotkeyText();
    }

    private void StartRecording(object? sender, EventArgs e)
    {
        _isRecording = true;
        _recordedButtons.Clear();
        _recordButton.Text = Lang.Get("PressCombo");
        _recordButton.Enabled = false;
        _okButton.Enabled = false;
        _recordButton.BackColor = Color.FromArgb(200, 60, 60);
        _recordButton.FlatAppearance.BorderColor = Color.FromArgb(180, 40, 40);
        UpdateComboText();

        _hookProc = RecordHookCallback;
        _hookId = SetWindowsHookEx(WH_MOUSE_LL, _hookProc, GetModuleHandle("user32.dll"), 0);

        _recordTimer.Interval = 3000;
        _recordTimer.Start();
    }

    private void UnhookMouse()
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }

    private void StopRecording()
    {
        _recordTimer.Stop();
        UnhookMouse();

        _isRecording = false;
        _recordButton.Text = Lang.Get("RecordCombo");
        _recordButton.Enabled = true;
        _okButton.Enabled = true;
        _recordButton.BackColor = Color.FromArgb(50, 50, 50);
        _recordButton.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);

        if (_recordedButtons.Count == 0)
        {
            _recordedButtons = new HashSet<MouseButton>(Settings.ButtonCombo);
        }

        UpdateComboText();
    }

    private IntPtr RecordHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _isRecording)
        {
            var hookStruct = System.Runtime.InteropServices.Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            int msg = (int)wParam;

            MouseButton? button = null;
            bool isDown = false;

            switch (msg)
            {
                case WM_LBUTTONDOWN:
                    button = MouseButton.Left; isDown = true; break;
                case WM_RBUTTONDOWN:
                    button = MouseButton.Right; isDown = true; break;
                case WM_MBUTTONDOWN:
                    button = MouseButton.Middle; isDown = true; break;
                case WM_XBUTTONDOWN when (hookStruct.mouseData >> 16) == XBUTTON1:
                    button = MouseButton.XButton1; isDown = true; break;
                case WM_XBUTTONDOWN when (hookStruct.mouseData >> 16) == XBUTTON2:
                    button = MouseButton.XButton2; isDown = true; break;
            }

            if (button.HasValue && isDown)
            {
                _recordedButtons.Add(button.Value);
                UpdateComboText();

                int targetCount = Settings.ButtonCount;
                if (_recordedButtons.Count >= targetCount)
                {
                    // Immediately unhook to stop capturing more buttons
                    UnhookMouse();
                    // Short delay before finalizing UI
                    _recordTimer.Stop();
                    _recordTimer.Interval = 500;
                    _recordTimer.Start();
                }
                else
                {
                    _recordTimer.Stop();
                    _recordTimer.Interval = 3000;
                    _recordTimer.Start();
                }
            }
        }

        // Don't pass events through while recording
        if (_isRecording)
            return (IntPtr)1;
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private void ResetToDefault(object? sender, EventArgs e)
    {
        _recordedButtons = new HashSet<MouseButton>
        {
            MouseButton.Left,
            MouseButton.Right,
            MouseButton.XButton1
        };
        UpdateComboText();
    }

    private void UpdateComboText()
    {
        _comboLabel.Text = _recordedButtons.Count > 0
            ? Lang.FormatCombo(_recordedButtons)
            : Lang.Get("NotSet");
    }

    private void UpdateHotkeyText()
    {
        _hotkeyLabel.Text = FormatHotkey(_recordedHotkeyModifiers, _recordedHotkeyKey);
    }

    private static string FormatHotkey(int modifiers, int key)
    {
        var parts = new List<string>();
        if ((modifiers & 0x0002) != 0) parts.Add("Ctrl");
        if ((modifiers & 0x0001) != 0) parts.Add("Alt");
        if ((modifiers & 0x0004) != 0) parts.Add("Shift");
        if ((modifiers & 0x0008) != 0) parts.Add("Win");

        string keyName = key >= 0x30 && key <= 0x5A ? ((char)key).ToString()
            : key >= 0x60 && key <= 0x69 ? $"Num{key - 0x60}"
            : key >= 0x70 && key <= 0x87 ? $"F{key - 0x6F}"
            : key.ToString();
        parts.Add(keyName);

        return string.Join(" + ", parts);
    }

    private void StartHotkeyRecording(object? sender, EventArgs e)
    {
        _isRecordingHotkey = true;
        _recordedHotkeyModifiers = 0;
        _recordedHotkeyKey = 0;
        _hotkeyLabel.Text = Lang.Get("PressHotkey");
        _hotkeyRecordButton.Enabled = false;
        _okButton.Enabled = false;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (_isRecordingHotkey)
        {
            int modifiers = 0;
            if (e.Control) modifiers |= 0x0002;
            if (e.Alt) modifiers |= 0x0001;
            if (e.Shift) modifiers |= 0x0004;

            if (modifiers != 0 && e.KeyValue != 0x10 && e.KeyValue != 0x11 && e.KeyValue != 0x12)
            {
                _recordedHotkeyModifiers = modifiers;
                _recordedHotkeyKey = e.KeyValue;
                _isRecordingHotkey = false;
                _hotkeyRecordButton.Enabled = true;
                _okButton.Enabled = true;
                UpdateHotkeyText();
                e.Handled = true;
                return;
            }
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_isRecording) StopRecording();
        base.OnFormClosing(e);
    }
}
