using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace ArtWork
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About
    {
        public About()
        {
            InitializeComponent();
            DataContext = this;
            setFlowDirection();
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            CopyRight = versionInfo.LegalCopyright;
            Version = $"v {versionInfo.FileVersion}";
        }

        public static readonly DependencyProperty CopyRightProperty = DependencyProperty.Register(
           "CopyRight", typeof(string), typeof(About), new PropertyMetadata(default(string)));

        public string CopyRight
        {
            get => (string)GetValue(CopyRightProperty);
            set => SetValue(CopyRightProperty, value);
        }

        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
            "Version", typeof(string), typeof(About), new PropertyMetadata(default(string)));

        public string Version
        {
            get => (string)GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }

        private void setFlowDirection()
        {
            var IsRightToLeft = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft;
            if (IsRightToLeft)
                main.FlowDirection = FlowDirection.RightToLeft;
        }
    }
}
