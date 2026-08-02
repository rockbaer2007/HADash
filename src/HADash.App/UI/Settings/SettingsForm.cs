using HADashboardBackupExporteurUGSo.Settings;
using HADashboardBackupExporteurUGSo.UI.Controls;

namespace HADashboardBackupExporteurUGSo;

internal sealed class SettingsForm : Form
{
    private readonly AppSettings _working;
    private readonly SettingsSidebar _navigation = new();
    private readonly Panel _content = new() { Dock = DockStyle.Fill, Padding = new Padding(8) };
    private readonly List<(string Name, SettingsPage Page, bool Enabled)> _pages;
    public AppSettings ResultSettings => _working;

    public SettingsForm(AppSettings current)
    {
        _working = SettingsManager.CreateWorkingCopy(current);
        Text = "Einstellungen";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(960, 700);
        MinimumSize = new Size(900, 650);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        Font = UgsoUi.DefaultFont;

        _pages =
        [
            ("⚙  Allgemein", new GeneralPage(), true),
            ("🖥  Darstellung", new AppearancePage(), true),
            ("📂  Dateien", new FilePage(), true),
            ("📝  Protokoll", new LogPage(), true),
            ("🔄  Updates", new UpdatePage(), false),
            ("ℹ  Info", new AboutPage(), true)
        ];

        foreach (var item in _pages)
        {
            item.Page.LoadFrom(_working);
            _navigation.AddItem(item.Name, item.Enabled);
        }
        _navigation.SelectedIndexChanged += index => ShowPage(index);

        var ok = UgsoUi.CreateDialogButton("OK", DialogResult.OK);
        var cancel = UgsoUi.CreateDialogButton("Abbrechen", DialogResult.Cancel);
        ok.Click += (_, _) =>
        {
            foreach (var page in _pages)
                page.Page.SaveTo(_working);
        };

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 10, 16, 10)
        };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(ok);

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.Controls.Add(_navigation, 0, 0);
        body.Controls.Add(_content, 1, 0);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        layout.Controls.Add(body, 0, 0);
        layout.Controls.Add(buttons, 0, 1);
        Controls.Add(layout);

        AcceptButton = ok;
        CancelButton = cancel;
        _navigation.Select(0, raiseEvent: true);
    }

    private void ShowPage(int index)
    {
        if (index < 0 || index >= _pages.Count || !_pages[index].Enabled) return;
        var page = _pages[index].Page;
        _content.SuspendLayout();
        _content.Controls.Clear();
        _content.Controls.Add(page);
        _content.ResumeLayout();
    }
}
