using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api.Models
{
    public class Scan
    {
        public int Id { get; set; }
        public string MetaData { get; set; }
        public bool Processed { get; set; }

        public byte[] pdf { get; set; }
        public Scan(int id)
        {
            Id = id;
        }
    }
}