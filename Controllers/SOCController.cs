using api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;

namespace api.Controllers
{
    public class SOCController : ApiController
    {
        readonly string DocushareUrl = ConfigurationManager.AppSettings["dsApiUrl"];

        [HttpGet]
        public HttpResponseMessage GetSOCDocuments(string fileNumber)
        {
            List<Uri> docLinks = new List<Uri>();
            string tempLink;
            using (var db = new DocushareEntities())
            {
                var dbItems = db.DSObject_table.Where(x => x.SOCDoc_FileNumber == fileNumber).Select(x => new { x.SOCDoc_FileNumber, x.handle_index}).ToList();
                if(dbItems.Count() > 0)
                {
                    foreach (var item in dbItems)
                    {
                        tempLink = $"{DocushareUrl}SOCDoc-{item.handle_index}";
                        docLinks.Add(new Uri(tempLink));
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, docLinks);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, $"No records found for {fileNumber}");
                }

            }
        }
    }
}
