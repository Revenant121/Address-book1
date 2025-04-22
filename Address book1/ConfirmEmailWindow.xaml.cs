using System.Windows;

namespace Address_book1
{
    public partial class ConfirmEmailWindow : Window
    {
        private string expectedCode;

        public bool IsConfirmed { get; private set; } = false;

        public ConfirmEmailWindow(string sentCode)
        {
            InitializeComponent();
            expectedCode = sentCode;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTextBox.Text.Trim() == expectedCode)
            {   
                IsConfirmed = true;
                MessageBox.Show("Код подтверждён!");
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный код, попробуйте снова.");
            }
        }
    }
}
