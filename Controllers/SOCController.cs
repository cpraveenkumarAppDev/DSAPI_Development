using api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using api.App_Code;
using System.Web.Http.Cors;
using Newtonsoft.Json;

namespace api.Controllers
{
    public class SOCController : ApiController
    {
        readonly string DocushareUrl = ConfigurationManager.AppSettings["dsApiUrl"];
        readonly Util utils = new Util();

        /// <summary>
        /// To get a link to a document in Docushare you must craft a url with the unique identifier "handle_index"
        /// Base url configured in web.config
        /// </summary>
        /// <param name="fileNumber"></param>
        /// <returns></returns>
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetSOCDocuments(string fileNumber)
        {
            List<Uri> docLinks = new List<Uri>();
            string tempLink;
            string socDocType = "SOCDoc";
            using (var db = new DocushareEntities())
            {
                var dbItems = db.DSObject_table.Where(x => x.SOCDoc_FileNumber == fileNumber && x.Object_isDeleted == 0).Select(x => new { x.SOCDoc_FileNumber, x.handle_index}).ToList();
                if(dbItems.Count() > 0)
                {
                    utils.WriteLog($"SOC fileNumber: {fileNumber}");
                    foreach (var item in dbItems)
                    {
                        tempLink = $"{DocushareUrl}{socDocType}-{item.handle_index}";
                        docLinks.Add(new Uri(tempLink));
                        utils.WriteLog($"\t{item.SOCDoc_FileNumber}: {item.handle_index}");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, docLinks);
                }
                else
                {
                    utils.WriteLog($"\tNo records found for {fileNumber}");
                    return Request.CreateResponse(HttpStatusCode.OK, $"No records found for {fileNumber}");
                }

            }
        }
    }
}
