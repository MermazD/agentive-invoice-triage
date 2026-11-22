using System.Text.Json.Serialization;

public class Customer
{
    [JsonPropertyName("first_name")]
    public required string FirstName { get; set; }
    [JsonPropertyName("last_name")]
    public required string LastName { get; set; }
    [JsonPropertyName("address")]
    public required Address CustomerAddress { get; set; }

    public Customer ExtractInvoice(string ocrText)
    {
        var customer = new Customer
        {
            FirstName = FirstName,
            LastName = LastName,
            CustomerAddress = CustomerAddress
        };
        //customer.ExtractInvoice = ocrText;
        return customer;
    }
}