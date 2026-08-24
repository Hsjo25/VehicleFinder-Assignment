using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using VehicleFinder.Api.Controllers;
using VehicleFinder.Api.DTOs;
using VehicleFinder.Api.Exceptions;
using VehicleFinder.Tests.TestDoubles;

namespace VehicleFinder.Tests.Controllers;

public class VehiclesControllerTests
{
    // Problem()/ValidationProblem() need a ProblemDetailsFactory reachable via HttpContext.RequestServices,
    // which only exists when a real request pipeline runs. Wiring a minimal one lets us unit test the
    // controller directly instead of spinning up a full host.
    private static VehiclesController CreateController(FakeVehicleService service)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ProblemDetailsFactory, TestProblemDetailsFactory>();
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };

        return new VehiclesController(service, NullLogger<VehiclesController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
        };
    }

    [Fact]
    public async Task GetVehicleTypes_InvalidMakeId_ReturnsBadRequest()
    {
        var controller = CreateController(new FakeVehicleService());

        var result = await controller.GetVehicleTypes(0, CancellationToken.None);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetModels_YearOutOfRange_ReturnsBadRequest()
    {
        var controller = CreateController(new FakeVehicleService());

        var result = await controller.GetModels(448, 1899, "Truck", CancellationToken.None);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetModels_EmptyVehicleType_ReturnsBadRequest()
    {
        var controller = CreateController(new FakeVehicleService());

        var result = await controller.GetModels(448, 2015, "   ", CancellationToken.None);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetModels_ValidRequest_ReturnsOkWithModels()
    {
        var service = new FakeVehicleService
        {
            Models = new[] { new VehicleModelDto(1, "Tacoma", "TOYOTA") },
        };
        var controller = CreateController(service);

        var result = await controller.GetModels(448, 2015, "Truck", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var models = Assert.IsAssignableFrom<IReadOnlyList<VehicleModelDto>>(okResult.Value);
        Assert.Single(models);
    }

    [Theory]
    [InlineData(NhtsaFailureReason.Timeout, StatusCodes.Status504GatewayTimeout)]
    [InlineData(NhtsaFailureReason.InvalidResponse, StatusCodes.Status502BadGateway)]
    [InlineData(NhtsaFailureReason.Unavailable, StatusCodes.Status503ServiceUnavailable)]
    public async Task GetMakes_NhtsaFailure_MapsToExpectedStatusCode(NhtsaFailureReason reason, int expectedStatus)
    {
        var service = new FakeVehicleService
        {
            ExceptionToThrow = new NhtsaApiException("failure", reason),
        };
        var controller = CreateController(service);

        var result = await controller.GetMakes(CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(expectedStatus, objectResult.StatusCode);
    }
}
