namespace HADashboardBackupExporteurUGSo.Settings;
internal static class SettingsManager
{
    public static AppSettings CreateWorkingCopy(AppSettings current) => current.Clone();
    public static void Commit(AppSettings target, AppSettings working) { target.CopyFrom(working); target.Save(); }
}
