using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using MessageBox = HandyControl.Controls.MessageBox;
namespace ArtWork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        internal static MainWindow mainWindow; // for accessing func from another View
        int TotalItem = 0; // get Total items for progress report

        #region Update App
        private string newVersion = string.Empty;

        private string ChangeLog = string.Empty;
        private string url = "";
        #endregion

        #region ObservableCollection for get Items

        ObservableCollection<string> sampleData = new ObservableCollection<string>(); // for artists items

        ObservableCollection<string> nudeData = new ObservableCollection<string>(); // for Nude items
        ObservableCollection<string> newnude = new ObservableCollection<string>(); // for Nude items

        ObservableCollection<string> galleryData = new ObservableCollection<string>(); // for gallery items
        ObservableCollection<string> cityData = new ObservableCollection<string>(); // for city items
        ObservableCollection<string> countryData = new ObservableCollection<string>(); // for country items

        #endregion

        ResourceManager rm = new ResourceManager(typeof(ArtWork.Properties.Langs.Lang)); // resource manager for get resource language

        CancellationTokenSource ts = new CancellationTokenSource(); // cancel tasks token

        IEnumerable<string> AllofItems;

        
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
            mainWindow = this;

            
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);

            setFlowDirection(); // set layout direction based on language to rtl

            #region load menu items from resources

            //get nude items
            var nudeResource = Properties.Resources.nudes;
            var nudeItems = nudeResource.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in nudeItems)
                nudeData.Add(line);

            //get gallery items
            var galleryResource = Properties.Resources.gallery;
            var galleryItems = galleryResource.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in galleryItems)
                galleryData.Add(item);

            //get city items
            var cityResource = Properties.Resources.city;
            var cityItems = cityResource.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in cityItems)
                cityData.Add(item);

            //get country items
            var countryResource = Properties.Resources.country;
            var countryItems = countryResource.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in countryItems)
                countryData.Add(item);

            #endregion


        }
        async void runOnce()
        {
          await  FileWriteAsync("log.txt", MasterCry.System_Details.getOperatingSystemInfo() + Environment.NewLine + "################################" + Environment.NewLine +

                "AllOfItems: " + AllofItems.Count() + "     sampleData: " + sampleData.Count + "     galleryData: " + galleryData.Count +
                "     cityData: " + cityData.Count + "     countryData: " + countryData.Count + Environment.NewLine

                );
            foreach (var item in sampleData)
            {
              await  FileWriteAsync("log.txt", Environment.NewLine +
                    item);
            }
        }
        static async void MyHandler(object sender, UnhandledExceptionEventArgs e)
        {
           await FileWriteAsync("log.txt",Environment.NewLine + e.ExceptionObject.ToString());
        }
        public static async Task FileWriteAsync(string filePath, string messaage, bool append = true)
        {
            using (FileStream stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            using (StreamWriter sw = new StreamWriter(stream))
            {
                await sw.WriteLineAsync(messaage);
            }
        }

        #region load menu items

        public ObservableCollection<string> SampleData
        {
            get
            {
                if (sampleData.Count < 1)
                    loadArtists();

                return sampleData;
            }
        }
        private void loadArtists()
        {
            sampleData.Clear();
            if (cover != null) cover.Items.Clear();
            var items = System.IO.Directory.GetDirectories(GlobalData.Config.DataPath);
            foreach (var line in items)
                sampleData.Add(line.Replace(Path.GetDirectoryName(line) + Path.DirectorySeparatorChar, ""));            
        }
        private void loadGallery()
        {
            sampleData.Clear();
            if (cover != null) cover.Items.Clear();
            foreach (var line in galleryData)
                sampleData.Add(line);
        }
        private void loadCity()
        {
            sampleData.Clear();
            if (cover != null) cover.Items.Clear();
            foreach (var line in cityData)
                sampleData.Add(line);
        }
        private void loadCountry()
        {
            sampleData.Clear();
            if (cover != null) cover.Items.Clear();
            foreach (var line in countryData)
                sampleData.Add(line);
        }
        #endregion

        #region ListBox Search

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(txtSearch.Text))
                return true;
            else
                return ((item as object).ToString().IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text)) return;

            CollectionViewSource.GetDefaultView(listbox.ItemsSource).Refresh();
        }

        #endregion

        #region GetFileList

        //get all jpg files exist in All Directory
        private async Task<FileInfo[]> GetFileListAsync(string rootFolderPath)
        {
            FileInfo[] allfiles = null;
            await Task.Run(() => {
                var dir = new DirectoryInfo(rootFolderPath);
                allfiles = dir.GetFiles("*.jpg*", SearchOption.AllDirectories);
            });
            TotalItem = allfiles.Count();
            return allfiles;
        }

        // get all files exist in Directory and SubDirectory

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
        #endregion

        #region show Items in View
        private void getArtistArt()
        {
            var CurrentIndex = listbox.SelectedIndex;
            AllofItems = GetFileList(GlobalData.Config.DataPath + @"\" + listbox.SelectedItem).ToArray();
            //Fix for Load All Items when Search
            if (AllofItems.Count() > 2000)
                return;

            //Check Nudes

            if (ButtonNude.IsChecked == true)
            {
                foreach (var item in AllofItems)
                {
                    foreach (var itemx in nudeData)
                    {
                        if (itemx.Equals(Path.GetFileNameWithoutExtension(item)))
                            newnude.Add(item.Replace(Path.GetFileName(item), itemx + ".jpg"));
                    }
                }
                AllofItems = AllofItems.Except(newnude);
            }
            cover.Items.Clear();

            foreach (var item in AllofItems)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (CurrentIndex != listbox.SelectedIndex)
                        return;

                    createControls(item);
                   
                    Task.Delay(50);
                }), DispatcherPriority.Background);
            }

        }
        private void createControls(string item)
        {
            // add the control.
            var cv = new CoverViewItem();
            var context = new ContextMenu();
            var SetAsDesktopMenuItem = new MenuItem();
            var GoToLocationMenuItem = new MenuItem();

            SetAsDesktopMenuItem.Header = rm.GetString("SetasDesktop");
            GoToLocationMenuItem.Header = rm.GetString("GoToLoc");

            SetAsDesktopMenuItem.Click += delegate { SetDesktopWallpaper(item, true); };
            GoToLocationMenuItem.Click += delegate { System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + item + "\""); };
            context.Items.Add(SetAsDesktopMenuItem);
            context.Items.Add(GoToLocationMenuItem);

            var contentImg = new Image();
            contentImg.Stretch = Stretch.Uniform;
            contentImg.Source = new BitmapImage(new Uri(item, UriKind.Absolute));

            var img = new Image();
            img.Source = new BitmapImage(new Uri(item, UriKind.Absolute));
            cv.Header = img;
            cv.Tag = item;
            cv.Content = contentImg;
            cv.ContextMenu = context;
            cv.Selected += Cv_Selected;
            cv.Deselected += Cv_Deselected;
            cv.MouseDoubleClick += Cv_MouseDoubleClick;
            //-< source >- 
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(item, UriKind.Absolute);
            //< thumbnail > 
            src.DecodePixelWidth = 160;
            src.CacheOption = BitmapCacheOption.OnLoad;
            //</ thumbnail > 

            src.EndInit();
            img.Source = src;
            //-</ source >- 

            img.Stretch = Stretch.Uniform;
            img.Height = 160;

            cover.Items.Add(cv);
        }
       

        /// <summary>
        /// getArtsTaskAsync for Searching based on Gallery, City, Country items
        /// </summary>
        /// <param name="progress">IProgress</param>
        /// <param name="ct">Cancellation Token</param>
        /// <param name="KeywordIndex">City = [0], Country = [1], Gallery = [2]</param>
        /// <returns>Await Task</returns>
        public async Task getArtsTaskAsync(IProgress<int> progress, CancellationToken ct, int KeywordIndex)
        {
            if (listbox.SelectedIndex == -1) return;

            var mprogress = 0; // integer variable for progress report
            prg.Value = 0;
            cover.Items.Clear();
            dynamic check;
            bool isNude = false;
            var SearchTask = Task.Run(async () =>
            {
                foreach (var file in await GetFileListAsync(GlobalData.Config.DataPath))
                {
                    isNude = false;
                    mprogress += 1;
                    progress.Report((mprogress * 100 / TotalItem));
                    if (!ct.IsCancellationRequested)
                    {
                        var item = ShellFile.FromFilePath(file.FullName);
                        await Dispatcher.InvokeAsync(() =>
                        {
                            if (ButtonNude.IsChecked == true)
                            {
                                foreach (var itemx in nudeData)
                                {
                                    if (itemx.Equals(Path.GetFileNameWithoutExtension(file.FullName)))
                                    {
                                        isNude = true;
                                        break;
                                    }

                                }
                            }

                            if (isNude)
                                return;

                            // check if it is gallery or not
                            if (KeywordIndex == 2)
                                check = item.Properties.System.Comment.Value;
                            else
                                check = item.Properties.System.Keywords.Value[KeywordIndex];

                            if (check.Equals(listbox.SelectedItem))
                            {
                                // add the control.
                                createControls(file.FullName);
                            }
                        }, DispatcherPriority.Background);

                    }
                }
            }, ct);

            await SearchTask;
        }
        #endregion

        #region Set as Wallpaper
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, String pvParam, uint fWinIni);

        private const uint SPI_SETDESKWALLPAPER = 0x14;
        private const uint SPIF_UPDATEINIFILE = 0x1;
        private const uint SPIF_SENDWININICHANGE = 0x2;
        public void SetDesktopWallpaper(string file_name, bool update_registry)
        {
            try
            {
                // If we should update the registry,
                // set the appropriate flags.
                uint flags = 0;
                if (update_registry)
                    flags = SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE;

                // Set the desktop background to this file.
                if (!SystemParametersInfo(SPI_SETDESKWALLPAPER,
                    0, file_name, flags))
                {
                    MessageBox.Show("SystemParametersInfo failed.", "Error");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error displaying picture ", "Error");
            }
        }

        //todo: fix lockscreen 
        // based on this answer https://stackoverflow.com/questions/51781921/programmatically-change-windows-10-lock-screen-background-on-desktop/51785687
        
        #endregion

        #region Cover Items Events

        private void Cv_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            List<string> items = new List<string>();
            foreach (var item in cover.Items.OfType<CoverViewItem>())
                items.Add(item.Tag.ToString());

            ImageViewer.Items = items;
            new ImageViewer().ShowDialog();
        }

        private void Cv_Deselected(object sender, RoutedEventArgs e)
        {
            var item = sender as CoverViewItem;
            item.Content = null;
        }

        private void Cv_Selected(object sender, RoutedEventArgs e)
        {
            var item = sender as CoverViewItem;
            var file = ShellFile.FromFilePath(item.Tag.ToString());
            try
            {
                var country = string.Empty;
                if (file.Properties.System.Keywords.Value[1].Equals("Empty"))
                    country = "Location Unknown";
                else
                    country = file.Properties.System.Keywords.Value[1];

                shTitle.Status = file.Properties.System.Title.Value;
                shSubject.Status = file.Properties.System.Subject.Value;
                shCountry.Status = country;
                shCity.Status = file.Properties.System.Keywords.Value[0];
                shGallery.Status = file.Properties.System.Comment.Value;
                shDate.Status = file.Properties.System.Keywords.Value[9] ?? file.Properties.System.Keywords.Value[8];
            }
            catch (IndexOutOfRangeException)
            {

            }
        }
        #endregion

        #region Nude Checked Button
        private void ButtonNude_Checked(object sender, RoutedEventArgs e)
        {
            setStyle((bool)ButtonNude.IsChecked);
            Listbox_SelectionChanged(null, null);
        }
        private void setStyle(bool isChecked)
        {
            Style style;

            if (isChecked)
                style = this.FindResource("ToggleButtonDanger") as Style;
            else
                style = this.FindResource("ToggleButtonPrimary") as Style;
            
            ButtonNude.Style = style;
            
        }
        #endregion

        #region Menu Items

        private void DownloaderMenu(object sender, RoutedEventArgs e)
        {
            new Downloader().Show();
        }

        private void ChangePathMenu(object sender, RoutedEventArgs e)
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

        private void AboutMenu(object sender, RoutedEventArgs e)
        {
            new About().ShowDialog();
        }

        private void ChangeLanguageMenu(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            if (item.Tag.Equals("Persian"))
                GlobalData.Config.Lang = "fa-IR";
            else
                GlobalData.Config.Lang = "en-US";
            GlobalData.Save();

            System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
            Environment.Exit(0);
        }

        private void CheckUpdateMenu(object sender, RoutedEventArgs e)
        {
            CheckUpdate();
        }
        #endregion

        #region Update App
        private void showGrowlNotification(bool isSuccess, params string[] param)
        {
            if (isSuccess)
            {
                Growl.Info(new GrowlInfo
                {
                    Message = string.Format(rm.GetString("NewVersionFind"),param[0]) + Environment.NewLine + ChangeLog,
                    CancelStr = rm.GetString("Cancel"),
                    ConfirmStr = rm.GetString("Download"),
                    ShowDateTime = false,
                    ActionBeforeClose = isConfirm =>
                    {
                        if (isConfirm)
                            System.Diagnostics.Process.Start(param[1]);

                        return true;
                    }
                });
            }
            else
            {
                Growl.Error(new GrowlInfo { Message = string.Format(rm.GetString("CurrentIsLastVersion"),Assembly.GetExecutingAssembly().GetName().Version.ToString()), ShowDateTime = false });
            }

        }
        private void CompareVersions()
        {
            if (AppVar.IsVersionLater(newVersion, Assembly.GetExecutingAssembly().GetName().Version.ToString()))
            {
                showGrowlNotification(true, newVersion, url);
            }
            else
            {
                showGrowlNotification(false);
            }
        }
        private void CheckUpdate()
        {
            try
            {
                newVersion = string.Empty;
                ChangeLog = string.Empty;
                url = "";

                XDocument doc = XDocument.Load(AppVar.UpdateServer);
                var items = doc
                    .Element(XName.Get(AppVar.UpdateXmlTag))
                    .Elements(XName.Get(AppVar.UpdateXmlChildTag));
                var versionItem = items.Select(ele => ele.Element(XName.Get(AppVar.UpdateVersionTag)).Value);
                var urlItem = items.Select(ele => ele.Element(XName.Get(AppVar.UpdateUrlTag)).Value);
                var changelogItem = items.Select(ele => ele.Element(XName.Get(AppVar.UpdateChangeLogTag)).Value);

                newVersion = versionItem.FirstOrDefault();
                url = urlItem.FirstOrDefault();
                ChangeLog = changelogItem.FirstOrDefault();
                CompareVersions();
            }
            catch (Exception)
            {
            }
        }

        #endregion

        private void setFlowDirection()
        {
            var IsRightToLeft = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft;
            if (IsRightToLeft)
                main.FlowDirection = FlowDirection.RightToLeft;
        }

        // show items to coverview
        private async void Listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cmbFilter.SelectedIndex)
            {
                case 0:
                    ts?.Cancel();
                    getArtistArt();
                    break;
                case 1:

                    var progressCity = new Progress<int>(percent =>
                    {
                        prg.Value = percent;
                    });
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await getArtsTaskAsync(progressCity, ts.Token, 0);
                    break;

                case 2:
                    var progressCountry = new Progress<int>(percent =>
                    {
                        prg.Value = percent;
                    });
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await getArtsTaskAsync(progressCountry, ts.Token, 1);
                    break;

                case 3:
                    var progressGallery = new Progress<int>(percent =>
                    {
                        prg.Value = percent;
                    });
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await getArtsTaskAsync(progressGallery, ts.Token, 2);
                    break;
            }
        }

        private void BlurWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AllofItems = GetFileList(GlobalData.Config.DataPath + @"\" + listbox.SelectedItem).ToArray();

            //Initialize Search
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listbox.ItemsSource);
            view.Filter = UserFilter;

            runOnce();
        }

        // load items to listbox
        private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (txtSearch != null && !string.IsNullOrEmpty(txtSearch.Text)) txtSearch.Text = string.Empty;

            switch (cmbFilter.SelectedIndex)
            {
                case 0:
                    loadArtists();
                    break;
                case 1:
                    loadCity();
                    break;

                case 2:
                    loadCountry();
                    break;

                case 3:
                    loadGallery();
                    break;
            }
        }

        
    }

}
