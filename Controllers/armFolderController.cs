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
    public class armFolderController : ApiController
    {
        #region Private Functions
        Util utils = new Util();
        private List<armFolder> FindFolderList()
        {
            string query = "SELECT FOLDERID,NAME,NVL(PARENT_ID,-1) PARENT_ID,NVL(ORDERID,0) ORDERID FROM RGR.RBFOLDER";
            DataTable TableX = utils.GetORADataTable(query);
            List<armFolder> folderList = new List<armFolder>();
            armFolder tmp;
            if (TableX.Rows.Count > 0)
                foreach (DataRow row in TableX.Rows)
                {
                    tmp = new armFolder(Convert.ToInt32(row["FOLDERID"]), row["NAME"].ToString(),
                        Convert.ToInt32(row["PARENT_ID"]), Convert.ToInt32(row["ORDERID"]));
                    folderList.Add(tmp);
                }
            return folderList;

        }
        #endregion

        [HttpGet]
        [ActionName("List")] //api call http://localhost:2349/api/armFolder/List
        public List<armFolder> List()
        {
            return FindFolderList();
        }

    }
}
