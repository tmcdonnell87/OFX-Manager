using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Security;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Xml;
using System.Security;
using System.Runtime.InteropServices;
using System.Net;
using System.Security.Authentication;

namespace OFXManager
{
    public abstract class OFXConnection
    {
        public enum SecurityType
        {
            NONE, TYPE1
        }

        public string OfxHeader {get;set;}
        public string Version {get;set;}
        public SecurityType Security {get;set;}
        public string AppID { get; set; }
        public string AppVer { get; set; }


        public OFXConnection(string header, string version, SecurityType sec, string appid, string appVer)
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                throw new ArgumentNullException("header", "An OFX Connection cannot have a null header");
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentNullException("version", "An OFX Connection cannot have a null version");
            }

            if (string.IsNullOrWhiteSpace(appid))
            {
                throw new ArgumentNullException("version", "An OFX Connection cannot have a null application ID");
            }


            this.OfxHeader = header;
            this.Version = version;
            this.Security = sec;
            this.AppID = appid;


            if (string.IsNullOrWhiteSpace(appVer)) appVer = "0100";
            this.AppVer = appVer;

        }

        public abstract bool Validate(string OFX);
 
        public abstract string RequestStatement(Account acct, SecureString password, DateTime? start, DateTime? end);

        protected virtual HttpWebRequest Connect(Uri path)
        {

            if (path.Scheme.ToUpper() != "HTTPS")
            {
                throw new AuthenticationException("Host institution connection not using secure HTTPS connection scheme: " + path.AbsoluteUri);

            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path);

            return request;
        }

        public class GeneralErrorException : Exception
        {
            public GeneralErrorException(string message)
                : base(message)
            {
            }

            public GeneralErrorException()
                : base()
            {
            }

        }

        public class LoginErrorException : Exception
        {
            public LoginErrorException(string message)
                : base(message)
            {
            }

            public LoginErrorException()
                : base()
            {
            }
        }

        public class InvalidVersionException : Exception
        {
            public InvalidVersionException(string message)
                : base(message)
            {
            }

            public InvalidVersionException()
                : base()
            {
            }
        }

        public class InvalidAccountException : Exception
        {
            public InvalidAccountException(string message)
                : base(message)
            {
            }

            public InvalidAccountException()
                : base()
            {
            }
        }
    }
}
