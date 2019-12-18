using api.App_Code;
using api.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SurfaceWaterController : ApiController
    {
        readonly string DocushareUrl = ConfigurationManager.AppSettings["dsApiUrl"];
        readonly Util utils = new Util();
        /// <summary>
        /// Get Surface water documents using a PC(program-certificate) identifier, e.g. 38-9000.
        /// </summary>
        /// <param name="pc"></param>
        /// <returns></returns>
        public HttpResponseMessage GetSWDocuments(string pc)
        {
            string SWDocType = ConfigurationManager.AppSettings["SWDocType"];

            using (var db = new DocushareEntities())
            {
                //ian - SW group has inconsistent SWDoc_AppNumber conventions, so check for direct comparison as well as "Contains"
                var dbItems = db.DSObject_table.Where(x => (x.SWDoc_AppNumber == pc || x.SWDoc_AppNumber.Contains(pc)) && x.Object_isDeleted == 0)
                    .AsEnumerable()
                    .Select(x => new DsFileInfo {
                        Handle = x.handle_index.ToString(),
                        FileIdentifier = x.SWDoc_AppNumber,
                        FileName = x.SWDoc_original_file_name,
                        DocType = SWDocType,
                        ObjSummary = x.Object_summary,
                        FileURL = DocushareUrl + SWDocType + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.SWDoc_original_file_name)
                    }).ToList();

                if (dbItems.Count() > 0)
                {
                    utils.WriteLog($"Surface Water Document AppNumber (Program-certificate): {pc}");
                    foreach (var item in dbItems)
                    {
                        utils.WriteLog($"\tPC:{item.FileIdentifier}, handle_index:{item.Handle}, originalName:{item.FileURL}");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, dbItems);
                }
                else
                {
                    utils.WriteLog($"\tNo records found for {pc}");
                    return Request.CreateResponse(HttpStatusCode.OK, $"No records found for {pc}");
                }
            }
        }
    }
}
