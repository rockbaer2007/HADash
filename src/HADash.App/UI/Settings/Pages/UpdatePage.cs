namespace HADashboardBackupExporteurUGSo.Settings;

internal sealed class UpdatePage : SettingsPage
{
    public UpdatePage() : base("Updates", "Diese Funktion ist vorbereitet, aber in dieser Version noch nicht verfügbar.")
    {
        AddSection(new Label { Text = "Automatische Update-Prüfung: derzeit deaktiviert", AutoSize = true, Enabled = false, Font = new Font("Segoe UI", 10F, FontStyle.Italic) });
        Enabled = false;
    }
    public override void LoadFrom(AppSettings settings) { }
    public override void SaveTo(AppSettings settings) { settings.CheckForUpdates = false; }
}
