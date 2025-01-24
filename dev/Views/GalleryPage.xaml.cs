using ArtWork.Views.UserControls;
using CommunityToolkit.WinUI.Controls;

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
        DataContext = ViewModel;
        Instance = this;
        ArtCommandBarViewModel = ArtCommandBar.Instance.ViewModel;

        var dialog = new SlideShowDialog();
        dialog.CommandBarViewModel = ArtCommandBarViewModel;
        ArtCommandBarViewModel.SlideShowDialog = dialog;
        Loaded += GalleryPage_Loaded;
    }

    private void GalleryPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (Settings.IsShowOnlyNudes)
        {
            SegmentedFilter.SelectedItem = SegmentedFilter.Items.Cast<SegmentedItem>().Where(x => x.Tag.ToString().Equals("OnlyNudes")).FirstOrDefault();
        }
        else if (Settings.IsShowNudes && !Settings.IsShowOnlyNudes)
        {
            SegmentedFilter.SelectedItem = SegmentedFilter.Items.Cast<SegmentedItem>().Where(x => x.Tag.ToString().Equals("ShowAll")).FirstOrDefault();
        }
        else if (!Settings.IsShowNudes)
        {
            SegmentedFilter.SelectedItem = SegmentedFilter.Items.Cast<SegmentedItem>().Where(x => x.Tag.ToString().Equals("NoNudes")).FirstOrDefault();
        }
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
            var itemIndex = ViewModel.Arts.IndexOf(art);
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

    private void Segmented_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = SegmentedFilter.SelectedItem as SegmentedItem;
        switch (item.Tag.ToString())
        {
            case "NoNudes":
                Settings.IsShowNudes = false;
                Settings.IsShowOnlyNudes = false;
                ViewModel.GetArts(false, false);
                break;
            case "ShowAll":
                Settings.IsShowNudes = true;
                Settings.IsShowOnlyNudes = false;
                ViewModel.GetArts(true, false);
                break;
            case "OnlyNudes":
                Settings.IsShowNudes = true;
                Settings.IsShowOnlyNudes = true;
                ViewModel.GetArts(true, true);
                break;
        }
    }
}
