using Fiddler;
using Imposter.Model;
using MahApps.Metro.Controls;
using System;
using System.Windows;

namespace Imposter
{
    /// <summary>
    /// Interaction logic for ProfileEditor.xaml
    /// </summary>
    public partial class ProfileEditor : MetroWindow
    {
        public Profile Profile
        {
            get
            {
                return new Profile
                {
                    Name = Name.Text,
                    LocalDirectory = Local.Text,
                    RemoteUrl = Remote.Text,
                    Port = int.Parse(Port.Text),
                    DecryptSsl = DecryptSsl.IsChecked != null ? DecryptSsl.IsChecked.Value : false
                };
            }
            set
            {
                Name.Text = value.Name == Profile.DefaultName ? string.Empty : value.Name;
                Local.Text = value.LocalDirectory;
                Remote.Text = value.RemoteUrl;
                Port.Text = value.Port.ToString();
                DecryptSsl.IsChecked = value.DecryptSsl;
            }
        }

        public ProfileEditor(Profile profile)
        {
            InitializeComponent();

            Profile = profile;

            Save.Click += Save_Click;
            Cancel.Click += Cancel_Click;
            DecryptSsl.Checked += DecryptSsl_Checked;
        }

        private void DecryptSsl_Checked(object sender, RoutedEventArgs e)
        {
            // Ensure fiddler certificate is trusted
            if (DecryptSsl.IsChecked != null && DecryptSsl.IsChecked.Value == true)
            {
                if (!CertMaker.rootCertExists())
                {
                    if (!Fiddler.CertMaker.createRootCert())
                    {
                        throw new Exception("Unable to create cert for FiddlerCore.");
                    }
                }

                if (!CertMaker.rootCertIsTrusted())
                {
                    if (!CertMaker.trustRootCert())
                    {
                        throw new Exception("Unable to install FiddlerCore's cert.");
                    }
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Name.Text) || string.IsNullOrEmpty(Local.Text) || 
                string.IsNullOrEmpty(Remote.Text) || string.IsNullOrEmpty(Port.Text))
            {
                MessageBox.Show("Name, Base Url, Local Directory and Port are required fields. Please fill them in before continuing.");
                return;
            }

            DialogResult = true;
        }
    }
}
