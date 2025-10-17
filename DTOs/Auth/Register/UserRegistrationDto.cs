using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace Erp.DTOs.Auth.Register
{



    public class UserRegistrationDto
    {
        [MaxLength(120)]
        [Required]
        public string FullName { get; set; } = default!;

        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, MinLength(6)]
        public string Password { get; set; } = default!;

        public string? Role { get; set; } 
    }


}