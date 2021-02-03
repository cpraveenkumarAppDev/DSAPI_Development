using api.App_Code;
using api.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/cws")]
    public class CWSController : ApiController
    {
        readonly string DocushareUrl = ConfigurationManager.AppSettings["dsApiUrl"];
        readonly Util utils = new Util();

        [HttpGet, Route("doc/{pcc}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult GetCWSDocuments(string pcc)
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
            string amaDocType = "CWSDoc";
            using (var db = new DocushareEntities())
            {
                List<DsFileInfo> dbItems = db.DSObject_table
                    .Where(x => x.CWSDoc_PCC == pcc && x.Object_isDeleted == 0)
                    .AsEnumerable()
                    .Select(x => new DsFileInfo
                    {
                        Handle = x.handle_index.ToString(),
                        DocType = amaDocType,
                        FileURL = DocushareUrl + amaDocType + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.CWSDoc_original_file_name),
                        ObjSummary = x.Object_summary,
                        FileIdentifier = x.CWSDoc_PCC
                    }).ToList();
                if (dbItems.Count() > 0)
                {
                    utils.WriteLog($"CWS fileNumber {pcc}: {dbItems.Count()} records found");
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

        [HttpGet, Route("doc/{pcc}/{year}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult GetCWSDocumentsByReportYear(string pcc, string year)
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
            string amaDocType = "CWSDoc";
            using (var db = new DocushareEntities())
            {
                List<DsFileInfo> dbItems = db.DSObject_table
                    .Where(x => x.CWSDoc_PCC == pcc && x.Object_isDeleted == 0 && x.CWSDoc_ARYear == year)
                    .AsEnumerable()
                    .Select(x => new DsFileInfo
                    {
                        Handle = x.handle_index.ToString(),
                        DocType = amaDocType,
                        FileURL = DocushareUrl + amaDocType + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.CWSDoc_original_file_name),
                        ObjSummary = x.Object_summary,
                        FileIdentifier = x.CWSDoc_PCC
                    }).ToList();
                if (dbItems.Count() > 0)
                {
                    utils.WriteLog($"CWS fileNumber {pcc}: {dbItems.Count()} records found");
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