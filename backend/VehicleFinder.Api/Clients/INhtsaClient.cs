using VehicleFinder.Api.Models.Nhtsa;

namespace VehicleFinder.Api.Clients;

public interface INhtsaClient
{
    Task<IReadOnlyList<NhtsaMake>> GetAllMakesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<NhtsaVehicleType>> GetVehicleTypesForMakeIdAsync(int makeId, CancellationToken cancellationToken);
}
