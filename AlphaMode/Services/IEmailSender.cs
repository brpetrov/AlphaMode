namespace AlphaMode.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string subject, string htmlBody, string toAddress, string? replyTo = null);
    }
}
