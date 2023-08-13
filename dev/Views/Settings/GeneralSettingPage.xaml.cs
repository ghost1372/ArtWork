namespace ArtWork.Views;

public sealed partial class GeneralSettingPage : Page
{
    public string BreadCrumbBarItemText { get; set; }
    private GeneralSettingViewModel ViewModel { get; set; }
    public GeneralSettingPage()
    {
        ViewModel = App.GetService<GeneralSettingViewModel>();
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        BreadCrumbBarItemText = e.Parameter as string;
    }
}
