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
        public MainWindow() : this(null) { }  

        private string connectionString = "Host=localhost;Port=5432;Database=AddressbookDB;Username=postgres;Password=QWERTY1221;Client Encoding=UTF8";
        private UserModel currentUser;

        public MainWindow(UserModel user)
        {
            InitializeComponent();
            currentUser = user;
            ApplyRoleRestrictions();
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

        private void ApplyRoleRestrictions()
        {
            if (currentUser == null) return;  

            if (currentUser.role == "user")
            {
                DeleteButton.IsEnabled = false;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            // Заглушка для проверки логина и пароля (замени на проверку из БД)
            if (username == "Lidzhi" && password == "1111")
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close(); 
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContactList.SelectedItem is ContactModel selectedContact)
            {
                MessageBoxResult result = MessageBox.Show($"Удалить контакт {selectedContact.FirstName} {selectedContact.LastName}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM contacts WHERE contact_id = @id";

                        using (var cmd = new NpgsqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", selectedContact.contactid);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    LoadContacts(); 
                }
            }
            else
            {
                MessageBox.Show("Выберите контакт для удаления.");
            }
        }


    }
}
