namespace HADashboardBackupExporteurUGSo.Export;

// Gemeinsamer Erweiterungspunkt für künftige Exportformate.
internal static class DashboardExportService
{
    public static void WriteText(string path, string content)
        => File.WriteAllText(path, content, new System.Text.UTF8Encoding(false));
}
