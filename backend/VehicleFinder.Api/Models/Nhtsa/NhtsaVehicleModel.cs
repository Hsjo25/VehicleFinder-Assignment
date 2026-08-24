using System.Text.Json.Serialization;

namespace VehicleFinder.Api.Models.Nhtsa;

public class NhtsaVehicleModel
{
    [JsonPropertyName("Model_ID")]
    public int ModelId { get; set; }

    [JsonPropertyName("Model_Name")]
    public string? ModelName { get; set; }

    [JsonPropertyName("Make_Name")]
    public string? MakeName { get; set; }
}
