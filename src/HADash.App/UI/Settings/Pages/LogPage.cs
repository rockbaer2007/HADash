namespace HADashboardBackupExporteurUGSo.Settings;

internal sealed class LogPage : SettingsPage
{
    private readonly CheckBox _enabled = new()
    {
        Text = "Protokollierung aktivieren",
        AutoSize = true
    };

    private readonly TextBox _directory = new()
    {
        Width = 250
    };

    private readonly Button _browse = new()
    {
        Text = "Durchsuchen …",
        AutoSize = false,
        Width = 120,
        Height = 34
    };

    public LogPage() : base("Protokoll", "Optionale technische Protokolle zur Fehlersuche.")
    {
        var path = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        path.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
        path.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));

        _directory.Anchor = AnchorStyles.Left;
        _directory.Margin = new Padding(0, 2, 10, 0);
        _browse.Anchor = AnchorStyles.Left;
        _browse.Margin = Padding.Empty;

        path.Controls.Add(_directory, 0, 0);
        path.Controls.Add(_browse, 1, 0);

        _browse.Click += (_, _) =>
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Logverzeichnis auswählen",
                SelectedPath = Path.IsPathRooted(_directory.Text)
                    ? _directory.Text
                    : AppSettings.ApplicationDirectory
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
                _directory.Text = dialog.SelectedPath;
        };

        var panel = FormLayout.Create();
        FormLayout.Add(panel, string.Empty, _enabled);
        FormLayout.Add(panel, "Logverzeichnis", path);
        AddSection(panel);
    }

    public override void LoadFrom(AppSettings settings)
    {
        _enabled.Checked = settings.LoggingEnabled;
        _directory.Text = settings.LogDirectory;
    }

    public override void SaveTo(AppSettings settings)
    {
        settings.LoggingEnabled = _enabled.Checked;
        settings.LogDirectory = string.IsNullOrWhiteSpace(_directory.Text)
            ? "logs"
            : _directory.Text.Trim();
    }
}
