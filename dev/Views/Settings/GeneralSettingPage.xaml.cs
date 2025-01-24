namespace ArtWork.Views;

public sealed partial class GeneralSettingPage : Page
{
    private GeneralSettingViewModel ViewModel { get; set; }
    public GeneralSettingPage()
    {
        ViewModel = App.GetService<GeneralSettingViewModel>();
        this.InitializeComponent();
    }
}
