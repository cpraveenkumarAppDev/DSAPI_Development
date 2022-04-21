using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api.Models
{
    public class ImageRecordRequest
    {
        public ImageRecordType RecordType { get; set; }

        public string Id { get; set; }

        public string Year { get; set; }

        public string Location { get; set; }

      //  public string ProgramCertificate { get; set; }

        public string ProgramConveyanceCertificate { get; set; }

        public DocuType DocumentType { get; set; }

        // public string FileName { get; set; }

        public string Name { get; set; }

        public string ActiveManagementArea { get; set; }

        public string Subbasin { get; set; }

        public string Owner { get; set; }

        //public string FacilityName { get; set; }
    }
}