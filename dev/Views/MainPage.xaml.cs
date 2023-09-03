namespace ArtWork.Views;

public sealed partial class MainPage : Page
{
    public static MainPage Instance { get; set; }
    public MainViewModel ViewModel { get; }
    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        this.InitializeComponent();
        Instance = this;
        appTitleBar.Window = App.currentWindow;
        ViewModel.JsonNavigationViewService.Initialize(NavView, NavFrame);
        ViewModel.JsonNavigationViewService.ConfigJson("Assets/NavViewMenu/AppData.json");
    }

    private void appTitleBar_BackButtonClick(object sender, RoutedEventArgs e)
    {
        if (NavFrame.CanGoBack)
        {
            NavFrame.GoBack();
        }
    }

    private void appTitleBar_PaneButtonClick(object sender, RoutedEventArgs e)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    private void NavFrame_Navigated(object sender, NavigationEventArgs e)
    {
        appTitleBar.IsBackButtonVisible = NavFrame.CanGoBack;
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        var element = App.currentWindow.Content as FrameworkElement;

        if (element.ActualTheme == ElementTheme.Light)
        {
            element.RequestedTheme = ElementTheme.Dark;
        }
        else if (element.ActualTheme == ElementTheme.Dark)
        {
            element.RequestedTheme = ElementTheme.Light;
        }
    }

    public AutoSuggestBox GetAutoSuggestBox()
    {
        return TxtSearch;
    }

    private void TxtSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        var viewModel = GetCurrentViewModel();
        if (viewModel != null)
        {
            viewModel.Search();
        }
    }

    private dynamic GetCurrentViewModel()
    {
        var rootFrame = ViewModel.JsonNavigationViewService.Frame;
        dynamic root = rootFrame.Content;
        dynamic viewModel = null;
        if (root is ArtWorkPage)
        {
            viewModel = ArtWorkPage.Instance.ViewModel;
        }
        else if (root is GalleryPage)
        {
            viewModel = GalleryPage.Instance.ViewModel;
        }

        return viewModel;
    }
}

