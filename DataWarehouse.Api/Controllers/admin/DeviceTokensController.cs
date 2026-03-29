using System.Security.Claims;
using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Notifications;
using DataWarehouse.Core.IServices.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin;

[Route("api/device-tokens")]
[ApiController]
[Authorize]
public class DeviceTokensController : ControllerBase
{
    private readonly IDeviceTokenService _deviceTokenService;

    public DeviceTokensController(IDeviceTokenService deviceTokenService)
    {
        _deviceTokenService = deviceTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDeviceTokenDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(GeneralResponse<DeviceTokenDto>.FailResponse("User ID not found in token."));

        var result = await _deviceTokenService.RegisterOrUpdateAsync(userId, dto);
        return Ok(result);
    }

    [HttpPost("deactivate")]
    public async Task<IActionResult> Deactivate([FromBody] DeactivateDeviceTokenDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(GeneralResponse<bool>.FailResponse("User ID not found in token."));

        var result = await _deviceTokenService.DeactivateAsync(userId, dto);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(GeneralResponse<List<DeviceTokenDto>>.FailResponse("User ID not found in token."));

        var result = await _deviceTokenService.GetActiveTokensByUserAsync(userId);
        return Ok(result);
    }

    [HttpPatch("deactivate-invalid")]
    public async Task<IActionResult> DeactivateInvalid([FromBody] List<string> tokens)
    {
        var result = await _deviceTokenService.DeactivateInvalidTokensAsync(tokens);
        return Ok(result);
    }
}
