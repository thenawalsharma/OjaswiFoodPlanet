namespace OFP.API.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string userName, string otpCode);
    }
}
