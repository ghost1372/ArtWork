using System.Text;
using System.Text.Json;

using ArtWork.Common;
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
    private int totalProgressMax;

    [ObservableProperty]
    private string previewImage;

    private readonly Queue<ArtWorkUrl> _downloadUrls = new Queue<ArtWorkUrl>();
    private IDownload download;
    private DownloadPackage downloadPack;
    private string directoryName;
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
            using (FileStream fs = new FileStream(file.Path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    for (int i = 0; i < urls.ImageUrls.Count; i++)
                    {
                        await writer.WriteLineAsync(urls.ImageUrls[i]);
                        await writer.WriteLineAsync(urls.JsonUrls[i]);
                    }
                }
            }
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
            ErrorMessage = ex.Message;
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

        foreach (string item in existFiles)
        {
            var removeImage = urls.ImageUrls.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).Equals(Path.GetFileNameWithoutExtension(item)));
            var removeJson = urls.JsonUrls.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).Equals(Path.GetFileNameWithoutExtension(item)));
            urls.ImageUrls.Remove(removeImage);
            urls.JsonUrls.Remove(removeJson);
        }

        for (int i = 0; i < urls.ImageUrls.Count; i++)
        {
            _downloadUrls.Enqueue(new ArtWorkUrl(urls.JsonUrls[i], urls.ImageUrls[i]));
        }

        TotalProgressMax = _downloadUrls.Count;
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
                    PreviewImage = url.ImageUrl;
                    using var client = new HttpClient();
                    var json = await client.GetStringAsync(url.JsonUrl);
                    var artWorkJson = JsonSerializer.Deserialize<ArtWorkModel>(json);

                    var sig = artWorkJson?.wikiartist;

                    if (string.IsNullOrEmpty(sig))
                    {
                        int index = artWorkJson.sig.IndexOf(',');
                        if (index > 0)
                        {
                            sig = artWorkJson.sig.Substring(0, index);
                        }

                        if (string.IsNullOrEmpty(sig))
                        {
                            sig = "Unknown Artist";
                        }
                    }

                    directoryName = string.Join("", sig.Split(Path.GetInvalidFileNameChars()));

                    var artistDir = Path.Combine(Settings.ArtWorkDirectory, directoryName);
                    if (!Directory.Exists(artistDir))
                    {
                        Directory.CreateDirectory(artistDir);
                    }

                    using (StreamWriter writer = new StreamWriter(Path.Combine(artistDir, Path.GetFileName(url.JsonUrl)), false, Encoding.UTF8))
                    await writer.WriteAsync(json);

                    download = DownloadBuilder.New()
                        .WithUrl(url.ImageUrl)
                        .WithConfiguration(new DownloadConfiguration
                        {
                            ChunkCount = 8,
                            ParallelDownload = true,
                        })
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
                    ErrorMessage = ex.Message;
                }
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
            ErrorMessage = string.Empty;
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

    private void RemoveInCompleteFiles()
    {
        IEnumerable<string> imageFiles = Directory.EnumerateFiles(Settings.ArtWorkDirectory, "*.jpg", SearchOption.AllDirectories);
        IEnumerable<string> jsonFiles = Directory.EnumerateFiles(Settings.ArtWorkDirectory, "*.json", SearchOption.AllDirectories);

        foreach (var item in imageFiles)
        {
            var jsonNotExist = jsonFiles.Any(x => Path.GetFileNameWithoutExtension(x).Equals(Path.GetFileNameWithoutExtension(item)));
            if (!jsonNotExist)
            {
                File.Delete(item);
            }
        }

        foreach (var item in jsonFiles)
        {
            var imageNotExist = imageFiles.Any(x => Path.GetFileNameWithoutExtension(x).Equals(Path.GetFileNameWithoutExtension(item)));
            if (!imageNotExist)
            {
                File.Delete(item);
            }
        }
    }
}
