using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OFP.API.DTO;
using OFP.API.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace OFP.API.Controllers.Users
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public UserController(ApplicationDbContext context, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> CreateNewUser([FromBody] SignUpDto dto)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.EmailAddress == dto.EmailAddress))
            {
                return BadRequest("User already exists with this email.");
            }

            var newUser = new AppUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone1 = dto.Phone1,
                AltPhoneNumber = dto.AltPhoneNumber,
                EmailAddress = dto.EmailAddress,
                Address = dto.Address,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                CreatedBy = dto.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                Provider = "Local"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            SendEmail();

            return Ok(new { message = "User registered successfully", userId = newUser.UserId });
        }

        public void SendEmail()
        {
            var fromAddress = new MailAddress("thenawalsharma07@gmail.com", "Nawal Kishor Sharma");
            var toAddress = new MailAddress("thenawalsharma@gmail.com");
            string fromPassword = "xnpc fxxr tvor dzax";
            string subject = "Test Email";
            string body = "This is a test email.";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }


        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailAddress == dto.EmailAddress && u.IsDeleted == false && u.IsActive == true);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            if (user.IsLocked)
            {
                return Forbid("Account is locked.");
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                userId = user.UserId,
                name = $"{user.FirstName} {user.LastName}",
                email = user.EmailAddress,
                phone1= user.Phone1,
            });
        }

        // In your UserController.cs
        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == email && !u.IsDeleted);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("request-reset-password")]
        public async Task<IActionResult> RequestResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == dto.Email);
                if (user == null)
                    return NotFound("User not found");

                // Generate OTP (real app should be random and secure)
                var otp = new Random().Next(100000, 999999).ToString();
                user.OTP = otp;
                user.OTPGeneratedAt = DateTime.UtcNow; // Set the OTP generation time
                await _context.SaveChangesAsync();

                // TODO: Send OTP via email (SMTP logic yahan call karo)
                Console.WriteLine($"Send this OTP to email: {otp}");

                // ✅ Send OTP email using IEmailService
                await _emailService.SendOtpEmailAsync(user.EmailAddress,$"{user.FirstName} {user.LastName}".Trim() ?? user.UserName,otp);


                return Ok("Reset OTP sent to your email.");
            }
            catch(Exception ex)
            {
                throw;
            } 
        }

        [HttpPost("validate-otp")]
        public async Task<IActionResult> ValidateOtp([FromBody] OtpValidationDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == dto.Email && u.OTP == dto.OTP);
            if (user == null) return BadRequest("Invalid OTP");

            var timeDiff = DateTime.UtcNow - user.OTPGeneratedAt.GetValueOrDefault();
            if (timeDiff.TotalMinutes > 10) return BadRequest("OTP expired");

            return Ok("OTP validated");
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == dto.Email && u.OTP == dto.OTP);
            if (user == null) return BadRequest("Invalid OTP");

            user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword); // In real app, hash the password
            user.OTP = null;
            await _context.SaveChangesAsync();

            return Ok("Password updated successfully.");
        }


        private string GenerateJwtToken(AppUser user)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.EmailAddress),
            new Claim(ClaimTypes.Name, user.FirstName)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
