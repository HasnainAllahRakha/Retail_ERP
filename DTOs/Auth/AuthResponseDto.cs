namespace Erp.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Role { get; set; } = default!;
        public bool EmailConfirmed { get; set; }
        public string? Token { get; set; }
    }
}
