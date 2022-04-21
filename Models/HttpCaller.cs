using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Diagnostics;

    public class HttpCaller
    {
        public static HttpWebResponse MetaPost(XmlDocument xmlDocument, List<KeyValuePair<string, string>> metaData, string authentication, string documentHandle)
        {
            if (String.IsNullOrEmpty(documentHandle) || documentHandle.Contains("Collection-"))
            {
                documentHandle = xmlDocument.SelectSingleNode("/document/handle").InnerXml.ToString().Split('\"')[1];
            }
            if (documentHandle.Contains("/GWDoc"))
            {
                documentHandle = documentHandle.Replace("/GWDoc", "");
            }
            //update document properties

            StringBuilder stringBuilder = new StringBuilder();
            string DocuShareURL = ConfigurationManager.AppSettings["DocuShareUrl"];
            string AuthToken = authentication;

            //Create the request XML
            stringBuilder.Append("<?xml version=\"1.0\" ?><propertyupdate>");
            stringBuilder.Append("<set><prop>");
            foreach (var item in metaData)
            {
                stringBuilder.Append("<" + item.Key + "><![CDATA[" + item.Value + "]]></" + item.Key + ">");
            }

            stringBuilder.Append("</prop></set>");
            stringBuilder.Append("</propertyupdate>");

            //Create the request header with the search command
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(DocuShareURL + "dsweb/PROPPATCH/" + documentHandle);
            httpWebRequest.ContentType = "text/xml";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Set("Cookie", AuthToken);
            httpWebRequest.Accept = "*/*, text/xml";
            httpWebRequest.Headers.Set("Accept-Language", "en");
            httpWebRequest.Headers.Set("DocuShare-Version", "5.0");
            httpWebRequest.UserAgent = "DsAxess/4.0";

            //Make the request
            byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            httpWebRequest.ContentLength = stringBuilder.Length;
            Stream requestStream = httpWebRequest.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            return httpWebResponse;
        }

        public static XmlDocument Post(string url, List<String> Paramaters, string contentType, string boundry, string authentication, string fileName, byte[] document, string header)
        {
            XmlDocument xmlDocument = new XmlDocument();
            string DocuShareUrl = ConfigurationManager.AppSettings["DocuShareUrl"] + url;

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("<?xml version=\"1.0\" ?>");

            foreach (var item in Paramaters)
            {
                stringBuilder.Append(item);
            }
            Uri Connection = new Uri(DocuShareUrl);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Connection);
            //The HTTP header information remains fairly consistant
            httpWebRequest.ContentLength = stringBuilder.Length;
            httpWebRequest.Method = "POST";

            httpWebRequest.ContentType = contentType + boundry;
            byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundry + "\r\n");

            
            httpWebRequest.Accept = ", text/xml";
            httpWebRequest.Headers.Set("Accept-Language", "en");
            httpWebRequest.Headers.Set("DocuShare-Version", "5.0");
            httpWebRequest.UserAgent = "DsAxess/4.0";

            httpWebRequest.Headers.Set("Cookie", authentication);

            byte[] buffer = new byte[4096];
            int bytesRead = 0;

            byte[] boundryByteArray = Encoding.ASCII.GetBytes("\r\n--" + boundry + "\r\n");
            byte[] headerbytes = Encoding.UTF8.GetBytes(header);

            httpWebRequest.ContentLength = document.Length + headerbytes.Length + (boundryByteArray.Length * 2) + 4;
            Stream stream = httpWebRequest.GetRequestStream();
            stream.Write(boundryByteArray, 0, boundryByteArray.Length);
            stream.Write(headerbytes, 0, headerbytes.Length);

            MemoryStream fileStream = new MemoryStream(document);
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                stream.Write(buffer, 0, bytesRead);
                stream.Flush();
            }

            var secondaryBoundry = System.Text.Encoding.ASCII.GetBytes("\r\n\r\n--" + boundry + "--\r\n");
            stream.Write(secondaryBoundry, 0, secondaryBoundry.Length);
            stream.Close();
            fileStream.Close();
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            if (httpWebResponse.StatusCode == HttpStatusCode.Created || httpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                xmlDocument.Load(httpWebResponse.GetResponseStream());
                return xmlDocument;
            }
            else
            {
                throw new Exception("File Failed to Uplaod");
            }
        }
            
        public static string Post(string url, List<String> Paramaters, string authorizationToken)
        {
            string DocuShareUrl = ConfigurationManager.AppSettings["DocuShareUrl"] + url;

            StreamWriter streamWriter = null;
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("<?xml version=\"1.0\" ?>");
            if(Paramaters != null)
            {
                foreach (var item in Paramaters)
                {
                    stringBuilder.Append(item);
                }
            }

            Uri Connection = new Uri(DocuShareUrl);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Connection);
            //The HTTP header information remains fairly consistant            
            httpWebRequest.ContentLength = stringBuilder.Length;
            httpWebRequest.Method = "POST";
            if (authorizationToken != null)
            {
                httpWebRequest.Headers.Set("Cookie", authorizationToken);
            }
            httpWebRequest.ContentType = "application/xml";//contentType;
            httpWebRequest.Accept = "*/*, text/xml";
            httpWebRequest.Headers.Set("Accept-Language", "en");
            httpWebRequest.Headers.Set("DocuShare-Version", "5.0");
            httpWebRequest.UserAgent = "DsAxess/4.0";
            //Make the request
            streamWriter = new StreamWriter(httpWebRequest.GetRequestStream(), Encoding.ASCII);
            streamWriter.Write(stringBuilder.ToString());
            streamWriter.Close();
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //Find the authentication token in the response header
            if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                return httpWebRequest.GetResponse().Headers.Get("Set-Cookie").ToString();
            else
                throw new Exception("Error Making HTTP Request");
        }
    /*public static string Post(string url, List<String> Paramaters, string authorizationToken)
    {
        string DocuShareUrl = ConfigurationManager.AppSettings["DocuShareUrl"] + url;

        StreamWriter streamWriter = null;
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.Append("<?xml version=\"1.0\" ?>");
        if (Paramaters != null)
        {
            foreach (var item in Paramaters)
            {
                stringBuilder.Append(item);
            }
        }

        Uri Connection = new Uri(DocuShareUrl);
        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Connection);
        //The HTTP header information remains fairly consistant            
        httpWebRequest.ContentLength = stringBuilder.Length;
        httpWebRequest.Method = "POST";
        if (authorizationToken != null)
        {
            httpWebRequest.Headers.Set("Cookie", authorizationToken);
        }
        httpWebRequest.ContentType = "application/xml";//contentType;
        httpWebRequest.Accept = "*, text/xml";
        httpWebRequest.Headers.Set("Accept-Language", "en");
        httpWebRequest.Headers.Set("DocuShare-Version", "5.0");
        httpWebRequest.UserAgent = "DsAxess/4.0";
        //Make the request
        streamWriter = new StreamWriter(httpWebRequest.GetRequestStream(), Encoding.ASCII);
        streamWriter.Write(stringBuilder.ToString());
        streamWriter.Close();
        HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        //Find the authentication token in the response header
        if (httpWebResponse.StatusCode == HttpStatusCode.OK)
            return httpWebRequest.GetResponse().Headers.Get("Set-Cookie").ToString();
        else
            throw new Exception("Error Making HTTP Request");
    }
    /*public static string Post(string url, List<String> Paramaters, string contentType)
        {
            return Post(url, Paramaters, contentType, null);
        }*/

        public static string ChangeLock(string url, string authorizationToken)
        {
            var result = "";
            string DocuShareUrl = ConfigurationManager.AppSettings["DocuShareUrl"] + url;                      

            StreamWriter streamWriter = null;
            StringBuilder stringBuilder = new StringBuilder();            
            stringBuilder.Append("<?xml version=\"1.0\" ?>");           
            Uri Connection = new Uri(DocuShareUrl);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Connection);
            //The HTTP header information remains fairly consistant
            
            httpWebRequest.Method = "POST";
            if (authorizationToken != null)
            {
               httpWebRequest.Headers.Set("Cookie", authorizationToken);
            }                        
            //Make the request
            streamWriter = new StreamWriter(httpWebRequest.GetRequestStream(), Encoding.ASCII);
            streamWriter.Write(stringBuilder.ToString());
            streamWriter.Close();
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //Find the authentication token in the response header

            if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                result = httpWebResponse.StatusCode.ToString(); //httpWebRequest.GetResponse().Headers.Get("Set-Cookie").ToString();
            else
            {
                result = httpWebResponse.StatusCode.ToString();
                throw new Exception("Error Making HTTP Request");
            }   
            return result;
        }
    }
