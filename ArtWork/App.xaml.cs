using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;
using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
namespace ArtWork
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));
        protected override void OnStartup(StartupEventArgs e)
        {
            GlobalData.Init();

            BlurWindow.SystemVersionInfo = CommonHelper.GetSystemVersionInfo();

            log4net.Config.XmlConfigurator.Configure();
            log.Info("        =============  Started Logging  =============        ");

            if (GlobalData.Config.Skin != SkinType.Default)
                UpdateSkin(GlobalData.Config.Skin);

            if (!System.IO.Directory.Exists(GlobalData.Config.DataPath))
                GlobalData.Config.DataPath = Environment.CurrentDirectory + @"\data";

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(GlobalData.Config.Lang);
            if (!System.IO.Directory.Exists(Environment.CurrentDirectory + @"\data"))
                System.IO.Directory.CreateDirectory(Environment.CurrentDirectory + @"\data");

            var junkFiles = System.IO.Directory.EnumerateFiles(GlobalData.Config.DataPath);
            foreach (var file in junkFiles)
                System.IO.File.Delete(file);

            if (!GetFileList(GlobalData.Config.DataPath).Any())
                new Downloader().ShowDialog();

            base.OnStartup(e);
        }
        internal void UpdateSkin(SkinType skin)
        {
            var skins0 = Resources.MergedDictionaries[0];
            skins0.MergedDictionaries.Clear();
            skins0.MergedDictionaries.Add(ResourceHelper.GetSkin(skin));
            skins0.MergedDictionaries.Add(ResourceHelper.GetSkin(typeof(App).Assembly, "Resources/Themes", skin));

            var skins1 = Resources.MergedDictionaries[1];
            skins1.MergedDictionaries.Clear();
            skins1.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
            });
            skins1.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/ArtWork;component/Resources/Themes/Theme.xaml")
            });
            Current.MainWindow?.OnApplyTemplate();
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

    }
}
