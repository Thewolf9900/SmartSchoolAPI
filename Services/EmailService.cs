using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SmartSchoolAPI.Interfaces;
using SmartSchoolAPI.Settings;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            // استخدام .Value هنا هو النمط الصحيح للتعامل مع IOptions
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // بناء رسالة البريد الإلكتروني
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
            emailMessage.To.Add(MailboxAddress.Parse(toEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            // استخدام SmtpClient لإرسال الرسالة
            // using يضمن التخلص من الاتصال بشكل صحيح حتى لو حدث خطأ
            using var smtpClient = new SmtpClient();

            // الاتصال بالخادم
            await smtpClient.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);

            // المصادقة
            await smtpClient.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);

            // الإرسال
            await smtpClient.SendAsync(emailMessage);

            // قطع الاتصال
            await smtpClient.DisconnectAsync(true);
        }
    }
}