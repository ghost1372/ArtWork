using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArtWork
{
    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer
    {
        public static ObservableCollection<ViewModel.ImageData> Items;
        public ImageViewer()
        {
            InitializeComponent();

            setFlowDirection();
            foreach (ViewModel.ImageData item in Items)
            {
                Image slidImg = new Image();
                using (FileStream imageStream = File.OpenRead(item.TagName))
                {
                    BitmapDecoder decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile,
                        BitmapCacheOption.Default);
                    int width = decoder.Frames[0].PixelWidth;
                    slidImg.Tag = item.TagName;
                    slidImg.Source = new BitmapImage(new Uri(item.TagName, UriKind.Absolute));
                    slidImg.Stretch = Stretch.Uniform;
                    slidImg.Width = width;
                }
                img.Items.Add(slidImg);
            }
        }

        private void GoToLoc_Click(object sender, RoutedEventArgs e)
        {
            Image item = img.Items[img.PageIndex] as Image;
            System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + item.Tag + "\"");
        }

        private void SetDesktopWallpaper_Click(object sender, RoutedEventArgs e)
        {
            Image item = img.Items[img.PageIndex] as Image;
            MainWindow.mainWindow.SetDesktopWallpaper(item.Tag.ToString(), true);
        }

        private void Img_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Image item = img.Items[img.PageIndex] as Image;
            ShellFile file = ShellFile.FromFilePath(item.Tag.ToString());
            try
            {
                string country = string.Empty;
                if (file.Properties.System.Keywords.Value[1].Equals("Empty"))
                {
                    country = "Location Unknown";
                }
                else
                {
                    country = file.Properties.System.Keywords.Value[1];
                }

                shItems.Status = img.Items.Count;
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
        private void setFlowDirection()
        {
            bool IsRightToLeft = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft;
            if (IsRightToLeft)
            {
                main.FlowDirection = FlowDirection.RightToLeft;
            }
        }


    }
}
