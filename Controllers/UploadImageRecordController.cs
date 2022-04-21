using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web.Http;
using System.Net;
using api.Models;
using System.Configuration;
using System.IO;
using System.Text;
using static api.App_Code.Util;
using System.Web.Services;

namespace api.Controllers
{
    public class UploadImageRecordController : ApiController
    {
        public static string Login()
        {
            var paramaterList = new List<String>();
            paramaterList.Add("<authorization>");
            paramaterList.Add("<username><![CDATA[" + ConfigurationManager.AppSettings["DocuShareUser"] + "]]></username>");
            paramaterList.Add("<password><![CDATA[" + ConfigurationManager.AppSettings["DocuSharePassword"] + "]]></password>");
            paramaterList.Add("<domain><![CDATA[" + ConfigurationManager.AppSettings["DocuShare"] + "]]></domain>");
            paramaterList.Add("</authorization>");
            return HttpCaller.Post("dsweb/LOGIN", paramaterList, "application/xml");
        }

        /**
         * UPload Meta data for the 
         * 
         * 
         */
        public static void UploadImageMetaData(byte[] documentArray, string fileName, string PermitCertificateConveyanceNumber, string Collection, List<KeyValuePair<string, string>> keyValueList, string documentCategory)
        {
            var authorizationToken = Login();
            var paramaterList = new List<String>();
            string boundry = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 16);
            if (documentCategory == "" || documentCategory is null)
            {
                documentCategory = "CWSDoc";
            }
            //  byte[] boundryByteArray = Encoding.ASCII.GetBytes("\r\n--" + boundry + "\r\n");

            StringBuilder headerStringBuilder = new StringBuilder();
            headerStringBuilder.Append("Content-Disposition: form-data; name=\"parent\"\r\n\r\n");
            headerStringBuilder.Append(Collection + "\r\n");
            headerStringBuilder.Append("--" + boundry + "\r\n");
            headerStringBuilder.Append("Content-Disposition: form-data; name=\"title\"\r\n\r\n");
            headerStringBuilder.Append(PermitCertificateConveyanceNumber + "\r\n");
            headerStringBuilder.Append("--" + boundry + "\r\n");
            headerStringBuilder.Append("Content-Disposition: form-data; name=\"file1\"; filename=\"");
            headerStringBuilder.Append(fileName + "\"\r\nContent-Type: application/octet-stream\r\n");
            headerStringBuilder.Append("Content-Transfer-Encoding: binary\r\n\r\n");
            string header = string.Format(headerStringBuilder.ToString(), "file", fileName);

            //Uploads File to Docushare
            var xmlDocument = HttpCaller.Post("dsweb/APPLYUPLOAD/" + Collection + "/" + documentCategory,
                paramaterList,
                "multipart/form-data; boundary=",
                boundry,
                authorizationToken,
                fileName,
                documentArray,
                header);
            string handle = GetFileHandle(PermitCertificateConveyanceNumber, DateTime.Now.AddYears(-1).Year.ToString(),null,null);

            HttpCaller.MetaPost(xmlDocument, keyValueList, authorizationToken, handle);
        }

        public static string UplaodImageFiles(ImageRecordRequest recordRequest,string Collection,string AuthorizationToken)
        {
           var paramaterList = new List<String>();
            string boundry = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 16);
            StringBuilder headerStringBuilder = new StringBuilder();
            headerStringBuilder.Append("Content-Disposition: form-data; name=\"parent\"\r\n\r\n");
            //headerStringBuilder.Append(ConfigurationManager.AppSettings["CommunityWaterCollection"] + "\r\n");
            headerStringBuilder.Append(recordRequest.DocumentType.ToString() + "\r\n");
            headerStringBuilder.Append("--" + boundry + "\r\n");
            headerStringBuilder.Append("Content-Disposition: form-data; name=\"title\"\r\n\r\n");
            headerStringBuilder.Append(recordRequest.ProgramConveyanceCertificate + "\r\n");
            headerStringBuilder.Append("--" + boundry + "\r\n");
            headerStringBuilder.Append("Content-Disposition: form-data; name=\"file1\"; filename=\"");
            headerStringBuilder.Append(Path.GetFileName(recordRequest.Name) + "\"\r\nContent-Type: application/octet-stream\r\n");
            headerStringBuilder.Append("Content-Transfer-Encoding: binary\r\n\r\n");
            string header = string.Format(headerStringBuilder.ToString(), "file", Path.GetFileName(recordRequest.Name));

