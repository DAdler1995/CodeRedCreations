using System.Threading.Tasks;

namespace CodeRedCreations.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
