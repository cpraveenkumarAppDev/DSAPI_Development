using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Data.Entity.Spatial;

[Table("DocushareDirectoryHandleView")]
    public class DocushareDirectoryHandleView : SqlRepository<DocushareDirectoryHandleView>
    {
        [Key]
        public Guid RowId { get; set; }
        public int HandleId { get; set; }
        public int DestinationIndex { get; set; }
        public decimal DestinationClass { get; set; }
        public string ObjectTitle { get; set; }
        public string PCCNumber { get; set; }
    }