            //string link = ConfigurationManager.AppSettings["CommunityWaterCollection"] + "/" + collection;
            string link = Collection;
            string handle;
            /**
             *  awrreport
             */

            handle = GetFileHandle(recordRequest.ProgramConveyanceCertificate, DateTime.Now.AddYears(-1).Year.ToString(), recordRequest.DocumentType.ToString(), "AR");

            if (!string.IsNullOrEmpty(handle))
            {
                link = handle;
            }

            if (string.IsNullOrEmpty(handle) && recordRequest.ProgramConveyanceCertificate.StartsWith("91"))
            {
                link = ConfigurationManager.AppSettings["CommunityWaterCollection"] + "\\CWSDoc";//jrn 5/16/19 quickfix for not uploading
            }

            //Uploads File to Docushare //TODO why is parameterList not used for upload?
            var xmlDocument = HttpCaller.Post("dsweb/APPLYUPLOAD/" + link,
                paramaterList,
                "multipart/form-data; boundary=",
                boundry,
                AuthorizationToken,
                recordRequest.Name,
                File.ReadAllBytes(recordRequest.Name),
                header);

            //Adds Meta Data to File
            var keyValueList = new List<KeyValuePair<string, string>>();
            keyValueList.Add(new KeyValuePair<string, string>("summary", DateTime.Now.AddYears(-1).Year + " ANNUAL REPORT"));
            keyValueList.Add(new KeyValuePair<string, string>("PCC", recordRequest.ProgramConveyanceCertificate));


            if (Collection.Contains("\\"))
            {
                if (Collection.Split('\\')[1].ToUpper() == "GWDOC")//Ian 4/30/2020 GWDOC docs have their own metadata profile
                {
                    keyValueList.Add(new KeyValuePair<string, string>("DocType", "item id=AR"));
                    keyValueList.Add(new KeyValuePair<string, string>("Year", DateTime.Now.AddYears(-1).Year.ToString()));
                    keyValueList.Add(new KeyValuePair<string, string>("Owner", recordRequest.Owner));
                    keyValueList.Add(new KeyValuePair<string, string>("AMA", recordRequest.ActiveManagementArea));
                    keyValueList.Add(new KeyValuePair<string, string>("Subbasin", recordRequest.Subbasin));
                    keyValueList.Add(new KeyValuePair<string, string>("Type",recordRequest.DocumentType.ToString()));
                }
                else if (Collection.Split('\\')[1].ToUpper() == "RECHARGEDOC")//Ian 4/30/2020 RECHARGE docs have their own metadata profile
                {
                    keyValueList.Add(new KeyValuePair<string, string>("DocType", "item id=AnnualReport"));
                    keyValueList.Add(new KeyValuePair<string, string>("Year", DateTime.Now.AddYears(-1).Year.ToString()));
                    keyValueList.Add(new KeyValuePair<string, string>("Owner", recordRequest.Owner));
                }
                else if (Collection.Split('\\')[1].ToUpper() == "AAWSDOC")//Ian 4/30/2020 DADE docs have their own metadata profile
                {
                    keyValueList.Add(new KeyValuePair<string, string>("ARYear", DateTime.Now.AddYears(-1).Year.ToString()));
                    keyValueList.Add(new KeyValuePair<string, string>("Status", "item id=Issued"));
                    keyValueList.Add(new KeyValuePair<string, string>("DocType", "item id=AnnualReport"));
                    keyValueList.Add(new KeyValuePair<string, string>("Misc", DateTime.Now.AddYears(-1).Year.ToString() + " ANNUAL REPORT"));
                    keyValueList.Add(new KeyValuePair<string, string>("Type", recordRequest.DocumentType.ToString()));
                    keyValueList.Add(new KeyValuePair<string, string>("ProjectName", recordRequest.Name));
                }
                else
                {
                    keyValueList.Add(new KeyValuePair<string, string>("Title", recordRequest.ProgramConveyanceCertificate));
                    keyValueList.Add(new KeyValuePair<string, string>("ARYear", DateTime.Now.AddYears(-1).Year.ToString()));
                    keyValueList.Add(new KeyValuePair<string, string>("DocType", "item id=AnnualReport"));
                    keyValueList.Add(new KeyValuePair<string, string>("SystemName", recordRequest.Name));
                }
            }
            else
            {
                keyValueList.Add(new KeyValuePair<string, string>("Title", recordRequest.ProgramConveyanceCertificate));
                keyValueList.Add(new KeyValuePair<string, string>("ARYear", DateTime.Now.AddYears(-1).Year.ToString()));
                keyValueList.Add(new KeyValuePair<string, string>("DocType", "item id=AnnualReport"));
                keyValueList.Add(new KeyValuePair<string, string>("SystemName", recordRequest.Name));
            }




