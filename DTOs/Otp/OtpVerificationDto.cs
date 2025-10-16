using System.ComponentModel.DataAnnotations;

namespace Erp.DTOs.Otp
{
    public class OtpVerificationDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = default!;
    }
}
