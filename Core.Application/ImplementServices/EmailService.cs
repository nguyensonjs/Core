using Core.Application.Handle.HandleEmail;
using Core.Application.InterfaceServices;
using Core.Configuration.Config;
using MailKit.Net.Smtp;
using MimeKit;

namespace Core.Application.ImplementServices
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;

        public EmailService(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public string SendVerificationEmail(string recipient, string verificationCode)
        {
            var message = new EmailMessage(
                to: new[] { recipient },
                subject: "Xác nhận tài khoản của bạn",
                content: CreateVerificationEmailContent(verificationCode)
            );
            return SendEmail(message);
        }

        public string SendEmail(EmailMessage message)
        {
            try
            {
                var email = CreateEmailMess(message);
                SendEmailViaSmtp(email);
                return "Email sent successfully.";
            }
            catch (Exception ex)
            {
                return $"Failed to send email: {ex.Message}";
            }
        }

        private string CreateVerificationEmailContent(string verificationCode)
        {
            return $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Xác nhận tài khoản</h2>
                        <p>Cảm ơn bạn đã đăng ký! Đây là mã xác nhận của bạn:</p>
                        <h3 style='color: #2c3e50;'>{verificationCode}</h3>
                        <p>Vui lòng sao chép mã này và nhập vào ứng dụng để hoàn tất xác nhận.</p>
                        <p><strong>Nhấn Ctrl + C (hoặc Command + C trên Mac) để sao chép mã.</strong></p>
                        <p>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email.</p>
                    </body>
                    </html>";

        }

        private MimeMessage CreateEmailMess(EmailMessage message)
        {
            var emailMess = new MimeMessage();
            AddSender(emailMess);
            AddRecipients(emailMess, message);
            SetEmailSubject(emailMess, message);
            SetEmailBody(emailMess, message);
            return emailMess;
        }

        private void AddSender(MimeMessage emailMess)
        {
            emailMess.From.Add(new MailboxAddress(_emailConfig.FromDisplayName, _emailConfig.From));
        }

        private void AddRecipients(MimeMessage emailMess, EmailMessage message)
        {
            emailMess.To.AddRange(message.To);
        }

        private void SetEmailSubject(MimeMessage emailMess, EmailMessage message)
        {
            emailMess.Subject = message.Subject;
        }

        private void SetEmailBody(MimeMessage emailMess, EmailMessage message)
        {
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message.Content
            };
            emailMess.Body = bodyBuilder.ToMessageBody();
        }

        private void SendEmailViaSmtp(MimeMessage email)
        {
            using (var client = CreateSmtpClient())
            {
                ConnectSmtpClient(client);
                AuthenticateSmtpClient(client);
                client.Send(email);
                DisconnectSmtpClient(client);
            }
        }

        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient();
        }

        private void ConnectSmtpClient(SmtpClient client)
        {
            client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, _emailConfig.EnableSsl);
        }

        private void AuthenticateSmtpClient(SmtpClient client)
        {
            if (!string.IsNullOrEmpty(_emailConfig.Username) && !string.IsNullOrEmpty(_emailConfig.Password))
            {
                client.Authenticate(_emailConfig.Username, _emailConfig.Password);
            }
        }

        private void DisconnectSmtpClient(SmtpClient client)
        {
            client.Disconnect(true);
        }
    }
}