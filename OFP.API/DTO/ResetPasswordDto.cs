namespace OFP.API.DTO
{
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string OTP { get; set; } // Or reset token
    }
}
