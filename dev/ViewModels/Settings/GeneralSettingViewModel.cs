using Windows.Storage;
using Windows.Storage.Pickers;

namespace ArtWork.ViewModels;
public partial class GeneralSettingViewModel : ObservableObject
{
    [ObservableProperty]
    public string artWorkFolderPath = Settings.ArtWorkDirectory;

    [RelayCommand]
    private async Task OnLaunchArtWorkPath()
    {
        await Launcher.LaunchUriAsync(new Uri(Settings.ArtWorkDirectory));
    }

    [RelayCommand]
    private async Task OnChooseArtWorkPath()
    {
        FolderPicker folderPicker = new();
        folderPicker.FileTypeFilter.Add("*");

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, WindowHelper.GetWindowHandleForCurrentWindow(App.currentWindow));

        StorageFolder folder = await folderPicker.PickSingleFolderAsync();
        if (folder is not null)
        {
            Settings.ArtWorkDirectory = folder.Path;
            ArtWorkFolderPath = folder.Path;
        }
    }
}
