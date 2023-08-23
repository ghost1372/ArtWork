namespace ArtWork.ViewModels;
public partial class HomeLandingViewModel : ObservableObject
{
    [ObservableProperty]
    private string headerCover;

    public IJsonNavigationViewService JsonNavigationViewService;
    public HomeLandingViewModel(IJsonNavigationViewService jsonNavigationViewService)
    {
        JsonNavigationViewService = jsonNavigationViewService;
        GetRandomHeaderCover();
    }

    [RelayCommand]
    private void OnItemClick(RoutedEventArgs e)
    {
        var args = (ItemClickEventArgs)e;
        var item = (DataItem)args.ClickedItem;

        JsonNavigationViewService.NavigateTo(item.UniqueId + item.Parameter?.ToString(), item);
    }

    private void GetRandomHeaderCover()
    {
        Task.Run(() =>
        {
            if (ImageFilesCacheForHomeLandingPage == null)
            {
                ImageFilesCacheForHomeLandingPage = Directory.EnumerateFiles(Settings.ArtWorkDirectory, "*.jpg", SearchOption.AllDirectories).ToArray();
            }

            if (ImageFilesCacheForHomeLandingPage.Any())
            {
                Random random = new Random();
                int randomIndex = random.Next(0, ImageFilesCacheForHomeLandingPage.Count());

                HeaderCover = ImageFilesCacheForHomeLandingPage[randomIndex];
            }
        });
    }
}
