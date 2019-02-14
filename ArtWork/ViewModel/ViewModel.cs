using HandyControl.Controls;
using log4net;
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
    
    public class ViewModel
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public class ImageData
        {
            public string TagName { get; set; }
            public ImageSource ImageSource { get; set; }
        }

        public class ArtistData
        {
            public string Name { get; set; }
        }

        ObservableCollection<string> nudeData = new ObservableCollection<string>(); // for Nude items

        public ObservableCollection<ImageData> Images { get; }
            = new ObservableCollection<ImageData>();

        public ObservableCollection<ArtistData> ArtistNames { get; }
            = new ObservableCollection<ArtistData>(); // for artists items


        public async Task LoadFolder(CancellationToken ct, ListBox listbox, CoverView cover, ToggleButton ButtonNude)
        {
            try
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
                                if (itemx.Equals(Path.GetFileNameWithoutExtension(path)))
                                {
                                    isNude = true;
                                    break;
                                }
                            }
                        }
                        if (isNude)
                            return;

                        await Dispatcher.CurrentDispatcher.InvokeAsync(async () =>
                        {
                            Images.Add(new ImageData
                            {
                                TagName = path,
                                ImageSource = await LoadImage(path)
                            });

                        }, DispatcherPriority.Background);
                        
                    }

                }
            }
            catch (NullReferenceException e) {
                log.Error("LoadFolder " + Environment.NewLine + e.Message);
            }
            catch (Exception ex)
            {
                log.Error("LoadFolder " + Environment.NewLine + ex.Message);
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
                    bitmap.DecodePixelHeight = 300;
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
        /// <param name="prg">Progressbar that you want to be Update</param>
        /// <param name="cover">CoverView Control</param>
        /// <param name="listbox">ListBox Control</param>
        /// <param name="ButtonNude">Button Nude Filter</param>
        /// <returns></returns>
        public async Task LoadCategoty(IProgress<int> progress, CancellationToken ct, int KeywordIndex, ProgressBar prg, CoverView cover, ListBox listbox, ToggleButton ButtonNude)
        {
            try
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
                            check = item?.Properties.System.Comment.Value ?? "null";
                        else
                            check = item?.Properties.System.Keywords.Value?[KeywordIndex] ?? "null";



                        await Dispatcher.CurrentDispatcher.InvokeAsync(async () =>
                        {
                            if (check.Equals(selectedItem.Name))
                            {
                                Images.Add(new ImageData
                                {
                                    TagName = file.FullName,
                                    ImageSource = await LoadImage(file.FullName)
                                });
                            }

                        }, DispatcherPriority.Background);
                    }
                }
            }
            catch (NullReferenceException e)
            {
                log.Error("LoadCategoty " + Environment.NewLine + e.Message);
            }
            catch (Exception ex)
            {
                log.Error("LoadCategoty " + Environment.NewLine + ex.Message);
            }
        }
        private async Task<FileInfo[]> GetFileListAsync(string rootFolderPath)
        {
            FileInfo[] allfiles = null;
            await Task.Run(() =>
            {
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
            nudeData.Clear();

            var nudeResource = Properties.Resources.nudes;
            var nudeItems = nudeResource.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in nudeItems)
                nudeData.Add(line);
        }
    }
}
