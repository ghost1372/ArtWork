namespace ArtWork.Views;

public sealed partial class DownloadPage : Page
{
    private DownloadViewModel ViewModel { get; set; }
    public DownloadPage()
    {
        ViewModel = App.GetService<DownloadViewModel>();
        this.InitializeComponent();
    }
}
