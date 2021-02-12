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
    [RoutePrefix("api/recharge")]
    public class RechargeController : ApiController
    {
        readonly string DocushareUrl = ConfigurationManager.AppSettings["dsApiUrl"];
        readonly Util utils = new Util();
        public string rechargeDocType = ConfigurationManager.AppSettings["RechargeDocType"];

        /// <summary>
        /// Use a PCC to lookup recharge documents from DSObject table
        /// </summary>
        /// <param name="pcc"></param>
        /// <returns>list of recharge docs matching PCC 200OK, or no records 200OK</returns>
        [HttpGet]
        public HttpResponseMessage GetRechargeDocuments(string pcc)
        {
            using (var db = new DocushareEntities())
            {
                var dbItems = db.DSObject_table
                    .Where(x => x.RechargeDoc_PCC == pcc && x.Object_isDeleted == 0)
                    .AsEnumerable()
                    .Select(x => new DsFileInfo {
                        FileIdentifier = x.RechargeDoc_PCC,
                        Handle = x.handle_index.ToString(),
                        FileName = x.RechargeDoc_original_file_name,
                        DocType = rechargeDocType,
                        ObjSummary = x.Object_summary,
                        FileURL = DocushareUrl + rechargeDocType + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.RechargeDoc_original_file_name)
                    }).ToList();
                if (dbItems.Count() > 0)
                {
                    utils.WriteLog($"Recharge Document PCC: {pcc}");
                    foreach (var item in dbItems)
                    {
                        utils.WriteLog($"\tPCC:{item.FileIdentifier}, handle_index:{item.Handle}, originalName:{item.FileName}");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, dbItems);
                }
                else
                {
                    utils.WriteLog($"\tNo records found for {pcc}");
                    return Request.CreateResponse(HttpStatusCode.OK, $"No records found for {pcc}");
                }
            }
        }

        [HttpGet, Route("doc/{pcc}/{year}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult GetRechargeDocumentsByReportYear(string pcc, string year)
        {
            if (pcc.Length == 12)
            {
                pcc = pcc.Substring(0, 2) + "-" + pcc.Substring(2, 6) + "." + pcc.Substring(8);
            }
            else
            {
                var a = 1;
            }
            List<Uri> docLinks = new List<Uri>();
            using (var db = new DocushareEntities())
            {
                List<DsFileInfo> dbItems = db.DSObject_table
                    .Where(x => x.RechargeDoc_PCC == pcc && x.Object_isDeleted == 0 && x.RechargeDoc_Year == year)
                    .AsEnumerable()
                    .Select(x => new DsFileInfo
                    {
                        Handle = x.handle_index.ToString(),
                        DocType = rechargeDocType,
                        FileURL = DocushareUrl + rechargeDocType + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.RechargeDoc_original_file_name),
                        ObjSummary = x.Object_summary,
                        FileIdentifier = x.RechargeDoc_PCC
                    }).ToList();
                if (dbItems.Count() > 0)
                {
                    utils.WriteLog($"Recharge fileNumber {pcc}: {dbItems.Count()} records found");
                    foreach (var item in dbItems)
                    {
                        utils.WriteLog($"\tPC:{item.FileIdentifier}, handle_index:{item.Handle}, originalName:{item.FileURL}");
                    }
                    return Ok(dbItems);
                }
                else
                {
                    utils.WriteLog($"\tNo records found for {pcc}");
                    return Ok($"No records found for {pcc}");
                }

            }
        }
    }
}
