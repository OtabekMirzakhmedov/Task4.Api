using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Task4.Api.Models
{
    public class AppUser: IdentityUser
    {

        [Required]
        [MaxLength(50)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
