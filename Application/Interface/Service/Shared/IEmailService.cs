namespace Application.Interface.Service.Shared
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);


    }
}
