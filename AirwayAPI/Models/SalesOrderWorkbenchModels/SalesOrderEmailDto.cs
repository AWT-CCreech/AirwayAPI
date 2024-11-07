namespace AirwayAPI.Models.SalesOrderWorkbenchModels
{
    public class SalesOrderEmailDto
    {
        public string FromEmail { get; set; }       // Sender's email
        public string ToEmail { get; set; }         // Recipient's email
        public string Subject { get; set; }         // Email subject
        public string RecipientName { get; set; }   // Recipient's name
        public string UserName { get; set; }        // SMTP username
        public string Password { get; set; }        // SMTP password
        public List<string> CCEmails { get; set; }  // List of CC emails
        public List<string> Attachments { get; set; } // List of attachment file paths

        // Additional sales order details
        public string SalesOrderNumber { get; set; }
        public string CompanyName { get; set; }
        public DateTime RequiredDate { get; set; }
        public DateTime ExpectedDelivery { get; set; }
        public string PartNumber { get; set; }
        public string Notes { get; set; }
    }
}
