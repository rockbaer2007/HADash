using System.Xml.Linq;

namespace HADashboardBackupExporteurUGSo;

internal sealed class AppSettings
{
    public string Language { get; set; } = "Deutsch";
    public bool AutoDetectLanguage { get; set; } = true;
    public bool OpenLastFileOnStartup { get; set; } = true;
    public bool CheckForUpdates { get; set; } = false;

    public string? LastFilePath { get; set; }
    public float FontSize { get; set; } = 10F;
    public int IconSize { get; set; } = 24;

    public List<string> RecentFiles { get; set; } = [];
    public int MaximumRecentFiles { get; set; } = 10;
    public bool CreateBackupCopies { get; set; } = true;
    public string DefaultDashboardFormat { get; set; } = "dash";

    public bool LoggingEnabled { get; set; } = false;
    public string LogDirectory { get; set; } = "logs";

    public int WindowLeft { get; set; } = 120;
    public int WindowTop { get; set; } = 80;
    public int WindowWidth { get; set; } = 1280;
    public int WindowHeight { get; set; } = 800;
    public FormWindowState WindowState { get; set; } = FormWindowState.Normal;

    public static string ApplicationDirectory => AppContext.BaseDirectory;
    public static string ConfigDirectory => Path.Combine(ApplicationDirectory, "config");
    public static string SettingsPath => Path.Combine(ConfigDirectory, "user.config");
    public static string LogsDirectory => Path.Combine(ApplicationDirectory, "logs");
    public static string BackupsDirectory => Path.Combine(ApplicationDirectory, "backups");
    public static string TempDirectory => Path.Combine(ApplicationDirectory, "temp");

    public string ResolvedLogDirectory => Path.IsPathRooted(LogDirectory)
        ? LogDirectory
        : Path.Combine(ApplicationDirectory, LogDirectory);

