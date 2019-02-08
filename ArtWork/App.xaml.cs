using System;
using System.Globalization;
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
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            GlobalData.Init();
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(GlobalData.Config.Lang);
            if (!System.IO.Directory.Exists(Environment.CurrentDirectory + @"\data"))
                System.IO.Directory.CreateDirectory(Environment.CurrentDirectory + @"\data");

            if (!AppVar.GetFileList(GlobalData.Config.DataPath).Any())
                new Downloader().ShowDialog();
        }
    }
}
