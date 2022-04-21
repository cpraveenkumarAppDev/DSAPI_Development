using System;
using System.Web;

namespace api.Models
{
    public class ProfileInfo
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public String Address { get; set; }
        

        public ProfileInfo(String id, String name, String address)
        {
            this.Id = id;
            this.Name = name;
            this.Address = address;

        }

        public ProfileInfo()
        {

        }

    }
}