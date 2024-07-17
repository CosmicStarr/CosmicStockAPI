

using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Data.Classes
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var EmailFromMySite = _configuration["ReturnPath:SenderEmail"];
            var newMessage = new MailMessage(EmailFromMySite, email, subject,htmlMessage);
            newMessage.IsBodyHtml = true;
            newMessage.Body = htmlMessage;
            using(var client = new SmtpClient(_configuration["STMP:Host"],int.Parse(_configuration["STMP:Port"]))
            {
                 Credentials = new NetworkCredential(_configuration["STMP:Username"],_configuration["STMP:Password"])
            })
            {
                 await client.SendMailAsync(newMessage);
            }
        }
    }
}