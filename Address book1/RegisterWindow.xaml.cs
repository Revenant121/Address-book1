using System.Windows;
using Npgsql;

namespace Address_book1
{
    public partial class RegisterWindow : Window
    {
        private string connectionString = "Host=localhost;Port=5432;Database=AddressbookDB;Username=postgres;Password=QWERTY1221;Client Encoding=UTF8";

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password1 = PasswordBox.Password;
            string password2 = ConfirmPasswordBox.Password;

            if (password1 != password2)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO users (username, password_hash, role) VALUES (@username, @password, 'user')";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("password", password1); // Тут позже будет хэш
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Регистрация успешна!");
            this.Close();
        }
    }
}
