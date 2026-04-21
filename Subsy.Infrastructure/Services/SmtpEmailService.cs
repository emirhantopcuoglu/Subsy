using Microsoft.Extensions.Configuration;
using Subsy.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Subsy.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            var smtpHost = _config["Email:SmtpHost"] ?? "localhost";
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var fromAddress = _config["Email:From"] ?? "noreply@subsy.app";
            var username = _config["Email:Username"] ?? "";
            var password = _config["Email:Password"] ?? "";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var message = new MailMessage(fromAddress, to, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message, ct);
        }
    }
}
