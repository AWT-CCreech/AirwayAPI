namespace AirwayAPI.Models.EmailModels
{
    public class EmailInputBase
    {
        public string? FromEmail { get; set; }       // Sender's email
        public string UserName { get; set; }         // SMTP username
        public string Password { get; set; }         // SMTP password
        public List<string> ToEmails { get; set; } = new List<string>();  // List of recipient emails
        public string Subject { get; set; }          // Email subject
        public string Body { get; set; }             // Email body
        public List<string> CCEmails { get; set; } = new List<string>();  // List of CC emails
        public List<string> Attachments { get; set; } = new List<string>(); // List of attachment file paths
        public bool Urgent { get; set; }
    }

}
