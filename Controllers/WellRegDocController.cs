using api.App_Code;
using api.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Xml;
using static api.App_Code.Util;

namespace api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class WellRegDocController : ApiController
    {
        readonly string DocushareUrl = ConfigurationManager.AppSettings["dsApiUrl"];
        Util utils = new Util();

        //http://localhost:24813/api/WellRegDoc/Get?regId=1
        [HttpGet]
        [ActionName("Get")]
        public WellRegDoc Get(string regId)
        {
            WellRegDoc docInfo = new WellRegDoc(regId);
            dsFileInfo info = FindWellRegDoc(docInfo);
            return docInfo;
        }

        [HttpGet]
        [ActionName("ListByLocation")] //api call http://localhost:52183/api/dstools/ListByLocation?location=blablabla
        public List<WellRegDoc> Location(string location)
        {
            return findWellsByLocation(location);
        }

        [HttpGet]
        [ActionName("AllDocs")]
        public HttpResponseMessage AllDocs(string registryNum)
        {
            string wellRegDocType = "WellRegDoc";
            using (var db = new DocushareEntities())
            {
                List<DsFileInfo> dbItems = db.DSObject_table
                    .Where(x => x.WellRegDoc_RegID == registryNum && x.Object_isDeleted == 0)
                    .AsEnumerable()
                    .Select(x => new DsFileInfo
                    {
                        Handle = x.handle_index.ToString(),
                        DocType = wellRegDocType,
                        FileURL = DocushareUrl + wellRegDocType + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.WellRegDoc_original_file_name),
                        ObjSummary = x.Object_summary,
                        FileIdentifier = x.WellRegDoc_RegID
                    }).ToList();
                if (dbItems.Count() > 0)
                {
                    utils.WriteLog($"WellRegDoc Registry ID {registryNum}: {dbItems.Count()} records found");
                    foreach (var item in dbItems)
                    {
                        utils.WriteLog($"\tPC:{item.FileIdentifier}, handle_index:{item.Handle}, originalName:{item.FileURL}");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, dbItems);
                }
                else
                {
                    utils.WriteLog($"\tNo records found for {registryNum}");
                    return Request.CreateResponse(HttpStatusCode.OK, $"No records found for {registryNum}");
                }
            }
        }


        [HttpPost]
        public string Post([FromBody] Scan myData)
        {
            utils.WriteLog("Post: id=" + myData.Id + " Metadata: " + myData.MetaData);
            string result = "";
            try
            {
                WellRegDoc docInfo = new WellRegDoc(getMetadata(myData, "WellId"));
                dsFileInfo info = FindWellRegDoc(docInfo);
                if (info.errorCode == dsErrorCodes.ecSuccess)
                {
                    //update file
                    utils.WriteLog("Update file: " + myData.Id);
                    result = uploadExistingWellRegDoc(info, myData);
                }
                if (info.errorCode == dsErrorCodes.ecNoFilesFound)
                {
                    result = uploadNewWellRegDoc(myData);
                    utils.WriteLog("New file: " + myData.Id);
                }
                if (info.errorCode == dsErrorCodes.ecTooManyFilesFound)
                    result = "More than one file was found in docushare.";
            }
            catch (Exception e)
            {
                result = e.Message;
            }
            return result;

        }
        #region wells35 Lookups
        /// <summary>
        /// Gets all docs where well35Doc_new or well35Doc_old equal RegId and are not deleted
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("getwell35docs")]
        public HttpResponseMessage GetWell35Docs(string registry)
        {
            string well35Doc = "Well35Doc";
            using (var db = new DocushareEntities())
            {
                //handle_class for Well35Doc is 22
                var dbItems = db.DSObject_table.Where(x => (x.Well35Doc_New == registry || x.Well35Doc_Old.Replace("35-", "") == registry || x.Well35Doc_Old == registry) && x.Object_isDeleted == 0)
                    .AsEnumerable()
                    .Select(x => new DsFileInfo
                    {
                        Handle = x.handle_index.ToString(),
                        FileIdentifier = x.Well35Doc_New == registry ? x.Well35Doc_New: x.Well35Doc_Old,
                        FileName = x.Well35Doc_original_file_name,
                        DocType = well35Doc,
                        ObjSummary = x.Object_summary,
                        FileURL = DocushareUrl + well35Doc + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.Well35Doc_original_file_name)
                    }).ToList();

                if (dbItems.Count() > 0)
                {
                    utils.WriteLog($"Well35 Document AppNumber (Program-certificate): {registry}");
                    foreach (var item in dbItems)
                    {
                        utils.WriteLog($"\tPC:{item.FileIdentifier}, handle_index:{item.Handle}, originalName:{item.FileURL}");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, dbItems);
                }
                else
                {
                    utils.WriteLog($"\tNo records found for {registry}");
                    return Request.CreateResponse(HttpStatusCode.OK, $"No records found for {registry}");
                }
            }
        }
        /// <summary>
        /// Legacy Lookup
        /// </summary>
        /// <param name="regid"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("findwell35regdoc")]
        public HttpResponseMessage FindWell35RegDoc(string regid)
        {
            dsFileInfo fileInfo = new dsFileInfo();
            //string query = "SELECT [Well35Doc_New], [Well35Doc_Old], [Well35Doc_original_file_name], [handle_index] FROM [DSObject_table] WHERE ([Well35Doc_New] = '" + RegId + "' or [Well35Doc_Old] like '%" + RegId + "') and [Object_isDeleted] = 0";
            string query = $"SELECT [Well35Doc_New], [Well35Doc_Old], [Well35Doc_original_file_name], [handle_index] FROM [DSObject_table] " +
                $"WHERE ([Well35Doc_New] = '{regid}' or replace([Well35Doc_Old],'35-','') = '{regid}' or Well35Doc_Old = '{regid}') and [Object_isDeleted] = 0";

            DataTable TableX = utils.GetSQLDataTable(query);

            if (TableX.Rows.Count == 1)
            {
                fileInfo.errorCode = dsErrorCodes.ecSuccess;
                fileInfo.handle = TableX.Rows[0]["HANDLE_INDEX"].ToString();
                fileInfo.docType = "Well35Doc";
                fileInfo.fileName = TableX.Rows[0]["Well35Doc_original_file_name"].ToString();
                fileInfo.fileURL = "http://infoshare.azwater.gov/docushare/dsweb/Get/" + fileInfo.docType + "-" + fileInfo.handle + "/" + fileInfo.fileName;
            }
            else
            {
                if (TableX.Rows.Count > 1)
                    fileInfo.errorCode = dsErrorCodes.ecTooManyFilesFound;
                else
                    fileInfo.errorCode = dsErrorCodes.ecNoFilesFound;
                fileInfo.handle = "";
                fileInfo.fileURL = "";
            }
            return Request.CreateResponse<dsFileInfo>(HttpStatusCode.OK, fileInfo);
        }
        #endregion

        #region private methods
        private dsFileInfo FindWellRegDoc(WellRegDoc myDoc)
        {
            dsFileInfo fileInfo = new dsFileInfo();

            //search for the desired PDF and obtain the document's DS handle and filename
            string strHandleID = "";
            string strFileName = "";
            string registry = myDoc.RegistryId;
            if (registry.Substring(0, 3) == "55-")
                registry = registry.Remove(0, 3);

            string query = "SELECT [WellRegDoc_RegID], [WellRegDoc_original_file_name], [handle_index], " +
                           "       [WellRegDoc_Location] " +
                           "  FROM [DSObject_table] " +
                           " WHERE ([WellRegDoc_RegID] = '" + registry + "'" +
                           "   AND [Object_isDeleted] = 0)";
            DataTable TableX = utils.GetSQLDataTable(query);

            if (TableX.Rows.Count == 1)
            {
                fileInfo.errorCode = dsErrorCodes.ecSuccess;
                fileInfo.handle = TableX.Rows[0]["HANDLE_INDEX"].ToString();
                fileInfo.docType = "WellRegDoc";
                fileInfo.fileName = TableX.Rows[0]["WellRegDoc_original_file_name"].ToString();
                fileInfo.fileURL = "http://infoshare.azwater.gov/docushare/dsweb/Get/" + fileInfo.docType + "-" + fileInfo.handle + "/" + fileInfo.fileName;
                myDoc.Location = TableX.Rows[0]["WellRegDoc_Location"].ToString();
                myDoc.docUrl = fileInfo.fileURL;
            }
            else
            {
                if (TableX.Rows.Count > 1)
                    fileInfo.errorCode = dsErrorCodes.ecTooManyFilesFound;
                else
                    fileInfo.errorCode = dsErrorCodes.ecNoFilesFound;
                fileInfo.handle = "";
                fileInfo.fileURL = "";
            }
            return fileInfo;
        }

        private string getMetadata(Scan json, string dataKey)
        {
            string data = "";
            var arr = JArray.Parse(json.MetaData);
            foreach (JObject obj in arr.Children<JObject>())
                if (obj[dataKey] != null)
                {
                    data = (string)obj[dataKey];
                    break;
                }
            return data;
        }

        private List<WellRegDoc> findWellsByLocation(string location)
        {
            string query = "SELECT o.WellRegDoc_original_file_name, o.WellRegDoc_RegID, o.WellRegDoc_Location, o.handle_index " +
                           "FROM DSObject_table o " +
                           "WHERE o.WellRegDoc_Location ='" + location + "' " +
                           "and o.Object_isDeleted = 0 " +
                           "ORDER BY o.WellRegDoc_RegID";
            DataTable TableX = utils.GetSQLDataTable(query);
            WellRegDoc docInfo;
            List<WellRegDoc> wellList = new List<WellRegDoc>();
            if (TableX.Rows.Count > 0)
                foreach (DataRow row in TableX.Rows)
                {
                    docInfo = new WellRegDoc(row["WellRegDoc_RegId"].ToString());
                    docInfo.Location = row["WellRegDoc_Location"].ToString();
                    docInfo.docUrl = "http://infoshare.azwater.gov/docushare/dsweb/Get/WellRegDoc-" + row["Handle_Index"].ToString() + "/";
                    wellList.Add(docInfo);
                }
            return wellList;

        }

        private string updateMetada(string strDocHandle, Scan myData)
        {
            utils.WriteLog("-- ** -- updateMetada");
            try
            {
                string DocType = "/WellRegDoc";
                utils.WriteLog("-- ** -- DocHandle: " + strDocHandle);
                //------------------------------------------------------------------- update document properties
                HttpWebRequest httpWebRequest = null;
                HttpWebResponse httpWebResponse = null;
                Stream requestStream = null;
                StringBuilder XML = new StringBuilder();
                string DocuShareURL = ConfigurationManager.AppSettings["dsURL"];
                string AuthToken = utils.loginDocuShare();
                try
                {
                    //Create the request XML
                    XML.Append("<?xml version=\"1.0\" ?><propertyupdate>");
                    XML.Append("<set><prop>");
                    //---------------------------------------------------------------------------------------Custom Fields
                    // Add Summary
                    string registry = getMetadata(myData, "WellId");
                    if (registry.Substring(0, 3) == "55-")
                        registry = registry.Remove(0, 3);
                    //XML.Append("<RegID><![CDATA[" + getMetadata(myData, "WellId") + "]]></RegID>");
                    XML.Append("<RegID><![CDATA[" + registry + "]]></RegID>");
                    XML.Append("<Location><![CDATA[" + getMetadata(myData, "Location") + "]]></Location>");
                    XML.Append("<summary><![CDATA[" + getMetadata(myData, "Location") + "]]></summary>");
                    XML.Append("</prop></set>");
                    XML.Append("</propertyupdate>");
                    //Create the request header with the search command
                    httpWebRequest = (HttpWebRequest)WebRequest.Create(DocuShareURL + "dsweb/PROPPATCH/" + strDocHandle);
                    httpWebRequest.ContentType = "text/xml";
                    httpWebRequest.Method = "POST";
                    httpWebRequest.Headers.Set("Cookie", AuthToken);
                    httpWebRequest.Accept = "*/*, text/xml";
                    httpWebRequest.Headers.Set("Accept-Language", "en");
                    httpWebRequest.Headers.Set("DocuShare-Version", "5.0");
                    httpWebRequest.UserAgent = "DsAxess/4.0";
                    //Make the request
                    byte[] bytes = Encoding.UTF8.GetBytes(XML.ToString());
                    httpWebRequest.ContentLength = XML.Length;
                    requestStream = httpWebRequest.GetRequestStream();
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                    httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    if (httpWebResponse.StatusCode.ToString() == "207")
                    {
                        //XMLDoc.Load(httpWebResponse.GetResponseStream());
                    }
                    //string SQL = "SELECT METADATA FROM ADWR_ADMIN.ADWR_SCAN WHERE PROCESSED IS NULL AND ID = " + myScan.Id.ToString();
                    utils.execORAQueryCommand("UPDATE ADWR_ADMIN.ADWR_SCAN SET PROCESSED=SYSDATE WHERE ID=" + myData.Id.ToString());
                }
                catch (Exception ex)
                {
                    //Handle any exceptions here
                    return ex.Message;
                }

                finally
                {
                    if (httpWebResponse != null)
                        httpWebResponse.Close();
                    if (requestStream != null)
                        requestStream.Close();
                }

            }
            catch (Exception e)
            {
                return e.Message;
            }
            return strDocHandle;
        }
        private string uploadExistingWellRegDoc(dsFileInfo myInfo, Scan myData)
        {
            string result = "";
            try
            {
                dsLockResult LockResult = utils.lockDocument(myInfo);
                if (LockResult.errorCode == dsErrorCodes.ecSuccess)
                {
                    using (WebClient client = new WebClient())
                    {
                        client.Headers.Set("Cookie", LockResult.token);

                        // Prepare new file
                        string newPagesFileName = utils.ByteArrayToFile(myData.pdf);
                        // Create Banner Page
                        string bannerNotes = getMetadata(myData, "BannerNotes");
                        utils.WriteLog("--------------  Creating Banner : " + bannerNotes);
                        byte[] splitterFile =
                            utils.rptSvc.GetPDFStream("2643",
                                                      "Message='Update for Registry: " +
                                                      getMetadata(myData, "WellId") + Environment.NewLine + bannerNotes +
                                                      " ';ReceivedDate='" +
                                                      DateTime.Now.ToShortDateString() + "'");
                        string splitterFileName = utils.ByteArrayToFile(splitterFile);
                        //Combine banner with new pages
                        string combinedFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";
                        utils.MergeFiles(splitterFileName, newPagesFileName, combinedFileName);

                        // Load original file
                        byte[] originalFile = client.DownloadData(myInfo.fileURL);
                        string originalFileName = utils.ByteArrayToFile(originalFile);
                        // Prepare final file
                        string finalFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";
                        //Merge Final File
                        utils.MergeFiles(combinedFileName, originalFileName, finalFileName);
                        byte[] final = File.ReadAllBytes(finalFileName);
                        try
                        {
                            XmlDocument XMLDoc = utils.uploadExistingDocument(final, myInfo.handle, myInfo.docType, LockResult.token, getMetadata(myData, "WellId"));
                            result = updateMetada(myInfo.docType + "-" + myInfo.handle, myData);
                            //result = "0";
                        }
                        catch (Exception ex)
                        {
                            string s = ex.Message;
                            result = "9";  //uploadExisting document failed
                            throw;
                        }
                        finally
                        {
                            //unlock file 
                            LockResult = utils.unlockDocument(myInfo.docType + myInfo.handle, LockResult.token);

                            //clean up and return reuslt
                            File.Delete(newPagesFileName);
                            File.Delete(originalFileName);
                            File.Delete(combinedFileName);
                            File.Delete(splitterFileName);
                            File.Delete(finalFileName);
                        }
                    }
                    return result;  //03/25/2014 jsh use try block
                }
            }
            catch (Exception e)
            {
                utils.WriteLog("------------- uploadExistingWellRegDoc error: " + e.Message);
                result = e.Message;
            }
            return result;
        }
        private string uploadNewWellRegDoc(Scan myData)
        {
            utils.WriteLog("------------- uploadNewWellRegDoc");
            string strDocHandle = "";
            try
            {

                string DocType = "/WellRegDoc";//why the forward slash?
                //upload as new document
                utils.WriteLog("--------------  Loading OriginalFile");
                string originalFilename = utils.ByteArrayToFile(myData.pdf);
                string bannerNotes = getMetadata(myData, "BannerNotes");
                utils.WriteLog("--------------  Creating Banner: " + bannerNotes);

                //moved reportbuilder.getpdfstream call to client programs 11/27/19 ian

                File.Delete(originalFilename);

                var wellId = getMetadata(myData, "WellId");

                string wellRegCollection = ConfigurationManager.AppSettings["wellRegCollection"];
                //"Collection-166651\\/WellRegDoc"
                XmlDocument XMLDoc = utils.uploadNewDocument(myData.pdf, wellRegCollection + "\\" + DocType, wellId);

                //find document handle
                string strDSRef = "";
                XmlNode node = XMLDoc.SelectSingleNode("/document/handle");
                strDSRef = node.InnerXml.ToString();
                int foundS1 = (strDSRef.IndexOf("=")) + 2;
                int foundS2 = strDSRef.IndexOf(">");
                int foundS3 = (foundS2 - foundS1) - 1;
                strDocHandle = strDSRef.Substring(foundS1, foundS3);
                utils.WriteLog("------------- strDocHandle: " + strDocHandle);
                //Update Metadata
                strDocHandle = updateMetada(strDocHandle, myData);


            }
            catch (Exception e)
            {
                utils.WriteLog("------------- uploadNewWellRegDoc error: " + e.Message);
                return e.Message;
            }
            utils.WriteLog("------------- uploadNewWellRegDoc Done!");
            return strDocHandle;
        }

        #endregion
    }
}
