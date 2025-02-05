namespace AirwayAPI.Models.EmailModels
{
    public class EmailInputBase
    {
        public string? FromEmail { get; set; }       // Sender's email
        public string UserName { get; set; }         // SMTP username
        public string Password { get; set; }         // SMTP password
        public List<string> ToEmails { get; set; } = [];  // List of recipient emails
        public string Subject { get; set; }          // Email subject
        public string Body { get; set; }             // Email body
        public List<string> CCEmails { get; set; } = [];  // List of CC emails
        public List<string> Attachments { get; set; } = []; // List of attachment file paths
        public List<string> InlineImages { get; set; } = []; // List of inline image file paths
        public IDictionary<string, string>? Placeholders { get; set; } // List of placeholders tp be handled by EmailService
        public bool Urgent { get; set; }
    }

}
