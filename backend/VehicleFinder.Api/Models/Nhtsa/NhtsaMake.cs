using System.Text.Json.Serialization;

namespace VehicleFinder.Api.Models.Nhtsa;

public class NhtsaMake
{
    [JsonPropertyName("Make_ID")]
    public int MakeId { get; set; }

    [JsonPropertyName("Make_Name")]
    public string? MakeName { get; set; }
}
