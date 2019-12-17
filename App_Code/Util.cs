using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace api.App_Code
{
    public class Util
    {
        public intranet.IReportServiceservice rptSvc = new intranet.IReportServiceservice();
        public enum dsErrorCodes
        {
            ecSuccess = 0,
            ecNoFilesFound = 1,
            ecTooManyFilesFound = 2,
            ecUnableToLock = 3,
            ecUnableToUnlock = 4

        }
        public struct dsFileInfo
        {
            public dsErrorCodes errorCode;
            public string handle;
            public string docType;
            public string fileName;
            public string fileURL;
            public string objSummary;
        }

        public struct dsLockResult
        {
            public dsErrorCodes errorCode;
            public string token;
        }

        //jsh 10/2012 errorCode here can be 0, 1, 2
        public struct dsFileInfoArray
        {
            public dsErrorCodes errorCode;
            public dsFileInfo[] dsFileInfoS;
        }

        public string connString(string dbConn)
        {
            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            System.Configuration.ConnectionStringSettings connString = rootWebConfig.ConnectionStrings.ConnectionStrings[dbConn];
            return connString.ConnectionString.ToString();
        } // end connString

        public string ByteArrayToFile(byte[] _ByteArray)
        {
            string tmp = "size:" + _ByteArray.Length.ToString() + " --- ";
            try
            {
                string _FileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";
                tmp = tmp + _FileName;
                // Open file for reading
                System.IO.FileStream _FileStream = new System.IO.FileStream(_FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                // Writes a block of bytes to this stream using data from a byte array.
                _FileStream.Write(_ByteArray, 0, _ByteArray.Length);
                // close file stream .
                _FileStream.Close();
                return _FileName;
            }
            catch //(Exception _Exception)
            {
                return "tmp: " + tmp;

                // Error
                //Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
            }
        }

        public bool MergeFiles(string file1, string file2, string newFile)
        {
            try
            {
                System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
                pProcess.StartInfo.FileName = "C:/inetpub/development/DSAPI/bin/MergePDFs.exe";
                pProcess.StartInfo.Arguments = file1 + " " + file2 + " " + newFile;
                pProcess.StartInfo.UseShellExecute = false;
                //Start the process
                pProcess.Start();
                //Wait for process to finish
                pProcess.WaitForExit();
                return true;
            }
            catch (Exception e)
            {
                WriteLog("------------MergeFiles error = " + e.Message + " file1 - " + file1 + " file2 - " + file2 + " newfile - " + newFile);
                return false;
            }
        }

        #region Docushare Code
        public string loginDocuShare()
        {
            string DocuShareURL = ConfigurationManager.AppSettings["dsURL"];
            string Username = ConfigurationManager.AppSettings["dsUsername"];
            string Password = ConfigurationManager.AppSettings["dsPassword"];
            string Domain = ConfigurationManager.AppSettings["dsDomain"];
            string AuthToken = string.Empty;
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            StreamWriter SW = null;
            try
            {
                //Append the login command to your server URL and create the request XML
                DocuShareURL += "dsweb/LOGIN";
                StringBuilder XML = new StringBuilder();
                XML.Append("<?xml version=\"1.0\" ?>");
                XML.Append("<authorization>");
                XML.Append("<username><![CDATA[" + Username + "]]></username>");
                XML.Append("<password><![CDATA[" + Password + "]]></password>");
                XML.Append("<domain><![CDATA[" + Domain + "]]></domain>");
                XML.Append("</authorization>");
                Uri Connection = new Uri(DocuShareURL);
                httpWebRequest = (HttpWebRequest)WebRequest.Create(Connection);
                //The HTTP header information remains fairly consistant
                httpWebRequest.ContentLength = XML.Length;
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/xml";
                httpWebRequest.Accept = "*/*, text/xml";
                httpWebRequest.Headers.Set("Accept-Language", "en");
                httpWebRequest.Headers.Set("DocuShare-Version", "5.0");
                httpWebRequest.UserAgent = "DsAxess/4.0";
                //Make the request
                SW = new StreamWriter(httpWebRequest.GetRequestStream(), Encoding.ASCII);
                SW.Write(XML.ToString());
                SW.Close();
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //Find the authentication token in the response header
                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                    AuthToken = httpWebRequest.GetResponse().Headers.Get("Set-Cookie").ToString();
            }
            catch //(Exception ex)
            {
                //Handle any exceptions here
            }
            finally
            {
                if (httpWebResponse != null)
                    httpWebResponse.Close();
                if (SW != null)
                    SW.Close();
            }
            return AuthToken;
        }

        public static string generateBoundary()
        {
            string sBoundary = Guid.NewGuid().ToString();
            sBoundary = sBoundary.Replace("-", string.Empty);
            sBoundary = sBoundary.Substring(0, 16);
            return sBoundary;
        }


        public dsLockResult lockDocument(dsFileInfo fileInfo)
        {
            dsLockResult Result = new dsLockResult();
            //lock the document that will be appended
            string DocuShareURL = ConfigurationManager.AppSettings["dsURL"];
            string strToken = loginDocuShare();

            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;

            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(DocuShareURL + "dsweb/LOCK/" + fileInfo.docType + "-" + fileInfo.handle);

                //The HTTP header information remains fairly consistant
                httpWebRequest.ContentType = "text/xml";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Set("Cookie", strToken);//passing in login information
                httpWebRequest.Accept = "*/*, text/xml";
                httpWebRequest.Headers.Set("Accept-Language", "en");
                httpWebRequest.Headers.Set("DocuShare-Version", "5.0");
                httpWebRequest.UserAgent = "DsAxess/4.0";

                //Make the request
                httpWebRequest.ContentLength = 0;
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                if (httpWebResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    //if the document is locked by other user this will be true
                    //add Email code here ******************** sending email from client
                }
                Result.errorCode = dsErrorCodes.ecSuccess;
                Result.token = strToken;
            }

            catch //(Exception ex)
            {
                //Handle any exceptions here
                Result.errorCode = dsErrorCodes.ecUnableToLock;
                Result.token = "";
            }
            finally
            {
                if (httpWebResponse != null)
                    httpWebResponse.Close();
            }
            return Result;
        }


        public dsLockResult unlockDocument(string strHandle, string strToken)
        {
            dsLockResult Result = new dsLockResult();
            Result.token = strToken;
            string DocuShareURL = ConfigurationManager.AppSettings["dsURL"];
            //string strHandle = "WellRegDoc-" + DSHandle;

            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;

            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(DocuShareURL + "dsweb/UNLOCK/" + strHandle);

                //The HTTP header information remains fairly consistant
                httpWebRequest.ContentType = "text/xml";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Set("Cookie", strToken);//passing in login information
                httpWebRequest.Accept = "*/*, text/xml";
                httpWebRequest.Headers.Set("Accept-Language", "en");
                httpWebRequest.Headers.Set("DocuShare-Version", "5.0");
                httpWebRequest.UserAgent = "DsAxess/4.0";

                //Make the request
                httpWebRequest.ContentLength = 0;
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                if (httpWebResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    //Successful Unlock
                    Result.errorCode = dsErrorCodes.ecSuccess;
                }
                else if (httpWebResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    //unsuccessful unlock
                    Result.errorCode = dsErrorCodes.ecUnableToUnlock;
                }
            }

            catch //(Exception ex)
            {
                //Handle any exceptions here
            }

            finally
            {
                if (httpWebResponse != null)
                    httpWebResponse.Close();
            }
            return Result;
        }


        public XmlDocument uploadNewDocument(byte[] _ByteArray, string Collection, string Title)
        {
            string DocuShareURL = ConfigurationManager.AppSettings["dsURL"];

            string FileName = ByteArrayToFile(_ByteArray);// "C:\\test.txt";
            string AuthToken = loginDocuShare();
            string Boundary = generateBoundary();
            XmlDocument XMLDoc = new XmlDocument();
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            Stream requestStream = null;
            try
            {
                //Create the request header with the upload command
                httpWebRequest = (HttpWebRequest)WebRequest.Create(DocuShareURL + "dsweb/APPLYUPLOAD/" + Collection);
                //Custom Object
                //httpWebRequest = (HttpWebRequest)WebRequest.Create(DocuShareURL 
                //+ "dsweb/APPLYUPLOAD/" + Collection + "/TestObject"); 
                httpWebRequest.ContentType = "multipart/form-data; boundary=" + Boundary;
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Set("Cookie", AuthToken);
                httpWebRequest.Accept = "*/*, text/xml";
                httpWebRequest.Headers.Set("Accept-Language", "en");
                httpWebRequest.Headers.Set("DocuShare-Version", "5.0");
                httpWebRequest.UserAgent = "DsAxess/4.0";
                //Make the request
                byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + Boundary + "\r\n");
                string headerTemplate = "Content-Disposition: form-data; name=\"parent\"\r\n\r\n";
                headerTemplate += (Collection + "\r\n");
                headerTemplate += ("--" + Boundary + "\r\n");
                headerTemplate += "Content-Disposition: form-data; name=\"title\"\r\n\r\n";
                headerTemplate += (Title + "\r\n");
                //Custom Property
                //headerTemplate += ("--" + Boundary + "\r\n");
                //headerTemplate += "Content-Disposition: form-data; name=\"TestProp\"\r\n\r\n";
                //headerTemplate += ("test" + "\r\n");
                headerTemplate += ("--" + Boundary + "\r\n");
                headerTemplate += "Content-Disposition: form-data; name=\"file1\"; filename=\"";
                headerTemplate += Title + ".pdf" + "\"\r\nContent-Type: application/octet-stream\r\n";
                //Path.GetFileName(FileName) + "\"\r\nContent-Type: application/octet-stream\r\n";
                headerTemplate += "Content-Transfer-Encoding: binary\r\n\r\n";
                string header = string.Format(headerTemplate, "file", Path.GetFileName(FileName));
                byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                httpWebRequest.ContentLength = new FileInfo(FileName).Length + headerbytes.Length + (boundarybytes.Length * 2) + 4;
                requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(boundarybytes, 0, boundarybytes.Length);
                requestStream.Write(headerbytes, 0, headerbytes.Length);
                FileStream fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                    requestStream.Flush();
                }
                boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n\r\n--" + Boundary + "--\r\n");
                requestStream.Write(boundarybytes, 0, boundarybytes.Length);
                requestStream.Close();
                fileStream.Close();
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                if (httpWebResponse.StatusCode == HttpStatusCode.Created)
                {
                    XMLDoc.Load(httpWebResponse.GetResponseStream());
                }
            }
            catch //(Exception ex)
            {
                //Handle any exceptions here
            }
            finally
            {
                if (httpWebResponse != null)
                    httpWebResponse.Close();
                if (requestStream != null)
                    requestStream.Close();
            }
            File.Delete(FileName);
            return XMLDoc;
        }


        public XmlDocument uploadExistingDocument(byte[] _ByteArray, string handle, string strDocType, string strToken, string Title)
        {
            //string strHandle = "WellRegDoc-" + DSHandle;
            //string strDocType = "WellRegDoc";
            //bool Result = true;
            string DocuShareURL = ConfigurationManager.AppSettings["dsURL"];
            string FileName = ByteArrayToFile(_ByteArray);
            string boundary = generateBoundary();

            XmlDocument XMLDoc = new XmlDocument();
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            Stream requestStream = null;
            WriteLog("------------Begin uploadExistingDocument for DocType = " + strDocType); //jsh 04/2015
            try
            {
                //Create the request header with the upload command for custom object
                httpWebRequest = (HttpWebRequest)WebRequest.Create(DocuShareURL + "dsweb/APPLYUPLOAD/" + strDocType + "-" + handle + "/" + strDocType);
                httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Set("Cookie", strToken);
                httpWebRequest.Accept = "*/*, text/xml";
                httpWebRequest.Headers.Set("Accept-Language", "en");
                httpWebRequest.Headers.Set("DocuShare-Version", "5.0");
                httpWebRequest.UserAgent = "DsAxess/4.0";

                //Make the request
                byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                string headerTemplate = "Content-Disposition: form-data; name=\"parent\"\r\n\r\n";
                //headerTemplate += (strCollection + "\r\n");
                headerTemplate += ("--" + boundary + "\r\n");
                //headerTemplate += "Content-Disposition: form-data; name=\"title\"\r\n\r\n";
                // headerTemplate += (strTitleDS + "\r\n");


                headerTemplate += ("--" + boundary + "\r\n");
                headerTemplate += "Content-Disposition: form-data; name=\"file1\"; filename=\"";
                //headerTemplate += Path.GetFileName(FileName) + "\"\r\nContent-Type: application/octet-stream\r\n";
                headerTemplate += Title + ".pdf" + "\"\r\nContent-Type: application/octet-stream\r\n";
                headerTemplate += "Content-Transfer-Encoding: binary\r\n\r\n";

                string header = string.Format(headerTemplate, "file", Path.GetFileName(FileName));
                byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                httpWebRequest.ContentLength = new FileInfo(FileName).Length + headerbytes.Length + (boundarybytes.Length * 2) + 4;

                requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(boundarybytes, 0, boundarybytes.Length);
                requestStream.Write(headerbytes, 0, headerbytes.Length);

                FileStream fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                    requestStream.Flush();
                }

                boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n\r\n--" + boundary + "--\r\n");
                requestStream.Write(boundarybytes, 0, boundarybytes.Length);
                requestStream.Close();
                fileStream.Close();

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                WriteLog("--------------Response Status: " + httpWebResponse.StatusCode.ToString());
                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    WriteLog("--------------XML Doc loading");
                    XMLDoc.Load(httpWebResponse.GetResponseStream());

                }
                WriteLog("complete calling uploadExistingDocument for DocType = " + strDocType); //jsh 04/2015
            }

            catch //(Exception ex)
            {
                //Handle any exceptions here
                WriteLog("Exceptioncalling uploadExistingDocument for DocType = " + strDocType);//jsh 04/2015
                //Result = false;
            }
            finally
            {
                if (httpWebResponse != null)
                    httpWebResponse.Close();

                if (requestStream != null)
                    requestStream.Close();

                //Delete the PDF from the file system
                File.Delete(FileName);
            }
            return XMLDoc;
        }

        #endregion

        #region Logging Code
        public void WriteLog(string content)
        {
            if (ConfigurationManager.AppSettings["WriteLog"].ToString() == "1")
            {
                FileStream fs;
                string folderPath = ConfigurationManager.AppSettings["LogPath"];
                if (!System.IO.Directory.Exists(folderPath))
                    System.IO.Directory.CreateDirectory(folderPath);
                fs = new FileStream(folderPath + "\\DSapiLog.txt", FileMode.OpenOrCreate, FileAccess.Write);

                // Write log
                StreamWriter m_streamWriter = new StreamWriter(fs);
                m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                m_streamWriter.WriteLine(DateTime.Now.ToShortDateString() + " " +
                                              DateTime.Now.ToLongTimeString() + " - " + content + "\n");
                m_streamWriter.Flush();
                m_streamWriter.Close();
            }
        }

        #endregion


        #region DBTools
        public bool execORAQueryCommand(string SQL)
        {
            try
            {
                OracleDataAdapter QueryAdapter = new OracleDataAdapter();
                OracleConnection conn = new OracleConnection(connString("databaseWrite"));
                conn.Open();
                OracleCommand Command = new OracleCommand(SQL, conn);
                int res = Command.ExecuteNonQuery();
                QueryAdapter.Dispose();
                conn.Close();
                conn.Dispose();
                return true;
            }
            catch //(Exception e)
            {
                return false;
            }
        }


        public DataSet GetORADataSet(string SQL)
        {
            OracleDataAdapter QueryAdapter = new OracleDataAdapter();
            OracleConnection conn = new OracleConnection(this.connString("databaseRead"));
            QueryAdapter.SelectCommand = new OracleCommand(SQL, conn);
            DataSet DataSetX = new DataSet("QueryX");
            QueryAdapter.Fill(DataSetX);
            QueryAdapter.Dispose();
            conn.Close();
            conn.Dispose();
            return DataSetX;
        } // end GetSQLDataSet

        public DataTable GetORADataTable(string SQL)
        {
            DataTable TableX = new DataTable("QueryX");
            try
            {
                OracleDataAdapter QueryAdapter = new OracleDataAdapter();
                OracleConnection conn = new OracleConnection(connString("databaseRead"));
                QueryAdapter.SelectCommand = new OracleCommand(SQL, conn);
                QueryAdapter.Fill(TableX);
                QueryAdapter.Dispose();
                conn.Close();
                conn.Dispose();
            }
            catch
            {

            }
            return TableX;

        } // end GetSQLDataTable

        public DataTable GetSQLDataTable(string SQL)
        {
            DataTable TableX = new DataTable("QueryX");
            try
            {
                SqlConnection conn = new SqlConnection(connString("docuShare"));
                SqlCommand cmd = new SqlCommand(SQL, conn);
                conn.Open();
                // create data adapter 
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(TableX);
                conn.Close();
                da.Dispose();
            }
            catch
            {

            }
            return TableX;

        } // end GetSQLDataTable
        #endregion


        #region APITools



        #endregion

    }
}