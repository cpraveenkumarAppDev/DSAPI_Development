using api.App_Code;
using api.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Xml;
using static api.App_Code.Util;

namespace api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ProfileInfoController : ApiController
    {

        Util utils = new Util();

        List<ProfileInfo> profiles = InitiateProfile();

        private List<ProfileInfo> InitiateProfile()
        {
            Profiles.Add(new ProfileInfo("1", "Charanya", "phoenix 85085"));
            Profiles.Add(new ProfileInfo("2", "jess", "Phoenix 85027"));
        }


        //http://localhost:24813/api/ProfileInfo/GetProfileInfo/{id}
        [HttpPost]
        [ActionName("GetProfileInfo")]
        public HttpResponseMessage GetProfileInfo(string id)
        {
            foreach (ProfileInfo p : Profiles)
            {
                if (id.Equals(p.Id))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, p);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, $"No records found for {od}");
        }

        //
        /*
         *http://localhost:24813/api/ProfileInfo/UpdateProfile/{id}
         *
         *Post Body
         *  {
         *     "Id": "10",
         *     "Name": "test",
         *     "Address" : "phoenix 85085"
         *  }
         * 
         */
        [HttpPost]
        public HttpResponseMessage UpdateProfile([FromBody] ProfileInfo pInfo, String id)
        {
            utils.WriteLog("Post: name=" + pInfo.Name);
            string result = "";
            try
            {
                foreach (ProfileInfo p : Profiles)
                {
                    if (id.Equals(p.Id))
                    {
                        p.Name = pInfo.Name;
                        p.Address = pInfo.Address;
                        return Request.CreateResponse(HttpStatusCode.OK, "success");
                    }
                }
                return Request.CreateResponse(HttpStatusCode.NotFound, "Id not available");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, e.Message);
            }
        }

    }
       
}
