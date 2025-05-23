using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFP.API.Models;
using OFP.API.Services;

namespace OFP.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class OtpController : ControllerBase
    {
        private readonly IEmailService _emailService;
        public OtpController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendOtp([FromBody] OtpRequest request)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            await _emailService.SendOtpEmailAsync(request.Email, request.UserName, otp);
            return Ok(new { message = "OTP sent successfully", otp }); // ⚠️ Never return OTP in real apps
        }
    }
}
