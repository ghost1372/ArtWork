namespace ArtWork.Views;

public sealed partial class DataBasePage : Page
{
    private DataBaseViewModel ViewModel { get; set; }
    public DataBasePage()
    {
        ViewModel = App.GetService<DataBaseViewModel>();
        this.InitializeComponent();
    }
}
