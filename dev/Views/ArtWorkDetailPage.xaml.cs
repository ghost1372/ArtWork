using ArtWork.Database.Tables;

using Microsoft.UI.Xaml.Controls;

namespace ArtWork.Views;

public sealed partial class ArtWorkDetailPage : Page
{
    public ArtWorkDetailViewModel ViewModel { get; private set; }
    public ArtWorkDetailPage()
    {
        ViewModel = App.GetService<ArtWorkDetailViewModel>();
        this.InitializeComponent();
        DataContext = this;
        SlideShowDialog.XamlRoot = App.currentWindow.Content.XamlRoot;
        ViewModel.SlideShowDialog = SlideShowDialog;
    }

    private void GridView_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
    {
        FrameworkElement element = e.OriginalSource as FrameworkElement;
        if (element != null)
        {
            var itemContainer = FindParent<ItemContainer>(element);
            if (itemContainer != null)
            {
                var art = itemContainer.DataContext as Art;
                var itemIndex = ViewModel.ArtsACV.IndexOf(art);
                if (itemIndex != -1)
                {
                    ArtItemsView.Select(itemIndex);
                }
            }
        }
    }

    private async void Grid_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        await ViewModel.OpenImageCommand.ExecuteAsync(null);
    }

    private void ArtItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        ViewModel.SelectedItem = sender.SelectedItem;
    }

    private void menuOpenImage_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenImageCommand.Execute(null);
    }

    private void menuGoToDirectory_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateToDirectoryCommand.Execute(null);
    }

    private void menuGotoFile_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateToFileCommand.Execute(null);
    }

    private void menuSetWallpaper_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SetWallpaperCommand.Execute(null);
    }

    private void menuSetSlideShow_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SetSlideShowCommand.Execute(null);
    }
}
