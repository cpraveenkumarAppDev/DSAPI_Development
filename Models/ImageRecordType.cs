using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api.Models
{
    public enum ImageRecordType
    {
        RechargeDoc,
        SWDoc,
        WellRegDoc,
        Well35Doc,
        CWSDoc,
        SOCDoc,
        GWDoc,
        AAWSDoc
    }

    public enum DocuType
    {
        AnnualReport,
        Correspondance,
        SystemPlan,
        SystemMap,
        AnnualRptSupplemental,
        Application,
        Permit,
        HydrologyReport,
        Map,
        HydrologyStudy,
        Other
    }
}