using HandyControl.Controls;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MessageBox = HandyControl.Controls.MessageBox;

namespace ArtWork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        IEnumerable<string> AllofItems;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        #region load menu items

        //todo: Load items dynamic
        ObservableCollection<string> sampleData = new ObservableCollection<string>();
        ObservableCollection<string> nudeData = new ObservableCollection<string>();
        ObservableCollection<string> newnude = new ObservableCollection<string>();
        public ObservableCollection<string> SampleData
        {
            get
            {
                if (sampleData.Count < 1)
                {
                    
                    var items = @"C:\Users\Mahdi\Desktop\title.txt";
                    var nudeItems = @"C:\Users\Mahdi\Desktop\nudes.txt";
                    

                    var lines = File.ReadAllLines(items);
                    foreach (var line in lines)
                    {

                        sampleData.Add(line);
                    }

                    var nudesLines = File.ReadAllLines(nudeItems);
                    foreach (var line in nudesLines)
                    {
                        nudeData.Add(line);
                    }

                }

                return sampleData;
            }
        }
        #endregion

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(txtSearch.Text))
                return true;
            else
                return ((item as object).ToString().IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

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

        

        private void Listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var CurrentIndex = listbox.SelectedIndex;
            AllofItems = GetFileList(@"E:\DL\newArtWork\Art\" + listbox.SelectedItem).ToArray();
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
               
                this.Dispatcher.Invoke(() => {

                    if (CurrentIndex != listbox.SelectedIndex)
                        return;

                    // add the control.
                    var cv = new CoverViewItem();
                    var context = new ContextMenu();
                    var menuItem = new MenuItem();
                    var menuItem2 = new MenuItem();

                    menuItem.Header = "Set as Desktop Wallpaper";
                    menuItem2.Header = "Go to Location";

                    menuItem.Click += delegate { DisplayPicture(item, true); };
                    menuItem2.Click += delegate { System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + item + "\""); };

                    context.Items.Add(menuItem);
                    context.Items.Add(menuItem2);

                    var contentImg = new Image();
                    contentImg.Stretch = Stretch.UniformToFill;
                    contentImg.Source = new BitmapImage(new Uri(item, UriKind.Absolute));

                    var img = new Image();
                    img.Source = new BitmapImage(new Uri(item, UriKind.Absolute));
                    cv.Header = img;
                    cv.Tag = item;
                    cv.Content = contentImg;
                    cv.ContextMenu = context;
                    cv.Selected += Cv_Selected;
                    cv.Deselected += Cv_Deselected;

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
                    Task.Delay(50);

                }, DispatcherPriority.Background);
            }
        }        

        #region Set as Desktop
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, String pvParam, uint fWinIni);

        private const uint SPI_SETDESKWALLPAPER = 0x14;
        private const uint SPIF_UPDATEINIFILE = 0x1;
        private const uint SPIF_SENDWININICHANGE = 0x2;
        private void DisplayPicture(string file_name, bool update_registry)
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
                    MessageBox.Show("SystemParametersInfo failed.","Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error displaying picture ", "Error");
            }
        }
        #endregion

        #region Cover Items Events
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
                shCountry.Status =country;
                shCity.Status = file.Properties.System.Keywords.Value[0];
                shGallery.Status = file.Properties.System.Comment.Value;
                shDate.Status = file.Properties.System.Keywords.Value[9] ?? file.Properties.System.Keywords.Value[8];
            }
            catch (IndexOutOfRangeException)
            {

            }
        }
        #endregion

        private void BlurWindow_Loaded(object sender, RoutedEventArgs e)
        {
            listbox.SelectedIndex = 0;
            AllofItems = GetFileList(@"E:\DL\newArtWork\Art\" + listbox.SelectedItem).ToArray();

            

            //Initialize Search
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listbox.ItemsSource);
            view.Filter = UserFilter;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(listbox.ItemsSource).Refresh();
        }

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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            new Downloader().Show();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
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
    }

}
