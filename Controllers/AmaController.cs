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
    [RoutePrefix("api/ama")]
    public class AmaController : ApiController
    {
        readonly string DocushareUrl = ConfigurationManager.AppSettings["dsApiUrl"];
        readonly Util utils = new Util();

        [HttpGet, Route("doc/{pcc}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetAMADocuments(string pcc)
        {
            if(pcc.Length == 12)
            {
                pcc = pcc.Substring(0, 2) + "-" + pcc.Substring(2, 6) + "." + pcc.Substring(8);
            }
            else
            {
                var a = 1;
            }
            List<Uri> docLinks = new List<Uri>();
            string amaDocType = "GWDoc";
            using (var db = new DocushareEntities())
            {
                List<DsFileInfo> dbItems = db.DSObject_table
                    .Where(x => x.GWDoc_PCC == pcc && x.Object_isDeleted == 0)
                    .AsEnumerable()
                    .Select(x => new DsFileInfo
                    {
                        Handle = x.handle_index.ToString(),
                        DocType = amaDocType,
                        FileURL = DocushareUrl + amaDocType + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.GWDoc_original_file_name),
                        ObjSummary = x.Object_summary,
                        FileIdentifier = x.GWDoc_PCC
                    }).ToList();
                if (dbItems.Count() > 0)
                {
                    utils.WriteLog($"AMA fileNumber {pcc}: {dbItems.Count()} records found");
                    foreach (var item in dbItems)
                    {
                        utils.WriteLog($"\tPC:{item.FileIdentifier}, handle_index:{item.Handle}, originalName:{item.FileURL}");
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
        public IHttpActionResult GetAMADocumentByReportYear(string pcc, string year)
        {
            if (pcc.Length == 12)
            {
                pcc = pcc.Substring(0, 2) + "-" + pcc.Substring(2, 6) + "." + pcc.Substring(8);
            }
            else
            {
                return BadRequest($"PCC: {pcc} format incorrect");
            }
            List<Uri> docLinks = new List<Uri>();
            string amaDocType = "GWDoc";
            using (var db = new DocushareEntities())
            {
                List<DsFileInfo> dbItems = db.DSObject_table
                    .Where(x => x.GWDoc_PCC == pcc && x.Object_isDeleted == 0 && x.GWDoc_Year == year)
                    .AsEnumerable()
                    .Select(x => new DsFileInfo
                    {
                        Handle = x.handle_index.ToString(),
                        DocType = amaDocType,
                        FileURL = DocushareUrl + amaDocType + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.GWDoc_original_file_name),
                        ObjSummary = x.Object_summary,
                        FileIdentifier = x.GWDoc_PCC
                    }).ToList();
                if (dbItems.Count() > 0)
                {
                    utils.WriteLog($"AMA fileNumber {pcc}: {dbItems.Count()} records found");
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
