using DataWarehouse.Core.Interfaces.Notifications;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Services.Repository.Notifications;

public class DeviceTokenRepository : IDeviceTokenRepository
{
    private readonly DataWarehouseDbContext _context;

    public DeviceTokenRepository(DataWarehouseDbContext context)
    {
        _context = context;
    }

    public Task<DeviceToken?> GetByUserAndDeviceIdAsync(string userId, string deviceId)
    {
        return _context.DeviceTokens
            .FirstOrDefaultAsync(x => x.UserId == userId && x.DeviceId == deviceId);
    }

    public Task<List<DeviceToken>> GetActiveByUserIdAsync(string userId)
    {
        return _context.DeviceTokens
            .Where(x => x.UserId == userId && x.IsActive)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();
    }

    public Task<DeviceToken?> GetByTokenAsync(string token)
    {
        return _context.DeviceTokens
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public Task<List<DeviceToken>> GetByTokensAsync(IEnumerable<string> tokens)
    {
        var normalizedTokens = tokens
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        return _context.DeviceTokens
            .Where(x => normalizedTokens.Contains(x.Token))
            .ToListAsync();
    }

    public Task AddAsync(DeviceToken deviceToken)
    {
        return _context.DeviceTokens.AddAsync(deviceToken).AsTask();
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
