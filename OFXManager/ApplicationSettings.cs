using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.IsolatedStorage;
using System.ComponentModel;

namespace OFXManager
{
    [Serializable]
    public class ApplicationSettings:ISerializable, INotifyPropertyChanged
    {
        [NonSerialized] private bool appSettingsChanged;
        [NonSerialized] private ObservableCollection<AccountViewModel> _accounts;
        private SaveType _savePreference;

        [Serializable]
        public enum SaveType
        {
            NONE,
            BANK,
            ACCOUNT
        }

        public SaveType SavePreference
        {
            get 
            {
                return _savePreference;
            }
            set
            {
                _savePreference = value;
                appSettingsChanged = true;
                OnPropertyChanged("SavePreference");
            }
        }

        public ObservableCollection<AccountViewModel> Accounts
        {
            get
            {
                return _accounts;
            }
            set
            {
                _accounts = value;
                appSettingsChanged = true;
                OnPropertyChanged("Accounts");
            }
        }

        public void SaveData(string FILE_NAME) 
        {
            if (FILE_NAME == null) return;
            if (!appSettingsChanged) return;

            IsolatedStorageFile _settingsFile = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                                                                 IsolatedStorageScope.Assembly, null, null);

            if (SavePreference == SaveType.NONE)
            {
                if (File.Exists(FILE_NAME))
                {
                    _settingsFile.DeleteFile(FILE_NAME);
                }
                return;
            }


            BinaryFormatter serializer = null;
            try
            {
                serializer = new BinaryFormatter();
                // Serialize this instance of the ApplicationSettings 
                // class to the config file.
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(FILE_NAME, FileMode.Create, FileAccess.Write, FileShare.None, _settingsFile)) serializer.Serialize(isoStream, this);
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            List<Account> accts = new List<Account>();
            foreach (AccountViewModel acct in Accounts)
            {
                if (SavePreference == SaveType.BANK)
                {
                    accts.Add(new Account(acct.HostInstitution, "", ""));
                }
                else
                {
                    accts.Add(acct.Base);
                }
            }
            info.AddValue("ACCOUNTS", accts.ToArray<Account>());
            info.AddValue("SAVETYPE", SavePreference);
        }

        public ApplicationSettings (SerializationInfo si, StreamingContext sc)
        {

            Accounts=new ObservableCollection<AccountViewModel>();

            List<Account> accts = new List<Account>((Account[])si.GetValue("ACCOUNTS",typeof(Account[])));


            if (accts != null)
            {

                foreach (Account acct in accts)
                {
                    Accounts.Add(new AccountViewModel(acct));
                }
            }
            SavePreference = (SaveType)si.GetValue("SAVETYPE",typeof(SaveType));

        }

        public ApplicationSettings()
        {
            this.SavePreference = SaveType.NONE;
            this.Accounts = new ObservableCollection<AccountViewModel>();
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
