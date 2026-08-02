namespace HADashboardBackupExporteurUGSo.Settings;

internal sealed class FilePage : SettingsPage
{
    private readonly NumericUpDown _recent = new() { Minimum = 1, Maximum = 25, Width = 100 };
    private readonly CheckBox _backups = new()
    {
        Text = "Vor dem Überschreiben\nautomatisch Sicherungskopie erstellen",
        AutoSize = true,
        TextAlign = ContentAlignment.MiddleLeft
    };
    private readonly ComboBox _format = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };

    public FilePage() : base("Dateien", "Vorgaben für Dateiverlauf, Sicherungen und Exporte.")
    {
        _format.Items.AddRange(["Dashboard (*.dash)", "YAML (*.yaml)"]);
        var panel = FormLayout.Create();
        FormLayout.Add(panel, "Zuletzt geöffnete Dateien", _recent);
        FormLayout.Add(panel, "Standardformat", _format);
        FormLayout.Add(panel, "Sicherungskopien", _backups);
        AddSection(panel);
    }

    public override void LoadFrom(AppSettings settings)
    {
        _recent.Value = Math.Clamp(settings.MaximumRecentFiles, 1, 25);
        _backups.Checked = settings.CreateBackupCopies;
        _format.SelectedIndex = settings.DefaultDashboardFormat == "yaml" ? 1 : 0;
    }

    public override void SaveTo(AppSettings settings)
    {
        settings.MaximumRecentFiles = (int)_recent.Value;
        settings.CreateBackupCopies = _backups.Checked;
        settings.DefaultDashboardFormat = _format.SelectedIndex == 1 ? "yaml" : "dash";
    }
}
