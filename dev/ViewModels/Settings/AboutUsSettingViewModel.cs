namespace ArtWork.ViewModels;
public partial class AboutUsSettingViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string AppInfo { get; set; } = ProcessInfoHelper.ProductNameAndVersion;
}
