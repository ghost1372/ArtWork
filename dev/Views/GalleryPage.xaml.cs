namespace ArtWork.Views;

public sealed partial class GalleryPage : Page
{
    public static GalleryPage Instance { get; set; }
    public GalleryViewModel ViewModel { get; set; }
    public GalleryPage()
    {
        ViewModel = App.GetService<GalleryViewModel>();
        this.InitializeComponent();
        Instance = this;
    }
}
