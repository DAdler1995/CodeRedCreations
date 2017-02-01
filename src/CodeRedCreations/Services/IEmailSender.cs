﻿using MimeKit;
using System.Threading.Tasks;

namespace CodeRedCreations.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string fromName, string subject, string message);
    }
}
