using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ArtWork
{
    /// <summary>
    /// Interaction logic for Downloader.xaml
    /// </summary>
    public partial class Downloader
    {
        private readonly int NumberOfAllItemExist = 9300;
        public readonly string imagesBaseLine = "https://kraken99.blob.core.windows.net/images4000xn/";

        private Queue<string> _downloadUrls = new Queue<string>();

        ObservableCollection<string> generatedLinks = new ObservableCollection<string>();

        public Downloader()
        {
            InitializeComponent();

            // Generate All Items
            for (int i = 1; i < NumberOfAllItemExist; i++)
            {
                generatedLinks.Add(imagesBaseLine + i + ".jpg");
            }

            //Get Exist Items
            var existItems = GetFileList(Environment.CurrentDirectory + @"\data");
            shDownloadedItem.Status = existItems.Count();

            //Remove Exist Item From Generated Links
            foreach (var item in existItems)
            {
                generatedLinks.Remove(imagesBaseLine + System.IO.Path.GetFileName(item));
            }
        }
      

        // 1-Get all items number ====================> Done
        // 2-Get all folders and subfolders files ====> Done
        // 3-calculate Downloaded Items ==============> Done
        // 4-DeSelect Downloaded Items ===============> Done
        // 5-Get Json Info
        // 6-Fix Json Invalid
        // 7-Download Jpg
        // Check File Exist
        // 8-Write Properties
        // 9-Put in Directory
        public IEnumerable<string> GetFileList(string rootFolderPath)
        {
            Queue<string> pending = new Queue<string>();
            pending.Enqueue(rootFolderPath);
            string[] tmp;
            while (pending.Count > 0)
            {
                rootFolderPath = pending.Dequeue();
                try
                {
                    tmp = Directory.GetFiles(rootFolderPath);
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                for (int i = 0; i < tmp.Length; i++)
                {
                    yield return tmp[i];
                }
                tmp = Directory.GetDirectories(rootFolderPath);
                for (int i = 0; i < tmp.Length; i++)
                {
                    pending.Enqueue(tmp[i]);
                }
            }
        }
     
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            prgButton.IsEnabled = false;
            prgButton.Content = "Downloading...";
            downloadFile(generatedLinks);
        }

        private void downloadFile(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                _downloadUrls.Enqueue(url);
            }

            // Starts the download
            
            DownloadFile();
        }
        private void DownloadFile()
        {
            if (_downloadUrls.Any())
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += Client_DownloadProgressChanged; ;
                client.DownloadFileCompleted += Client_DownloadFileCompleted; ;

                var url = _downloadUrls.Dequeue();
                string FileName = url.Substring(url.LastIndexOf("/") + 1,
                            (url.Length - url.LastIndexOf("/") - 1));

                client.DownloadFileAsync(new Uri(url), Environment.CurrentDirectory + @"\data\" + FileName);
                return;
            }

            // End of the download
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // handle error scenario
                throw e.Error;
            }
            if (e.Cancelled)
            {
                // handle cancelled scenario
            }
            shDownloadedItem.Status = Convert.ToInt32(shDownloadedItem.Status) + 1;
            // Handle Rename 

            //

            DownloadFile();
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            prgButton.Progress = int.Parse(Math.Truncate(percentage).ToString());
        }
    }
}
