
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace Erp.DTOs.Auth.Login
{



    public class UserLoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, MinLength(6)]
        public string Password { get; set; } = default!;
    }

}