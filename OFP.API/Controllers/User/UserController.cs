using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OFP.API.DTO;
using System;
using System.IdentityModel.Tokens.Jwt;
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
        public UserController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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

            return Ok(new { message = "User registered successfully", userId = newUser.UserId });
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
