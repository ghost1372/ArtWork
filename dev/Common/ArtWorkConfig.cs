using Nucs.JsonSettings.Examples;
using Nucs.JsonSettings.Modulation;

namespace ArtWork.Common;

[GenerateAutoSaveOnChange]
public partial class ArtWorkConfig : NotifiyingJsonSettings, IVersionable
{
    [EnforcedVersion("2.3.1.0")]
    public Version Version { get; set; } = new Version(2, 3, 1, 0);
    public string fileName { get; set; } = Constants.AppConfigPath;

    public string artWorkDirectory { get; set; } = Constants.ArtWorkPath;
    public int availableArtWorkCount { get; set; } = 10431;
    public bool isShowNudes { get; set; } = false;
    public bool isShowOnlyNudes { get; set; } = false;
}
