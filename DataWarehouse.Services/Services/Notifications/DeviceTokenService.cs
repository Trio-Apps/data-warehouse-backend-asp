using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Notifications;
using DataWarehouse.Core.Interfaces.Notifications;
using DataWarehouse.Core.IServices.Notifications;
using DataWarehouse.Domain.Entities.Notifications;

namespace DataWarehouse.Services.Services.Notifications;

public class DeviceTokenService : IDeviceTokenService
{
    private readonly IDeviceTokenRepository _deviceTokenRepository;

    public DeviceTokenService(IDeviceTokenRepository deviceTokenRepository)
    {
        _deviceTokenRepository = deviceTokenRepository;
    }

    public async Task<GeneralResponse<DeviceTokenDto>> RegisterOrUpdateAsync(string userId, RegisterDeviceTokenDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DeviceId) || string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.Platform))
            return GeneralResponse<DeviceTokenDto>.FailResponse("DeviceId, Token, and Platform are required.");

        var existingByDevice = await _deviceTokenRepository.GetByUserAndDeviceIdAsync(userId, dto.DeviceId);
        var existingByToken = await _deviceTokenRepository.GetByTokenAsync(dto.Token);
        var now = DateTime.UtcNow;

        if (existingByDevice != null)
        {
            existingByDevice.Token = dto.Token;
            existingByDevice.Platform = dto.Platform.Trim().ToLowerInvariant();
            existingByDevice.IsActive = true;
            existingByDevice.UpdatedAt = now;

            await _deviceTokenRepository.SaveChangesAsync();
            return GeneralResponse<DeviceTokenDto>.SuccessResponse(Map(existingByDevice), "Device token updated successfully.");
        }

        if (existingByToken != null)
        {
            existingByToken.UserId = userId;
            existingByToken.DeviceId = dto.DeviceId;
            existingByToken.Platform = dto.Platform.Trim().ToLowerInvariant();
            existingByToken.IsActive = true;
            existingByToken.UpdatedAt = now;

            await _deviceTokenRepository.SaveChangesAsync();
            return GeneralResponse<DeviceTokenDto>.SuccessResponse(Map(existingByToken), "Device token updated successfully.");
        }

        var deviceToken = new DeviceToken
        {
            UserId = userId,
            DeviceId = dto.DeviceId,
            Token = dto.Token,
            Platform = dto.Platform.Trim().ToLowerInvariant(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _deviceTokenRepository.AddAsync(deviceToken);
        await _deviceTokenRepository.SaveChangesAsync();

        return GeneralResponse<DeviceTokenDto>.SuccessResponse(Map(deviceToken), "Device token registered successfully.");
    }

    public async Task<GeneralResponse<List<DeviceTokenDto>>> GetActiveTokensByUserAsync(string userId)
    {
        var tokens = await _deviceTokenRepository.GetActiveByUserIdAsync(userId);
        return GeneralResponse<List<DeviceTokenDto>>.SuccessResponse(tokens.Select(Map).ToList());
    }

    public async Task<GeneralResponse<bool>> DeactivateAsync(string userId, DeactivateDeviceTokenDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DeviceId) && string.IsNullOrWhiteSpace(dto.Token))
            return GeneralResponse<bool>.FailResponse("DeviceId or Token is required.");

        DeviceToken? deviceToken = null;

        if (!string.IsNullOrWhiteSpace(dto.DeviceId))
            deviceToken = await _deviceTokenRepository.GetByUserAndDeviceIdAsync(userId, dto.DeviceId);

        if (deviceToken == null && !string.IsNullOrWhiteSpace(dto.Token))
            deviceToken = await _deviceTokenRepository.GetByTokenAsync(dto.Token);

        if (deviceToken == null || deviceToken.UserId != userId)
            return GeneralResponse<bool>.FailResponse("Device token not found.");

        if (deviceToken.IsActive)
        {
            deviceToken.IsActive = false;
            deviceToken.UpdatedAt = DateTime.UtcNow;
            await _deviceTokenRepository.SaveChangesAsync();
        }

        return GeneralResponse<bool>.SuccessResponse(true, "Device token deactivated successfully.");
    }

    public async Task<GeneralResponse<int>> DeactivateInvalidTokensAsync(IEnumerable<string> tokens)
    {
        var invalidTokens = tokens
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        if (invalidTokens.Count == 0)
            return GeneralResponse<int>.SuccessResponse(0, "No invalid tokens supplied.");

        var deviceTokens = await _deviceTokenRepository.GetByTokensAsync(invalidTokens);
        var now = DateTime.UtcNow;
        var changedCount = 0;

        foreach (var deviceToken in deviceTokens.Where(x => x.IsActive))
        {
            deviceToken.IsActive = false;
            deviceToken.UpdatedAt = now;
            changedCount++;
        }

        if (changedCount > 0)
            await _deviceTokenRepository.SaveChangesAsync();

        return GeneralResponse<int>.SuccessResponse(changedCount, "Invalid tokens were deactivated successfully.");
    }

    private static DeviceTokenDto Map(DeviceToken deviceToken)
    {
        return new DeviceTokenDto
        {
            Id = deviceToken.Id,
            UserId = deviceToken.UserId,
            DeviceId = deviceToken.DeviceId,
            Token = deviceToken.Token,
            Platform = deviceToken.Platform,
            IsActive = deviceToken.IsActive,
            CreatedAt = deviceToken.CreatedAt,
            UpdatedAt = deviceToken.UpdatedAt
        };
    }
}
