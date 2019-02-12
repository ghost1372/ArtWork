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
            log4net.Config.XmlConfigurator.Configure();
            log.Info("        =============  Started Logging  =============        ");
            base.OnStartup(e);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            GlobalData.Init();
            if (!System.IO.Directory.Exists(GlobalData.Config.DataPath))
                GlobalData.Config.DataPath = Environment.CurrentDirectory + @"\data";

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(GlobalData.Config.Lang);
            if (!System.IO.Directory.Exists(Environment.CurrentDirectory + @"\data"))
                System.IO.Directory.CreateDirectory(Environment.CurrentDirectory + @"\data");

            if (!GetFileList(GlobalData.Config.DataPath).Any())
                new Downloader().ShowDialog();
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
