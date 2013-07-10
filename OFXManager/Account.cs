
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Xml;

namespace OFXManager
{
    [Serializable]
    public class Account : IEquatable<Account>
    {
        Institution _hostInstitution;
        string _userID;
        string _accountNumber;

        public Institution HostInstitution
        {
            get
            {
                return _hostInstitution;
            }
            protected set
            {
                _hostInstitution = value;
            }
        }

        public string UserID
        {
            get
            {
                return _userID;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;
                _userID=value;
            }
        }

        public string AccountNumber
        {
            get
            {
                return _accountNumber;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;
                _accountNumber=value;
            }
        }



        public Account(Institution host, string userID, string accountNumber)
        {
            this.HostInstitution = host;
            this.UserID = userID;
            this.AccountNumber = accountNumber;
        }

        public override string ToString()
        {
            StringBuilder name = new StringBuilder();
            name.Append(HostInstitution.ToString());
            if (!String.IsNullOrWhiteSpace(AccountNumber))
            {
                name.AppendFormat("  ({0})", AccountNumber.Length > 5 ? "..." + AccountNumber.Substring(AccountNumber.Length - 5) : AccountNumber);
            }
            return name.ToString();
        }

        public Account Clone()
        {
            return new Account(HostInstitution, UserID, AccountNumber);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Account other = obj as Account;
            if ((object)other == null) return false;

            if (HostInstitution != other.HostInstitution) return false;
            if (UserID != other.UserID) return false;
            if (AccountNumber != other.AccountNumber) return false;

            return true;
            
        }

        public bool Equals(Account other)
        {
            if ((object)other == null) return false;

            if (HostInstitution != other.HostInstitution) return false;
            if (UserID != other.UserID) return false;
            if (AccountNumber != other.AccountNumber) return false;

            return true;

        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Account a, Account b)
        {
            if (System.Object.ReferenceEquals(a, b)) return true;

            if ((object)a == null || (object)b == null) return false;

            if (a.HostInstitution != b.HostInstitution) return false;
            if (a.UserID != b.UserID) return false;
            if (a.AccountNumber != b.AccountNumber) return false;

            return true;
        }

        public static bool operator !=(Account a, Account b)
        {
            return !(a == b);
        }
    }
}
