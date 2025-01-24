namespace ArtWork.Views;

public sealed partial class ArtWorkPage : Page
{
    public static ArtWorkPage Instance { get; private set; }
    public ArtWorkViewModel ViewModel { get; set; }
    public ArtWorkPage()
    {
        ViewModel = App.GetService<ArtWorkViewModel>();
        this.InitializeComponent();
        DataContext = ViewModel;
        Instance = this;
    }
}
