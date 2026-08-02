namespace HADashboardBackupExporteurUGSo.Settings;

internal sealed class AppearancePage : SettingsPage
{
    private readonly NumericUpDown _fontSize = new()
    {
        Minimum = 8,
        Maximum = 16,
        DecimalPlaces = 1,
        Increment = 0.5M,
        Width = 100
    };

    private readonly ComboBox _iconSize = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 120
    };

    public AppearancePage()
        : base("Darstellung", "Schrift- und Symbolgrößen der Benutzeroberfläche.")
    {
        _iconSize.Items.AddRange(["16 px", "20 px", "24 px", "28 px", "32 px"]);

        var panel = FormLayout.Create();
        FormLayout.Add(panel, "Schriftgröße", _fontSize);
        FormLayout.Add(panel, "Icongröße", _iconSize);
        AddSection(panel);
    }

    public override void LoadFrom(AppSettings settings)
    {
        _fontSize.Value = (decimal)Math.Clamp(settings.FontSize, 8F, 16F);

        var sizes = new[] { 16, 20, 24, 28, 32 };
        var nearest = sizes.OrderBy(size => Math.Abs(size - settings.IconSize)).First();
        _iconSize.SelectedIndex = Array.IndexOf(sizes, nearest);
    }

    public override void SaveTo(AppSettings settings)
    {
        settings.FontSize = (float)_fontSize.Value;

        if (_iconSize.SelectedItem is string selected &&
            int.TryParse(selected.Split(' ')[0], out var iconSize))
        {
            settings.IconSize = iconSize;
        }
    }
}
