using System.Net.Http.Json;
using System.Text.Json;
using VehicleFinder.Api.Exceptions;
using VehicleFinder.Api.Models.Nhtsa;

namespace VehicleFinder.Api.Clients;

/// <summary>
/// Thin wrapper around the NHTSA VPIC API. Builds request URLs, deserializes responses,
/// and translates transport/parsing failures into <see cref="NhtsaApiException"/> so callers
/// never see raw HttpClient/Json exceptions.
/// </summary>
public class NhtsaClient : INhtsaClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NhtsaClient> _logger;

    public NhtsaClient(HttpClient httpClient, ILogger<NhtsaClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<NhtsaMake>> GetAllMakesAsync(CancellationToken cancellationToken)
    {
        var response = await GetAsync<NhtsaMake>("vehicles/getallmakes?format=json", cancellationToken);
        return response.Results;
    }

    public async Task<IReadOnlyList<NhtsaVehicleType>> GetVehicleTypesForMakeIdAsync(int makeId, CancellationToken cancellationToken)
    {
        var response = await GetAsync<NhtsaVehicleType>($"vehicles/GetVehicleTypesForMakeId/{makeId}?format=json", cancellationToken);
        return response.Results;
    }

    public async Task<IReadOnlyList<NhtsaVehicleModel>> GetModelsForMakeYearAndTypeAsync(int makeId, int year, string vehicleType, CancellationToken cancellationToken)
    {
        var encodedVehicleType = Uri.EscapeDataString(vehicleType);
        var response = await GetAsync<NhtsaVehicleModel>(
            $"vehicles/GetModelsForMakeIdYear/makeId/{makeId}/modelyear/{year}/vehicletype/{encodedVehicleType}?format=json",
            cancellationToken);
        return response.Results;
    }

    private async Task<NhtsaApiResponse<T>> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(relativeUrl, cancellationToken);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // The caller didn't cancel, so this is our own HttpClient.Timeout firing.
            _logger.LogWarning(ex, "NHTSA request to {Url} timed out", relativeUrl);
            throw new NhtsaApiException("The vehicle data provider timed out.", NhtsaFailureReason.Timeout, ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "NHTSA request to {Url} failed", relativeUrl);
            throw new NhtsaApiException("The vehicle data provider is unavailable.", NhtsaFailureReason.Unavailable, ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("NHTSA request to {Url} returned status {StatusCode}", relativeUrl, response.StatusCode);
            throw new NhtsaApiException("The vehicle data provider is unavailable.", NhtsaFailureReason.Unavailable);
        }

        try
        {
            var result = await response.Content.ReadFromJsonAsync<NhtsaApiResponse<T>>(cancellationToken: cancellationToken);
            if (result is null)
            {
                throw new NhtsaApiException("The vehicle data provider returned an unexpected response.", NhtsaFailureReason.InvalidResponse);
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "NHTSA response from {Url} could not be parsed", relativeUrl);
            throw new NhtsaApiException("The vehicle data provider returned an unexpected response.", NhtsaFailureReason.InvalidResponse, ex);
        }
    }
}
