using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirwayAPI.Models
{
    public class LoginInfo
    {
        public string userid { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public bool isPasswordEncrypted { get; set; }
    }
}
