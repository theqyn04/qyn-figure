using System.Net.Mail;
using System.Net;

namespace qyn_figure.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("quyennn3pp1@gmail.com", "diuddzmcwovgblaa")
            };

            return client.SendMailAsync(
                new MailMessage(from: "quyennn3pp1@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
