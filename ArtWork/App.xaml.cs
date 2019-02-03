using System.Linq;
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
            if (!AppVar.GetFileList(GlobalData.Config.DataPath).Any())
                new Downloader().ShowDialog();
        }
    }
}
