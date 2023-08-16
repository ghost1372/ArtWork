namespace ArtWork.Views;

public sealed partial class ArtWorkDetailPage : Page
{
    public ArtWorkDetailViewModel ViewModel { get; private set; }
    public ArtWorkDetailPage()
    {
        ViewModel = App.GetService<ArtWorkDetailViewModel>();
        this.InitializeComponent();
    }
}
