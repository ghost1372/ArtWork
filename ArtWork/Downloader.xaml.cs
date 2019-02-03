using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Newtonsoft.Json;
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
        
        public static readonly string NudPath = @"C:\Users\Mahdi\Desktop\nudes.txt";

        private Queue<string> _downloadUrls = new Queue<string>();

        ObservableCollection<string> generatedLinks = new ObservableCollection<string>();

        public string title { get; set; }
        public string sig { get; set; }
        public string gal { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public double lat { get; set; }
        public double @long { get; set; }
        public string @Imagepath { get; set; }
        public string @JsonPath { get; set; }
        public string wikiartist { get; set; }

        public Downloader()
        {
            InitializeComponent();

            GlobalData.Init();
           

            // Generate All Items
            for (int i = 1; i < AppVar.NumberOfAllItemExist; i++)
            {
                generatedLinks.Add(AppVar.imagesBaseUrl + i + ".jpg");
            }

            //Get Exist Items
            var existItems = GetFileList(GlobalData.Config.DataPath);
            shDownloadedItem.Status = existItems.Count();

            //Remove Exist Item From Generated Links
            foreach (var item in existItems)
            {
                generatedLinks.Remove(AppVar.imagesBaseUrl + System.IO.Path.GetFileName(item));
            }
        }


        // 1-Get all items number ====================> Done
        // 2-Get all folders and subfolders files ====> Done
        // 3-calculate Downloaded Items ==============> Done
        // 4-DeSelect Downloaded Items ===============> Done
        // 5-Get Json Info ===========================> Done
        // 6-Fix Json Invalid ========================> Fixed
        // 7-Download Jpg with Fixed Name ============> Fixed
        // Check File Exist ==========================> Fixed
        // Check Nudus ===============================> Fixed
        // 8-Write Properties ========================> Fixed
        // 9-Put in Directory ========================> Fixed
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

        public class RootObject
        {
            public string title { get; set; }
            public string sig { get; set; }
            public string gal { get; set; }
            public string city { get; set; }
            public string country { get; set; }
            public double lat { get; set; }
            public double @long { get; set; }
            public string wiki { get; set; }
            public string wikiartist { get; set; }
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

                var json = client.DownloadString(AppVar.jsonBaseUrl + System.IO.Path.GetFileNameWithoutExtension(url) + ".json");
                var root = JsonConvert.DeserializeObject<RootObject>(json);
                shTitle.Status = root.title;
                shSubject.Status = root.sig;

                title = root.title;
                sig = root.sig;
                wikiartist = root.wikiartist;
                country = root.country;
                city = root.city;
                gal = root.gal;
                @long = root.@long;
                lat = root.lat;
                @Imagepath = GlobalData.Config.DataPath + @"\" + FileName;
                @JsonPath = AppVar.jsonBaseUrl + System.IO.Path.GetFileNameWithoutExtension(url) + ".json";

                client.DownloadFileAsync(new Uri(url), GlobalData.Config.DataPath + @"\" + FileName);
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

            var file = ShellFile.FromFilePath(@Imagepath);

            string isNude = "NOTNUDE";

          
            //Check Nud

            if (DomainExists(System.IO.Path.GetFileNameWithoutExtension(@JsonPath), NudPath))
            {
                isNude = "ITSNUDE";

            }
            else
            {
                isNude = "NOTNUDE";

            }


            var date = sig.Substring(sig.LastIndexOf(',') + 1);
            

            try
            {
                //Set Attrib
                ShellPropertyWriter propertyWriter = file.Properties.GetPropertyWriter();
                propertyWriter.WriteProperty(SystemProperties.System.Title, title ?? "Empty");
                propertyWriter.WriteProperty(SystemProperties.System.Subject, sig ?? "Empty");
                propertyWriter.WriteProperty(SystemProperties.System.Comment, gal ?? "Empty");
                propertyWriter.WriteProperty(SystemProperties.System.Author, wikiartist ?? "Unknown Artist");
                propertyWriter.WriteProperty(SystemProperties.System.Keywords, new string[] {
                   city ?? "Location Unknown", country ?? "Location Unknown", lat.ToString() ?? "Empty",
                   @long.ToString() ?? "Empty", sig ?? "Empty", title ?? "Empty",
                   wikiartist ?? "Empty", gal ?? "Empty", isNude ?? "Empty", date ?? "Empty"
                });

                propertyWriter.Close();
            }
            catch (Exception)
            {
            }
            string cleanFileName = String.Join("", wikiartist.Split(System.IO.Path.GetInvalidFileNameChars()));
            if (!Directory.Exists(GlobalData.Config.DataPath + @"\" + cleanFileName))
            {
                Directory.CreateDirectory(GlobalData.Config.DataPath + @"\" + cleanFileName);
            }
            File.Move(@Imagepath, GlobalData.Config.DataPath + @"\" + cleanFileName + @"\" + System.IO.Path.GetFileName(@Imagepath));

            //

            DownloadFile();
        }
        private static bool DomainExists(string domain, string path)
        {
            foreach (string line in File.ReadLines(path))
                if (domain == line)
                    return true; // and stop reading lines

            return false;
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
