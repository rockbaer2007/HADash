using System.Diagnostics;

namespace HADashboardBackupExporteurUGSo;

public sealed class InstructionForm : Form
{
    private const string HomeAssistantUrl = "https://www.home-assistant.io/";
    private const string NotepadPlusPlusUrl = "https://notepad-plus-plus.org/";

    public InstructionForm(ThemePalette palette)
    {
        Text = "Anleitung – HA Dashboard Backup & Ansichten Exporteur by UGSo";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(800, 650);
        Size = new Size(940, 780);
        Font = new Font("Segoe UI", 10F);
        ShowIcon = false;

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(18, 16, 18, 10)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "Dashboard-Dateien und Home-Assistant-Backups verwenden",
            AutoSize = true,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            Anchor = AnchorStyles.Left
        };
        var dots = new Label
        {
            Text = "⋮",
            AutoSize = true,
            Font = new Font("Segoe UI", 26F, FontStyle.Bold),
            Padding = new Padding(12, 0, 12, 0),
            Anchor = AnchorStyles.Right,
            AccessibleDescription = "Symbol für das Drei-Punkte-Menü in Home Assistant"
        };
        header.Controls.Add(title, 0, 0);
        header.Controls.Add(dots, 1, 0);

        var instructions = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle,
            DetectUrls = true,
            WordWrap = true,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            Font = new Font("Segoe UI", 11F),
            Text =
                "MÖGLICHKEIT 1 – DASHBOARD ÜBER DEN RAW-KONFIGURATIONSEDITOR SPEICHERN\n\n" +
                "1. Öffnen Sie den Bearbeitungsmodus Ihres Dashboards.\n\n" +
                "2. Klicken Sie oben rechts auf die drei Punkte (⋮).\n\n" +
                "3. Wählen Sie „RAW-Konfigurationseditor“ aus.\n\n" +
                "4. Klicken Sie in den angezeigten YAML-Code.\n\n" +
                "5. Drücken Sie STRG + A, um den gesamten Code auszuwählen.\n\n" +
                "6. Drücken Sie STRG + C, um den Code zu kopieren.\n\n" +
                "7. Öffnen Sie einen Texteditor und fügen Sie den Code mit STRG + V ein.\n\n" +
                "   Bevorzugter Editor: Notepad++\n" +
                "   https://notepad-plus-plus.org/\n\n" +
                "8. Speichern Sie die Datei zum Beispiel als Wohnzimmer.dash oder Wohnzimmer.yaml.\n\n" +
                "MÖGLICHKEIT 2 – DASHBOARD-DATEN AUS EINEM HOME-ASSISTANT-BACKUP ÖFFNEN\n\n" +
                "1. Entpacken Sie das Home-Assistant-Backup.\n\n" +
                "2. Wählen Sie die Lovelace-/Dashboard-Datei aus dem Storage-Bereich aus. Solche Dateien können JSON enthalten und beispielsweise lovelace.lovelace, .json oder .txt heißen.\n\n" +
                "3. Verwenden Sie in diesem Programm „Datei öffnen“. Im Dateidialog können Sie auch „Alle Dateien (*.*)“ auswählen.\n\n" +
                "4. Das Programm erkennt JSON automatisch. Bei einer Home-Assistant-Storage-Struktur wird data.config beziehungsweise config extrahiert und intern in YAML umgewandelt.\n\n" +
                "5. Danach können Sie Ansichten auswählen, einzelne Ansichten exportieren oder mit „Dashboard speichern“ das komplette Dashboard als .dash, .yaml oder .yml speichern.\n\n" +
                "HINWEIS\n\n" +
                "Unterstützt werden .dash, .yaml, .yml, .json, .txt, .lovelace sowie Dateien ohne bekannte Endung über die Auswahl „Alle Dateien (*.*)“."
        };
        instructions.LinkClicked += (_, e) => OpenUrl(e.LinkText);

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(12)
        };
        var closeButton = new Button { Text = "Schließen", AutoSize = true, Padding = new Padding(10, 4, 10, 4) };
        var npButton = new Button { Text = "Notepad++ öffnen", AutoSize = true, Padding = new Padding(10, 4, 10, 4) };
        var haButton = new Button { Text = "Home Assistant öffnen", AutoSize = true, Padding = new Padding(10, 4, 10, 4) };
        closeButton.Click += (_, _) => Close();
        npButton.Click += (_, _) => OpenUrl(NotepadPlusPlusUrl);
        haButton.Click += (_, _) => OpenUrl(HomeAssistantUrl);
        footer.Controls.Add(closeButton);
        footer.Controls.Add(npButton);
        footer.Controls.Add(haButton);

        Controls.Add(instructions);
        Controls.Add(header);
        Controls.Add(footer);
        AcceptButton = closeButton;
        CancelButton = closeButton;

        ApplyTheme(palette, instructions, header, footer);
    }

    private void ApplyTheme(ThemePalette palette, RichTextBox textBox, params Control[] panels)
    {
        ThemeManager.ApplyControl(this, palette);
        textBox.BackColor = palette.EditorBackground;
        textBox.ForeColor = palette.EditorForeground;
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Die Webseite konnte nicht geöffnet werden.\n\n{ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
