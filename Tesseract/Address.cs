using System.Text.Json.Serialization;

public class Address
{
    [JsonPropertyName("street")]
    public required string Street { get; set; } 
    [JsonPropertyName("postal_code")]
    public required string PostalCode { get; set; } 
    [JsonPropertyName("city")]
    public required string City { get; set; } 
    [JsonPropertyName("country")]
    public required string Country { get; set; }
}