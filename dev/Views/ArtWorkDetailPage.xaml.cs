using ArtWork.Views.UserControls;
using CommunityToolkit.WinUI.Controls;

namespace ArtWork.Views;

public sealed partial class ArtWorkDetailPage : Page
{
    public ArtWorkDetailViewModel ViewModel { get; private set; }
    public ArtCommandBarViewModel ArtCommandBarViewModel { get; private set; }
    public ArtWorkDetailPage()
    {
        ViewModel = App.GetService<ArtWorkDetailViewModel>();
        this.InitializeComponent();
        DataContext = ViewModel;
        ArtCommandBarViewModel = ArtCommandBar.Instance.ViewModel;

        var dialog = new SlideShowDialog();
        dialog.CommandBarViewModel = ArtCommandBarViewModel;
        ArtCommandBarViewModel.SlideShowDialog = dialog;
        Loaded += GalleryPage_Loaded;
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

    private void ArtItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        ArtCommandBarViewModel.SelectedItem = sender.SelectedItem;
    }

    private void menuOpenImage_Click(object sender, RoutedEventArgs e)
    {
        ArtCommandBarViewModel.OpenImageCommand.Execute(null);
    }

    private void menuGoToDirectory_Click(object sender, RoutedEventArgs e)
    {
        ArtCommandBarViewModel.NavigateToDirectoryCommand.Execute(null);
    }

    private void menuGotoFile_Click(object sender, RoutedEventArgs e)
    {
        ArtCommandBarViewModel.NavigateToFileCommand.Execute(null);
    }

    private void menuSetWallpaper_Click(object sender, RoutedEventArgs e)
    {
        ArtCommandBarViewModel.SetWallpaperCommand.Execute(null);
    }

    private void menuSetSlideShow_Click(object sender, RoutedEventArgs e)
    {
        ArtCommandBarViewModel.SetSlideShowCommand.Execute(null);
    }

    private void Segmented_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = SegmentedFilter.SelectedItem as SegmentedItem;
        switch (item.Tag.ToString())
        {
            case "NoNudes":
                Settings.IsShowNudes = false;
                Settings.IsShowOnlyNudes = false;
                ViewModel.GetArts(false);
                break;
            case "ShowAll":
                Settings.IsShowNudes = true;
                Settings.IsShowOnlyNudes = false;
                ViewModel.GetArts(true);
                break;
        }
    }

    private void GalleryPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (Settings.IsShowNudes)
        {
            SegmentedFilter.SelectedItem = SegmentedFilter.Items.Cast<SegmentedItem>().Where(x => x.Tag.ToString().Equals("ShowAll")).FirstOrDefault();
        }
        else
        {
            SegmentedFilter.SelectedItem = SegmentedFilter.Items.Cast<SegmentedItem>().Where(x => x.Tag.ToString().Equals("NoNudes")).FirstOrDefault();
        }
    }
}
