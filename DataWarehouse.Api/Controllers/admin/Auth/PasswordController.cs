using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.IServices.Auth;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.Auth;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PasswordController : ControllerBase
{
    private readonly IPasswordServices _passwordServices;
    private readonly IAuthServices _authServices;
    private readonly ILogger<PasswordController> _logger;

    public PasswordController(
        IPasswordServices passwordServices,
        IAuthServices authServices,
        ILogger<PasswordController> logger)
    {
        _passwordServices = passwordServices;
        _authServices = authServices;
        _logger = logger;
    }


    [HttpPost("change")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Passwords_Change}")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _authServices.GetUserByClaimsAsync(User);
        if (user == null)
            return Unauthorized();

        var result = await _passwordServices.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "Password changed successfully." });
    }

    [HttpPost("forgot")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _authServices.GetUserByEmailAsync(dto.Email);
        if (user == null)
            return Ok(new { message = "If the email exists, a password reset link has been sent." });

        var token = await _passwordServices.GeneratePasswordResetTokenAsync(user);
        
        // TODO: Send email with token
        // For now, return token (remove this in production)
        return Ok(new { message = "Password reset token generated.", token });
    }

    [HttpPost("reset")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _authServices.GetUserByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest("Invalid email or token.");

        var result = await _passwordServices.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "Password reset successfully." });
    }


    [HttpPost("validate")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Passwords_Validate}")]
    public async Task<IActionResult> ValidatePassword([FromBody] string password)
    {
        var user = await _authServices.GetUserByClaimsAsync(User);
        if (user == null)
            return Unauthorized();

        var isValid = await _passwordServices.ValidatePasswordAsync(user, password);
        return Ok(new { isValid });
    }

    [HttpGet("has-password")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Passwords_Has_Password}")]
    public async Task<IActionResult> HasPassword()
    {
        var user = await _authServices.GetUserByClaimsAsync(User);
        if (user == null)
            return Unauthorized();


        var hasPassword = await _passwordServices.HasPasswordAsync(user);
        return Ok(new { hasPassword });
    }
}

