namespace HADashboardBackupExporteurUGSo.Settings;

internal sealed class GeneralPage : SettingsPage
{
    private readonly ComboBox _language = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 220
    };

    private readonly CheckBox _autoDetect = new()
    {
        Text = "Windows-Sprache automatisch erkennen",
        AutoSize = true
    };

    private readonly Label _detectedLanguage = new()
    {
        AutoSize = true,
        ForeColor = SystemColors.GrayText,
        Margin = new Padding(3, 3, 3, 12)
    };

    private readonly CheckBox _openLast = new()
    {
        Text = "Beim Programmstart\ndie zuletzt verwendete Datei automatisch öffnen",
        AutoSize = false,
        Width = 430,
        Height = 50,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private readonly Label _lastFileCaption = new()
    {
        Text = "Letzte Datei",
        AutoSize = true,
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        Margin = new Padding(0, 5, 0, 3)
    };

    private readonly Label _lastFileValue = new()
    {
        AutoSize = true,
        AutoEllipsis = true,
        MaximumSize = new Size(500, 44),
        ForeColor = SystemColors.GrayText,
        Margin = new Padding(0, 0, 0, 12)
    };

    public GeneralPage() : base("Allgemein", "Grundlegendes Verhalten der Anwendung.")
    {
        _language.Items.AddRange(["Deutsch", "English", "Français"]);
        _autoDetect.CheckedChanged += (_, _) => UpdateLanguageControls();

        var lastFilePanel = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            MaximumSize = new Size(520, 0)
        };
        lastFilePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        lastFilePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        lastFilePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        lastFilePanel.Controls.Add(_lastFileCaption, 0, 0);
        lastFilePanel.Controls.Add(_lastFileValue, 0, 1);

        var panel = FormLayout.Create();
        FormLayout.Add(panel, "Sprache", _language);
        FormLayout.Add(panel, string.Empty, _autoDetect);
        FormLayout.Add(panel, string.Empty, _detectedLanguage);
        FormLayout.Add(panel, "Startverhalten", _openLast);
        FormLayout.Add(panel, string.Empty, lastFilePanel);
        AddSection(panel);
    }

    public override void LoadFrom(AppSettings settings)
    {
        var index = _language.Items.IndexOf(settings.Language);
        _language.SelectedIndex = index >= 0 ? index : 0;
        _autoDetect.Checked = settings.AutoDetectLanguage;
        _openLast.Checked = settings.OpenLastFileOnStartup;
        _lastFileValue.Text = string.IsNullOrWhiteSpace(settings.LastFilePath)
            ? "Keine Datei gespeichert"
            : $"{Path.GetFileName(settings.LastFilePath)}\n{settings.LastFilePath}";
        _lastFileValue.Tag = settings.LastFilePath;
        UpdateLanguageControls();
    }

    public override void SaveTo(AppSettings settings)
    {
        settings.Language = _language.SelectedItem?.ToString() ?? "Deutsch";
        settings.AutoDetectLanguage = _autoDetect.Checked;
        settings.OpenLastFileOnStartup = _openLast.Checked;
    }

    private void UpdateLanguageControls()
    {
        _language.Enabled = !_autoDetect.Checked;
        _detectedLanguage.Text = _autoDetect.Checked
            ? $"Erkannte Windows-Sprache: {LanguageManager.DetectWindowsLanguage()}"
            : "Manuelle Sprachauswahl aktiv";
    }
}
