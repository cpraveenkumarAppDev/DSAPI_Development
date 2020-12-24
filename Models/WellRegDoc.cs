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
            //Well registry documents are denoted with a program number "55-" in their authority number.
            //This function should respect that and check if the provided registryId has the preceding 55-
            //otherwise client apps have to take on that work
            RegistryId = registryId;
        }

    }
}