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
using System.Collections.ObjectModel;

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
            }

            allContacts = new ObservableCollection<ContactModel>(contacts); 
            ContactList.ItemsSource = allContacts;
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

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContactList.SelectedItem is ContactModel selectedContact)
            {
                var editWindow = new EditContactWindow(selectedContact);
                if (editWindow.ShowDialog() == true)
                {
                    using (var conn = new Npgsql.NpgsqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = @"UPDATE contacts SET 
                                    ""FirstName"" = @firstName,
                                    ""LastName"" = @lastName,
                                    ""address"" = @address,
                                    ""phone_number"" = @phone
                                 WHERE contact_id = @id";

                        using (var cmd = new Npgsql.NpgsqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@firstName", selectedContact.FirstName);
                            cmd.Parameters.AddWithValue("@lastName", selectedContact.LastName);
                            cmd.Parameters.AddWithValue("@address", selectedContact.address);
                            cmd.Parameters.AddWithValue("@phone", selectedContact.phone_number);
                            cmd.Parameters.AddWithValue("@id", selectedContact.contactid);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    LoadContacts(); // Перезагрузить список после редактирования
                }
            }
            else
            {
                MessageBox.Show("Выберите контакт для редактирования.");
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Поиск по ключевым словам")
            {
                SearchBox.Text = "";
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Поиск по ключевым словам";
            }
        }
        private ObservableCollection<ContactModel> allContacts = new ObservableCollection<ContactModel>();

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = SearchBox.Text.ToLower().Trim();

            var filtered = allContacts.Where(c =>
                (c.FirstName != null && c.FirstName.ToLower().Contains(keyword)) ||
                (c.LastName != null && c.LastName.ToLower().Contains(keyword)) ||
   
                (c.address != null && c.address.ToLower().Contains(keyword)) ||
                (c.phone_number != null && c.phone_number.ToLower().Contains(keyword))
            );

            ContactList.ItemsSource = new ObservableCollection<ContactModel>(filtered);
        }
        private void AddContactButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new EditContactWindow(); // окно без параметров — для нового контакта
            if (addWindow.ShowDialog() == true)
            {
                var newContact = addWindow.Contact;

                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO contacts (
                                contact_id, ""FirstName"", ""LastName"", ""address"", ""phone_number"", ""Latitude"", ""Longitude"")
                             VALUES (@id, @firstName, @lastName, @address, @phone, @lat, @lon)";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", newContact.contactid);
                        cmd.Parameters.AddWithValue("@firstName", newContact.FirstName);
                        cmd.Parameters.AddWithValue("@lastName", newContact.LastName);
                        cmd.Parameters.AddWithValue("@address", newContact.address);
                        cmd.Parameters.AddWithValue("@phone", newContact.phone_number);
                        cmd.Parameters.AddWithValue("@lat", newContact.Latitude);
                        cmd.Parameters.AddWithValue("@lon", newContact.Longitude);

                        cmd.ExecuteNonQuery();
                    }
                }

                LoadContacts(); // Обновить список контактов
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
