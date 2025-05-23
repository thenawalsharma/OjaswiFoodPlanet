using Microsoft.Extensions.Options;
using OFP.API.Models;
using System.Net;
using System.Net.Mail;

namespace OFP.API.Services
{
    public class EmailService:IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }
        public async Task SendOtpEmailAsync(string toEmail, string userName, string otpCode)
        {
            var subject = "Your OTP Code";
            var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; padding: 20px;'>
                <h2 style='color: #2c3e50;'>Hi {userName},</h2>
                <p>You requested an OTP to verify your identity.</p>
                <h3 style='color: #e74c3c;'>Your OTP Code:</h3>
                <div style='font-size: 24px; font-weight: bold; color: #3498db;'>{otpCode}</div>
                <p>This OTP is valid for 10 minutes. Do not share it with anyone.</p>
                <p>If you did not request this, ignore this email or contact support.</p>
                <br/>
                <p>Thanks,<br/>Your Company Team</p>
            </div>";

            var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}
