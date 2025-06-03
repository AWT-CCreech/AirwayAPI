using AirwayAPI.Models.EmailModels;

namespace AirwayAPI.Models.MassMailerModels;

public class MassMailerEmailInput : EmailInputBase
{
    public List<int> RecipientIds { get; set; } = [];
    public List<string> RecipientNames { get; set; } = [];
    public List<string> RecipientCompanies { get; set; } = [];
    public List<string> CCNames { get; set; } = [];
    public List<MassMailerPartItem> Items { get; set; } = [];
}
