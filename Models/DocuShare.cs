using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    [Table("DSObject_Table")]
    public class DocuShare: SqlRepository<DocuShare>
    {
        [Column("handle_id")]
        public long DocushareId { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("handle_index")]
        public int? HandleId { get; set; }

        [Column("Object_title")]
        public string Title { get; set; }

        [Column("Document_original_file_name")]
        public string FileName { get; set; }

        [Column("Object_modified_date")]
        public DateTime ModifiedDate { get; set; }

        [Column("Object_isDeleted")]
        public decimal? IsDeleted { get; set; }

        [Column("WellRegDoc_RegID")]
        public string WellId { get; set; }

        [Column("WellRegDoc_original_file_name")]
        public string WellFileName { get; set; }

        [Column("Well35Doc_New")]
        public string Well35Id { get; set; }

        [Column("Well35Doc_Old")]
        public string Well35OldId { get; set; }

        [Column("Well35Doc_original_file_name")]
        public string Well35FileName { get; set; }

        [Column("RechargeDoc_PCC")]
        public string RechargeId { get; set; }

        [Column("RechargeDoc_original_file_name")]
        public string RechargeFileName { get; set; }

        [Column("Object_summary")]
        public string Description { get; set; }

        [Column("RechargeDoc_DocType")]
        public string RechargeType { get; set; }

        [Column("RechargeDoc_Year")]
        public string RechargeYear { get; set; }

        [Column("CWSDoc_PCC")]
        public string CommunityId { get; set; }

        [Column("CWSDoc_original_file_name")]
        public string CommunityFileName { get; set; }

        [Column("CWSDoc_ARYear")]
        public string CommunityYear { get; set; }

        [Column("CWSDoc_DocType")]
        public string CommunityType { get; set; }

        [Column("AAWSDoc_PCC")]
        public string AssuredId { get; set; }

        [Column("AAWSDoc_original_file_name")]
        public string AssuredFileName { get; set; }

        [Column("AAWSDoc_ARYear")]
        public string AssuredYear { get; set; }

        [Column("AAWSDoc_DocType")]
        public string AssuredType { get; set; }

        [Column("GWDoc_PCC")]
        public string GroundWaterId { get; set; }

        [Column("GWDoc_original_file_name")]
        public string GroundWaterFileName { get; set; }

        [Column("GWDoc_Year")]
        public string GroundWaterYear { get; set; }

        [Column("SWDoc_AppNumber")]
        public string SurfaceId { get; set; }

        [Column("SWDoc_original_file_name")]
        public string SurfaceFileName { get; set; }

        [Column("SWDoc_Misc")]
        public string SurfaceNotes { get; set; }

        [Column("GWDoc_DocType")]
        public string GroundWaterType { get; set; }

        [Column("SOCDoc_FileNumber")]
        public string StatementOfClaimFileNumber { get; set; }

        [Column("SOCDoc_original_file_name")]
        public string StatementOfClaimFileName { get; set; }

    }

