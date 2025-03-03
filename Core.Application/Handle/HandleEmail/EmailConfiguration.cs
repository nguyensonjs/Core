namespace Core.Application.Handle.HandleEmail
{
    public class EmailConfiguration
    {
        // Địa chỉ email của người gửi
        public string From { get; set; } = string.Empty;

        // Tên hiển thị của người gửi (ví dụ: "Administrator")
        public string FromDisplayName { get; set; } = "Administrator";

        // Địa chỉ máy chủ SMTP (ví dụ: smtp.gmail.com)
        public string SmtpServer { get; set; } = string.Empty;

        // Cổng SMTP (thường là 587 cho TLS hoặc 465 cho SSL)
        public int Port { get; set; }

        // Tên người dùng để đăng nhập vào SMTP server
        public string Username { get; set; } = string.Empty;

        // Mật khẩu hoặc App Password (nếu dùng Gmail với xác thực 2 yếu tố)
        public string Password { get; set; } = string.Empty;

        // Bật/tắt SSL hoặc TLS (thường là true cho các dịch vụ hiện đại)
        public bool EnableSsl { get; set; } = true;
    }
}