namespace HADashboardBackupExporteurUGSo.Backup;

internal static class BackupService
{
    public static string CreateBackupPath(string sourcePath)
    {
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        return Path.Combine(AppSettings.BackupsDirectory, $"{Path.GetFileNameWithoutExtension(sourcePath)}-{stamp}{Path.GetExtension(sourcePath)}");
    }
}
