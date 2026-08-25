using Microsoft.Extensions.Caching.Memory;
using VehicleFinder.Api.Clients;
using VehicleFinder.Api.DTOs;

namespace VehicleFinder.Api.Services;

public class VehicleService : IVehicleService
{
    // Makes and vehicle types are reference data NHTSA rarely changes; caching them avoids
    // re-fetching/re-mapping the full ~12k-make list on every page load. Model search isn't
    // cached — the make/year/vehicle-type combinations are too varied for caching to pay off.
    private static readonly TimeSpan ReferenceDataCacheDuration = TimeSpan.FromHours(6);
    private const string MakesCacheKey = "vehicle-makes";

    private readonly INhtsaClient _nhtsaClient;
    private readonly IMemoryCache _cache;

    public VehicleService(INhtsaClient nhtsaClient, IMemoryCache cache)
    {
        _nhtsaClient = nhtsaClient;
        _cache = cache;
    }

    public async Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(MakesCacheKey, out IReadOnlyList<MakeDto>? cached) && cached is not null)
        {
            return cached;
        }

        var makes = await _nhtsaClient.GetAllMakesAsync(cancellationToken);

        var result = makes
            .Where(m => m.MakeId > 0 && !string.IsNullOrWhiteSpace(m.MakeName))
            .Select(m => new MakeDto(m.MakeId, m.MakeName!.Trim()))
            .GroupBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _cache.Set(MakesCacheKey, (IReadOnlyList<MakeDto>)result, ReferenceDataCacheDuration);
        return result;
    }

    public async Task<IReadOnlyList<VehicleTypeDto>> GetVehicleTypesAsync(int makeId, CancellationToken cancellationToken)
    {
        var cacheKey = $"vehicle-types:{makeId}";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<VehicleTypeDto>? cached) && cached is not null)
        {
            return cached;
        }

        var vehicleTypes = await _nhtsaClient.GetVehicleTypesForMakeIdAsync(makeId, cancellationToken);

        var result = vehicleTypes
            .Where(t => t.VehicleTypeId > 0 && !string.IsNullOrWhiteSpace(t.VehicleTypeName))
            .Select(t => new VehicleTypeDto(t.VehicleTypeId, t.VehicleTypeName!.Trim()))
            .GroupBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _cache.Set(cacheKey, (IReadOnlyList<VehicleTypeDto>)result, ReferenceDataCacheDuration);
        return result;
    }

    public async Task<IReadOnlyList<VehicleModelDto>> GetModelsAsync(int makeId, int year, string vehicleType, CancellationToken cancellationToken)
    {
        var models = await _nhtsaClient.GetModelsForMakeYearAndTypeAsync(makeId, year, vehicleType, cancellationToken);

        return models
            .Where(m => m.ModelId > 0 && !string.IsNullOrWhiteSpace(m.ModelName))
            .Select(m => new VehicleModelDto(m.ModelId, m.ModelName!.Trim(), m.MakeName?.Trim() ?? string.Empty))
            .GroupBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
