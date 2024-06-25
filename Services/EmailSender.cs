using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Web_TracNghiem_HTSV.Services
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
            var emailSettings = _configuration.GetSection("EmailSettings");

            using (var smtpClient = new SmtpClient(emailSettings.GetValue<string>("MailServer")))
            {
                smtpClient.Port = emailSettings.GetValue<int>("MailPort");
                smtpClient.EnableSsl = emailSettings.GetValue<bool>("EnableSSL");
                smtpClient.Credentials = new NetworkCredential
                {
                    UserName = emailSettings.GetValue<string>("Sender"),
                    Password = emailSettings.GetValue<string>("Password")
                };

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(emailSettings.GetValue<string>("Sender"));
                    message.To.Add(new MailAddress(email));
                    message.Subject = subject;
                    message.Body = htmlMessage;
                    message.IsBodyHtml = true;

                    await smtpClient.SendMailAsync(message);
                }
            }
        }
    }
}
