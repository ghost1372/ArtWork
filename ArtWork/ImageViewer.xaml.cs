using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer
    {
        public static List<string> Items;
        public ImageViewer()
        {
            InitializeComponent();

            setFlowDirection();
            foreach (var item in Items)
            {
                var slidImg = new Image();
                using (var imageStream = File.OpenRead(item))
                {
                    var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile,
                        BitmapCacheOption.Default);
                    var width = decoder.Frames[0].PixelWidth;
                    slidImg.Tag = item;
                    slidImg.Source = new BitmapImage(new Uri(item, UriKind.Absolute));
                    slidImg.Stretch = Stretch.Uniform;
                    slidImg.Width = width;
                }
                img.Items.Add(slidImg);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var item = img.Items[img.PageIndex] as Image;
            System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + item.Tag + "\"");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var item = img.Items[img.PageIndex] as Image;
            MainWindow.mainWindow.DisplayPicture(item.Tag.ToString(), true);
        }

        private void Img_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = img.Items[img.PageIndex] as Image;
            var file = ShellFile.FromFilePath(item.Tag.ToString());
            try
            {
                var country = string.Empty;
                if (file.Properties.System.Keywords.Value[1].Equals("Empty"))
                    country = "Location Unknown";
                else
                    country = file.Properties.System.Keywords.Value[1];

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
            var IsRightToLeft = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft;
            if (IsRightToLeft)
                main.FlowDirection = FlowDirection.RightToLeft;
        }
    }
}
