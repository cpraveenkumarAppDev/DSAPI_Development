    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    
    public partial class SqlContext : DbContext
    {
        public SqlContext()
            : base("name=SqlContext")
        {
        }
        public virtual DbSet<DocushareDirectoryHandleView> DocushareDirectoryHandleView { get; set; }
        public virtual DbSet<DocuShareView> DocuShareView { get; set; }
        public virtual DbSet<DocuShare> DocuShares { get; set; }
       }