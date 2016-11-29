using CodeRedCreations.Data;
using CodeRedCreations.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Cryptography;
using MimeKit.Text;
using System.Threading.Tasks;

namespace CodeRedCreations.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link https://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private readonly AppSettings _settings;
        private readonly CodeRedContext _context;
        public AuthMessageSender(IOptions<AppSettings> settings,
            CodeRedContext context)
        {
            _settings = settings.Value;
            _context = context;
        }

        public async Task SendEmailAsync(string toEmail, string fromEmail, string subject, string message)
        {
            var from = new MailboxAddress(fromEmail, $"{fromEmail.Replace(" ", "")}@CodeRedPerformance.com");

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(from);
            emailMessage.To.Add(new MailboxAddress(toEmail.Split('@')[0], toEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(TextFormat.Html) { Text = message };

            using (var client = new SmtpClient())
            {
                var username = _settings.SmtpUsername;
                var password = _settings.SmtpPassword;

                var localDomain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;

                client.LocalDomain = localDomain;
                await client.ConnectAsync(_settings.SmtpHost);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
