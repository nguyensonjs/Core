using MimeKit;

namespace Core.Application.Handle.HandleEmail
{
    public class EmailMessage
    {
        public EmailMessage()
        {
            To = new List<MailboxAddress>();
            Subject = string.Empty;
            Content = string.Empty;
        }

        public EmailMessage(IEnumerable<string> to,
                            string subject,
                            string content,
                            string displayName = "Administrator")
        {
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(x => new MailboxAddress(displayName, x)));
            Subject = subject ?? string.Empty;
            Content = content ?? string.Empty;
        }

        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}