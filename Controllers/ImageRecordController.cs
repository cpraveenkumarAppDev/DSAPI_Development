using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Net;
using System.Net.Http;
using api.App_Code;
using api.Models;
using System.Configuration;
using api.Models;
using static api.App_Code.Util;

namespace api.Controllers
{
    public class ImageRecordController : ApiController
    {
        readonly string DocushareUrl = ConfigurationManager.AppSettings["dsApiUrl"];
        Util utils = new Util();
        string Log;

        /*
          http://localhost:24813/api/ImageRecord/GetImageRecords
          {
            "RecordType":Well35Doc, // Note this is enumRechargeDoc, SWDoc,WellRegDoc,Well35Doc,CWSDoc,SOCDoc
            "Id":"abcdeefd",
            "Year":"2011",
            "Location":"phoenix"
           }
         */

        [HttpPost]
        [ActionName("GetImageRecords")]
        public HttpResponseMessage GetImageRecords([FromBody] ImageRecordRequest recordRequest)
        {

            List<DsFileInfo> dbItems = new List<DsFileInfo>();
            if (ValidateRequest(recordRequest))
            {
                ImageRecordType type = recordRequest.RecordType;
                //DocumentType docuType = recordRequest.DocumentType;

                // string location = recordRequest.Location;

                if (type.Equals(ImageRecordType.Well35Doc))
                {
                    dbItems = FindWells35Docs(recordRequest);
                }
                else if (type.Equals(ImageRecordType.WellRegDoc))
                {
                    dbItems = FindWellRegDocs(recordRequest);
                }
                else if (type.Equals(ImageRecordType.SWDoc))
                {
                    dbItems = FindSWDocs(recordRequest);
                }
                else if (type.Equals(ImageRecordType.RechargeDoc))
                {
                    dbItems = FindRechargeDoc(recordRequest);
                }
                else if (type.Equals(ImageRecordType.SOCDoc))
                {
                    dbItems = FindSOCDoc(recordRequest);
                }
                else if (type.Equals(ImageRecordType.CWSDoc))
                {
                    dbItems = FindCWSDoc(recordRequest);
                }
                else if (type.Equals(ImageRecordType.GWDoc))
                {
                    dbItems = FindGWDoc(recordRequest);
                }
                else if (type.Equals(ImageRecordType.AAWSDoc))
                {
                    dbItems = FindAAWSDoc(recordRequest);
                }


                if (dbItems.Count() > 0)
                {
                    string id = recordRequest.Id;
                    string fileno = recordRequest.FileNumber;
                    utils.WriteLog($"{type.ToString()} Identifier: {recordRequest.Id}");
                    foreach (var item in dbItems)
                    {
                        utils.WriteLog($"\tPC:{id}, FileNumber:{fileno}, originalName:{item.FileURL},DocType:{recordRequest.RecordType}");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, dbItems);
                }
                else
                {
                    utils.WriteLog($"\tNo records found for {recordRequest.Id}");
                    return Request.CreateResponse(HttpStatusCode.OK, $"No records found for {recordRequest.Id}");
                }

            }
            else
            {
                utils.WriteLog($"\tRequest is not valid");
                return Request.CreateResponse(HttpStatusCode.BadRequest, $"Not a valid request");
            }
        }


        private List<DsFileInfo> FindWells35Docs(ImageRecordRequest request)
        {

            List<DsFileInfo> dbitems = new List<DsFileInfo>();
            string ImageRecordType = "Well35Doc";
            using (var db = new DocushareEntities())
            {
                IQueryable<DSObject_table> query = db.DSObject_table.Where(x => x.Object_isDeleted == 0);
                if (request.Id != null)
                {
                    query = query.Where(x => x.Well35Doc_New == request.Id ||
                                        x.Well35Doc_Old.Replace("35-", "") == request.Id ||
                                        x.Well35Doc_Old == request.Id);
                }

                //TODO : check what is the default identifier, old or new Id if no request with Id.---> Request id s mandatory
                dbitems = query.AsEnumerable().Select(x => new DsFileInfo
                {
                    Handle = x.handle_index.ToString(),
                    FileIdentifier = x.Well35Doc_New == request.Id ? x.Well35Doc_New : x.Well35Doc_Old,
                    FileName = x.Well35Doc_original_file_name,
                    DocType = ImageRecordType,
                    ObjSummary = x.Object_summary,
                    FileURL = DocushareUrl + ImageRecordType + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.Well35Doc_original_file_name)
                }).ToList();
            }
            return dbitems;

        }

