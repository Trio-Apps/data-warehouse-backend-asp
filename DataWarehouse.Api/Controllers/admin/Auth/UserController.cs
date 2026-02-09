using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.IServices.Auth;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Services.Repository.Permissions;
using DataWarehouse.Services.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Auth;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IAuthServices authServices;
    private readonly IUserServices _userServices;
    private readonly ILogger<UserController> _logger;

    public UserController(IAuthServices authServices, IUserServices userServices, ILogger<UserController> logger)
    {
        this.authServices = authServices;
        _userServices = userServices;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetAllUsers()
    {
        var users = await _userServices.GetAllUsersAsync();
        return Ok(users);
    }


    [HttpGet("{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Users_Get}")] 
    public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetAllUsersWithPagination(int skip,int pageSize,int? companyId,int? sapId
        ,string? email,string? fullName)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var users = await _userServices.GetAllUsersWithPaginationAsync(userId,skip, pageSize,
            companyId,sapId,email,fullName);

        return Ok(users);
    }
   
    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Users_Create}")]
    public async Task<IActionResult> CreateUser([FromBody] AddUserDTO user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }


        if (string.IsNullOrWhiteSpace(user.Password))
            return BadRequest("Password is required.");

        if (await _userServices.EmailExistsAsync(user.Email))
            return Conflict($"Email '{user.Email}' already exists.");


       // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

       // var checkTypeRole = await authServices.GetLoginContextAsync(userId, roles);
        var checkTypeUser = await authServices.GetLoginContextForUserAsync(userId);


        if (checkTypeUser == null)
        {

            if (user.CompanyId == null)
                return BadRequest("select the company for add admin user");


        }
        else
        {

            if (user.SapIds == null)
                return BadRequest("select the sap for add new user");

            if (user.WarehouseIds == null)
                return BadRequest("select the warehouse for add new user");
        }


        var result = await _userServices.CreateUserAsync(userId, user);

        if (!result.Success)
            return BadRequest(result.Errors);

        return Ok(result);
    }

    [HttpPut]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Users_Edit}")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

      
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

      //  var checkTypeRole = await authServices.GetLoginContextAsync(userId, roles);
        var checkTypeUser = await authServices.GetLoginContextForUserAsync(userId);


        if (checkTypeUser == null)
        {

            if (user.CompanyId == null)
                return BadRequest("select the company for add admin user");

            //  if (user.RoleName !="admin")
            //    return BadRequest("you must add admins only");
        }
        else
        {

            if (user.SapIds == null)
                return BadRequest("select the sap for add new user");

            if (user.WarehouseIds == null)
                return BadRequest("select the warehouse for add new user");
        }


        var result = await _userServices.UpdateUserAsync(userId, user);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Users_Delete}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userServices.GetUserByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found.");

        var result = await _userServices.DeleteUserAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpGet("exists/{id}")]
    public async Task<ActionResult<bool>> UserExists(string id)
    {
        var exists = await _userServices.UserExistsAsync(id);
        return Ok(new { exists });
    }

    [HttpGet("email-exists/{email}")]
    public async Task<ActionResult<bool>> EmailExists(string email)
    {
        var exists = await _userServices.EmailExistsAsync(email);
        return Ok(new { exists });
    }

    [HttpGet("username-exists/{userName}")]
    public async Task<ActionResult<bool>> UserNameExists(string userName)
    {
        var exists = await _userServices.UserNameExistsAsync(userName);
        return Ok(new { exists });
    }

    [HttpPost("{id}/set-phone")]
    public async Task<IActionResult> SetPhoneNumber(string id, [FromBody] string phoneNumber)
    {
        var user = await _userServices.GetUserByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found.");

        var result = await _userServices.SetPhoneNumberAsync(user, phoneNumber);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "Phone number set successfully." });
    }

    [HttpGet("{id}/two-factor-enabled")]
    public async Task<ActionResult<bool>> GetTwoFactorEnabled(string id)
    {
        var user = await _userServices.GetUserByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found.");

        var enabled = await _userServices.GetTwoFactorEnabledAsync(user);
        return Ok(new { enabled });
    }

    [HttpPost("{id}/set-two-factor")]
    public async Task<IActionResult> SetTwoFactorEnabled(string id, [FromBody] bool enabled)
    {
        var user = await _userServices.GetUserByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found.");

        var result = await _userServices.SetTwoFactorEnabledAsync(user, enabled);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = $"Two-factor authentication {(enabled ? "enabled" : "disabled")} successfully." });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicationUser>> GetUserById(string id)
    {
        var user = await _userServices.GetUserByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found.");

        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<ApplicationUser>> GetUserByEmail(string email)
    {
        var user = await _userServices.GetUserByEmailAsync(email);
        if (user == null)
            return NotFound($"User with email '{email}' not found.");

        return Ok(user);
    }

    [HttpGet("username/{userName}")]
    public async Task<ActionResult<ApplicationUser>> GetUserByUserName(string userName)
    {
        var user = await _userServices.GetUserByUserNameAsync(userName);
        if (user == null)
            return NotFound($"User with username '{userName}' not found.");

        return Ok(user);
    }

}

