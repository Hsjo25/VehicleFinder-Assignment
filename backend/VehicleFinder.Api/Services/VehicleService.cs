using VehicleFinder.Api.Clients;
using VehicleFinder.Api.DTOs;

namespace VehicleFinder.Api.Services;

public class VehicleService : IVehicleService
{
    private readonly INhtsaClient _nhtsaClient;

    public VehicleService(INhtsaClient nhtsaClient)
    {
        _nhtsaClient = nhtsaClient;
    }

    public async Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken cancellationToken)
    {
        var makes = await _nhtsaClient.GetAllMakesAsync(cancellationToken);

        return makes
            .Where(m => m.MakeId > 0 && !string.IsNullOrWhiteSpace(m.MakeName))
            .Select(m => new MakeDto(m.MakeId, m.MakeName!.Trim()))
            .GroupBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
