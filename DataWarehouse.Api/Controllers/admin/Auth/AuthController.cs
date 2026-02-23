using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Permissions;
using DataWarehouse.Core.IServices.Auth;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Services.Repository.CompanyRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DataWarehouse.Api.Controllers.admin.Auth;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthServices _authServices;
    private readonly IRoleServices _roleServices;
    private readonly IUserServices _userServices;
    private readonly IPermissionsRepository permissionsRepository;
    private readonly IOptions<JWT> _jwt;
    private readonly ILogger<AuthController> _logger;
    private readonly ISapSettingsRepository sap;
    private readonly ICompanyRepository company;
    private readonly ICompanyCache companyCache;

    public AuthController(
        IAuthServices authServices,
        IRoleServices roleServices,
        IUserServices userServices,
        IPermissionsRepository permissionsRepository,
        IOptions<JWT> jwt,
        ILogger<AuthController> logger,
        ISapSettingsRepository sap,
        ICompanyRepository company,
        ICompanyCache companyCache)
    {
        _authServices = authServices;
        _roleServices = roleServices;
        _userServices = userServices;
        this.permissionsRepository = permissionsRepository;
        _jwt = jwt;
        _logger = logger;
        this.sap = sap;
        this.company = company;
        this.companyCache = companyCache;
    }


    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _authServices.GetUserByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized("Invalid email or password.");

        //if (await _authServices.IsUserLockedOutAsync(user))
        //    return Unauthorized("Account is locked. Please try again later.");

        var result = await _authServices.LoginAsync(dto.Email, dto.Password, dto.RememberMe);
        if (!result.Succeeded)
            return Unauthorized("Invalid email or password.");

        var roles = await _roleServices.GetUserRolesAsync(user.Id);

        // ✅ مهم: في login استخدم user.Id مباشرة
        var userId = user.Id;
        int? companyId = user.CompanyUser?.CompanyId;
        //  var companyId = await _authServices.GetLoginContextForUserAsync(userId);
        _logger.LogInformation("companyId is : {companyId}", companyId);

        // (اختياري) الكاش بتاع الشركة و SAP
        //await company.PutCompanyInCacheToUsers(userId);

        if (companyId != null)
            await companyCache.SetCompanyToUserClaimAsync(companyId.ToString());

        await sap.PutSapInCacheToUsers(userId);


        // ✅ هات permissions للمستخدم (roles + user-specific إن وجد)
        var permissions = await permissionsRepository.GetUserPermissionsAsync(userId); // <-- هكتب لك تنفيذها تحت
        _logger.LogInformation("permissions is : {permissions}", permissions);


        var token = GenerateJwtToken(user, roles, companyId, permissions);

        return Ok(new AuthResponseDTO
        {
            Token = token,
            ExpiresOn = DateTime.UtcNow.AddMinutes(_jwt.Value.DurationInMinutes), // ✅ Minutes مش Days
            User = new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles,
                companyId = companyId
            },
            Permissions = permissions // ✅ أضفها في DTO
        });
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

    private string GenerateJwtToken(
     ApplicationUser user,
     IList<string> roles,
     int? companyId,
     IReadOnlyCollection<string> permissions)
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email ?? ""),
        new Claim("companyId", companyId.ToString())
    };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        // ✅ permissions claims
        //foreach (var p in permissions.Distinct(StringComparer.OrdinalIgnoreCase))
        //    claims.Add(new Claim("perm", p));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Value.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Value.Issuer,
            audience: _jwt.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_jwt.Value.DurationInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}

