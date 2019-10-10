using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api.Models
{
    public class armFolder
    {
        public int folderId { get; set; }
        public string name { get; set; }
        public int parentId { get; set; }
        public int orderId { get; set; }

        public armFolder(int _folder, string _name, int _parent, int _order)
        {
            folderId = _folder;
            name = _name;
            parentId = _parent;
            orderId = _order;

        }

    }
}