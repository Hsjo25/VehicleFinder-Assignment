using VehicleFinder.Api.Clients;
using VehicleFinder.Api.Exceptions;
using VehicleFinder.Api.Models.Nhtsa;

namespace VehicleFinder.Tests.TestDoubles;

/// <summary>In-memory stand-in for INhtsaClient so service tests don't depend on HTTP or the real API.</summary>
public class FakeNhtsaClient : INhtsaClient
{
    public IReadOnlyList<NhtsaMake> Makes { get; set; } = Array.Empty<NhtsaMake>();
    public IReadOnlyList<NhtsaVehicleType> VehicleTypes { get; set; } = Array.Empty<NhtsaVehicleType>();
    public IReadOnlyList<NhtsaVehicleModel> Models { get; set; } = Array.Empty<NhtsaVehicleModel>();
    public NhtsaApiException? ExceptionToThrow { get; set; }

    public Task<IReadOnlyList<NhtsaMake>> GetAllMakesAsync(CancellationToken cancellationToken)
    {
        if (ExceptionToThrow is not null) throw ExceptionToThrow;
        return Task.FromResult(Makes);
    }

    public Task<IReadOnlyList<NhtsaVehicleType>> GetVehicleTypesForMakeIdAsync(int makeId, CancellationToken cancellationToken)
    {
        if (ExceptionToThrow is not null) throw ExceptionToThrow;
        return Task.FromResult(VehicleTypes);
    }

    public Task<IReadOnlyList<NhtsaVehicleModel>> GetModelsForMakeYearAndTypeAsync(int makeId, int year, string vehicleType, CancellationToken cancellationToken)
    {
        if (ExceptionToThrow is not null) throw ExceptionToThrow;
        return Task.FromResult(Models);
    }
}
