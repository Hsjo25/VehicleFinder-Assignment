using Microsoft.AspNetCore.Mvc;
using VehicleFinder.Api.Exceptions;
using VehicleFinder.Api.Services;

namespace VehicleFinder.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
public class VehiclesController : ControllerBase
{
    private const int MinModelYear = 1900;

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

    /// <summary>Returns vehicle models matching a make, model year and vehicle type, deduplicated and sorted alphabetically.</summary>
    [HttpGet("models")]
    public async Task<IActionResult> GetModels([FromQuery] int makeId, [FromQuery] int year, [FromQuery] string? vehicleType, CancellationToken cancellationToken)
    {
        var maxModelYear = DateTime.UtcNow.Year + 1;

        if (makeId <= 0)
        {
            return ValidationProblem("Make ID must be a positive integer.");
        }

        if (year < MinModelYear || year > maxModelYear)
        {
            return ValidationProblem($"Year must be between {MinModelYear} and {maxModelYear}.");
        }

        if (string.IsNullOrWhiteSpace(vehicleType))
        {
            return ValidationProblem("Vehicle type must not be empty.");
        }

        try
        {
            var models = await _vehicleService.GetModelsAsync(makeId, year, vehicleType.Trim(), cancellationToken);
            return Ok(models);
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
