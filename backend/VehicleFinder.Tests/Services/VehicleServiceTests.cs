using VehicleFinder.Api.Exceptions;
using VehicleFinder.Api.Models.Nhtsa;
using VehicleFinder.Api.Services;
using VehicleFinder.Tests.TestDoubles;

namespace VehicleFinder.Tests.Services;

public class VehicleServiceTests
{
    [Fact]
    public async Task GetMakesAsync_MapsAndSortsAlphabetically()
    {
        var client = new FakeNhtsaClient
        {
            Makes = new[]
            {
                new NhtsaMake { MakeId = 2, MakeName = "  Toyota  " },
                new NhtsaMake { MakeId = 1, MakeName = "Honda" },
            },
        };
        var service = new VehicleService(client);

        var result = await service.GetMakesAsync(CancellationToken.None);

        Assert.Equal(new[] { "Honda", "Toyota" }, result.Select(m => m.Name));
        Assert.Equal("Toyota", result.Single(m => m.Id == 2).Name); // whitespace trimmed
    }

    [Fact]
    public async Task GetMakesAsync_RemovesDuplicateNamesCaseInsensitively()
    {
        var client = new FakeNhtsaClient
        {
            Makes = new[]
            {
                new NhtsaMake { MakeId = 1, MakeName = "Toyota" },
                new NhtsaMake { MakeId = 2, MakeName = "TOYOTA" },
            },
        };
        var service = new VehicleService(client);

        var result = await service.GetMakesAsync(CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetMakesAsync_FiltersOutInvalidEntries()
    {
        var client = new FakeNhtsaClient
        {
            Makes = new[]
            {
                new NhtsaMake { MakeId = 0, MakeName = "Bad Id" },
                new NhtsaMake { MakeId = 5, MakeName = "" },
                new NhtsaMake { MakeId = 6, MakeName = null },
                new NhtsaMake { MakeId = 7, MakeName = "Valid" },
            },
        };
        var service = new VehicleService(client);

        var result = await service.GetMakesAsync(CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal("Valid", dto.Name);
    }

    [Fact]
    public async Task GetVehicleTypesAsync_MapsAndDedupes()
    {
        var client = new FakeNhtsaClient
        {
            VehicleTypes = new[]
            {
                new NhtsaVehicleType { VehicleTypeId = 3, VehicleTypeName = "Truck" },
                new NhtsaVehicleType { VehicleTypeId = 2, VehicleTypeName = "Passenger Car" },
                new NhtsaVehicleType { VehicleTypeId = 4, VehicleTypeName = "Truck" },
            },
        };
        var service = new VehicleService(client);

        var result = await service.GetVehicleTypesAsync(448, CancellationToken.None);

        Assert.Equal(new[] { "Passenger Car", "Truck" }, result.Select(t => t.Name));
    }

    [Fact]
    public async Task GetModelsAsync_MapsIncludingMakeName()
    {
        var client = new FakeNhtsaClient
        {
            Models = new[]
            {
                new NhtsaVehicleModel { ModelId = 1, ModelName = "Tacoma", MakeName = "TOYOTA" },
            },
        };
        var service = new VehicleService(client);

        var result = await service.GetModelsAsync(448, 2015, "Truck", CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal("Tacoma", dto.Name);
        Assert.Equal("TOYOTA", dto.MakeName);
    }

    [Fact]
    public async Task GetModelsAsync_RemovesDuplicateModelNames()
    {
        var client = new FakeNhtsaClient
        {
            Models = new[]
            {
                new NhtsaVehicleModel { ModelId = 1, ModelName = "Tacoma", MakeName = "TOYOTA" },
                new NhtsaVehicleModel { ModelId = 2, ModelName = "Tacoma", MakeName = "TOYOTA" },
                new NhtsaVehicleModel { ModelId = 3, ModelName = "Tundra", MakeName = "TOYOTA" },
            },
        };
        var service = new VehicleService(client);

        var result = await service.GetModelsAsync(448, 2015, "Truck", CancellationToken.None);

        Assert.Equal(new[] { "Tacoma", "Tundra" }, result.Select(m => m.Name));
    }

    [Fact]
    public async Task GetMakesAsync_DoesNotSwallowNhtsaApiException()
    {
        var client = new FakeNhtsaClient
        {
            ExceptionToThrow = new NhtsaApiException("The vehicle data provider timed out.", NhtsaFailureReason.Timeout),
        };
        var service = new VehicleService(client);

        var ex = await Assert.ThrowsAsync<NhtsaApiException>(() => service.GetMakesAsync(CancellationToken.None));
        Assert.Equal(NhtsaFailureReason.Timeout, ex.Reason);
    }
}
