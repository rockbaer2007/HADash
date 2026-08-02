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
            ApplyButton(button, palette);
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

    private static void ApplyButton(Button button, ThemePalette palette)
    {
        button.UseVisualStyleBackColor = false;
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = palette.ButtonBackground;
        button.ForeColor = GetContrastingButtonForeground(palette.ButtonBackground);
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = palette.ButtonBorder;
        button.FlatAppearance.MouseOverBackColor = palette.ButtonHoverBackground;
        button.FlatAppearance.MouseDownBackColor = palette.ButtonPressedBackground;

        // Ein deaktivierter Button soll auch im dunklen Theme noch als Steuerelement
        // erkennbar bleiben. WinForms zeichnet den Text selbst abgeblendet.
        if (!button.Enabled)
            button.BackColor = ControlPaint.Dark(palette.ButtonBackground, 0.08f);
    }

    private static Color GetContrastingButtonForeground(Color background)
    {
        // Helle Buttonflächen benötigen dunklen Text. Das gilt insbesondere
        // für Hell, Blau und Home Assistant, deren Buttons bewusst weiß sind.
        var brightness = (background.R * 299 + background.G * 587 + background.B * 114) / 1000;
        return brightness >= 160 ? Color.FromArgb(24, 24, 24) : Color.White;
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
