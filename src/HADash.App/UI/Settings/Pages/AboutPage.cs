namespace HADashboardBackupExporteurUGSo.Settings;

internal sealed class AboutPage : SettingsPage
{
    public AboutPage() : base("Info", "Informationen zur Anwendung.")
    {
        AddSection(new Label { Text = "HADash\nVersion 2.5.3\n\nEntwickelt von UGSo\nMit Hilfe von ChatGPT", AutoSize = true });
    }
    public override void LoadFrom(AppSettings settings) { }
    public override void SaveTo(AppSettings settings) { }
}
