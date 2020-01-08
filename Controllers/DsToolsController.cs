using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using api.App_Code;

namespace api.Controllers
{
    public class DsToolsController : ApiController
    {
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
        public struct dsFileInfoArray
        {
            public dsErrorCodes errorCode;
            public dsFileInfo[] dsFileInfoS;
        }
        //find RGR Annual Report, it's possible to find more than 1 documents
        //this find all groundwater and recharge annual reports
        [HttpGet]
        public dsFileInfoArray findRgrArDoc(string PCC, int year)
        {
            Util util = new Util();
            dsFileInfoArray fileInfo = new dsFileInfoArray();
            //search for the desired PDF and obtain the document's DS handle and filename
            #region Write Log file "Form_mgr_Log1.txt"
            //System.IO.StreamWriter StreamWriter1 = null;
            //try
            //{
            //    StreamWriter1 = new System.IO.StreamWriter(this.Context.Server.MapPath("Form_mgr_Log1.txt"));
            //}
            //catch
            //{
            //}
            //if (StreamWriter1 != null)
            //{
            //    StreamWriter1.WriteLine("Begin Form_Manager.findRgrArDoc");
            //    StreamWriter1.WriteLine("PCC: " + PCC + "; YEAR: " + year);
            //    StreamWriter1.WriteLine("from Form_Manager.findRgrArDoc, ready to call docushare conn.open() ");
            //    StreamWriter1.WriteLine(DateTime.Now.ToString() + " -----------------------------");
            //    // more to write later on, StreamWriter1.Close();
            //}

            #endregion
            bool is_Recharge = false;
            bool is_CWS = false;  //jsh 02/2014
            bool is_DADE = false; //jsh 02/2014, 40, 41
            string query;
            if (PCC.Substring(0, 1) == "7")
                is_Recharge = true;
            else if (PCC.Substring(0, 1) == "9")   //jsh 02/2014
                is_CWS = true;
            else if (PCC.Substring(0, 1) == "4")   //jsh 02/2014
                is_DADE = true;

            //test whether there is alreay a imaged file with the same attributes
            if (is_Recharge)
                query = "SELECT rechargeDoc_PCC, RechargeDoc_original_file_name, handle_index, object_summary" +
                       " from dsobject_table" +
                       " where object_isDeleted = 0" +
                       " and rechargedoc_pcc = '" + PCC + "'" +
                       " and rechargeDoc_DocTYpe = 'AnnualReport'" +
                       " and rechargeDoc_Year = " + year.ToString() + " order by handle_index";
            else if (is_CWS)  //jsh 02/2014
                query = "SELECT CWSDoc_PCC, CWSDoc_Original_file_name, handle_index, Object_summary" +
                        " FROM DSObject_table" +
                        " WHERE Object_isDeleted = 0" +
                        " and CWSDoc_PCC =  '" + PCC + "'" +
                        " and CWSDoc_ARyear =" + year.ToString() +
                        " and CWSDoc_DocType = 'AnnualReport'" +
                        " order by handle_index";
            else if (is_DADE) //jsh 02/2014
                query = "SELECT AAWSDoc_PCC, AAWSDoc_Original_file_name, handle_index, Object_summary" +
                        " FROM DSObject_table" +
                        " WHERE Object_isDeleted = 0" +
                        " and AAWSDoc_PCC = '" + PCC + "'" +
                        " and AAWSDoc_ARyear = " + year.ToString() +
                        " and AAWSDoc_DocType = 'AnnualReport'" +
                        " order by handle_index";
            else  //the rest of RGR, like 5x, 6x, 8x etc.
                query = "SELECT GWDoc_PCC, GWDoc_original_file_name, handle_index, Object_summary" +
                        " FROM DSObject_table" +
                        " WHERE Object_isDeleted = 0 and GWDoc_PCC = '" + PCC + "'" + " and GWDoc_year = " + year.ToString() + " and GWDoc_DOcType = 'AR' order by handle_index";
            DataTable TableX = new DataTable();
            SqlConnection conn = new SqlConnection(util.connString("docuShare"));
            SqlCommand cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                // create data adapter 
                //StreamWriter1.WriteLine ("Conn Open() succeeds");
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(TableX);
                conn.Close();
                da.Dispose();
                //StreamWriter1.WriteLine("number of records found in docushare = " + TableX.Rows.Count.ToString());
                if (TableX.Rows.Count == 1)
                    fileInfo.errorCode = dsErrorCodes.ecSuccess;
                else if (TableX.Rows.Count > 1)
                    fileInfo.errorCode = dsErrorCodes.ecTooManyFilesFound;
                else
                    fileInfo.errorCode = dsErrorCodes.ecNoFilesFound;

                if (fileInfo.errorCode != dsErrorCodes.ecNoFilesFound)
                {
                    fileInfo.dsFileInfoS = new dsFileInfo[TableX.Rows.Count];
                    for (int i = 0; i < TableX.Rows.Count; i++)
                    {
                        fileInfo.dsFileInfoS[i].handle = TableX.Rows[i]["HANDLE_INDEX"].ToString();
                        if (is_Recharge)
                        {
                            fileInfo.dsFileInfoS[i].docType = "RechargeDoc";
                            fileInfo.dsFileInfoS[i].fileName = TableX.Rows[i]["RechargeDoc_original_file_name"].ToString();
                        }
                        else if (is_CWS)  //jsh 02/2014
                        {
                            fileInfo.dsFileInfoS[i].docType = "CWSDoc";
                            // CWSDoc_Original_file_name for some CWS images are null
                            object value = TableX.Rows[i]["CWSDoc_Original_file_name"];
                            if (value == DBNull.Value)
                                fileInfo.dsFileInfoS[i].fileName = "null";
                            else
                                fileInfo.dsFileInfoS[i].fileName = TableX.Rows[i]["CWSDoc_Original_file_name"].ToString();
                        }
                        else if (is_DADE)  //jsh 02/2014
                        {
                            fileInfo.dsFileInfoS[i].docType = "AAWSDoc";
                            object value = TableX.Rows[i]["AAWSDoc_Original_file_name"];
                            if (value == DBNull.Value)
                                fileInfo.dsFileInfoS[i].fileName = "null";
                            else
                                fileInfo.dsFileInfoS[i].fileName = TableX.Rows[i]["AAWSDoc_Original_file_name"].ToString();
                        }
                        else
                        {
                            fileInfo.dsFileInfoS[i].docType = "GWDoc";
                            fileInfo.dsFileInfoS[i].fileName = TableX.Rows[i]["GWDoc_original_file_name"].ToString();
                        }
                        //production docushare            fileInfo.dsFileInfoS[i].fileURL = "https://infoshare.azwater.gov/docushare/dsweb/Get/" + fileInfo.dsFileInfoS[i].docType + "-" + fileInfo.dsFileInfoS[i].handle + "/" + fileInfo.dsFileInfoS[i].fileName;
                        fileInfo.dsFileInfoS[i].fileURL = ConfigurationManager.AppSettings["dsApiUrl"] + fileInfo.dsFileInfoS[i].docType + "-" + fileInfo.dsFileInfoS[i].handle + "/" + fileInfo.dsFileInfoS[i].fileName;  //test docushare, 3/6/2019 Jorge helped me
                        fileInfo.dsFileInfoS[i].objSummary = TableX.Rows[i]["Object_summary"].ToString();
                    }
                }
            }
            catch (Exception e)
            {
                //StreamWriter1.WriteLine("Conn Open() exception : " + e.Message);

            }
            //StreamWriter1.WriteLine(DateTime.Now.ToString() + " -----------------------------");
            //StreamWriter1.Close();
            return fileInfo;
        }
    }
}
