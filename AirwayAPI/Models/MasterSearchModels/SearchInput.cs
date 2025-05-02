namespace AirwayAPI.Models.MasterSearchModels;

public class SearchInput
{
    public string Search { get; set; }
    public bool ID { get; set; }
    public bool SONo { get; set; }
    public bool PartNo { get; set; }
    public bool PartDesc { get; set; }
    public bool PONo { get; set; }
    public bool Mfg { get; set; }
    public bool Company { get; set; }
    public bool InvNo { get; set; }
    public string Uname { get; set; }
}
