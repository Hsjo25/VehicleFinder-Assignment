using Microsoft.AspNetCore.Mvc;
using VehicleFinder.Api.Exceptions;
using VehicleFinder.Api.Services;

namespace VehicleFinder.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(IVehicleService vehicleService, ILogger<VehiclesController> logger)
    {
        _vehicleService = vehicleService;
        _logger = logger;
    }

    /// <summary>Returns all known vehicle makes, deduplicated and sorted alphabetically.</summary>
    [HttpGet("makes")]
    public async Task<IActionResult> GetMakes(CancellationToken cancellationToken)
    {
        try
        {
            var makes = await _vehicleService.GetMakesAsync(cancellationToken);
            return Ok(makes);
        }
        catch (NhtsaApiException ex)
        {
            return ProblemFromNhtsaException(ex);
        }
    }

    /// <summary>Returns the vehicle types available for a given make, deduplicated and sorted alphabetically.</summary>
    [HttpGet("makes/{makeId}/types")]
    public async Task<IActionResult> GetVehicleTypes(int makeId, CancellationToken cancellationToken)
    {
        if (makeId <= 0)
        {
            return ValidationProblem("Make ID must be a positive integer.");
        }

        try
        {
            var vehicleTypes = await _vehicleService.GetVehicleTypesAsync(makeId, cancellationToken);
            return Ok(vehicleTypes);
        }
        catch (NhtsaApiException ex)
        {
            return ProblemFromNhtsaException(ex);
        }
    }

    private ObjectResult ProblemFromNhtsaException(NhtsaApiException ex)
    {
        var (statusCode, title) = ex.Reason switch
        {
            NhtsaFailureReason.Timeout => (StatusCodes.Status504GatewayTimeout, "Vehicle data provider timed out"),
            NhtsaFailureReason.InvalidResponse => (StatusCodes.Status502BadGateway, "Vehicle data provider returned unexpected data"),
            _ => (StatusCodes.Status503ServiceUnavailable, "Vehicle data is temporarily unavailable"),
        };

        _logger.LogError(ex, "NHTSA integration failure: {Reason}", ex.Reason);

        return Problem(statusCode: statusCode, title: title, detail: ex.Message);
    }
}
