namespace HADashboardBackupExporteurUGSo.Settings;

internal sealed class ThemePage : SettingsPage
{
    private readonly ComboBox _theme = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    private readonly NumericUpDown _fontSize = new() { Minimum = 8, Maximum = 16, DecimalPlaces = 1, Increment = 0.5M, Width = 100 };
    private readonly ComboBox _iconSize = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
    public ThemePage() : base("Darstellung", "Farbschema und Größen der Benutzeroberfläche.")
    {
        _theme.Items.AddRange([new Choice("Hell", AppTheme.Light), new Choice("Dunkel", AppTheme.Dark), new Choice("Blau", AppTheme.Blue), new Choice("Home Assistant", AppTheme.HomeAssistant)]);
        _iconSize.Items.AddRange(["16 px", "20 px", "24 px", "28 px", "32 px"]);
        var panel = FormLayout.Create(); FormLayout.Add(panel, "Theme", _theme); FormLayout.Add(panel, "Schriftgröße", _fontSize); FormLayout.Add(panel, "Icongröße", _iconSize); AddSection(panel);
    }
    public override void LoadFrom(AppSettings s)
    {
        var current = Enum.TryParse<AppTheme>(s.ThemeName, true, out var t) ? t : AppTheme.Light;
        _theme.SelectedIndex = Enumerable.Range(0, _theme.Items.Count).FirstOrDefault(i => _theme.Items[i] is Choice c && c.Theme == current);
        _fontSize.Value = (decimal)Math.Clamp(s.FontSize, 8F, 16F);
        var sizes = new[] { 16,20,24,28,32 }; _iconSize.SelectedIndex = Array.IndexOf(sizes, sizes.OrderBy(x => Math.Abs(x-s.IconSize)).First());
    }
    public override void SaveTo(AppSettings s)
    {
        if (_theme.SelectedItem is Choice c) { s.ThemeName = c.Theme.ToString(); s.DarkMode = c.Theme == AppTheme.Dark; }
        s.FontSize = (float)_fontSize.Value; s.IconSize = int.Parse(_iconSize.Text.Split(' ')[0]);
    }
    private sealed record Choice(string Name, AppTheme Theme) { public override string ToString() => Name; }
}
