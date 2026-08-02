namespace HADashboardBackupExporteurUGSo;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var settings = AppSettings.Load();
        LanguageManager.Apply(settings);
        Application.Run(new MainForm());
    }
}
