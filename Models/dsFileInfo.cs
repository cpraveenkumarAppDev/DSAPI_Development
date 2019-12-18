using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api.Models
{
    public class DsFileInfo
    {
        public string Handle { get; set; }
        public string DocType { get; set; }
        public string FileName { get; set; }
        public string FileURL { get; set; }
        public string ObjSummary { get; set; }
        public string FileIdentifier { get; set; }

        public DsFileInfo(string handle, string docType, string fileUrl, string objSummary, string fileID)
        {
            this.Handle = handle;
            this.DocType = docType;
            this.FileURL = FileURL;
            this.ObjSummary = objSummary;
            this.FileIdentifier = fileID;
        }

        public DsFileInfo()
        {

        }

    }
}