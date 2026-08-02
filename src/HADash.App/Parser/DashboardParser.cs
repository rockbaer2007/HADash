namespace HADashboardBackupExporteurUGSo.Parser;

// Erweiterungspunkt für die schrittweise Auslagerung der YAML-/JSON-Erkennung aus MainForm.
internal static class DashboardParser
{
    public static bool LooksLikeJson(string text)
        => !string.IsNullOrWhiteSpace(text) && text.TrimStart().StartsWith('{');
}
