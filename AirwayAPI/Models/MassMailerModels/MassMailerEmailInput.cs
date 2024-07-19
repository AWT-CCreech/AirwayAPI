using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirwayAPI.Data
{
    public class MassMailerEmailInput
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SenderUserName { get; set; }
        public int[] RecipientIds { get; set; }
        public string[] RecipientEmails { get; set; }
        public string[] RecipientNames { get; set; }
        public string[] RecipientCompanies { get; set; }
        public string[] AttachFiles { get; set; }
        public string Password { get; set; }
        public string[] CCEmails { get; set; }
        public string[] CCNames { get; set; }
        public MassMailerPartItem[] items { get; set; }
    }
}