        private List<DsFileInfo> FindWellRegDocs(ImageRecordRequest request)
        {
            List<DsFileInfo> dbitems = new List<DsFileInfo>();
            string ImageRecordType = "WellRegDoc";
            if (request != null)
            {
                string id = request.Id;

                using (var db = new DocushareEntities())
                {
                    IQueryable<DSObject_table> query = db.DSObject_table.Where(x => x.Object_isDeleted == 0);
                    if (id != null)
                    {
                        //remove the first three char, if id starts with 55-
                        id = id.Substring(0, 3) == "55-" ? id.Remove(0, 3) : id;
                        query = query.Where(x => x.WellRegDoc_RegID == id);
                    }
                    if (request.Location != null)
                    {
                        query = query.Where(x => x.WellRegDoc_Location == request.Location);
                    }

                    dbitems = query.AsEnumerable().Select(x => new DsFileInfo
                    {
                        Handle = x.handle_index.ToString(),
                        FileIdentifier = x.WellRegDoc_RegID,
                        FileName = x.WellRegDoc_original_file_name,
                        DocType = ImageRecordType,
                        ObjSummary = x.Object_summary,
                        FileURL = DocushareUrl + ImageRecordType + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.WellRegDoc_original_file_name),
                        Location = x.WellRegDoc_Location
                    }).ToList();
                }

            }
            return dbitems;
        }


        private List<DsFileInfo> FindSWDocs(ImageRecordRequest request)
        {
            List<DsFileInfo> dbitems = new List<DsFileInfo>();
            if (request != null)
            {
                string pc = request.Id;

                using (var db = new DocushareEntities())
                {
                    IQueryable<DSObject_table> query = db.DSObject_table.Where(x => x.Object_isDeleted == 0);
                    if (pc != null)
                    {
                        query = query.Where(x => x.SWDoc_AppNumber == pc || x.SWDoc_AppNumber.Contains(pc));
                    }

                    //TODO : check what is the default identifier, old or new Id if no request with Id.
                    dbitems = query.AsEnumerable().Select(x => new DsFileInfo
                    {
                        Handle = x.handle_index.ToString(),
                        FileIdentifier = x.SWDoc_AppNumber,
                        FileName = x.SWDoc_original_file_name,
                        DocType = ImageRecordType.SWDoc.ToString(),
                        ObjSummary = x.Object_summary,
                        FileURL = DocushareUrl + ImageRecordType.SWDoc.ToString() + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.SWDoc_original_file_name)
                    }).ToList();
                }

            }
            return dbitems;

        }

        private List<DsFileInfo> FindRechargeDoc(ImageRecordRequest request)
        {
            List<DsFileInfo> dbitems = new List<DsFileInfo>();
            if (request != null)
            {
                string pcc = request.Id;
                string year = request.Year;
                string documentType = request.DocumentType;
                DocuType docuType = new Models.DocuType();
                using (var db = new DocushareEntities())
                {
                    IQueryable<DSObject_table> query = db.DSObject_table.Where(x => x.Object_isDeleted == 0);

                    if (pcc != null)
                    {
                        query = query.Where(x => x.RechargeDoc_PCC == pcc);
                    }
                    if (year != null)
                    {
                        query = query.Where(x => x.RechargeDoc_Year == year);
                    }
                    if (Enum.TryParse<DocuType>(documentType, true, out docuType))
                    {
                        query = query.Where(x => x.RechargeDoc_DocType == documentType);
                    }
                    //TODO : check what is the default identifier, old or new Id if no request with Id.
                    dbitems = query.AsEnumerable()
                                .Select(x => new DsFileInfo
                                {
                                    Handle = x.handle_index.ToString(),
                                    FileIdentifier = x.RechargeDoc_PCC,
                                    FileName = x.RechargeDoc_original_file_name,
                                    DocType = ImageRecordType.RechargeDoc.ToString(),
                                    ObjSummary = x.Object_summary,
                                    FileURL = DocushareUrl + ImageRecordType.RechargeDoc.ToString() + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.RechargeDoc_original_file_name)
                                }).ToList();
                }

            }
            return dbitems;

        }
        private List<DsFileInfo> FindSOCDoc(ImageRecordRequest request)
        {

            List<DsFileInfo> dbitems = new List<DsFileInfo>();
            string fileNumber = request.FileNumber;
            using (var db = new DocushareEntities())
            {
                IQueryable<DSObject_table> query = db.DSObject_table.Where(x => x.Object_isDeleted == 0);
                if (fileNumber != null || fileNumber != "")
                {
                    query = query.Where(x => x.SOCDoc_FileNumber == fileNumber);
                }
                dbitems = query.AsEnumerable()
                            .Select(x => new DsFileInfo
                            {
                                Handle = x.handle_index.ToString(),
                                FileIdentifier = x.SOCDoc_FileNumber,
                                FileName = x.SOCDoc_original_file_name,
                                DocType = ImageRecordType.SOCDoc.ToString(),
                                ObjSummary = x.Object_summary,
                                FileURL = DocushareUrl + ImageRecordType.SOCDoc.ToString() + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.SOCDoc_original_file_name)
                            }).ToList();
            }
            return dbitems;

        }

        private List<DsFileInfo> FindCWSDoc(ImageRecordRequest request)
        {
            List<DsFileInfo> dbitems = new List<DsFileInfo>();
            if (request != null)
            {
                string pcc = request.Id;
                string year = request.Year;
                string documentType = request.DocumentType;
                DocuType docuType = new DocuType();
                using (var db = new DocushareEntities())
                {
                    IQueryable<DSObject_table> query = db.DSObject_table.Where(x => x.Object_isDeleted == 0);

                    if (pcc != null)
                    {
                        query = query.Where(x => x.CWSDoc_PCC == pcc);
                    }
                    if (year != null)
                    {
                        query = query.Where(x => x.CWSDoc_ARYear == year);
                    }

                    if (Enum.TryParse<DocuType>(documentType, true, out docuType))
                    {
                        query = query.Where(x => x.CWSDoc_DocType == documentType);
                    }
                    dbitems = query.AsEnumerable()
                                .Select(x => new DsFileInfo
                                {
                                    Handle = x.handle_index.ToString(),
                                    FileIdentifier = x.CWSDoc_PCC,
                                    FileName = x.CWSDoc_original_file_name,
                                    DocType = ImageRecordType.CWSDoc.ToString(),
                                    ObjSummary = x.Object_summary,
                                    FileURL = DocushareUrl + ImageRecordType.CWSDoc.ToString() + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.CWSDoc_original_file_name)
                                }).ToList();
                }

            }
            return dbitems;

        }

        private List<DsFileInfo> FindGWDoc(ImageRecordRequest request)
        {
            List<DsFileInfo> dbitems = new List<DsFileInfo>();
            if (request != null)
            {
                string pcc = request.Id;
                string year = request.Year;

                using (var db = new DocushareEntities())
                {
                    IQueryable<DSObject_table> query = db.DSObject_table.Where(x => x.Object_isDeleted == 0);

                    if (pcc != null)
                    {
                        query = query.Where(x => x.GWDoc_PCC == pcc);
                    }
                    if (year != null)
                    {
                        query = query.Where(x => x.GWDoc_Year == year);
                    }
                    dbitems = query.AsEnumerable()
                                .Select(x => new DsFileInfo
                                {
                                    Handle = x.handle_index.ToString(),
                                    FileIdentifier = x.GWDoc_PCC,
                                    FileName = x.GWDoc_original_file_name,
                                    DocType = ImageRecordType.GWDoc.ToString(),
                                    ObjSummary = x.Object_summary,
                                    FileURL = DocushareUrl + ImageRecordType.GWDoc.ToString() + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.GWDoc_original_file_name)
                                }).ToList();
                }

            }
            return dbitems;

        }

        private List<DsFileInfo> FindAAWSDoc(ImageRecordRequest request)
        {
            List<DsFileInfo> dbitems = new List<DsFileInfo>();

            if (request != null)
            {
                string pcc = request.Id;
                string year = request.Year; // check where to use it
                string documentType = request.DocumentType;
                DocuType docuType = new DocuType();
                using (var db = new DocushareEntities())
                {
                    IQueryable<DSObject_table> query = db.DSObject_table.Where(x => x.Object_isDeleted == 0);

                    if (pcc != null)
                    {
                        query = query.Where(x => x.AAWSDoc_PCC == pcc);
                    }
                    if (year != null && (year.Length == 4))
                    {
                        query = query.Where(x => x.AAWSDoc_ARYear == year);
                    }
                    if (Enum.TryParse<DocuType>(documentType, true, out docuType))
                    {
                        query = query.Where(x => x.AAWSDoc_DocType == documentType);
                    }

                    //TODO : check what is the default identifier, old or new Id if no request with Id.
                    dbitems = query.AsEnumerable()
                                .Select(x => new DsFileInfo
                                {
                                    Handle = x.handle_index.ToString(),
                                    FileIdentifier = x.AAWSDoc_PCC,
                                    FileName = x.AAWSDoc_original_file_name,
                                    DocType = ImageRecordType.AAWSDoc.ToString(),
                                    ObjSummary = x.Object_summary,
                                    FileURL = DocushareUrl + ImageRecordType.AAWSDoc.ToString() + "-" + x.handle_index + "/" + Uri.EscapeUriString(x.AAWSDoc_original_file_name)
                                }).ToList();
                }

            }
            return dbitems;

        }
        private bool ValidateRequest(ImageRecordRequest request)
        {
            bool isValid = false;
            try
            {
                ImageRecordType type = request.RecordType;
                String id = request.Id;
                String fileNumber = request.FileNumber;
                //check if the recordtype is mandatory
                if (request != null && Enum.IsDefined(typeof(ImageRecordType), request.RecordType))
                {
                    if (type.Equals(ImageRecordType.WellRegDoc) && (id != null || (id != "")))
                    {
                        id = id.Substring(0, 3) == "55-" ? id.Remove(0, 3) : id;
                        request.Id = id;

                    }
                    else if ((type.Equals(ImageRecordType.RechargeDoc)) || (type.Equals(ImageRecordType.CWSDoc)) || (type.Equals(ImageRecordType.GWDoc)) || (type.Equals(ImageRecordType.AAWSDoc)))
                    {
                        if (id != null || (id != ""))
                        {
                            if (id.Length == 12)
                            {
                                id = id.Substring(0, 2) + "-" + id.Substring(2, 6) + "." + id.Substring(8);
                            }
                            request.Id = id;
                        }
                    }
                    else if (type.Equals(ImageRecordType.SOCDoc))
                    {
                        if (fileNumber != null || fileNumber != "")
                        {
                            if (fileNumber.Length > 0)
                            {
                                fileNumber = fileNumber.Substring(0, 2) + "-" + fileNumber.Substring(2, 5);
                            }
                            request.FileNumber = fileNumber;
                        }
                    }
                    isValid = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }

            return isValid;
        }
    }


}