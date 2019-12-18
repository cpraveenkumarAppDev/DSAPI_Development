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
    public class RechargeController : ApiController
    {
        readonly string DocushareUrl = ConfigurationManager.AppSettings["dsApiUrl"];
        readonly Util utils = new Util();

        /// <summary>
        /// Use a PCC to lookup recharge documents from DSObject table
        /// </summary>
        /// <param name="pcc"></param>
        /// <returns>list of recharge docs matching PCC 200OK, or no records 200OK</returns>
        [HttpGet]
        public HttpResponseMessage GetRechargeDocuments(string pcc)
        {
            List<Uri> docLinks = new List<Uri>();
            string tempLink;
            string rechargeDocType = ConfigurationManager.AppSettings["RechargeDocType"];

            using (var db = new DocushareEntities())
            {
                var dbItems = db.DSObject_table.Where(x => x.RechargeDoc_PCC == pcc && x.Object_isDeleted == 0).Select(x => new { x.RechargeDoc_PCC, x.handle_index, x.RechargeDoc_original_file_name });
                if (dbItems.Count() > 0)
                {
                    utils.WriteLog($"Recharge Document PCC: {pcc}");
                    foreach (var item in dbItems)
                    {
                        //url encode the original file name
                        string temp = Uri.EscapeUriString(item.RechargeDoc_original_file_name);
                        tempLink = $"{DocushareUrl}{rechargeDocType}-{item.handle_index}/{temp}";
                        docLinks.Add(new Uri(tempLink));
                        utils.WriteLog($"\tPCC:{item.RechargeDoc_PCC}, handle_index:{item.handle_index}, originalName:{temp}");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, docLinks);
                }
                else
                {
                    utils.WriteLog($"\tNo records found for {pcc}");
                    return Request.CreateResponse(HttpStatusCode.OK, $"No records found for {pcc}");
                }
            }
        }
    }
}
