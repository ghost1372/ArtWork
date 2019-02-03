using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
            var path = GlobalData.Config.DataPath;
            //if (!System.IO.Directory.Exists(path) || System.IO.Directory.Exists(path) && !System.IO.Directory.GetFiles(path).Any())
                new Downloader().ShowDialog();
        }
    }
}
