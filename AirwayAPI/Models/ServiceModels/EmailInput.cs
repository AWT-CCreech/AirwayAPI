namespace AirwayAPI.Models.ServiceModels
{
    public class EmailInput
    {
        public string? FromEmail { get; set; }       // Sender's email
        public string ToEmail { get; set; }         // Recipient's email
        public string Subject { get; set; }         // Email subject
        public string HtmlBody { get; set; }        // HTML content of the email
        public string UserName { get; set; }        // SMTP username (usually same as FromEmail)
        public string Password { get; set; }        // SMTP password
        public List<string> CCEmails { get; set; }  // List of CC emails
        public List<string> Attachments { get; set; } // List of attachment file paths
        public bool Urgent { get; set; }
    }
}
