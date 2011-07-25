using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using Microsoft.Win32;

namespace MineWorld
{
    public static class HttpRequest
    {
        public static string Post(string url, Dictionary<string, string> parameters)
        {
            WebRequest request = WebRequest.Create(url);
            RegistryKey IEsettings = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            if ((int)IEsettings.GetValue("ProxyEnable") == 0)
            {
                request.Proxy = null;
            }
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";

            // Write the POST data.
            string paramString = EncodeParameters(parameters);
            byte[] bytes = Encoding.ASCII.GetBytes(paramString);
            Stream os = null;

            try
            {
                request.ContentLength = bytes.Length;
                os = request.GetRequestStream();
                os.Write (bytes, 0, bytes.Length);
            }
            catch (WebException ex)
            {
                throw new Exception("Request error", ex);
            }
            finally
            {
                if (os != null) os.Close();
            }

            return ReadResponse(request);
        }

        public static string Get(string url, Dictionary<string, string> parameters)
        {
            // Append the parameters to the URL.
            string paramString = EncodeParameters(parameters);
            if (paramString != "") url = url + "?" + paramString;
            WebRequest request = WebRequest.Create(url);
            RegistryKey IEsettings = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            if ((int)IEsettings.GetValue("ProxyEnable") == 0)
            {
                request.Proxy = null;
            }
            return ReadResponse(request);
        }

        private static string ReadResponse(WebRequest request)
        {
            string responseText;

            try
            {
                WebResponse response = null; //= request.GetResponse();
                if (response == null)
                {
                    //throw new Exception("No response");
                    //We couldt get a response from the server so lets set it to ""
                    responseText = "";
                }
                else
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    responseText = sr.ReadToEnd().Trim();
                }
            }
            catch
            {
                //throw new Exception("Response error ["+request.ToString()+"]", ex);
                //Server didnt respond so lets set it to "" so that we dont send a null string back
                responseText = "";
            }

            return responseText;
        }

        public static string EncodeParameters(Dictionary<string, string> parameters)
        {
            if (parameters == null) return "";

            // Parameters are of the form: "name1=value1&name2=value2"
            string[] entryStrings = new string[parameters.Count];
            int i = 0;
            foreach (KeyValuePair<string, string> entry in parameters)
            {
                entryStrings[i] = entry.Key + "=" + entry.Value;
                i += 1;
            }
            return string.Join("&", entryStrings);
        }
    }
}
