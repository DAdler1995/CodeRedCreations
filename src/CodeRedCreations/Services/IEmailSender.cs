using MimeKit;
using System.Threading.Tasks;

namespace CodeRedCreations.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string fromEmail, string subject, string message);
    }
}
