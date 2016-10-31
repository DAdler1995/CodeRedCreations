using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Threading.Tasks;

namespace CodeRedCreations.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private readonly IOptions<CodeRedPerformanceSettings> _settings;
        public AuthMessageSender(IOptions<CodeRedPerformanceSettings> codeRedPerformanceSettings)
        {
            _settings = codeRedPerformanceSettings;
        }

        public async Task SendEmailAsync(string toEmail, string fromEmail, string subject, string message)
        {
            var from = new MailboxAddress(fromEmail, $"{fromEmail.Replace(" ", "")}@CodeRedPerformance.com");

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(from);
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(TextFormat.Html) { Text = message };

            using (var client = new SmtpClient())
            {
                //var username = _settings.Value.SendGridUsername;
                //var password = _settings.Value.SendGridPassword;

                var username = "azure_b058dcf0c8d19e3a31d9bc5529eed72d@azure.com";
                var password = "albert26";

                var localDomain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;

                client.LocalDomain = localDomain;
                await client.ConnectAsync("smtp.sendgrid.net");
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
