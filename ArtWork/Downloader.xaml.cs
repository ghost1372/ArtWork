using FileDownloader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public Downloader()
        {
            InitializeComponent();
        }


        // 1-Get all items number
        // 2-Get all folders and subfolders files
        // 3-calculate Downloaded Items
        // 4-DeSelect Downloaded Items
        // 5-Get Json Info
        // 6-Fix Json Invalid
        // 7-Download Jpg
        // 8-Write Properties
        // 9-Put in Directory

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //IFileDownloader fileDownloader = new FileDownloader.FileDownloader();
            //fileDownloader.DownloadFileCompleted += DownloadFileCompleted;
            //fileDownloader.DownloadProgressChanged += OnDownloadProgressChanged;


            //fileDownloader.DownloadFileAsync(new Uri("https://kraken99.blob.core.windows.net/images4000xn/1.jpg"), Environment.CurrentDirectory + @"\data\1.jpg");

            

            
        }
        void OnDownloadProgressChanged(object sender, DownloadFileProgressChangedArgs args)
        {
            Console.WriteLine("Downloaded {0} of {1} bytes", args.BytesReceived, args.TotalBytesToReceive);
        }
        void DownloadFileCompleted(object sender, DownloadFileCompletedArgs eventArgs)
        {
            if (eventArgs.State == CompletedState.Succeeded)
            {
                //download completed
            }
            else if (eventArgs.State == CompletedState.Failed)
            {
                //download failed
            }
        }
    }
}
