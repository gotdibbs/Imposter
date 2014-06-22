using Fiddler;
using Imposter.Model;
using MahApps.Metro.Controls;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.IO;

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
                    DecryptSsl = DecryptSsl.IsChecked != null ? DecryptSsl.IsChecked.Value : false,
                    Overrides = new List<Override>(Overrides.ItemsSource as IEnumerable<Override>)
                };
            }
            set
            {
                Name.Text = value.Name == Profile.DefaultName ? string.Empty : value.Name;
                Local.Text = value.LocalDirectory;
                Remote.Text = value.RemoteUrl;
                Port.Text = value.Port.ToString();
                DecryptSsl.IsChecked = value.DecryptSsl;
                Overrides.ItemsSource = value.Overrides;
            }
        }

        public ProfileEditor(Profile profile)
        {
            InitializeComponent();

            Profile = profile;

            Save.Click += Save_Click;
            Cancel.Click += Cancel_Click;
            DecryptSsl.Checked += DecryptSsl_Checked;
            //AddOverride.Click += AddOverride_Click;
            //DeleteOverride.Click += DeleteOverride_Click;
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Name.Text) || string.IsNullOrEmpty(Local.Text) || 
                string.IsNullOrEmpty(Remote.Text) || string.IsNullOrEmpty(Port.Text))
            {
                MessageBox.Show("Name, Base Url, Local Directory and Port are required fields. Please fill them in before continuing.");
                return;
            }

            // Check to make sure the directory we're supposed to be serving from actually exists
            if (!Directory.Exists(Local.Text))
            {
                if (MessageBox.Show("The chosen local directory does not exist. Attempt to create it?", string.Empty, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    try 
                    {
                        Directory.CreateDirectory(Local.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to create local directory. This is probably a permissions issue. Please correct this issue before continuining.");
                        return;
                    }
                }
                else
                {
                    // Disallow save if the directory is not valid.
                    return;
                }
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
