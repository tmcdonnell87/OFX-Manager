using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.IO;
using System.Net.Security;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace OFXManager
{
    class OFX100:OFXConnection
    {
        public override string RequestStatement(Account acct, SecureString password, DateTime? start, DateTime? end)
        {
            /*
            FileStream file=new FileStream(@"C:\tmp\ofxtest.log",FileMode.Open, FileAccess.Read,FileShare.Read);
            StreamReader input = new StreamReader(file);
            string query=input.ReadToEnd();
            StreamWriter outStream = new StreamWriter(File.Create(@"C:\tmp\ofxout2.log"));
            */

            Uri path = new Uri(acct.HostInstitution.URL);
            HttpWebRequest request = null;
            Stream stream=null;
            string output = null;
            HttpWebResponse response;
            try
            {
                request = Connect(path);
                request.ContentType = "text/xml";
                request.Method = "POST";
                request.Proxy = null;

                stream = request.GetRequestStream();
                StreamWriter writer = new StreamWriter(stream);

                writer.Write(GenerateHeader("NONE", Guid.NewGuid().ToString().ToUpper()));
                writer.WriteLine("<OFX>");
                WriteSignonToStream(writer, acct, password);
                writer.Write(GenerateStatementRequest(acct, start, end));
                writer.Write("</OFX>");
                writer.Flush();
                stream.Close();
            }
            catch (Exception e)
            {
                if (request != null) request.Abort();
                if (stream != null) stream.Close();
                throw e;
            }

            using (response = (HttpWebResponse)request.GetResponse())
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);


                output=readStream.ReadToEnd();
                response.Close();
                readStream.Close();
            }
            return output;            

        }

        private void WriteSignonToStream(StreamWriter writer, Account acct, SecureString password)
        {

            writer.WriteLine("<SIGNONMSGSRQV1>");
            writer.WriteLine("<SONRQ>");
            writer.WriteLine("<DTCLIENT>{0}", DateTime.Now.ToString("yyyyMMddHHmmss.fff"));
            writer.WriteLine("<USERID>{0}", acct.UserID);
            writer.Write("<USERPASS>");
            WritePasswordToStream(writer, password);
            writer.WriteLine();
            writer.WriteLine("<LANGUAGE>{0}","ENG");
            writer.WriteLine("<FI>\r\n<ORG>{0}\r\n<FID>{1}\r\n</FI>", acct.HostInstitution.ORG, acct.HostInstitution.FID);
            writer.WriteLine("<APPID>{0}", this.AppID);
            writer.WriteLine("<APPVER>{0}", this.AppVer);
            writer.WriteLine("</SONRQ>");
            writer.WriteLine("</SIGNONMSGSRQV1>");

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

        private string GenerateHeader(string oldid, string newid)
        {
            if (String.IsNullOrWhiteSpace(oldid)) oldid = "NONE";
            if (String.IsNullOrWhiteSpace(newid)) newid = "NONE";

            StringBuilder output = new StringBuilder();
            output.AppendLine("OFXHEADER:"+this.OfxHeader);
            output.AppendLine("DATA:OFXSGML");
            output.AppendLine("VERSION:"+this.Version);
            output.AppendLine("SECURITY:NONE");
            output.AppendLine("ENCODING:UNICODE");
            output.AppendLine("COMPRESSION:NONE");
            output.AppendLine("OLDFILEUID:"+oldid);
            output.AppendLine("NEWFILEUID:"+newid);
            output.AppendLine();
            return output.ToString();


        }

        private string GenerateStatementRequest(Account acct, DateTime? start, DateTime? end)
        {
            StringBuilder request = new StringBuilder();

            request.AppendLine("<INVSTMTMSGSRQV1>");
            request.AppendLine("<INVSTMTTRNRQ>");
            request.AppendLine("<TRNUID>"+Guid.NewGuid().ToString().ToUpper());
            request.AppendLine("<INVSTMTRQ>");

            request.AppendLine("<INVACCTFROM>");
            request.AppendLine("<BROKERID>"+ acct.HostInstitution.BrokerID);
            request.AppendLine("<ACCTID>"+ acct.AccountNumber);
            request.AppendLine("</INVACCTFROM>");

            request.AppendLine("<INCTRAN>");
            if (start != null) request.AppendLine("<DTSTART>"+ ((DateTime)start).ToString("yyyyMMdd"));
            if (end != null) request.AppendLine("<DTEND>" + ((DateTime)end).ToString("yyyyMMdd"));
            request.AppendLine("<INCLUDE>Y");
            request.AppendLine("</INCTRAN>");
            request.AppendLine("<INCOO>Y");
            request.AppendLine("<INCPOS>");
            if (end != null) request.AppendLine("<DTASOF>"+ ((DateTime)end).ToString("yyyyMMddHHmmss.fff"));
            request.AppendLine("<INCLUDE>Y");
            request.AppendLine("</INCPOS>");
            request.AppendLine("<INCBAL>Y");

            request.AppendLine("</INVSTMTRQ>");
            request.AppendLine("</INVSTMTTRNRQ>");
            request.AppendLine("</INVSTMTMSGSRQV1>");

            return request.ToString();

        }


        public override bool Validate(string OFX)
        {
            Match code;
            Match severity;
            Match message;
            int error;

            //Get error status code, if applicable.
            foreach (Match status in Regex.Matches(OFX, @"(?<=(<STATUS>)).*?(?=</STATUS>)", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                code = Regex.Match(status.ToString(), @"(?<=(<CODE>))[^<]+", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (code.Success == false || code.ToString().Trim() == "0")
                    continue;

                error = Int32.Parse(code.ToString());
                message = Regex.Match(status.ToString(), @"(?<=(<MESSAGE>))[^<]+", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                switch (error)
                {
                    case 15500:
                        throw new LoginErrorException(message.ToString());                        
                    case 2021:
                        throw new InvalidVersionException(message.ToString());
                    case 2003:
                        throw new InvalidAccountException(message.ToString());
                    default:
                        severity = Regex.Match(OFX, @"(?<=(<SEVERITY>))\w+(?=<)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        if (severity.ToString() != "ERROR")
                            throw new GeneralErrorException(message.ToString());
                        break;
                        

                }


            }

            return true;

        }

        public OFX100(string header, SecurityType sec, string appid, string appVer):base(header,"100",sec,appid,appVer)
        {

        }

    }
}
