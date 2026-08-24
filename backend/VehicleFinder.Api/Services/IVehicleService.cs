using VehicleFinder.Api.DTOs;

namespace VehicleFinder.Api.Services;

public interface IVehicleService
{
    Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<VehicleTypeDto>> GetVehicleTypesAsync(int makeId, CancellationToken cancellationToken);
}
