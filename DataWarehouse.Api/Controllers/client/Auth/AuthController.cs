using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.IServices.Auth;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DataWarehouse.Api.Controllers.client.Auth;

[Route("api/client/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthServices _authServices;
    private readonly IRoleServices _roleServices;
    private readonly IUserServices _userServices;
    private readonly IOptions<JWT> _jwt;
    private readonly ILogger<AuthController> _logger;
    private readonly ISapSettingsRepository sap;
    private readonly ICompanyRepository company;

    public AuthController(
        IAuthServices authServices,
        IRoleServices roleServices,
        IUserServices userServices,
        IOptions<JWT> jwt,
        ILogger<AuthController> logger,
        ISapSettingsRepository sap,
        ICompanyRepository company)
    {
        _authServices = authServices;
        _roleServices = roleServices;
        _userServices = userServices;
        _jwt = jwt;
        _logger = logger;
        this.sap = sap;
        this.company = company;
    }

  
  //  [HttpPost("login")]
    //public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginDTO dto)
    //{
    //    if (!ModelState.IsValid)
    //        return BadRequest(ModelState);

    //    var user = await _authServices.GetUserByEmailAsync(dto.Email);
    //    if (user == null)
    //        return Unauthorized("Invalid email or password.");

    //    //if (await _authServices.IsUserLockedOutAsync(user))
    //    //    return Unauthorized("Account is locked. Please try again later.");

    //    var result = await _authServices.LoginAsync(dto.Email, dto.Password, dto.RememberMe);

    //    if (!result.Succeeded)
    //        return Unauthorized("Invalid email or password.");


    //    var roles = await _roleServices.GetUserRolesAsync(user.Id);
    //    var token = GenerateJwtToken(user, roles);
    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


    //    if (!roles.Contains("admin") && !roles.Contains("super-admin"))
    //    {
    //        // any person isn't admin or super-admin


    //        _logger.LogInformation("userId is : {userId}", userId);
    //        var res1 = await company.PutCompanyInCacheToUsers(userId);
    //        var res2 = await sap.PutSapInCacheToUsers(userId);


    //        if (!res2.Success)
    //            return Unauthorized("This the User has Problem.");

    //        _logger.LogInformation("result from sap is : {res}", res2);
    //    }
    //    else
    //    {
    //        await _authServices.LogoutAsync(userId);
    //        return Unauthorized("This the User has Problem.");
    //    }


    //    return Ok(new AuthResponseDTO
    //    {
    //        Token = token,
    //        ExpiresOn = DateTime.UtcNow.AddDays(_jwt.Value.DurationInMinutes),
    //        User = new UserDTO
    //        {
    //            Id = user.Id,
    //            FullName = user.FullName,
    //            Email = user.Email,
    //            PhoneNumber = user.PhoneNumber,
    //            Roles = roles
    //        }
    //    });
    //}
    [HttpGet("my-warehouses")]
    [Authorize]
    public async Task<IActionResult> MyWarehouse()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
         
        var warehouses = await sap.GetYourWarehousesToEmployees(userId);

        return Ok(warehouses);

    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await _authServices.LogoutAsync(userId);
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDTO>> GetCurrentUser()
    {
        var user = await _authServices.GetUserByClaimsAsync(User);
        if (user == null)
            return Unauthorized();

        var roles = await _roleServices.GetUserRolesAsync(user.Id);

        return Ok(new UserDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Roles = roles
        });
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Value.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Value.Issuer,
            audience: _jwt.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_jwt.Value.DurationInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

