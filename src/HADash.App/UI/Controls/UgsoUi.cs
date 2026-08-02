namespace HADashboardBackupExporteurUGSo.UI.Controls;

internal static class UgsoUi
{
    public const int DialogButtonWidth = 112;
    public const int DialogButtonHeight = 36;
    public const int StandardSpacing = 12;
    public static readonly Font DefaultFont = new("Segoe UI", 10F);
    public static readonly Font HeadingFont = new("Segoe UI", 15F, FontStyle.Bold);

    public static Button CreateDialogButton(string text, DialogResult result)
        => new()
        {
            Text = text,
            DialogResult = result,
            Width = DialogButtonWidth,
            Height = DialogButtonHeight,
            Margin = new Padding(6, 0, 0, 0)
        };
}
