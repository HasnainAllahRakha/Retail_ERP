using System.ComponentModel.DataAnnotations;

namespace Erp.DTOs.Otp
{
    public class ResendOtpDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;
    }
}
