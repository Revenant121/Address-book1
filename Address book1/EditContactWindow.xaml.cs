using System.Windows;
using Address_book1.Models;

namespace Address_book1
{
    public partial class EditContactWindow : Window
    {
        public ContactModel Contact { get; private set; }

        public EditContactWindow(ContactModel contact)
        {
            InitializeComponent();
            Contact = contact;

            // Заполняем поля текущими значениями контакта
            FirstNameBox.Text = contact.FirstName;
            LastNameBox.Text = contact.LastName;
            PhoneBox.Text = contact.phone_number;
            AddressBox.Text = contact.address;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем значения в объекте Contact
            Contact.FirstName = FirstNameBox.Text;
            Contact.LastName = LastNameBox.Text;
            Contact.phone_number = PhoneBox.Text;
            Contact.address = AddressBox.Text;

            DialogResult = true;
            Close();
        }
    }
}
