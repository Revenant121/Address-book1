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
using Npgsql;
using Address_book1.Models;


namespace Address_book1
{

    public partial class MainWindow : Window
    {
        private string connectionString = "Host=localhost;Port=5432;Database=AddressbookDB;Username=postgres;Password=QWERTY1221;Client Encoding=UTF8";

        public MainWindow()
        {
            InitializeComponent();
            LoadMap();
            LoadContacts();

        }


        private async void LoadMap()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "map.html");

            if (File.Exists(path))
            {
                await MapBrowser.EnsureCoreWebView2Async();
                MapBrowser.Source = new Uri(path);
            }
            else
            {
                MessageBox.Show("Файл карты не найден: map.html должен быть в папке Assets");
            }
        }

        
        private void LoadContacts()
        {
            List<ContactModel> contacts = new List<ContactModel>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT \"contact_id\", \"FirstName\", \"LastName\", \"address\", \"phone_number\", \"Latitude\", \"Longitude\" FROM contacts";

                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contacts.Add(new ContactModel
                        {
                            contactid = reader.GetGuid(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            address = reader.GetString(3),
                            phone_number = reader.GetString(4),
                            Latitude = reader.GetDouble(5),   
                            Longitude = reader.GetDouble(6)
                        });
                    }
                }


                ContactList.ItemsSource = contacts; 
            }
        }


        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContactList.SelectedItem is ContactModel selectedContact)
            {
                AddContactToMap(selectedContact.Latitude, selectedContact.Longitude, $"{selectedContact.FirstName} {selectedContact.LastName}");
                MessageBox.Show($"Выбран контакт: {selectedContact.FirstName} {selectedContact.LastName}");
            }
        }

        private async void AddContactToMap(double latitude, double longitude, string name)
        {
            string script = $"addMarker({latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, " +
                            $"{longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, '{name}');";
            await MapBrowser.ExecuteScriptAsync(script);
        }

            
    }

}
