using Nucs.JsonSettings;
using Nucs.JsonSettings.Modulation;

namespace ArtWork.Common;
public class ArtWorkConfig : JsonSettings, IVersionable
{
    [EnforcedVersion("2.0.0.0")]
    public virtual Version Version { get; set; } = new Version(2, 0, 0, 0);
    public override string FileName { get; set; } = Constants.AppConfigPath;

    public virtual string ArtWorkDirectory { get; set; } = Constants.ArtWorkPath;
    public virtual int AvailableArtWorkCount { get; set; } = 10430;
    public virtual bool IsShowNudes { get; set; } = true;
    public virtual bool IsShowOnlyNudes { get; set; } = false;
}
