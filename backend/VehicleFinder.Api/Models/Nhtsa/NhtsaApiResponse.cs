namespace VehicleFinder.Api.Models.Nhtsa;

/// <summary>Envelope shape shared by all NHTSA VPIC endpoints.</summary>
public class NhtsaApiResponse<T>
{
    public int Count { get; set; }
    public string? Message { get; set; }
    public List<T> Results { get; set; } = new();
}
