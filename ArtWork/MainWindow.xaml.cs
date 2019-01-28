using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArtWork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        string[] TotalList;
        string[] CurrentList;
        public MainWindow()
        {
            InitializeComponent();

            getArts();
        }

        private async void getArts()
        {

            var b = await Task.Run(async () =>
            {
                {
                    var items = System.IO.Directory.GetFiles(@"E:\DL\ArtWork");
                    TotalList = items;
                    CurrentList = TotalList.Skip(0).Take(50).ToArray();
                    await Dispatcher.InvokeAsync(() => {
                       page.MaxPageCount = items.Count() / 50;
                        CurrentList.ForEachWithIndex((item, idx) =>
                        {
                            var cv = new CoverViewItem();
                            var txt = new TextBlock();
                            var img = new Image();
                            txt.Text = "Test " + idx;
                            img.Source = new BitmapImage(new Uri(item, UriKind.Absolute));
                            cv.Header = img;
                            cv.Content = txt;
                            cover.Items.Add(cv);
                        });

                    });
                   
                    return items.ToList();
                }
            });
        }

        private void Pagination_PageUpdated(object sender, HandyControl.Data.FunctionEventArgs<int> e)
        {
            cover.Items.Clear();
            CurrentList = TotalList.Skip((e.Info - 1) * 50).Take(50).ToArray();
            CurrentList.ForEachWithIndex((item, idx) =>
            {
                var cv = new CoverViewItem();
                var txt = new TextBlock();
                var img = new Image();
                txt.Text = "Test " + idx;
                img.Source = new BitmapImage(new Uri(item, UriKind.Absolute));
                cv.Header = img;
                cv.Content = txt;
                cover.Items.Add(cv);
            });
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                cover.Items.Clear();
                CurrentList = TotalList.Where(x => x.Contains(txtSearch.Text)).ToArray();
                CurrentList.ForEachWithIndex((item, idx) =>
                {
                    var cv = new CoverViewItem();
                    var txt = new TextBlock();
                    var img = new Image();
                    txt.Text = "Test " + idx;
                    img.Source = new BitmapImage(new Uri(item, UriKind.Absolute));
                    cv.Header = img;
                    cv.Content = txt;
                    cover.Items.Add(cv);
                });
            }
        }
    }
}
public static class ForEachExtensions
{
    public static void ForEachWithIndex<T>(this IEnumerable<T> enumerable, Action<T, int> handler)
    {
        int idx = 0;
        foreach (T item in enumerable)
            handler(item, idx++);
    }
}
