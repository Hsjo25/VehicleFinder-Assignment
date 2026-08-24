using VehicleFinder.Api.DTOs;
using VehicleFinder.Api.Services;

namespace VehicleFinder.Tests.TestDoubles;

/// <summary>In-memory stand-in for IVehicleService so controller tests only exercise routing/validation.</summary>
public class FakeVehicleService : IVehicleService
{
    public IReadOnlyList<MakeDto> Makes { get; set; } = Array.Empty<MakeDto>();
    public IReadOnlyList<VehicleTypeDto> VehicleTypes { get; set; } = Array.Empty<VehicleTypeDto>();
    public IReadOnlyList<VehicleModelDto> Models { get; set; } = Array.Empty<VehicleModelDto>();
    public Exception? ExceptionToThrow { get; set; }

    public Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken cancellationToken)
    {
        if (ExceptionToThrow is not null) throw ExceptionToThrow;
        return Task.FromResult(Makes);
    }

    public Task<IReadOnlyList<VehicleTypeDto>> GetVehicleTypesAsync(int makeId, CancellationToken cancellationToken)
    {
        if (ExceptionToThrow is not null) throw ExceptionToThrow;
        return Task.FromResult(VehicleTypes);
    }

    public Task<IReadOnlyList<VehicleModelDto>> GetModelsAsync(int makeId, int year, string vehicleType, CancellationToken cancellationToken)
    {
        if (ExceptionToThrow is not null) throw ExceptionToThrow;
        return Task.FromResult(Models);
    }
}
