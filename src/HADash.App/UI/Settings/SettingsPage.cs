using HADashboardBackupExporteurUGSo.UI.Controls;

namespace HADashboardBackupExporteurUGSo.Settings;

internal abstract class SettingsPage : UserControl
{
    protected SettingsPage(string title, string description)
    {
        Dock = DockStyle.Fill;
        Padding = new Padding(28, 24, 28, 24);
        AutoScroll = true;
        AutoScaleMode = AutoScaleMode.Dpi;

        Content = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 2 };
        Content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Content.Controls.Add(new Label { Text = title, AutoSize = true, Font = UgsoUi.HeadingFont, Margin = new Padding(0, 0, 0, 7) }, 0, 0);
        Content.Controls.Add(new Label { Text = description, AutoSize = true, MaximumSize = new Size(650, 0), Margin = new Padding(0, 0, 0, 26) }, 0, 1);
        Controls.Add(Content);
    }

    protected TableLayoutPanel Content { get; }
    protected void AddSection(Control control)
    {
        Content.RowCount++;
        Content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Content.Controls.Add(control, 0, Content.RowCount - 1);
    }
    public abstract void LoadFrom(AppSettings settings);
    public abstract void SaveTo(AppSettings settings);
}
