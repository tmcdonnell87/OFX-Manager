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
using System.Xml;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;

namespace OFXManager
{
    /// <summary>
    /// Interaction logic for OFXViewer.xaml
    /// </summary>
    public partial class OFXViewer : Window, INotifyPropertyChanged
    {
        AccountViewModel _account;
        string _OFX;

        public AccountViewModel Account
        {
            get
            {
                return _account;
            }
            set
            {
                _account = value;
                OnPropertyChanged("Account");
            }
        }

        public string OFX
        {
            get
            {
                return _OFX;
            }
            set
            {
                _OFX = value;
                OnPropertyChanged("OFX");
            }
        }

        public OFXViewer(string display, AccountViewModel acct)
        {
            InitializeComponent();
            this.Account = acct;
            try
            {
                XmlDocument root = new XmlDocument();
                root.LoadXml(display);
                OFX = FormatXMLAsString(root, Formatting.Indented);
            }
            catch (XmlException)
            {
                OFX=display;
            }

        }


        private string FormatXMLAsString(XmlDocument root, Formatting format)
        {
            StringBuilder sb = new StringBuilder();
            TextWriter tr = new StringWriter(sb);
            XmlTextWriter writer = new XmlTextWriter(tr);
            writer.Formatting = format;
            root.Save(writer);
            writer.Close();
            return sb.ToString();

        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            string path;
             SaveFileDialog dlg = new SaveFileDialog();

             dlg.Filter = "OFX files (*.ofx)|*.ofx"  ;
             dlg.FilterIndex = 1;
             dlg.FileName = Account.AccountNumber + "_" + DateTime.Now.ToString("MM-dd-yyyy");
             

             dlg.RestoreDirectory = true ;

             if(dlg.ShowDialog() == true)
             {
                 path=dlg.FileName;
                 using (StreamWriter outStream = new StreamWriter(File.Create(path)))
                 {
                     outStream.Write(OFX);
                 }
             }
             this.Close();


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
