﻿using HandyControl.Controls;
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

            if (!System.IO.Directory.Exists(GlobalData.Config.DataPath))
                GlobalData.Config.DataPath = Environment.CurrentDirectory + @"\data";

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(GlobalData.Config.Lang);
            if (!System.IO.Directory.Exists(Environment.CurrentDirectory + @"\data"))
                System.IO.Directory.CreateDirectory(Environment.CurrentDirectory + @"\data");

            if (!GetFileList(GlobalData.Config.DataPath).Any())
                new Downloader().ShowDialog();

            var junkFiles = Directory.EnumerateFiles(GlobalData.Config.DataPath);
            foreach (var file in junkFiles)
                File.Delete(file);

            base.OnStartup(e);
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
