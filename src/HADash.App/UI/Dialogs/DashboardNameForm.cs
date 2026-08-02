namespace HADashboardBackupExporteurUGSo;

public sealed class DashboardNameForm : Form
{
    private readonly TextBox _nameTextBox = new();

    public string DashboardName => _nameTextBox.Text;

    public DashboardNameForm(string suggestedName, ThemePalette palette)
    {
        Text = "Neues Dashboard benennen";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(480, 155);
        Font = new Font("Segoe UI", 10F);

        var label = new Label
        {
            Text = "Name des neuen Dashboards:",
            AutoSize = true,
            Location = new Point(18, 18)
        };

        _nameTextBox.Location = new Point(18, 48);
        _nameTextBox.Size = new Size(444, 27);
        _nameTextBox.Text = suggestedName;
        _nameTextBox.SelectAll();

        var hint = new Label
        {
            Text = "Der Name wird als Dashboard-Titel und als Dateiname vorgeschlagen.",
            AutoSize = true,
            Location = new Point(18, 82)
        };

        var okButton = new Button
        {
            Text = "Weiter",
            DialogResult = DialogResult.OK,
            Size = new Size(95, 32),
            Location = new Point(265, 112)
        };
        var cancelButton = new Button
        {
            Text = "Abbrechen",
            DialogResult = DialogResult.Cancel,
            Size = new Size(95, 32),
            Location = new Point(367, 112)
        };

        okButton.Click += (_, e) =>
        {
            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MessageBox.Show(this, "Bitte geben Sie einen Dashboardnamen ein.", "Name fehlt",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.None;
            }
        };

        Controls.AddRange([label, _nameTextBox, hint, okButton, cancelButton]);
        AcceptButton = okButton;
        CancelButton = cancelButton;

        ThemeManager.ApplyControl(this, palette);
    }
}
