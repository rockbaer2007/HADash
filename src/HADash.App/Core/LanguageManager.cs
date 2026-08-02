using System.Globalization;

namespace HADashboardBackupExporteurUGSo;

internal static class LanguageManager
{
    public static string DetectWindowsLanguage()
    {
        var code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
        return code switch
        {
            "en" => "English",
            "fr" => "Français",
            _ => "Deutsch"
        };
    }

    public static string ResolveLanguage(AppSettings settings) =>
        settings.AutoDetectLanguage ? DetectWindowsLanguage() : settings.Language;

    public static void Apply(AppSettings settings)
    {
        var cultureName = ResolveLanguage(settings) switch
        {
            "English" => "en-US",
            "Français" => "fr-FR",
            _ => "de-DE"
        };

        var culture = CultureInfo.GetCultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
