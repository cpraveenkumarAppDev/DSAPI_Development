using api.App_Code;
using api.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace api.Controllers
{
    public class ScanController : ApiController
    {
        [HttpGet]
        public Scan Get(int ScanId)
        {
            Util utils = new Util();
            Scan myScan = new Scan(ScanId);
            string SQL = "SELECT METADATA FROM ADWR_ADMIN.ADWR_SCAN WHERE PROCESSED IS NULL AND ID = " + myScan.Id.ToString();
            DataTable tbl = utils.GetORADataTable(SQL);
            if (tbl.Rows.Count > 0)
            {
                myScan.MetaData = tbl.Rows[0][0].ToString();
                myScan.Processed = false;
                //var data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(tbl.Rows[0][0].ToString());
            }
            return myScan;
        }

        [HttpGet]
        public Scan GetUniversalScanById(int id)
        {
            Util utils = new Util();
            Scan myScan = new Scan(id);
            string SQL = "SELECT * FROM ADWR_ADMIN.ADWR_SCAN WHERE ID = " + myScan.Id.ToString();
            DataTable tbl = utils.GetORADataTable(SQL);
            if (tbl.Rows.Count > 0)
            {
                myScan.MetaData = tbl.Rows[0][1].ToString();
                myScan.Processed = tbl.Rows[0][1] != null ? true: false;
            }
            return myScan;
        }
    }
}
