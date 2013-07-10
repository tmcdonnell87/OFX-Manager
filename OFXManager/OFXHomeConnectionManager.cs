using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows;
using System.IO;
using System.Xml;
using System.Web;

namespace OFXManager
{
    class OFXHomeConnectionManager:IInstitutionProfileSource
    {

        public string SearchString { get; set; }
        public string DetailsString {get; set; }

        public OFXHomeConnectionManager(string search, string details)
        {
            this.SearchString = search;
            this.DetailsString = details;
        }

        public List<Institution> Search(string name)
        {

            List<Institution> institutionList = new List<Institution>();
            if (String.IsNullOrWhiteSpace(name)) return institutionList;

            string searchSite = SearchString + name;


            XmlDocument doc = new XmlDocument();

            // Load data  
            try
            {
                doc.Load(searchSite);
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not open file from \"" + searchSite + "\".\n\n" + e.Message, "Error Retrieving Institutions", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            XmlNodeList list = doc.DocumentElement.SelectNodes("//institutionid");

            if (list.Count == 0) return institutionList;

            string instName;
            string instID;
            foreach (XmlNode inst in list)
            {
                if (inst.Attributes["name"] == null || inst.Attributes["id"] == null) continue;
                instName = inst.Attributes["name"].InnerText;
                instID = inst.Attributes["id"].InnerText;
                if (!String.IsNullOrWhiteSpace(instName) && !String.IsNullOrWhiteSpace(instID))
                {
                    institutionList.Add(new Institution(instName, instID));
                }
            }

            return institutionList;
        }


        public bool DownloadInstitutionData(Institution inst)
        {
            //false if failure
            bool failure = !true;

            if (inst == null||string.IsNullOrWhiteSpace(inst.ID)) return failure;

            XmlDocument doc = new XmlDocument();

            string searchString = DetailsString + inst.ID;

            doc.Load(searchString);

            

            XmlNode head = doc.DocumentElement;
            XmlNode node;
            if (head.Attributes["id"]==null||head.Attributes["id"].InnerText!=inst.ID||
                head.SelectSingleNode("name") == null || head.SelectSingleNode("name").InnerText != inst.Name)
            {
                
                throw new InvalidDataException("Institution returned did not match \"" + inst.Name + "\".");
            }

            node = head.SelectSingleNode("url");
            if (node == null||String.IsNullOrWhiteSpace(node.InnerText)) throw new InvalidDataException("Institution returned did not return a valid connection URL.");
            inst.URL = node.InnerText;

            node = head.SelectSingleNode("org");
            if (node == null || String.IsNullOrWhiteSpace(node.InnerText)) throw new InvalidDataException("Institution returned did not return a valid connection ORG.");
            inst.ORG = node.InnerText;


            node = head.SelectSingleNode("fid");
            if (node == null || String.IsNullOrWhiteSpace(node.InnerText)) throw new InvalidDataException("Institution returned did not return a valid identifier (FID).");
            inst.FID = node.InnerText;

            node = head.SelectSingleNode("brokerid");
            if (node == null || String.IsNullOrWhiteSpace(node.InnerText))
            {
                Uri path = new Uri(inst.URL);
                if (path.Host != null) inst.BrokerID = path.Host.Substring(path.Host.IndexOf(".")+1);
                else throw new InvalidDataException("Institution returned did not return a valid Broker ID.");
            }
            else inst.BrokerID = node.InnerText;

            return !failure;


        }
    }
}
