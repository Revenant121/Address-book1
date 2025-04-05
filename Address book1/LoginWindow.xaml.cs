using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Npgsql;

namespace Address_book1
{
    public partial class LoginWindow : Window
    {
        private string connectionString = "Host=localhost;Port=5432;Database=AddressbookDB;Username=postgres;Password=QWERTY1221;Client Encoding=UTF8";

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void UsernameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameBox.Text == "Имя пользователя")
            {
                UsernameBox.Text = "";
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = "";
        }

        private void UsernameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                UsernameBox.Text = "Имя пользователя";
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                PasswordBox.Password = "";
            }
        }

        public UserModel AuthenticatedUser { get; private set; }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            UserModel user = AuthenticateUser(username, password);
            if (user != null)
            {
                MainWindow mainWindow = new MainWindow(user);
                mainWindow.Show();

                this.Close(); // Теперь можно закрывать — это не завершит приложение
            }
            else
            {
                MessageBox.Show("Неверные данные!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private UserModel AuthenticateUser(string username, string password)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT user_id, username, password_hash, role FROM users WHERE username = @username AND password_hash = @password";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("password", password); // Убираем хеширование

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserModel
                            {
                                user_id = reader.GetInt32(0),
                                username = reader.GetString(1),
                                password_hash = reader.GetString(2),
                                role = reader.GetString(3)
                            };
                        }
                    }
                }
            }
            return null;
        }


        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] enteredHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));
                string enteredHashString = BitConverter.ToString(enteredHash).Replace("-", "").ToLower();

                return enteredHashString == storedHash;
            }
        }
    }
}
