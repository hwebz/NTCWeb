using System.Threading.Tasks;

namespace Niteco.Common.Mail
{
    public interface IEmailService
    {
        void SendEmail(string recipients, string fromField, string subject, string body, bool isAsync);
        string Encrypt(byte[] data);

        Task SendSendgridEmailAsync(string recipientEmail, string fromEmail, string fromName, string subject, string body, string apiKey);
    }
}
