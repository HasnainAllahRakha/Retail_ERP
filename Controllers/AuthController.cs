using Microsoft.AspNetCore.Mvc;
using Erp.Services;
using Erp.DTOs.Auth;
using Erp.DTOs.Auth.Login;
using Erp.DTOs.Auth.Register;
using Erp.DTOs.Password;
using Erp.Models.ApplicationUsers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace Erp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(AuthService authService, UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        #region Register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(request);

            if (result.Contains("successfully"))
                return Ok(new { message = result });

            return BadRequest(new { message = result });
        }
        #endregion

        #region Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(request, Response);

            if (result == "Login successful")
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                var roles = await _userManager.GetRolesAsync(user!);

                var response = new AuthResponseDto
                {
                    Email = user.Email!,
                    FullName = user.FullName,
                    Role = roles.FirstOrDefault() ?? "Candidate",
                    EmailConfirmed = user.EmailConfirmed
                };

                return Ok(new { message = result, user = response });
            }

            return BadRequest(new { message = result });
        }
        #endregion


        #region Logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(new { message = "Logged out successfully" });
        }
        #endregion

        #region Get user details
        [HttpGet("me")]
        [Authorize]//This means: Only authenticated users (with a valid JWT token or cookie) can access this endpoint.If you call it without a token → you'll get 401 Unauthorized.
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not found" });

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var roles = await _userManager.GetRolesAsync(user);

            var response = new AuthResponseDto
            {
                Email = user.Email!,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? "Candidate",
                EmailConfirmed = user.EmailConfirmed
            };

            return Ok(new { user = response });
        }
        #endregion

        #region Delete User
        [HttpDelete("delete/{email}")]
        // [Authorize]
        public async Task<IActionResult> DeleteUser(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { message = "Email is required" });

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
                return Ok(new { message = "User deleted successfully" });

            return BadRequest(new { message = "Failed to delete user", errors = result.Errors });
        }
        #endregion

        #region Request password reset
        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] ResetPasswordRequestDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound(new { message = "No user found with this email." });

            // Prevent generating another valid link within 20 mins if already sent
            if (user.PasswordResetExpiry.HasValue && user.PasswordResetExpiry > DateTime.UtcNow && !user.IsPasswordResetUsed)
            {
                return BadRequest(new { message = "A reset link was already sent. Please wait until it expires." });
            }

            // Generate token (use Identity's secure token generator)
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Store it temporarily for validation (optional — since Identity also validates)
            user.PasswordResetToken = token;
            user.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(20);
            user.IsPasswordResetUsed = false;
            await _userManager.UpdateAsync(user);

            await _authService.SendPasswordResetEmailAsync(user, token);

            return Ok(new { message = "Password reset link sent successfully." });
        }
        #endregion

        #region Reset Password

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound(new { message = "Invalid email or token." });

            if (user.PasswordResetExpiry < DateTime.UtcNow)
                return BadRequest(new { message = "Reset link expired." });

            if (user.IsPasswordResetUsed)
                return BadRequest(new { message = "This reset link has already been used." });

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new { message = "Password reset failed.", errors = result.Errors });

            // Mark link as used
            user.IsPasswordResetUsed = true;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Password has been reset successfully." });
        }
        #endregion
    }
}
