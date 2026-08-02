namespace HADashboardBackupExporteurUGSo.Helpers;

internal static class FileNameHelper
{
    public static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray()).Trim();
    }
}
