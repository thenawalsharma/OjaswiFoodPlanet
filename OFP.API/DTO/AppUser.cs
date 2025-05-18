using System.ComponentModel.DataAnnotations;

namespace OFP.API.DTO
{
    public class AppUser
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string? UserName { get; set; }

        [Required]
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        [Required]
        public string? Phone1 { get; set; }

        public string? AltPhoneNumber { get; set; }

        [Required]
        public string? EmailAddress { get; set; }

        public string? Address { get; set; }

        [Required]
        public string? PasswordHash { get; set; }

        public string Provider { get; set; } = "Local";

        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsLocked { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }
}
