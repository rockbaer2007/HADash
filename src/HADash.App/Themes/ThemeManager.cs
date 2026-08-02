namespace HADashboardBackupExporteurUGSo;

public static class ThemeManager
{
    public static ThemePalette CurrentPalette { get; private set; } = LightTheme.Palette;

    public static ThemePalette GetPalette(AppTheme theme) => theme switch
    {
        AppTheme.Dark => DarkTheme.Palette,
        AppTheme.Blue => BlueTheme.Palette,
        AppTheme.HomeAssistant => HomeAssistantTheme.Palette,
        _ => LightTheme.Palette
    };

    public static void ApplyTheme(Form form, AppTheme theme)
    {
        CurrentPalette = GetPalette(theme);
        ApplyControl(form, CurrentPalette);
    }

    public static void ApplyControl(Control control, ThemePalette palette)
    {
        control.ForeColor = palette.Foreground;
        control.BackColor = GetBackground(control, palette);

        if (control is Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = palette.Accent;
            button.FlatAppearance.MouseOverBackColor = palette.ButtonHoverBackground;
            button.FlatAppearance.MouseDownBackColor = palette.Accent;
        }
        else if (control is RichTextBox richTextBox)
        {
            richTextBox.BackColor = palette.EditorBackground;
            richTextBox.ForeColor = palette.EditorForeground;
        }
        else if (control is MenuStrip menuStrip)
        {
            menuStrip.BackColor = palette.PanelBackground;
            menuStrip.ForeColor = palette.Foreground;
        }
        else if (control is StatusStrip statusStrip)
        {
            statusStrip.BackColor = palette.PanelBackground;
            statusStrip.ForeColor = palette.Foreground;
        }

        foreach (Control child in control.Controls)
            ApplyControl(child, palette);
    }

    private static Color GetBackground(Control control, ThemePalette palette) => control switch
    {
        RichTextBox => palette.EditorBackground,
        TextBoxBase or ListBox or ComboBox => palette.InputBackground,
        Button => palette.ButtonBackground,
        MenuStrip or StatusStrip or TableLayoutPanel or FlowLayoutPanel or SplitContainer => palette.PanelBackground,
        _ => palette.WindowBackground
    };
}
