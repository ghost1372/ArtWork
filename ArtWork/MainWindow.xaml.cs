﻿using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Linq;
using MessageBox = HandyControl.Controls.MessageBox;
namespace ArtWork
{
    //Todo: HandyControl CoverView Must be Disbale for popup
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        internal static MainWindow mainWindow; // for accessing func from another View

        #region Update App
        private string newVersion = string.Empty;

        private string ChangeLog = string.Empty;
        private string url = "";
        #endregion

        private CancellationTokenSource ts = new CancellationTokenSource(); // cancel tasks token


        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;

            setFlowDirection(); // set layout direction based on language to rtl

            ((ViewModel)DataContext).loadArtists();
            ((ViewModel)DataContext).loadNude();


            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listbox.ItemsSource);
            view.Filter = UserFilter;
        }

        #region ListBox Search


        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                return;
            }

            CollectionViewSource.GetDefaultView(listbox.ItemsSource).Refresh();
        }

        private bool UserFilter(object item)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                return true;
            }
            else
            {
                return ((item as ViewModel.ArtistData).Name.IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }
        #endregion

        #region Set as Wallpaper
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

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
                {
                    flags = SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE;
                }

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
            ObservableCollection<ViewModel.ImageData> items = cover.ItemsSource as ObservableCollection<ViewModel.ImageData>;
            ImageViewer.Items = items;
            new ImageViewer().ShowDialog();
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }
        private void Cv_Selected(object sender, RoutedEventArgs e)
        {
            CoverViewItem selectedCover = sender as CoverViewItem;
            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(selectedCover);

            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            Image selectedImg = (Image)myDataTemplate.FindName("ImageHeader", myContentPresenter);
            ShellFile file = ShellFile.FromFilePath(selectedImg.Tag.ToString());
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
            {
                style = FindResource("ToggleButtonDanger") as Style;
            }
            else
            {
                style = FindResource("ToggleButtonPrimary") as Style;
            }

            ButtonNude.Style = style;

        }
        #endregion

        #region Menu Items

        private void DownloaderMenu(object sender, RoutedEventArgs e)
        {
            new Downloader().ShowDialog();
        }

        private void ChangePathMenu(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog browserDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = Title,
                InitialDirectory = GlobalData.Config.DataPath
            };

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
            MenuItem item = sender as MenuItem;
            Growl.Ask(new GrowlInfo
            {
                Message = Properties.Langs.Lang.ChangeLangAsk,
                ShowDateTime = false,
                CancelStr = Properties.Langs.Lang.Cancel,
                ConfirmStr = Properties.Langs.Lang.Confirm,
                ActionBeforeClose = isConfirm =>
                {
                    if (isConfirm)
                    {
                        GlobalData.Config.Lang = item.Tag.ToString();
                        GlobalData.Save();
                        System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
                        Environment.Exit(0);
                    }

                    return true;
                }
            });
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
                    Message = string.Format(Properties.Langs.Lang.NewVersionFind, param[0]) + Environment.NewLine + ChangeLog,
                    CancelStr = Properties.Langs.Lang.Cancel,
                    ConfirmStr = Properties.Langs.Lang.Download,
                    ShowDateTime = false,
                    ActionBeforeClose = isConfirm =>
                    {
                        if (isConfirm)
                        {
                            System.Diagnostics.Process.Start(param[1]);
                        }

                        return true;
                    }
                });
            }
            else
            {
                Growl.Error(new GrowlInfo { Message = string.Format(Properties.Langs.Lang.CurrentIsLastVersion, Assembly.GetExecutingAssembly().GetName().Version.ToString()), ShowDateTime = false });
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
                IEnumerable<XElement> items = doc
                    .Element(XName.Get(AppVar.UpdateXmlTag))
                    .Elements(XName.Get(AppVar.UpdateXmlChildTag));
                IEnumerable<string> versionItem = items.Select(ele => ele.Element(XName.Get(AppVar.UpdateVersionTag)).Value);
                IEnumerable<string> urlItem = items.Select(ele => ele.Element(XName.Get(AppVar.UpdateUrlTag)).Value);
                IEnumerable<string> changelogItem = items.Select(ele => ele.Element(XName.Get(AppVar.UpdateChangeLogTag)).Value);

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
            bool IsRightToLeft = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft;
            if (IsRightToLeft)
            {
                main.FlowDirection = FlowDirection.RightToLeft;
            }
        }

        // show items to coverview
        private async void Listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listbox.SelectedIndex == -1)
            {
                return;
            }

            switch (cmbFilter.SelectedIndex)
            {
                case 0:
                    GC.Collect();

                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await ((ViewModel)DataContext).LoadFolder(ts.Token, listbox, ButtonNude);
                    break;
                case 1:
                    GC.Collect();

                    Progress<int> progressCity = new Progress<int>(percent =>
                    {
                        prg.Value = percent;
                    });
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await ((ViewModel)DataContext).LoadCategoty(progressCity, ts.Token, 0, prg, listbox, ButtonNude);
                    break;

                case 2:
                    GC.Collect();

                    Progress<int> progressCountry = new Progress<int>(percent =>
                    {
                        prg.Value = percent;
                    });
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await ((ViewModel)DataContext).LoadCategoty(progressCountry, ts.Token, 1, prg, listbox, ButtonNude);
                    break;

                case 3:
                    GC.Collect();

                    Progress<int> progressGallery = new Progress<int>(percent =>
                    {
                        prg.Value = percent;
                    });
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await ((ViewModel)DataContext).LoadCategoty(progressGallery, ts.Token, 2, prg, listbox, ButtonNude);
                    break;
                case 4:
                    dynamic selectedItem = listbox.SelectedItems[0];
                    await ((ViewModel)DataContext).LoadFolder(selectedItem.Tag, listbox, ButtonNude);
                    break;
            }
        }

        // load items to listbox
        private async void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtSearch != null && !string.IsNullOrEmpty(txtSearch.Text))
            {
                txtSearch.Text = string.Empty;
            }

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
                case 4:
                    GC.Collect();
                    Progress<int> progressTitle = new Progress<int>(percent =>
                    {
                        prg.Value = percent;
                    });
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await ((ViewModel)DataContext).loadTitles(progressTitle, ts.Token, prg);
                    break;
            }
        }

        private void CoverContextMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem info = sender as MenuItem;
            if (info.Header.Equals(Properties.Langs.Lang.SetasDesktop))
            {
                SetDesktopWallpaper(info.Tag.ToString(), true);
            }
            else if (info.Header.Equals(Properties.Langs.Lang.GoToLoc))
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + info.Tag + "\"");
            }
            else if (info.Header.Equals(Properties.Langs.Lang.FullScreenSee))
            {
                ImageBrowser imgBrowser = new ImageBrowser(new Uri(info.Tag.ToString(), UriKind.Absolute))
                {
                    ResizeMode = ResizeMode.CanResize
                };
                imgBrowser.Show();

            }
            else if (info.Header.Equals(Properties.Langs.Lang.Fav))
            {
                if (System.IO.File.Exists(AppVar.FavoriteFilePath))
                {
                    bool lines = System.IO.File.ReadAllLines(AppVar.FavoriteFilePath).Any(x => x.Equals(info.Tag.ToString().Trim()));
                    if (!lines)
                    {
                        System.IO.File.AppendAllText(AppVar.FavoriteFilePath, info.Tag.ToString().Trim() + Environment.NewLine);
                        Growl.Info(new GrowlInfo
                        {
                            Message = Properties.Langs.Lang.AddedToFav,
                            ConfirmStr = Properties.Langs.Lang.Confirm,
                            ShowDateTime = false
                        });
                    }
                    else
                    {
                        Growl.Warning(new GrowlInfo
                        {
                            Message = Properties.Langs.Lang.ExistFav,
                            ConfirmStr = Properties.Langs.Lang.Confirm,
                            ShowDateTime = false
                        });
                    }
                }
            }
            else if (info.Header.Equals(Properties.Langs.Lang.RemoveFromFav))
            {
                List<string> lines = System.IO.File.ReadAllLines(AppVar.FavoriteFilePath).ToList();
                lines.Remove(info.Tag.ToString().Trim());
                System.IO.File.WriteAllLines(AppVar.FavoriteFilePath, lines.ToArray());
            }
        }
        private void CancelTaskButton_Click(object sender, RoutedEventArgs e)
        {
            ts?.Cancel();
            Growl.Info(new GrowlInfo
            {
                Message = Properties.Langs.Lang.TaskCanceled,
                ShowDateTime = false,
                ShowCloseButton = false
            });
        }

        private void ButtonLangs_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is string tag)
            {
                PopupConfig.IsOpen = false;
                if (tag.Equals(GlobalData.Config.Lang))
                {
                    return;
                }

                Growl.Ask(new GrowlInfo
                {
                    Message = Properties.Langs.Lang.ChangeLangAsk,
                    ShowDateTime = false,
                    CancelStr = Properties.Langs.Lang.Cancel,
                    ConfirmStr = Properties.Langs.Lang.Confirm,
                    ActionBeforeClose = isConfirm =>
                    {
                        if (isConfirm)
                        {
                            GlobalData.Config.Lang = tag;
                            GlobalData.Save();
                            System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
                            Environment.Exit(0);
                        }

                        return true;
                    }
                });
            }
        }

        private void ButtonConfig_OnClick(object sender, RoutedEventArgs e)
        {
            PopupConfig.IsOpen = true;
        }

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is SkinType tag)
            {
                PopupConfig.IsOpen = false;
                if (tag.Equals(GlobalData.Config.Skin))
                {
                    return;
                }

                GlobalData.Config.Skin = tag;
                GlobalData.Save();
                ((App)Application.Current).UpdateSkin(tag);
            }
        }

        private readonly List<string> list = new List<string>();

        private async void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tab.SelectedIndex == 1)
            {
                list.Clear();
                GC.Collect();
                ts?.Cancel();
                ts = new CancellationTokenSource();
                await ((ViewModel)DataContext).LoadFavorite(ts.Token);
            }
        }
        private void SetAsDesktopRandom_Click(object sender, RoutedEventArgs e)
        {
            //Todo: add
            if (list != null)
            {
                //set as desktop for list
            }
            else
            {
                //set as dekstop all
            }
        }

        private void CoverViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ObservableCollection<ViewModel.ImageData> items = coverFav.ItemsSource as ObservableCollection<ViewModel.ImageData>;
            ImageViewer.Items = items;
            new ImageViewer().ShowDialog();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton item = sender as ToggleButton;
            if (item.IsChecked == true)
            {
                list.Add(item.Tag.ToString().Trim());
            }
            else
            {
                list.Remove(item.Tag.ToString().Trim());
            }
        }
    }

}
