using YamlDotNet.RepresentationModel;

namespace HADashboardBackupExporteurUGSo;

internal sealed class DashboardViewItem
{
    public required int Index { get; init; }
    public required string DisplayName { get; init; }
    public required YamlNode Node { get; init; }

    public override string ToString() => DisplayName;
}
