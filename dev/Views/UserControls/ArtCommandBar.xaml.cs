namespace ArtWork.Views.UserControls;
public sealed partial class ArtCommandBar : UserControl
{
    public static ArtCommandBar Instance { get; private set; }
    public ArtCommandBarViewModel ViewModel { get; set; }
    public ArtCommandBar()
    {
        ViewModel = App.GetService<ArtCommandBarViewModel>();
        this.InitializeComponent();
        Instance = this;
    }
}
