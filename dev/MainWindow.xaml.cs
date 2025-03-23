namespace ArtWork;

public sealed partial class MainWindow : Window
{
    internal static MainWindow Instance { get; private set; }
    public MainWindow()
    {
        this.InitializeComponent();
        Instance = this;

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        Title = AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;
        AppWindow.SetIcon("Assets/icon.ico");

        var NavService = App.GetService<IJsonNavigationService>() as JsonNavigationService;
        if (NavService != null)
        {
            NavService.Initialize(NavView, NavFrame, NavigationPageMappings.PageDictionary)
                .ConfigureDefaultPage(typeof(HomeLandingPage))
                .ConfigureSettingsPage(typeof(SettingsPage))
                .ConfigureJsonFile("Assets/NavViewMenu/AppData.json")
                .ConfigureTitleBar(AppTitleBar)
                .ConfigureBreadcrumbBar(BreadCrumbNav, BreadcrumbPageMappings.PageDictionary);
        }
    }

    private void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxTextChangedEvent(sender, args, NavFrame);
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxQuerySubmittedEvent(sender, args, NavFrame);
    }

    private void KeyboardAccelerator_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        HeaderAutoSuggestBox.Focus(FocusState.Programmatic);
    }

    public AutoSuggestBox GetAutoSuggestBox()
    {
        return HeaderAutoSuggestBox;
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        ThemeService.ChangeThemeWithoutSave(this);
    }
}
