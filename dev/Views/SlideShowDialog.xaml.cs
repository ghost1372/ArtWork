namespace ArtWork.Views;

public sealed partial class SlideShowDialog : ContentDialog
{
    public ArtCommandBarViewModel CommandBarViewModel { get; set; }
    public SlideShowDialog()
    {
        this.InitializeComponent();
        XamlRoot = App.MainWindow.Content.XamlRoot;
    }
}
