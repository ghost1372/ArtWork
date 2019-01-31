using HandyControl.Controls;
using HandyControl.Tools.Extension;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace ArtWork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        IEnumerable<string> AllofItems;
        IEnumerable<string> CurrentofItems;
        List<string> currentList;
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
            
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


        #region Load Artist name
        //todo: dynamic datatext
        ObservableCollection<string> sampleData = new ObservableCollection<string>();
        public ObservableCollection<string> SampleData
        {
            get
            {
                if (sampleData.Count < 1)
                {
                    var items = @"C:\Users\Mahdi\Desktop\title.txt";
                    var lines = File.ReadAllLines(items);
                    foreach (var line in lines)
                        sampleData.Add(line);
                }

                if (SearchText == null) return sampleData;

                // Convert IEnum to Observ https://stackoverflow.com/questions/3559821/how-to-convert-ienumerable-to-observablecollection
                var myObservableCollection = new ObservableCollection<string>(sampleData.Where(x => x.ToLower().StartsWith(SearchText.ToLower())));

                return myObservableCollection;
            }
        }

        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/28d7aa0e-21c2-4fb8-b780-ad020b4556ad/how-to-create-a-search-textbox-in-wpf-to-search-the-listbox-databound-items-?forum=wpf

        //Search in Listbox
        private string _searchText;

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;

                OnPropertyChanged("SearchText");
                OnPropertyChanged("SampleData");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion



        #region Search in Images
        //if (!string.IsNullOrWhiteSpace(txtSearch.Text))
        //    {
        //        cover.Items.Clear();
        //        CurrentList = TotalList.Where(x => x.Contains(txtSearch.Text)).ToArray();
        //CurrentList.ForEachWithIndex((item, idx) =>
        //        {


        //            Dispatcher.BeginInvoke(new Action(() =>
        //            {
        //    var cv = new CoverViewItem();
        //    var txt = new TextBlock();
        //    var img = new Image();
        //    txt.Text = "Test " + idx;
        //    img.Source = new BitmapImage(new Uri(item, UriKind.Absolute));
        //    cv.Header = img;
        //    cv.Content = txt;
        //    cover.Items.Add(cv);

        //}), DispatcherPriority.Background);
        //        });
        //    }
        #endregion



        private void Listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            AllofItems = GetFileList(@"E:\DL\newArtWork\Art\" + listbox.SelectedItem).ToArray();
            
            loading.Visibility = Visibility.Visible;
            cover.Items.Clear();
            AllofItems.ForEachWithIndex((item, idx) =>
            {
               
                var cv = new CoverViewItem();
                var contentImg = new Image();
                contentImg.Stretch = Stretch.UniformToFill;
                contentImg.Source = new BitmapImage(new Uri(item, UriKind.Absolute));



                var img = new Image();
                img.Source = new BitmapImage(new Uri(item, UriKind.Absolute));




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







                cv.Header = img;
                cv.Tag = item;
                cv.Content = contentImg;
                cv.Selected += Cv_Selected;
                cover.Items.Add(cv);
            });
            loading.Visibility = Visibility.Collapsed;


        }


        private void Cv_Selected(object sender, RoutedEventArgs e)
        {
            var item = sender as CoverViewItem;
            var file = ShellFile.FromFilePath(item.Tag.ToString());
            shTitle.Status = file.Properties.System.Title.Value;
            shSubject.Status = file.Properties.System.Subject.Value;
            shCountry.Status = file.Properties.System.Keywords.Value[0];
            shCity.Status = file.Properties.System.Title.Value;
            shGallery.Status = file.Properties.System.Comment.Value;
        }

        private void BlurWindow_Loaded(object sender, RoutedEventArgs e)
        {
            listbox.SelectedIndex = 0;
            AllofItems = GetFileList(@"E:\DL\newArtWork\Art\" + listbox.SelectedItem).ToArray();
        }
    }
}
public static class ForEachExtensions
{
    public static void ForEachWithIndex<T>(this IEnumerable<T> enumerable, Action<T, int> handler)
    {
        try
        {
            int idx = 0;
            foreach (T item in enumerable)
                handler(item, idx++);
        }
        catch (Exception)
        {

        }
    }
}
