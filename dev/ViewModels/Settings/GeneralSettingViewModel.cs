using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ArtWork.ViewModels;
public partial class GeneralSettingViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string ArtWorkFolderPath { get; set; } = Settings.ArtWorkDirectory;

    [RelayCommand]
    private async Task OnLaunchArtWorkPath()
    {
        await Launcher.LaunchUriAsync(new Uri(Settings.ArtWorkDirectory));
    }

    [RelayCommand]
    private async Task OnChooseArtWorkPath()
    {
        DevWinUI.FolderPicker folderPicker = new(App.MainWindow);
        StorageFolder folder = await folderPicker.PickSingleFolderAsync();
        if (folder is not null)
        {
            Settings.ArtWorkDirectory = folder.Path;
            ArtWorkFolderPath = folder.Path;
        }
    }
}
