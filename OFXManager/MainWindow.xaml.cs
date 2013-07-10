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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security;
using System.Net;
using System.Collections.ObjectModel;

namespace OFXManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        List<Institution> _institutions;
        IInstitutionProfileSource _source;
        OFXConnection _connection;
        ApplicationSettings _settings;
        const string FILE_NAME = @"settings.ini";



        public ApplicationSettings Settings
        {
            get { return _settings; }
            private set
            {
                _settings = value;
                OnPropertyChanged("Settings");
            }
        }

        
         
        public List<Institution> Institutions
        {
            get { return _institutions; }
            private set
            {
                _institutions = value;
                OnPropertyChanged("Institutions");
            }
        }

        public MainWindow()
        {
            _source = new OFXHomeConnectionManager("http://www.ofxhome.com/api.php?search=", "http://www.ofxhome.com/api.php?lookup=");
            _connection = new OFX100("100", OFXConnection.SecurityType.NONE, "Money","");


            try
            {
                IsolatedStorageFile _settingsFile = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                                                                                 IsolatedStorageScope.Assembly, null, null);
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(FILE_NAME, FileMode.Open, FileAccess.Read, FileShare.None, _settingsFile))
                {
                    BinaryFormatter serializer = new BinaryFormatter();
                    Settings = (ApplicationSettings)serializer.Deserialize(isoStream);
                }

            }
            catch(System.IO.FileNotFoundException)
            {
                //new user
                Settings = new ApplicationSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading stored user settings: "+ex.ToString(), "Load error", MessageBoxButton.OK, MessageBoxImage.Warning);
                Settings = new ApplicationSettings();
            }
            InitializeComponent();

            
        }



        private void Button_Search_Click(object sender, RoutedEventArgs e)
        {
            List<Institution> result=_source.Search(TextBox_Search.Text);
            if (result == null || result.Count == 0)
            {
                MessageBox.Show("No results found for '" + TextBox_Search.Text+"'.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            Institutions = result;
            Grid_Account.DataContext = null;
            


        }



        private void Combo_Select_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Institution inst = Combo_Select.SelectedItem as Institution;
            if (inst == null) return;
            PasswordBox.Clear();
            foreach (Window view in this.OwnedWindows)
            {
                view.Close();
            }

            try
            {
                if (!_source.DownloadInstitutionData(inst)) return;
            }
            catch (XmlException)
            {
                MessageBox.Show("Could not retrieve valid account information for this institution. The institution will not be loaded.", "Error Retrieving Institutions", MessageBoxButton.OK, MessageBoxImage.Error);
                Grid_Account.DataContext = null;
                return;
            }
            catch(InvalidDataException ex)
            {
                MessageBox.Show(ex.Message+"\n\n"+"The institution will not be loaded.", "Error Retrieving Institution", MessageBoxButton.OK, MessageBoxImage.Warning);
                Grid_Account.DataContext = null;
                return;
            }
            AccountViewModel account = new AccountViewModel(inst, "", "");
            Grid_Account.DataContext = account;
          
        }

        private void ButtonGetTransactions_Click(object sender, RoutedEventArgs e)
        {
            
            if (PasswordBox.SecurePassword.Length == 0)
            {
                MessageBox.Show("Please enter a password.", "Password Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            AccountViewModel avm = Grid_Account.DataContext as AccountViewModel;
            if (avm == null) return;
            Account userAccount = avm.Base;
            if (userAccount == null) return;

            string response = TryRetrieveStatement(userAccount, PasswordBox.SecurePassword, DateStart.SelectedDate, DateEnd.SelectedDate);
            if (!String.IsNullOrWhiteSpace(response))
            {
                Window view = new OFXViewer(response,avm);
                view.Owner = this;
                view.ShowDialog();
                if(!Settings.Accounts.Contains(avm)) Settings.Accounts.Add(avm);
            }
            
        }

        private string TryRetrieveStatement(Account acct, SecureString password, DateTime? start, DateTime? end)
        {
            string response=null;
            try
            {
                response = _connection.RequestStatement(acct, PasswordBox.SecurePassword, DateStart.SelectedDate, DateEnd.SelectedDate);
            }
            catch (WebException ex)
            {
                MessageBox.Show("Error connecting to "+acct.HostInstitution.URL+".\n\n" +ex.Message,"Connection Error",MessageBoxButton.OK,MessageBoxImage.Warning);
                return null;
            }
            try
            {
                if (_connection.Validate(response)) return response;
                return null;
            }
            catch (OFXConnection.LoginErrorException)
            {
                MessageBox.Show("Error signing in to " + acct.ToString() +"\n\nCheck your username and password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
            catch (OFXConnection.InvalidVersionException ex)
            {
                MessageBox.Show("Invalid version of OFX used" + "\n\n" + ex.Message, "Application Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
            catch (OFXConnection.GeneralErrorException ex)
            {
                MessageBox.Show("General error retrieving OFX from " + acct.ToString() + "\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
            catch (OFXConnection.InvalidAccountException)
            {
                MessageBox.Show("Invalid account '"+acct.AccountNumber+"'.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }



        }


        private void PasswordBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox.Clear();
        }

        private void Menu_Accounts_Click(object sender, RoutedEventArgs e)
        {

            AccountViewModel account=((MenuItem)e.OriginalSource).Header as AccountViewModel;
            if (account == null) return;
            Grid_Account.DataContext = account.Clone();
            
        }

        private void MenuItem_RemoveSavedAccount_Click(object sender, RoutedEventArgs e)
        {


            MenuItem source = e.OriginalSource as MenuItem;
            if (source == null) return;
            ContextMenu context = source.Parent as ContextMenu;
            if (context == null) return;
            MenuItem selectedMenu = context.PlacementTarget as MenuItem;
            AccountViewModel account = selectedMenu.Header as AccountViewModel;
            _settings.Accounts.Remove(account);
 
        }


        private void Root_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                _settings.SaveData(FILE_NAME);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving preference information:\n"+ex.Message, "Save error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Preferences_Click(object sender, RoutedEventArgs e)
        {
            Window preferences = new Preferences(Settings);
            preferences.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }


    }
}
