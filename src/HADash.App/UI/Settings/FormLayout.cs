namespace HADashboardBackupExporteurUGSo.Settings;

internal static class FormLayout
{
    public static TableLayoutPanel Create()
    {
        var panel = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            ColumnCount = 2,
            Padding = new Padding(0, 8, 0, 0),
            GrowStyle = TableLayoutPanelGrowStyle.AddRows,
            Margin = Padding.Empty
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return panel;
    }

    public static void Add(TableLayoutPanel panel, string label, Control control)
    {
        var row = panel.RowCount++;
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            MaximumSize = new Size(205, 0),
            Margin = new Padding(0, 10, 15, 14)
        }, 0, row);

        control.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        control.Margin = new Padding(0, 5, 0, 14);
        panel.Controls.Add(control, 1, row);
    }
}
