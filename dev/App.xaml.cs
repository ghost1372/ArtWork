﻿namespace ArtWork;

public partial class App : Application
{
    public static Window MainWindow = Window.Current;
    public IServiceProvider Services { get; }
    public new static App Current => (App)Application.Current;
    public IJsonNavigationService GetJsonNavigationService => GetService<IJsonNavigationService>();
    public IThemeService GetThemeService => GetService<IThemeService>();

    public static T GetService<T>() where T : class
    {
        if ((App.Current as App)!.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public App()
    {
        Services = ConfigureServices();
        this.InitializeComponent();
        if (!Directory.Exists(Settings.ArtWorkDirectory))
        {
            Directory.CreateDirectory(Settings.ArtWorkDirectory);
        }
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new Window();

        if (MainWindow.Content is not Frame rootFrame)
        {
            MainWindow.Content = rootFrame = new Frame();
        }

        if (GetThemeService != null)
        {
            GetThemeService.AutoInitialize(MainWindow)
                .ConfigureTintColor();
        }

        rootFrame.Navigate(typeof(MainPage));

        MainWindow.Title = MainWindow.AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;
        MainWindow.AppWindow.SetIcon("Assets/icon.ico");

        MainWindow.Activate();

        await AddNudesIntoDataBase();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IJsonNavigationService, JsonNavigationService>();

        services.AddTransient<DownloadViewModel>();
        services.AddTransient<DataBaseViewModel>();
        services.AddTransient<ArtWorkViewModel>();
        services.AddTransient<ArtWorkDetailViewModel>();
        services.AddTransient<GalleryViewModel>();
        services.AddTransient<ArtCommandBarViewModel>();

        //Settings
        services.AddTransient<AboutUsSettingViewModel>();
        services.AddTransient<GeneralSettingViewModel>();

        return services.BuildServiceProvider();
    }
}

