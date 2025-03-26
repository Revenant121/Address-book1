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
using System.IO;
using Microsoft.Web.WebView2.Core;

namespace Address_book1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadMap();
        }
        private async void LoadMap()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "map.html");

            if (File.Exists(path))  
            {
                await MapBrowser.EnsureCoreWebView2Async();/// гарант того что webview2 полностью инициализирован
                MapBrowser.Source = new Uri(path);
            }
            else
            {
                MessageBox.Show("Файл карты не найден: map.html должен быть в папке Assets");
            }
        }
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