            HttpCaller.MetaPost(xmlDocument, keyValueList, AuthorizationToken, handle);
            if (Collection.Contains("\\"))
            {
                return GetFileLink(recordRequest.ProgramConveyanceCertificate, DateTime.Now.AddYears(-1).Year.ToString(), Collection.Split('\\')[1]);
            }
            else
            {
                return GetFileLink(recordRequest.ProgramConveyanceCertificate, DateTime.Now.AddYears(-1).Year.ToString(), Collection);
            }

        }

        public static string GetFileHandle(string ProgramCertificateConveyanceNumber, string Year, string DocumentType, string WaterType)
        {
            var docuShareList = new List<DocuShare>();
            switch (DocumentType)
            {
                case "CWSDoc":
                    docuShareList = DocuShare.GetList(x => x.CommunityId == ProgramCertificateConveyanceNumber && x.CommunityYear == Year && x.IsDeleted == 0 && x.CommunityType.Contains(WaterType));
                    if (docuShareList.Count() > 0)
                        return "CWSDoc-" + docuShareList.First().HandleId;
                    else
                        return null;
                case "GWDoc":
                    docuShareList = DocuShare.GetList(x => x.GroundWaterId == ProgramCertificateConveyanceNumber && x.GroundWaterYear == Year && x.IsDeleted == 0 && x.GroundWaterType.Contains(WaterType));
                    if (docuShareList.Count() > 0)
                        return "GWDoc-" + docuShareList.First().HandleId + "/GWDoc";
                    else
                        return null;
                case "RECHARGEDOC":
                    var reachargeList = DocushareDirectoryHandleView.GetList(p => p.PCCNumber == ProgramCertificateConveyanceNumber); //DocuShare.GetList(x => x.RechargeId == ProgramCertificateConveyanceNumber && x.RechargeYear == Year && x.IsDeleted == 0);
                    if (reachargeList.Count() > 0)
                        return "Collection-" + reachargeList.First().DestinationIndex.ToString();
                    else
                        return "Collection-17344";
                case "AAWSDoc":
                    if (WaterType == "AR")
                        WaterType = "AnnualReport";
                    docuShareList = DocuShare.GetList(x => x.AssuredId == ProgramCertificateConveyanceNumber && x.AssuredYear == Year && x.IsDeleted == 0 && x.AssuredType.Contains(WaterType));
                    if (docuShareList.Count() > 0)
                    {
                        var tempFileHandle = "AAWSDoc-" + docuShareList.Last().HandleId + "/AAWSDoc";
                        return tempFileHandle;
                    }

                    else
                        return null;
                default:
                    docuShareList = DocuShare.GetList(x => x.GroundWaterId == ProgramCertificateConveyanceNumber && x.GroundWaterYear == Year && x.IsDeleted == 0 && x.GroundWaterType.Contains(WaterType));
                    if (docuShareList.Count() > 0)
                        return "GWDoc-" + docuShareList.First().DocushareId.ToString().Substring(0, 6) + "/GWDoc";
                    else
                        return null;
            }
        }


        public static string GetFileLink(string ProgramCertificateConveyanceNumber, string Year, string WaterDocument)
        {
            DocuShare result = new DocuShare();
            if (WaterDocument == "" || WaterDocument is null)
            {
                WaterDocument = "CWSDoc";
            }
            switch (WaterDocument)
            {
                case "CWSDoc":
                    result = DocuShare.GetList(x => x.CommunityId == ProgramCertificateConveyanceNumber && x.IsDeleted == 0 && x.CommunityYear == Year).OrderByDescending(x => x.ModifiedDate).FirstOrDefault();
                    var resultList = DocuShare.GetList(x => x.CommunityId == ProgramCertificateConveyanceNumber && x.IsDeleted == 0 && x.CommunityYear == Year).OrderByDescending(x => x.ModifiedDate);

                    if (result != null)
                    {
                        return ConfigurationManager.AppSettings["DocuShareUrl"] + "dsweb/Get/" + WaterDocument + "-" + result.HandleId + "/" + result.CommunityFileName;
                    }
                    break;
                case "GWDoc":
                    if (ProgramCertificateConveyanceNumber.StartsWith("7"))
                    {
                        WaterDocument = "RECHARGEDOC";
                    }
                    else if (ProgramCertificateConveyanceNumber.StartsWith("4"))
                    {
                        WaterDocument = "AAWSDoc";
                    }
                    else
                    {
                    }
                    result = DocuShare.Get(x => x.Title == ProgramCertificateConveyanceNumber && x.IsDeleted == 0 && x.Description == Year + " ANNUAL REPORT");
                    var result2 = DocuShare.GetDesc(x => x.Title == ProgramCertificateConveyanceNumber && x.IsDeleted == 0 && x.Description == Year + " ANNUAL REPORT", y => y.HandleId);
                    if (result2 != null)
                    {
                        return ConfigurationManager.AppSettings["DocuShareUrl"] + "dsweb/Get/" + WaterDocument + "-" + result2.HandleId + "/" + result2.GroundWaterFileName;
                    }
                    break;
                case "RECHARGEDOC":
                    result = DocuShare.Get(x => x.Title == ProgramCertificateConveyanceNumber && x.IsDeleted == 0 && x.Description == Year + " ANNUAL REPORT");
                    if (result != null)
                    {
                        return ConfigurationManager.AppSettings["DocuShareUrl"] + "dsweb/Get/Document-" + result.HandleId + "/" + result.RechargeFileName;
                    }
                    break;
                case "AAWSDoc":
                    result = DocuShare.Get(x => x.Title == ProgramCertificateConveyanceNumber && x.IsDeleted == 0 && x.Description == Year + " ANNUAL REPORT");
                    result2 = DocuShare.GetDesc(x => x.Title == ProgramCertificateConveyanceNumber && x.IsDeleted == 0 && x.Description == Year + " ANNUAL REPORT", y => y.HandleId);
                    if (result2 != null)
                    {
                        return ConfigurationManager.AppSettings["DocuShareUrl"] + "dsweb/Get/AAWSDoc-" + result2.HandleId + "/" + result2.AssuredFileName;
                    }
                    break;
                case "SOC":
                    result = DocuShare.Get(x => x.StatementOfClaimFileNumber == ProgramCertificateConveyanceNumber && x.IsDeleted == 0);
                    if (result != null)
                    {
                        return ConfigurationManager.AppSettings["DocuShareUrl"] + "dsweb/Get/SOCDoc-" + result.HandleId + "/" + result.StatementOfClaimFileName;
                    }
                    break;
                case "SW":
                    result = DocuShare.Get(x => x.SurfaceId == ProgramCertificateConveyanceNumber && x.IsDeleted == 0);
                    if (result != null)
                    {
                        return ConfigurationManager.AppSettings["DocuShareUrl"] + "dsweb/Get/SWDoc-" + result.HandleId + "/" + result.StatementOfClaimFileName;
                    }
                    break;
                case "WELL":
                    result = DocuShare.Get(x => x.WellId == ProgramCertificateConveyanceNumber && x.IsDeleted == 0);
                    if (result != null)
                    {
                        return ConfigurationManager.AppSettings["DocuShareUrl"] + "dsweb/Get/WellRegDoc-" + result.HandleId + "/" + result.StatementOfClaimFileName;
                    }
                    break;
                default:
                    return String.Empty;
            }
            return String.Empty;
        }

        [WebMethod]
        public dsLockResult lockDocument(ImageRecordRequest request)
        {
            dsLockResult Result = new dsLockResult();
            //lock the document that will be appended
            string DocuShareURL = ConfigurationManager.AppSettings["dsURL"];
            string authToken = Login();

            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;

            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(DocuShareURL + "dsweb/LOCK/" + request.DocumentType.ToString() + "-" + fileInfo.handle);

                //The HTTP header information remains fairly consistant
                httpWebRequest.ContentType = "text/xml";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Set("Cookie", authToken);//passing in login information
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
                Result.token = authToken;
            }

            catch (Exception ex)
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

        [WebMethod]
        public dsLockResult unlockDocument(string strHandle, string strToken)
        {
            dsLockResult Result = new dsLockResult();
            Result.token = strToken;
            string DocuShareURL = ConfigurationManager.AppSettings["dsURL"];
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

            catch (Exception ex)    
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

        public static void lockFile(string documentType, string handleId)
        {
            HttpCaller.ChangeLock("dsweb/ChangeLock/" + documentType + "-" + handleId + "?LOCK=true", Login());
        }

        public static void UnlockFile(string documentType, string handleId)
        {
            HttpCaller.ChangeLock("dsweb/ChangeLock/" + documentType + "-" + handleId + "?UNLOCK=true", Login());
        }



    }
}