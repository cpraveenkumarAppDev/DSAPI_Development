using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api.Models
{
    public class WellRegDoc
    {
        public string RegistryId { get; set; }
        public string Location { get; set; }
        public string docUrl { get; set; }

        public WellRegDoc(string registryId)
        {
            RegistryId = registryId;
        }

    }
}