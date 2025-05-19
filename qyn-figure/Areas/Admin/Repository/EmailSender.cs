using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace qyn_figure.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");

            var client = new SmtpClient(smtpSettings["Host"], int.Parse(smtpSettings["Port"]))
            {
                EnableSsl = bool.Parse(smtpSettings["EnableSsl"]),
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    smtpSettings["Username"],
                    smtpSettings["Password"])
            };

            return client.SendMailAsync(
                new MailMessage(
                    from: smtpSettings["FromEmail"],
                    to: email,
                    subject,
                    message
                ));
        }
    }
}