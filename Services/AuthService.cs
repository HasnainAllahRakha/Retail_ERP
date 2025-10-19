using Erp.DTOs.Auth.Login;
using Erp.DTOs.Auth.Register;
using Erp.Models.ApplicationUsers;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using Erp.Data.Enum;


namespace Erp.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;
        private readonly EmailSettings _emailSettings;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            EmailService emailService,
   IOptions<EmailSettings> emailSettings,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _config = config;
        }


        #region Generate Token
        private string GenerateJwtToken(ApplicationUser user, HttpResponse response)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            // Determine expiry: prefer days config, otherwise minutes config, otherwise default 2 days
            int expiryDays = 0;
            if (!string.IsNullOrEmpty(_config["Jwt:AccessTokenExpiryDays"]) && int.TryParse(_config["Jwt:AccessTokenExpiryDays"], out var d))
            {
                expiryDays = d;
            }

            DateTime expiresAt;
            if (expiryDays > 0)
            {
                expiresAt = DateTime.UtcNow.AddDays(expiryDays);
            }
            else if (!string.IsNullOrEmpty(_config["Jwt:AccessTokenExpiryMinutes"])
                     && int.TryParse(_config["Jwt:AccessTokenExpiryMinutes"], out var mins))
            {
                expiresAt = DateTime.UtcNow.AddMinutes(mins);
            }
            else
            {
                // default: 2 days
                expiresAt = DateTime.UtcNow.AddDays(2);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email!)
                }),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            // Set cookie
            response.Cookies.Append("jwt", jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = expiresAt
            });

            return jwt;
        }
        #endregion


        #region Registration
        public async Task<string> RegisterAsync(UserRegistrationDto request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                if (existingUser.EmailConfirmed)
                    return "Account already exists. Please login.";
                else
                    return "Account exists but not verified. Please login to verify.";
            }

            // ✅ Default role fallback
            var roleName = string.IsNullOrWhiteSpace(request.Role) ? AppRoles.Staff.ToString() : request.Role;

            // ✅ Validate against enum (case-insensitive)
            if (!Enum.TryParse(typeof(AppRoles), roleName, true, out var parsedRole))
                return "Invalid role. Must be one of the defined system roles.";

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return string.Join(", ", result.Errors.Select(e => e.Description));

            // ✅ No need to check DB — seeder already ensured roles exist
            await _userManager.AddToRoleAsync(user, parsedRole.ToString());

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return "User registered successfully.";
        }

        #endregion

        #region Login
        public async Task<string> LoginAsync(UserLoginDto request, HttpResponse response)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return "Invalid login";

            GenerateJwtToken(user, response);
            return "Login successful";
        }
        #endregion

        #region Password-reset-email
        public async Task SendPasswordResetEmailAsync(ApplicationUser user, string token)
        {
            var resetLink = $"{_emailSettings.FrontendUrl}/reset-password?email={user.Email}&token={WebUtility.UrlEncode(token)}";

            var subject = "Password Reset Request";
            var body = $@"
                <p>Hello {user.FullName},</p>
                <p>You requested to reset your password. Click the link below to reset it (valid for 20 minutes):</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you didn't request this, you can safely ignore this email.</p>
            ";

            await _emailService.SendEmailAsync(user.Email, subject, body);
        }
        #endregion

    }
}
