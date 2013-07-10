using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFXManager
{

    [Serializable]
    public class Institution
    {

        private string _ID;
        
        public string ID
        {
            get
            {
                return _ID;
            }
            private set
            {
                _ID = value;
            }
        }
        
        private string _name;

        public string Name
        {
            get
            {
                return _name;
            }
            private set
            {
                _name = value;
            }
        }

        private string _fid;

        public string FID
        {
            get
            {
                return _fid;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                _fid = value;
            }
        }


        private string _org;

        public string ORG
        {
            get
            {
                return _org;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                _org = value;
            }
        }

        private string _url;

        public string URL
        {
            get
            {
                return _url;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                _url = value;
            }
        }

        private string _brokerId;

        public string BrokerID
        {
            get
            {
                return _brokerId;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                _brokerId = value;
            }
        }

        public Institution(string name, string ID)
        {
            this.ID = ID;
            this.Name = name;
        }



        public override string ToString()
        {
            return _name;
        }


    }
}
