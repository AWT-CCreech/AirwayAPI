using AirwayAPI.Models.EmailModels;

namespace AirwayAPI.Models.MassMailerModels
{
    public class MassMailerEmailInput : EmailInputBase
    {
        public List<int> RecipientIds { get; set; } = new List<int>();
        public List<string> RecipientNames { get; set; } = new List<string>();
        public List<string> RecipientCompanies { get; set; } = new List<string>();
        public List<string> CCNames { get; set; } = new List<string>();
        public List<MassMailerPartItem> Items { get; set; } = new List<MassMailerPartItem>();
    }
}
