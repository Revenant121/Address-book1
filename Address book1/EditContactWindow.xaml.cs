using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Address_book1.Models;


namespace Address_book1
{
    public partial class EditContactWindow : Window
    {
        public ContactModel Contact { get; private set; }
        private string selectedPhotoPath;

        public EditContactWindow(ContactModel contact)
        {
            InitializeComponent();
            Contact = contact;

            FirstNameBox.Text = contact.FirstName;
            LastNameBox.Text = contact.LastName;
            PhoneBox.Text = contact.phone_number;
            AddressBox.Text = contact.address;

            if (!string.IsNullOrEmpty(contact.PhotoPath) && File.Exists(contact.PhotoPath))
            {
                PhotoPreview.Source = new BitmapImage(new Uri(contact.PhotoPath, UriKind.RelativeOrAbsolute));

            }
        }

        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.png;*.bmp";

            if (dlg.ShowDialog() == true)
            {
                selectedPhotoPath = dlg.FileName;
                PhotoPreview.Source = new BitmapImage(new Uri(selectedPhotoPath));
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Contact.FirstName = FirstNameBox.Text;
            Contact.LastName = LastNameBox.Text;
            Contact.phone_number = PhoneBox.Text;
            Contact.address = AddressBox.Text;

            if (!string.IsNullOrEmpty(selectedPhotoPath))
            {
                string imagesDir = "Images";
                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                string fileName = System.IO.Path.GetFileName(selectedPhotoPath);
                string destPath = System.IO.Path.Combine(imagesDir, fileName);

                File.Copy(selectedPhotoPath, destPath, true);
                Contact.PhotoPath = Path.GetFullPath(destPath);
            }

            DialogResult = true;
            Close();
        }
    }
}
