namespace VehicleFinder.Api.Configuration;

public class NhtsaOptions
{
    public const string SectionName = "Nhtsa";

    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 15;
}
