using System.Text;
using System.Text.Json;

using ArtWork.Common;
using ArtWork.Database;
using ArtWork.Database.Tables;
using ArtWork.Models;

using Downloader;

namespace ArtWork.ViewModels;
public partial class DownloadViewModel : ObservableRecipient
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [ObservableProperty]
    private string titleStatus = $"Total ArtWorks: {Settings.AvailableArtWorkCount}";

    [ObservableProperty]
    private string messageStatus;

    [ObservableProperty]
    private string errorMessage;

    [ObservableProperty]
    private int progressValue;

    [ObservableProperty]
    private int totalProgressValue;

    [ObservableProperty]
    private string previewImage;

    [ObservableProperty]
    private int chunkCount = 1;

    [ObservableProperty]
    private bool useParallelDownload;

    [ObservableProperty]
    private bool usePreviewImage;

    private readonly Queue<ArtWorkUrl> _downloadUrls = new Queue<ArtWorkUrl>();
    private IDownload download;
    private DownloadPackage downloadPack;
    private bool isCanceled = false;

    private IJsonNavigationViewService jsonNavigationViewService;
    public DownloadViewModel(IJsonNavigationViewService jsonNavigationViewService)
    {
        this.jsonNavigationViewService = jsonNavigationViewService;
    }

    [RelayCommand]
    private async Task OnNavigateToDirectory()
    {
        await Launcher.LaunchUriAsync(new Uri(Settings.ArtWorkDirectory));
    }

    [RelayCommand]
    private void OnChangeDirectory()
    {
        jsonNavigationViewService.NavigateTo(typeof(GeneralSettingPage));
    }

    [RelayCommand]
    private async Task OnGenerateIDMDownloadFile()
    {
        var urls = GenerateUrls();

        var fileTypeChoices = new Dictionary<string, IList<string>>();
        fileTypeChoices.Add("Text", new List<string> { ".txt" });
        var file = await ApplicationHelper.PickSaveFileAsync(App.currentWindow, fileTypeChoices);
        if (file != null)
        {
            using FileStream fs = new FileStream(file.Path, FileMode.Create);
            using StreamWriter writer = new StreamWriter(fs);
            for (int i = 0; i < urls.ImageUrls.Count; i++)
            {
                await writer.WriteLineAsync(urls.ImageUrls[i]);
                await writer.WriteLineAsync(urls.JsonUrls[i]);
            }
        }
    }

    [RelayCommand]
    private async Task OnValidate()
    {
        IEnumerable<string> onlyImageFileNames = Directory.EnumerateFiles(Settings.ArtWorkDirectory, "*.jpg", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension);
        IEnumerable<string> onlyJsonFileNames = Directory.EnumerateFiles(Settings.ArtWorkDirectory, "*.json", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension);

        var allItems = Enumerable.Range(1, Settings.AvailableArtWorkCount);
        var existingItems = onlyImageFileNames.Select(name => int.Parse(name)).Union(onlyJsonFileNames.Select(name => int.Parse(name)));
        var missingItems = allItems.Except(existingItems);
        
        if (missingItems != null && missingItems.Count() > 0)
        {
            var fileTypeChoices = new Dictionary<string, IList<string>>();
            fileTypeChoices.Add("Text", new List<string> { ".txt" });
            var file = await ApplicationHelper.PickSaveFileAsync(App.currentWindow, fileTypeChoices);
            if (file != null)
            {
                using FileStream fs = new FileStream(file.Path, FileMode.Create);
                using StreamWriter writer = new StreamWriter(fs);
                await writer.WriteLineAsync("The following items have not been downloaded");
                await writer.WriteLineAsync("############################################");
                foreach (var item in missingItems)
                {
                    await writer.WriteLineAsync(item.ToString());
                }
            }
        }
        else
        {
            ContentDialog contentDialog = new ContentDialog();
            contentDialog.XamlRoot = App.currentWindow.Content.XamlRoot;
            contentDialog.Title = "Validate";
            contentDialog.Content = "All files have been downloaded successfully";
            contentDialog.PrimaryButtonText = "Ok";
            await contentDialog.ShowAsync();
        }
    }

    [RelayCommand]
    private async Task OnCancel()
    {
        try
        {
            MessageStatus = "Canceled";
            isCanceled = true;
            IsActive = false;
            ProgressValue = 0;
            download?.Dispose();
            download = null;

            if (File.Exists(downloadPack.FileName))
            {
                await Task.Delay(1500);
                File.Delete(downloadPack.FileName);
            }

            downloadPack.Clear();
        }
        catch (Exception ex)
        {
            ErrorMessage =  $"{ErrorMessage}\n\n{ex.Message} -##- {downloadPack.FileName}";
        }
    }

    [RelayCommand]
    private void OnDownload()
    {
        isCanceled = false;
        IsActive = true;
        var urls = GenerateUrls();

        RemoveInCompleteFiles();

        IEnumerable<string> existFiles = Directory.EnumerateFiles(Settings.ArtWorkDirectory, "*.jpg", SearchOption.AllDirectories);

        var filesToRemove = new HashSet<string>(existFiles.Select(item => Path.GetFileNameWithoutExtension(item)));

        urls.ImageUrls = urls.ImageUrls.Where(imageUrl => !filesToRemove.Contains(Path.GetFileNameWithoutExtension(imageUrl))).ToList();
        urls.JsonUrls = urls.JsonUrls.Where(jsonUrl => !filesToRemove.Contains(Path.GetFileNameWithoutExtension(jsonUrl))).ToList();


        for (int i = 0; i < urls.ImageUrls.Count; i++)
        {
            _downloadUrls.Enqueue(new ArtWorkUrl(urls.JsonUrls[i], urls.ImageUrls[i]));
        }

        TotalProgressValue = existFiles.Count();
        DownloadImage();
    }

    private async void DownloadImage()
    {
        if (!isCanceled)
        {
            if (_downloadUrls.Any())
            {
                try
                {
                    var url = _downloadUrls.Dequeue();
                    if (UsePreviewImage)
                    {
                        PreviewImage = url.ImageUrl;
                    }
                    using var client = new HttpClient();
                    var json = await client.GetStringAsync(url.JsonUrl);
                    var artWorkJson = JsonSerializer.Deserialize<ArtWorkModel>(json);

                    var dir = GetDirectoryName(artWorkJson?.wikiartist, artWorkJson?.sig);

                    var artistDir = Path.Combine(Settings.ArtWorkDirectory, dir.DirectoryName);
                    if (!Directory.Exists(artistDir))
                    {
                        Directory.CreateDirectory(artistDir);
                    }

                    using (StreamWriter writer = new StreamWriter(Path.Combine(artistDir, Path.GetFileName(url.JsonUrl)), false, Encoding.UTF8))
                    await writer.WriteAsync(json);

                    using var db = new ArtWorkDbContext();
                    var art = new Art
                    {
                        FolderName = NormalizeString(dir.DirectoryName),
                        FileName = Path.GetFileName(url.ImageUrl),
                        City = NormalizeString(artWorkJson.city),
                        Country = NormalizeString(artWorkJson.country),
                        Gallery = NormalizeString(artWorkJson.gal),
                        Latitude = artWorkJson.lat,
                        Longitude = artWorkJson.@long,
                        Sig = NormalizeString(artWorkJson.sig),
                        SimplifiedSig = NormalizeString(dir.SimplifiedSig),
                        Title = NormalizeString(artWorkJson.title),
                        Wikiartist = NormalizeString(artWorkJson.wikiartist)
                    };

                    await db.Arts.AddAsync(art);
                    await db.SaveChangesAsync();
                    var downloadConfiguration = new DownloadConfiguration();
                    downloadConfiguration.ParallelDownload = UseParallelDownload;
                    downloadConfiguration.ChunkCount = ChunkCount;

                    download = DownloadBuilder.New()
                        .WithUrl(url.ImageUrl)
                        .WithConfiguration(downloadConfiguration)
                        .WithDirectory(artistDir)
                        .WithFileName(Path.GetFileName(url.ImageUrl))
                        .WithConfiguration(new DownloadConfiguration())
                        .Build();

                    download.DownloadProgressChanged += Download_DownloadProgressChanged;
                    download.DownloadFileCompleted += Download_DownloadFileCompleted;
                    downloadPack = download.Package;

                    MessageStatus = $"Downloading ArtWork: {Path.GetFileName(url.ImageUrl)}";
                    TotalProgressValue++;
                    await download.StartAsync();

                    return;
                }
                catch (Exception ex)
                {
                    DownloadImage();
                    ErrorMessage = $"{ErrorMessage}\n\n{ex.Message} -##- {downloadPack.FileName} ";
                }
            }
            else
            {
                IsActive = false;
                ProgressValue = 0;
                MessageStatus = "All files have been downloaded successfully. Press the Validate button to be more sure";
            }
        }
        else
        {
            IsActive = false;
            ProgressValue = 0;
        }
    }

    private void Download_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            if (e.Cancelled)
            {
                IsActive = false;
                isCanceled = true;
                ProgressValue = 0;
            }
            else
            {
                DownloadImage();
            }
        });
    }

    private void Download_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            ProgressValue = (int)e.ProgressPercentage;
        });
    }

    private (IList<string> ImageUrls, IList<string> JsonUrls) GenerateUrls()
    {
        List<string> imageUrls = new List<string>();
        List<string> jsonUrls = new List<string>();
        for (int i = 1; i <= Settings.AvailableArtWorkCount; i++)
        {
            imageUrls.Add(Constants.ArtWorkBaseUrl + i.ToString() + ".jpg");
            jsonUrls.Add(Constants.JsonBaseUrl + i.ToString() + ".json");
        }
        return (imageUrls, jsonUrls);
    }
}
