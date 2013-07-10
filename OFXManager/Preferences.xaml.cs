using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OFXManager
{
    /// <summary>
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Preferences : Window
    {
        public ApplicationSettings Settings { get; set; }

        public Preferences(ApplicationSettings settings)
        {
            this.Settings = settings;
            InitializeComponent();
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (RadioNone.IsChecked == true) Settings.SavePreference = ApplicationSettings.SaveType.NONE;
            if (RadioBank.IsChecked == true) Settings.SavePreference = ApplicationSettings.SaveType.BANK;
            if (RadioAccount.IsChecked == true) Settings.SavePreference = ApplicationSettings.SaveType.ACCOUNT;
            this.Close();

        }
    }
}