    public static AppSettings Load()
    {
        EnsurePortableDirectories();
        try
        {
            if (!File.Exists(SettingsPath)) return new AppSettings();
            var root = XDocument.Load(SettingsPath).Root;
            if (root is null) return new AppSettings();

            var settings = new AppSettings
            {
                Language = NormalizeLanguage((string?)root.Element("Language")),
                AutoDetectLanguage = ReadBool(root, "AutoDetectLanguage", true),
                OpenLastFileOnStartup = ReadBool(root, "OpenLastFileOnStartup", true),
                CheckForUpdates = false,
                LastFilePath = EmptyToNull((string?)root.Element("LastFilePath")),
                FontSize = Math.Clamp(ReadFloat(root, "FontSize", 10F), 8F, 16F),
                IconSize = Math.Clamp(ReadInt(root, "IconSize", 24), 16, 32),
                MaximumRecentFiles = Math.Clamp(ReadInt(root, "MaximumRecentFiles", 10), 1, 25),
                CreateBackupCopies = ReadBool(root, "CreateBackupCopies", true),
                DefaultDashboardFormat = NormalizeFormat((string?)root.Element("DefaultDashboardFormat")),
                LoggingEnabled = ReadBool(root, "LoggingEnabled", false),
                LogDirectory = EmptyToNull((string?)root.Element("LogDirectory")) ?? "logs",
                RecentFiles = root.Element("RecentFiles")?.Elements("File")
                    .Select(item => (string?)item)
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(path => path!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? [],
                WindowLeft = ReadInt(root, "WindowLeft", 120),
                WindowTop = ReadInt(root, "WindowTop", 80),
                WindowWidth = Math.Max(980, ReadInt(root, "WindowWidth", 1280)),
                WindowHeight = Math.Max(640, ReadInt(root, "WindowHeight", 800)),
                WindowState = ReadWindowState(root)
            };
            settings.RecentFiles = settings.RecentFiles.Take(settings.MaximumRecentFiles).ToList();
            return settings;
        }
        catch { return new AppSettings(); }
    }

    public void Save()
    {
        try
        {
            EnsurePortableDirectories();
            var document = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("UserSettings", new XAttribute("version", "3.0"),
                    new XElement("Language", Language),
                    new XElement("AutoDetectLanguage", AutoDetectLanguage),
                    new XElement("OpenLastFileOnStartup", OpenLastFileOnStartup),
                    new XElement("CheckForUpdates", false),
                    new XElement("FontSize", FontSize.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                    new XElement("IconSize", IconSize),
                    new XElement("MaximumRecentFiles", MaximumRecentFiles),
                    new XElement("CreateBackupCopies", CreateBackupCopies),
                    new XElement("DefaultDashboardFormat", DefaultDashboardFormat),
                    new XElement("LoggingEnabled", LoggingEnabled),
                    new XElement("LogDirectory", LogDirectory),
                    new XElement("LastFilePath", LastFilePath ?? string.Empty),
                    new XElement("WindowLeft", WindowLeft),
                    new XElement("WindowTop", WindowTop),
                    new XElement("WindowWidth", WindowWidth),
                    new XElement("WindowHeight", WindowHeight),
                    new XElement("WindowState", WindowState),
                    new XElement("RecentFiles", RecentFiles
                        .Where(path => !string.IsNullOrWhiteSpace(path))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(MaximumRecentFiles)
                        .Select(path => new XElement("File", path)))));
            document.Save(SettingsPath);
        }
        catch { }
    }

    public AppSettings Clone() => new()
    {
        Language = Language,
        AutoDetectLanguage = AutoDetectLanguage,
        OpenLastFileOnStartup = OpenLastFileOnStartup,
        CheckForUpdates = false,
        LastFilePath = LastFilePath,
        FontSize = FontSize,
        IconSize = IconSize,
        RecentFiles = [.. RecentFiles],
        MaximumRecentFiles = MaximumRecentFiles,
        CreateBackupCopies = CreateBackupCopies,
        DefaultDashboardFormat = DefaultDashboardFormat,
        LoggingEnabled = LoggingEnabled,
        LogDirectory = LogDirectory,
        WindowLeft = WindowLeft,
        WindowTop = WindowTop,
        WindowWidth = WindowWidth,
        WindowHeight = WindowHeight,
        WindowState = WindowState
    };

    public void CopyFrom(AppSettings source)
    {
        Language = source.Language; AutoDetectLanguage = source.AutoDetectLanguage; OpenLastFileOnStartup = source.OpenLastFileOnStartup;
        CheckForUpdates = false;
        FontSize = source.FontSize; IconSize = source.IconSize;
        MaximumRecentFiles = source.MaximumRecentFiles; CreateBackupCopies = source.CreateBackupCopies;
        DefaultDashboardFormat = source.DefaultDashboardFormat; LoggingEnabled = source.LoggingEnabled;
        LogDirectory = source.LogDirectory;
        if (RecentFiles.Count > MaximumRecentFiles)
            RecentFiles.RemoveRange(MaximumRecentFiles, RecentFiles.Count - MaximumRecentFiles);
    }

    private static void EnsurePortableDirectories()
    {
        try { Directory.CreateDirectory(ConfigDirectory); Directory.CreateDirectory(LogsDirectory); Directory.CreateDirectory(BackupsDirectory); Directory.CreateDirectory(TempDirectory); }
        catch { }
    }
    private static int ReadInt(XElement root, string name, int fallback) => int.TryParse((string?)root.Element(name), out var v) ? v : fallback;
    private static float ReadFloat(XElement root, string name, float fallback) => float.TryParse((string?)root.Element(name), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : fallback;
    private static bool ReadBool(XElement root, string name, bool fallback) => bool.TryParse((string?)root.Element(name), out var v) ? v : fallback;
    private static FormWindowState ReadWindowState(XElement root) => Enum.TryParse<FormWindowState>((string?)root.Element("WindowState"), true, out var s) && s != FormWindowState.Minimized ? s : FormWindowState.Normal;
    private static string NormalizeLanguage(string? value)
    {
        if (string.Equals(value, "English", StringComparison.OrdinalIgnoreCase)) return "English";
        if (string.Equals(value, "Français", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "Francais", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "French", StringComparison.OrdinalIgnoreCase)) return "Français";
        return "Deutsch";
    }
    private static string NormalizeFormat(string? value) => string.Equals(value, "yaml", StringComparison.OrdinalIgnoreCase) ? "yaml" : "dash";
    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
