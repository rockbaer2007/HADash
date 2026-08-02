namespace HADashboardBackupExporteurUGSo.UI.Controls;

internal sealed class SettingsSidebar : Panel
{
    private readonly FlowLayoutPanel _items = new()
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = false,
        Padding = new Padding(10, 14, 10, 14)
    };
    private readonly List<Button> _buttons = [];
    private Button? _selected;

    public event Action<int>? SelectedIndexChanged;
    public int SelectedIndex => _selected is null ? -1 : _buttons.IndexOf(_selected);

    public SettingsSidebar()
    {
        Dock = DockStyle.Fill;
        Controls.Add(_items);
    }

    public void AddItem(string text, bool enabled = true)
    {
        var index = _buttons.Count;
        var button = new Button
        {
            Text = text,
            Tag = index,
            Width = 196,
            Height = 44,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 6, 0),
            Margin = new Padding(0, 0, 0, 4),
            Enabled = enabled,
            TabStop = enabled
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => Select(index, raiseEvent: true);
        _buttons.Add(button);
        _items.Controls.Add(button);
    }

    public void Select(int index, bool raiseEvent = false)
    {
        if (index < 0 || index >= _buttons.Count || !_buttons[index].Enabled) return;
        _selected = _buttons[index];
        RefreshSelection();
        if (raiseEvent) SelectedIndexChanged?.Invoke(index);
    }

    public void ApplyPalette(ThemePalette palette)
    {
        BackColor = palette.PanelBackground;
        _items.BackColor = palette.PanelBackground;
        RefreshSelection(palette);
    }

    private void RefreshSelection(ThemePalette? palette = null)
    {
        palette ??= ThemeManager.CurrentPalette;
        foreach (var button in _buttons)
        {
            var selected = ReferenceEquals(button, _selected);
            button.BackColor = selected ? palette.Accent : palette.PanelBackground;
            button.ForeColor = button.Enabled
                ? selected ? Color.White : palette.Foreground
                : SystemColors.GrayText;
        }
    }
}
