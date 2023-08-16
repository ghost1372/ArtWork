namespace ArtWork.Views;

public sealed partial class ArtWorkPage : Page
{
    private ArtWorkViewModel ViewModel { get; set; }
    public ArtWorkPage()
    {
        ViewModel = App.GetService<ArtWorkViewModel>();
        this.InitializeComponent();
    }
}
