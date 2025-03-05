using Core.Application.Handle.HandleEmail;

namespace Core.Application.InterfaceServices
{
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email chung với thông điệp được cung cấp.
        /// </summary>
        /// <param name="message">Thông điệp email chứa thông tin người nhận, tiêu đề và nội dung.</param>
        /// <returns>Chuỗi thông báo kết quả gửi email (thành công hoặc lỗi).</returns>
        string SendEmail(EmailMessage message);

        /// <summary>
        /// Gửi email chứa mã xác nhận đến người nhận.
        /// </summary>
        /// <param name="recipient">Địa chỉ email của người nhận.</param>
        /// <param name="verificationCode">Mã xác nhận để gửi.</param>
        /// <returns>Chuỗi thông báo kết quả gửi email (thành công hoặc lỗi).</returns>
        string SendVerificationEmail(string recipient, string verificationCode);
    }
}