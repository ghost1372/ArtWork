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
            GridViewItem clickedItem = FindParent<GridViewItem>(element);
            if (clickedItem != null)
            {
                ArtGridView.SelectedItem = clickedItem.Content;
            }
        }
    }

    private async void Grid_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        await ViewModel.OpenImageCommand.ExecuteAsync(null);
    }
}
