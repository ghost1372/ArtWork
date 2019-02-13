using HandyControl.Controls;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ArtWork
{
    public class ImageData
    {
        public string Name { get; set; }
        public ImageSource ImageSource { get; set; }
    }
    public class ArtistData
    {
        public string Name { get; set; }
    }
    public class ViewModel
    {
        ObservableCollection<string> nudeData = new ObservableCollection<string>(); // for Nude items

        public ObservableCollection<ImageData> Images { get; }
            = new ObservableCollection<ImageData>();

        public ObservableCollection<ArtistData> ArtistNames { get; }
            = new ObservableCollection<ArtistData>(); // for artists items


        public async Task LoadFolder(CancellationToken ct, ListBox listbox, CoverView cover, ToggleButton ButtonNude)
        {
            cover.Items.Clear();
            Images.Clear();

            bool isNude = false;
            dynamic selectedItem = listbox.SelectedItems[0];
            foreach (var path in Directory.EnumerateFiles(GlobalData.Config.DataPath + @"\" + selectedItem.Name, "*.jpg"))
            {
                isNude = false;
                if (!ct.IsCancellationRequested)
                {
                    if (ButtonNude.IsChecked == true)
                    {
                        foreach (var itemx in nudeData)
                        {
                            Console.WriteLine(itemx);
                            if (itemx.Equals(Path.GetFileNameWithoutExtension(path)))
                            {
                                isNude = true;
                                break;
                            }
                        }
                    }
                    if (isNude)
                        return;

                    Images.Add(new ImageData
                    {
                        Name = path,
                        ImageSource = await LoadImage(path)
                    });
                   //await Task.Delay(10);
                }
                    
            }
        }

        public Task<BitmapImage> LoadImage(string path)
        {
            return Task.Run(() =>
            {
                var bitmap = new BitmapImage();

                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    bitmap.BeginInit();
                    bitmap.DecodePixelHeight = 100;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }

                return bitmap;
            });
        }


        int TotalItem = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="progress">IProgress</param>
        /// <param name="ct">Cancellation Token</param>
        /// <param name="KeywordIndex">City = [0], Country = [1], Gallery = [2]</param>
        /// <param name="prg"></param>
        /// <param name="cover"></param>
        /// <param name="listbox"></param>
        /// <param name="ButtonNude"></param>
        /// <returns></returns>
        public async Task LoadCategoty(IProgress<int> progress, CancellationToken ct, int KeywordIndex, ProgressBar prg, CoverView cover, ListBox listbox, ToggleButton ButtonNude)
        {
            cover.Items.Clear();
            Images.Clear();

            var mprogress = 0; // integer variable for progress report
            prg.Value = 0;
            dynamic check;
            bool isNude = false;
           
            foreach (var file in await GetFileListAsync(GlobalData.Config.DataPath))
            {
                dynamic selectedItem = listbox.SelectedItems[0];

                isNude = false;
                mprogress += 1;
                progress.Report((mprogress * 100 / TotalItem));
                if (!ct.IsCancellationRequested)
                {
                    var item = ShellFile.FromFilePath(file.FullName);
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

                    if (check.Equals(selectedItem.Name))
                    {
                        Images.Add(new ImageData
                        {
                            Name = file.FullName,
                            ImageSource = await LoadImage(file.FullName)
                        });
                    }

                }
                else
                {
                    MessageBox.Error("adad", "adad");
                }

            }
        }
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

        public void loadArtists()
        {
            ArtistNames.Clear();

            foreach (var line in System.IO.Directory.EnumerateDirectories(GlobalData.Config.DataPath))
            {
                ArtistNames.Add(new ArtistData
                {
                    Name = line.Replace(Path.GetDirectoryName(line) + Path.DirectorySeparatorChar, "")
                });
            }
        }
        public void loadCity()
        {
            ArtistNames.Clear();

            var cityResource = Properties.Resources.city;
            var cityItems = cityResource.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in cityItems)
                ArtistNames.Add(new ArtistData { Name = item });
        }

        public void loadCountry()
        {
            ArtistNames.Clear();

            var countryResource = Properties.Resources.country;
            var countryItems = countryResource.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in countryItems)
                ArtistNames.Add(new ArtistData { Name = item });
        }
        public void loadGallery()
        {
            ArtistNames.Clear();

            var galleryResource = Properties.Resources.gallery;
            var galleryItems = galleryResource.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in galleryItems)
                ArtistNames.Add(new ArtistData { Name = item });
        }

        public void loadNude()
        {
            var nudeResource = Properties.Resources.nudes;
            var nudeItems = nudeResource.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in nudeItems)
                nudeData.Add(line);
        }
    }
}
