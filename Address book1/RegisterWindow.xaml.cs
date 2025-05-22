using System;
using System.Net;
using System.Net.Mail;
using System.Windows;
using Npgsql;
using Microsoft.VisualBasic;


namespace Address_book1
{
    public partial class RegisterWindow : Window
    {
        private string connectionString = "Host=localhost;Port=5432;Database=AddressbookDB;Username=postgres;Password=QWERTY1221;Client Encoding=UTF8";
        private string verificationCode;

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string password1 = PasswordBox.Password;
            string password2 = ConfirmPasswordBox.Password;
            string email = EmailBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password1) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            if (password1 != password2)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            try
            {
                
                verificationCode = new Random().Next(100000, 999999).ToString();

                
                SendVerificationCode(email, verificationCode);

               
                
                var confirmWindow = new ConfirmEmailWindow(verificationCode);
                bool? result = confirmWindow.ShowDialog();

                if (result != true || !confirmWindow.IsConfirmed)
                {
                    MessageBox.Show("Неверный код подтверждения!");
                    return;
                }


                
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO users (username, password_hash, email, role) VALUES (@username, @password, @email, 'user')";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("username", username);
                        cmd.Parameters.AddWithValue("password", password1); 
                        cmd.Parameters.AddWithValue("email", email);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Регистрация успешна!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void SendVerificationCode(string toEmail, string code)
        {
            string fromEmail = "revenantrevenant6@gmail.com";       
            string fromPassword = "zfam kwkl tslc bxlb\r\n";  

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage(fromEmail, toEmail)
            {
                Subject = "Код подтверждения",
                Body = $"Вот твой код подтверждения: {code}",
                IsBodyHtml = false
            };

            smtpClient.Send(mailMessage);
        }
    }
}
