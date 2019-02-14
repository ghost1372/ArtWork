using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using log4net;
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
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static MainWindow mainWindow; // for accessing func from another View
        int TotalItem = 0; // get Total items for progress report

        #region Update App
        private string newVersion = string.Empty;

        private string ChangeLog = string.Empty;
        private string url = "";
        #endregion

        ResourceManager rm = new ResourceManager(typeof(ArtWork.Properties.Langs.Lang)); // resource manager for get resource language

        CancellationTokenSource ts = new CancellationTokenSource(); // cancel tasks token


        public MainWindow()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);

            mainWindow = this;

            setFlowDirection(); // set layout direction based on language to rtl

            ((ViewModel)DataContext).loadArtists();
            ((ViewModel)DataContext).loadNude();


            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listbox.ItemsSource);
            view.Filter = UserFilter;

        }
       
        static void MyHandler(object sender, UnhandledExceptionEventArgs e)
    {
            log.Error(e.ExceptionObject);
    }


        #region ListBox Search


        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text)) return;

            CollectionViewSource.GetDefaultView(listbox.ItemsSource).Refresh();
        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(txtSearch.Text))
                return true;
            else
                return ((item as ArtistData).Name.IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
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
            //Todo: Fix null tag

            var item = sender as CoverViewItem;
            MessageBox.Error(item.Tag.ToString());
            //var file = ShellFile.FromFilePath(item.Tag.ToString());
            //try
            //{
            //    var country = string.Empty;
            //    if (file.Properties.System.Keywords.Value[1].Equals("Empty"))
            //        country = "Location Unknown";
            //    else
            //        country = file.Properties.System.Keywords.Value[1];

            //    shTitle.Status = file.Properties.System.Title.Value;
            //    shSubject.Status = file.Properties.System.Subject.Value;
            //    shCountry.Status = country;
            //    shCity.Status = file.Properties.System.Keywords.Value[0];
            //    shGallery.Status = file.Properties.System.Comment.Value;
            //    shDate.Status = file.Properties.System.Keywords.Value[9] ?? file.Properties.System.Keywords.Value[8];
            //}
            //catch (IndexOutOfRangeException)
            //{

            //}
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
            if (listbox.SelectedIndex == -1) return;

            switch (cmbFilter.SelectedIndex)
            {
                case 0:
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await ((ViewModel)DataContext).LoadFolder(ts.Token, listbox, cover, ButtonNude);
                    break;
                case 1:

                    var progressCity = new Progress<int>(percent =>
                    {
                        prg.Value = percent;
                    });
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await ((ViewModel)DataContext).LoadCategoty(progressCity,ts.Token,0, prg,cover, listbox, ButtonNude);
                    break;

                case 2:
                    var progressCountry = new Progress<int>(percent =>
                    {
                        prg.Value = percent;
                    });
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await ((ViewModel)DataContext).LoadCategoty(progressCountry, ts.Token, 1, prg, cover, listbox, ButtonNude);
                    break;

                case 3:
                    var progressGallery = new Progress<int>(percent =>
                    {
                        prg.Value = percent;
                    });
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await ((ViewModel)DataContext).LoadCategoty(progressGallery, ts.Token, 2, prg, cover, listbox, ButtonNude);
                    break;
            }
        }


        private void BlurWindow_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        // load items to listbox
        private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (txtSearch != null && !string.IsNullOrEmpty(txtSearch.Text)) txtSearch.Text = string.Empty;

            switch (cmbFilter.SelectedIndex)
            {
                case 0:
                    ((ViewModel)DataContext).loadArtists();
                    break;
                case 1:
                    ((ViewModel)DataContext).loadCity();
                    break;

                case 2:
                    ((ViewModel)DataContext).loadCountry();
                    break;

                case 3:
                    ((ViewModel)DataContext).loadGallery();
                    break;
            }
        }

        private void CoverContextMenu_Click(object sender, RoutedEventArgs e)
        {
            var info = sender as MenuItem;
            if(info.Equals(rm.GetString("SetasDesktop")))
                SetDesktopWallpaper(info.Tag.ToString(), true);
            else
                System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + info.Tag + "\"");
        }
    }
   
}
