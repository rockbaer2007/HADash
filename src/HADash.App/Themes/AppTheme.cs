namespace HADashboardBackupExporteurUGSo;

public enum AppTheme
{
    Light,
    Dark,
    Blue,
    HomeAssistant
}

public sealed record ThemePalette(
    string DisplayName,
    Color WindowBackground,
    Color PanelBackground,
    Color InputBackground,
    Color EditorBackground,
    Color Foreground,
    Color ButtonBackground,
    Color ButtonHoverBackground,
    Color ButtonPressedBackground,
    Color ButtonBorder,
    Color Accent,
    Color EditorForeground,
    Color SyntaxKey,
    Color SyntaxComment,
    Color SyntaxKeyword,
    Color SyntaxNumber,
    bool IsDark);
