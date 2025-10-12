using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AlphaMode.Services
{
    public class EmailOptions
    {
        public string FromName { get; set; } = "";
        public string FromAddress { get; set; } = "";
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool UseStartTls { get; set; } = true;
        public string AppPassword { get; set; } = ""; // set via user-secrets
    }

    public class GmailEmailSender : IEmailSender
    {
        private readonly EmailOptions _opt;
        public GmailEmailSender(IOptions<EmailOptions> opt) => _opt = opt.Value;

        public async Task SendAsync(string subject, string htmlBody, string toAddress, string? replyTo = null)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_opt.FromName, _opt.FromAddress));
            message.To.Add(MailboxAddress.Parse(toAddress));
            message.Subject = subject;

            if (!string.IsNullOrWhiteSpace(replyTo))
                message.ReplyTo.Add(MailboxAddress.Parse(replyTo));

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_opt.Host, _opt.Port, SecureSocketOptions.StartTlsWhenAvailable);
            await client.AuthenticateAsync(_opt.FromAddress, _opt.AppPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
