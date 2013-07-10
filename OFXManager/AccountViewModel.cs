using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Security;

namespace OFXManager
{
    public class AccountViewModel:INotifyPropertyChanged, IEquatable<AccountViewModel>
    {
        private Account _base;

        public Account Base
        {
            get{
                return _base;
            }
            private set
            {
                _base = value;
                OnPropertyChanged("Base");
            }
        }

        public Institution HostInstitution
        {
            get
            {
                return _base.HostInstitution;
            }                  
        }

        public string UserID
        {
            get
            {
                return _base.UserID;
            }
            set
            {
                _base.UserID = value;
                OnPropertyChanged("UserID");
            }
        }

        public string AccountNumber
        {
            get
            {
                return _base.AccountNumber;
            }
            set
            {
                _base.AccountNumber = value;
                OnPropertyChanged("AccountNumber");
            }
        }


        public override string ToString()
        {
            return _base.ToString();
        }

        public AccountViewModel(Account acct)
        {
            Base = acct;
        }

        public AccountViewModel(Institution host,string user, string number)
        {
            Base = new Account(host,user,number);
        }

        public AccountViewModel Clone()
        {
            return new AccountViewModel(Base.Clone());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            AccountViewModel other = obj as AccountViewModel;
            if (other == null) return false;

            return Base == other.Base;
        }

        public bool Equals(AccountViewModel other)
        {
            if (other == null) return false;
            return Base == other.Base;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(AccountViewModel a, AccountViewModel b)
        {
            if (System.Object.ReferenceEquals(a, b)) return true;

            if ((object)a == null || (object)b == null) return false;

            return a.Base == b.Base;
        }

        public static bool operator !=(AccountViewModel a, AccountViewModel b)
        {
            return !(a == b);
        }
    }
}
