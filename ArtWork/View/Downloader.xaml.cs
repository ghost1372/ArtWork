using log4net;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;

namespace ArtWork
{
    /// <summary>
    /// Interaction logic for Downloader.xaml
    /// </summary>
    public partial class Downloader
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Queue<string> _downloadUrls = new Queue<string>();

        ObservableCollection<string> generatedLinks = new ObservableCollection<string>();
        WebClient client = new WebClient();

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

        string DEBUG_LINK_INFO = string.Empty;

        public Downloader()
        {
            InitializeComponent();

            log4net.Config.XmlConfigurator.Configure();
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);

            // Generate All Items
            for (int i = 1; i < AppVar.NumberOfAllItemExist; i++)
            {
                generatedLinks.Add(AppVar.imagesBaseUrl + i + ".jpg");
            }

            //Get Exist Items
            var files = Directory.EnumerateFiles(GlobalData.Config.DataPath, "*.jpg", SearchOption.AllDirectories);

            shDownloadedItem.Status = files.Count();

            //Remove Exist Item From Generated Links
            foreach (var item in files)
            {
                generatedLinks.Remove(AppVar.imagesBaseUrl + System.IO.Path.GetFileName(item));

            }
        }
        static void MyHandler(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error(e.ExceptionObject);
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
     
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            prgButton.IsEnabled = false;
            prgButton.Content = Properties.Langs.Lang.Downloading;
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

               
                try
                {
                    client = new WebClient();
                    client.DownloadProgressChanged += Client_DownloadProgressChanged; ;
                    client.DownloadFileCompleted += Client_DownloadFileCompleted; ;

                    var url = _downloadUrls.Dequeue();
                    DEBUG_LINK_INFO = url;
                    string FileName = url.Substring(url.LastIndexOf("/") + 1,
                                (url.Length - url.LastIndexOf("/") - 1));

                    var json = client.DownloadString(AppVar.jsonBaseUrl + System.IO.Path.GetFileNameWithoutExtension(url) + ".json");
                    var fixJson = json;

                    //Fix for Json 8271
                    if (fixJson.Contains("\"Sunflowers\""))
                        fixJson = fixJson.Replace("\"Sunflowers\"", "Sunflowers");
                    var root = JsonConvert.DeserializeObject<RootObject>(fixJson);
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
                catch (System.Net.WebException ex) {
                    if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                    {
                        DownloadFile();
                    }
                    log.Error("System.NET.WEBEXCEPTION PART1" + Environment.NewLine+ DEBUG_LINK_INFO +Environment.NewLine + ex.Message);
                }
                catch (Exception e)
                {
                    log.Error("Exception PART 2" + Environment.NewLine + DEBUG_LINK_INFO + Environment.NewLine + e.Message);
                }
            }

            // End of the download
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                HandyControl.Controls.MessageBox.Error("Download Canceled!");
                log.Error("DownloadFileCompleted Canceled" + Environment.NewLine + e.Error);
            }
            else
            {
                shDownloadedItem.Status = Convert.ToInt32(shDownloadedItem.Status) + 1;
                // Handle Rename 

                var file = ShellFile.FromFilePath(@Imagepath);

                string isNude = "NOTNUDE";


                //Check Nud

                if (DomainExists(System.IO.Path.GetFileNameWithoutExtension(@JsonPath)))
                    isNude = "ITSNUDE";
                else
                    isNude = "NOTNUDE";

                var date = sig.Substring(sig.LastIndexOf(',') + 1);


                
                try
                {
                    //Set Attrib
                    ShellPropertyWriter propertyWriter = file.Properties.GetPropertyWriter();
                    propertyWriter.WriteProperty(SystemProperties.System.Title, FixInvalidCharacter(title) ?? "Empty");
                    propertyWriter.WriteProperty(SystemProperties.System.Subject, FixInvalidCharacter(sig) ?? "Empty");
                    propertyWriter.WriteProperty(SystemProperties.System.Comment, FixInvalidCharacter(gal) ?? "Empty");
                    propertyWriter.WriteProperty(SystemProperties.System.Author, FixInvalidCharacter(wikiartist) ?? "Unknown Artist");
                    propertyWriter.WriteProperty(SystemProperties.System.Keywords, new string[] {
                   FixInvalidCharacter(city) ?? "Location Unknown", FixInvalidCharacter(country) ?? "Location Unknown", FixInvalidCharacter(lat.ToString()) ?? "Empty",
                   FixInvalidCharacter(@long.ToString()) ?? "Empty", FixInvalidCharacter(sig) ?? "Empty", FixInvalidCharacter(title) ?? "Empty",
                   FixInvalidCharacter(wikiartist) ?? "Empty", FixInvalidCharacter(gal) ?? "Empty", isNude ?? "Empty", FixInvalidCharacter(date) ?? "Empty"
                });

                    propertyWriter.Close();

                    var wikiart = wikiartist ?? "Unknown Artist";
                    string cleanFileName = String.Join("", wikiart.Split(System.IO.Path.GetInvalidFileNameChars()));
                    if (!Directory.Exists(GlobalData.Config.DataPath + @"\" + cleanFileName))
                    {
                        Directory.CreateDirectory(GlobalData.Config.DataPath + @"\" + cleanFileName);
                    }
                    File.Move(@Imagepath, GlobalData.Config.DataPath + @"\" + cleanFileName.Trim() + @"\" + System.IO.Path.GetFileName(@Imagepath));
                }
                catch (Exception ex)
                {
                    log.Error("Set Attribute" + Environment.NewLine + ex.Message);
                }

                DownloadFile();
            }
           
        }
        private string FixInvalidCharacter(string Character)
        {
            if (Character != null && Character.Contains(";"))
                Character = Character.Replace(";", " ");
            return Character;
        }
        private static bool DomainExists(string domain)
        {
            var nudeResource = Properties.Resources.nudes;
            var nudeItems = nudeResource.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in nudeItems)
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

        private void PrgDirectory_Click(object sender, RoutedEventArgs e)
        {
            var browserDialog = new CommonOpenFileDialog();
            browserDialog.IsFolderPicker = true;
            browserDialog.Title = Title;
            browserDialog.InitialDirectory = GlobalData.Config.DataPath;

            if (browserDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                GlobalData.Config.DataPath = browserDialog.FileName;
                GlobalData.Save();
            }
        }

        private void PrgCancel_Click(object sender, RoutedEventArgs e)
        {
            client.CancelAsync();
            prgButton.IsEnabled = true;
            prgButton.IsChecked = false;
            prgButton.Progress = 0;
            prgButton.Content = Properties.Langs.Lang.Download;
        }
    }
}
