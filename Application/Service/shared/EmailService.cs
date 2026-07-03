using Application.Interface.Service.Shared;
using Domain.Model.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace Application.Service.shared
{
    public class EmailService(EmailOptions emailOptions) : IEmailService
    {
    

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromMail = emailOptions.FromEmail ?? throw new InvalidOperationException("Email:FromEmail is not configured");
            var fromPassword = emailOptions.FromPassword ?? throw new InvalidOperationException("Email:FromPassword is not configured");

            var theMsg = new MailMessage(fromMail, toEmail, subject, body);
            theMsg.IsBodyHtml = true;

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential(fromMail, fromPassword),
                EnableSsl = true,
            };

            await smtp.SendMailAsync(theMsg);
        }
    }
}


