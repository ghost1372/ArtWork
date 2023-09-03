using ArtWork.Views.UserControls;

namespace ArtWork.Views;

public sealed partial class GalleryPage : Page
{
    public static GalleryPage Instance { get; set; }
    public GalleryViewModel ViewModel { get; set; }
    public ArtCommandBarViewModel ArtCommandBarViewModel { get; private set; }
    public GalleryPage()
    {
        ViewModel = App.GetService<GalleryViewModel>();
        this.InitializeComponent();
        Instance = this;
        ArtCommandBarViewModel = ArtCommandBar.Instance.ViewModel;

        var dialog = new SlideShowDialog();
        dialog.CommandBarViewModel = ArtCommandBarViewModel;
        ArtCommandBarViewModel.SlideShowDialog = dialog;
    }

    private void ArtItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        ArtCommandBarViewModel.SelectedItem = sender.SelectedItem;
    }

    private void GridView_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
    {
        var art = GetArtFromRightClick(e);
        if (art != null)
        {
            var itemIndex = ViewModel.ArtsACV.IndexOf(art);
            if (itemIndex != -1)
            {
                ArtItemsView.Select(itemIndex);
            }
        }
    }

    private async void Grid_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        await ArtCommandBarViewModel.OpenImageCommand.ExecuteAsync(null);
    }

    private void OnOpenImage(object sender, RoutedEventArgs e)
    {
        ArtCommandBarViewModel.OpenImageCommand.Execute("");
    }

    private void OnNavigateToDirectory(SplitButton sender, SplitButtonClickEventArgs args)
    {
        ArtCommandBarViewModel.NavigateToDirectoryCommand.Execute("");
    }

    private void OnNavigateToFile(object sender, RoutedEventArgs e)
    {
        ArtCommandBarViewModel.NavigateToFileCommand.Execute("");
    }

    private void OnSetWallpaper(object sender, RoutedEventArgs e)
    {
        ArtCommandBarViewModel.SetWallpaperCommand.Execute(sender);
    }

    private void OnSetWallpaperDefault(SplitButton sender, SplitButtonClickEventArgs args)
    {
        ArtCommandBarViewModel.SetWallpaperCommand.Execute("");
    }
}
