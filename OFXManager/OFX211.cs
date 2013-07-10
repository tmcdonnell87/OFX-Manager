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



namespace OFXManager
{
    class OFX211:OFXConnection
    {

        public OFX211(string header, SecurityType sec, string appid, string appVer):base(header,"211",sec,appid,appVer)
        {

        }

        public override string RequestStatement(Account acct, SecureString password, DateTime? start, DateTime? end)
        {

            string str = "";
            
            Uri path = new Uri(acct.HostInstitution.URL);
            HttpWebRequest request = null;
            Stream stream=null;

            HttpWebResponse response;
            try
            {
                request = Connect(path);
                request.ContentType = "text/xml";
                request.Method = "POST";
                request.Proxy = null;

                stream = request.GetRequestStream();

                StreamWriter writer = new StreamWriter(stream);
                //writer.Write(GenerateDeclaration());
                //writer.Write(GenerateOFXHeader("NONE",Guid.NewGuid().ToString().ToUpper()));
                writer.Write("<OFX>");
                WriteSignonToStream(writer, acct, password);
                writer.Write(GenerateStatementRequest(acct, start,end));
                writer.Write("</OFX>");
                writer.Flush();
                stream.Close();

            }
            catch (Exception e)
            {
                if (request!=null) request.Abort();
                if (stream != null) stream.Close();
                throw e;
            }

            using (response = (HttpWebResponse)request.GetResponse())
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                Console.WriteLine("Response stream received.");
                Console.WriteLine(readStream.ReadToEnd());
                response.Close();
                readStream.Close();
            }
            
            return str;            
        }



        private string GenerateDeclaration()
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>";
        }


        private string GenerateOFXHeader(string oldid, string newid)
        {
            StringBuilder header = new StringBuilder();
            if (String.IsNullOrWhiteSpace(oldid)) oldid = "NONE";
            if (String.IsNullOrWhiteSpace(newid)) newid = "NONE";
            header.AppendFormat("<OFX OFXHEADER=\"{0}\" VERSION=\"{1}\" SECURITY=\"{2}\" OLDFILEUID=\"{3}\" NEWFILEUID=\"{4}\">", OfxHeader, Version, Security, oldid, newid);
            return header.ToString();
        }

        private void WriteSignonToStream(StreamWriter writer, Account acct, SecureString password)
        {

            writer.Write("<SIGNONMSGSRQV1>");
            writer.Write("<SONRQ>");
            writer.Write("<DTCLIENT>{0}</DTCLIENT>", DateTime.Now.ToString("yyyyMMddHHmmss.fff"));
            writer.Write("<USERID>{0}</USERID>", acct.UserID);
            writer.Write("<USERPASS>");
            WritePasswordToStream(writer, password);
            writer.Write("</USERPASS>");
            writer.Write("<FI><ORG>{0}</ORG><FID>{1}</FID></FI>", acct.HostInstitution.ORG, acct.HostInstitution.FID);
            writer.Write("<APPID>{0}</APPID>",this.AppID);
            writer.Write("<APPVER>{0}</APPVER>", this.AppVer);
            writer.Write("</SONRQ>");
            writer.Write("</SIGNONMSGSRQV1>");

        }

        private string GenerateStatementRequest(Account acct, DateTime? start, DateTime? end)
        {
            StringBuilder request = new StringBuilder();

            request.Append("<INVSTMTMSGSRQV1>");
            request.Append("<INVSTMTTRNRQ>");
            request.AppendFormat("<TRNUID>{0}</TRNUID>",Guid.NewGuid().ToString().ToUpper());
            request.Append("<INVSTMTRQ>");

            request.Append("<INVACCTFROM>");
            request.AppendFormat("<BROKERID>{0}</BROKERID>",acct.HostInstitution.BrokerID);
            request.AppendFormat("<ACCTID>{0}</ACCTID>", acct.HostInstitution.ID);
            request.Append("</INVACCTFROM>");

            request.Append("<INCTRAN>");
            if (start!=null)  request.AppendFormat("<DTSTART>{0}</DTSTART>", ((DateTime)start).ToString("yyyyMMddHHmmss.fff"));
            if (end!=null) request.AppendFormat("<DTEND>{0}</DTEND>", ((DateTime)end).ToString("yyyyMMddHHmmss.fff"));
            request.AppendFormat("<INCLUDE>Y</INCLUDE>");
            request.Append("</INCTRAN>");
            request.Append("<INCOO>Y</INCOO>");
            request.Append("<INCPOS>");
            if (end!=null) request.AppendFormat("<DTASOF>{0}</DTASOF>", ((DateTime)end).ToString("yyyyMMddHHmmss.fff"));
            request.AppendFormat("<INCLUDE>Y</INCLUDE>");
            request.Append("</INCPOS>");
            request.Append("<INCBAL>Y</INCBAL>");

            request.Append("</INVSTMTRQ>");
            request.Append("</INVSTMTTRNRQ>");
            request.Append("</INVSTMTMSGSRQV1>");
            
            return request.ToString();

        }

        private void WritePasswordToStream(StreamWriter writer, SecureString password)
        {
            IntPtr passPtr = Marshal.SecureStringToBSTR(password);

            int charSize = Marshal.SystemDefaultCharSize;

            //ascii
            if (charSize == 1)
            {
                for (int i = 0; i < password.Length; i++)
                {
                    writer.Write(Marshal.ReadByte(passPtr, i));
                }
            }
            //unicode - don't read entire password into managed memory
            else if (charSize == 2)
            {
                byte[] oneChar = new byte[2];
                UnicodeEncoding enc = new UnicodeEncoding();
                for (int i = 0; i < password.Length; i++)
                {
                    oneChar[0] = Marshal.ReadByte(passPtr, i * 2);
                    oneChar[1] = Marshal.ReadByte(passPtr, (i * 2) + 1);
                    char[] c = enc.GetChars(oneChar);
                    writer.Write(enc.GetChars(oneChar));
                }
                oneChar[0] = 0; oneChar[1] = 0;
            }
            else
            {
                throw new Exception("Could not detect default character encoding");
            }

            Marshal.ZeroFreeBSTR(passPtr);
        }



        public override bool Validate(string OFX)
        {
            throw new NotImplementedException();
        }
        

    }


}
