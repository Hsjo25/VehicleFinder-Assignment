using VehicleFinder.Api.Models.Nhtsa;

namespace VehicleFinder.Api.Clients;

public interface INhtsaClient
{
    Task<IReadOnlyList<NhtsaMake>> GetAllMakesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<NhtsaVehicleType>> GetVehicleTypesForMakeIdAsync(int makeId, CancellationToken cancellationToken);

    Task<IReadOnlyList<NhtsaVehicleModel>> GetModelsForMakeYearAndTypeAsync(int makeId, int year, string vehicleType, CancellationToken cancellationToken);
}
