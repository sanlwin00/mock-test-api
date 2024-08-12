using System.Text.Json.Serialization;

namespace MockTestApi.Models
{
    public class StripeRequestDto
    {
        [JsonPropertyName("sessionId")]
        public string? SessionId { get; set; }

        [JsonPropertyName("sessionUrl")]
        public string? SessionUrl { get; set; }

        [JsonPropertyName("approvedUrl")]
        public string ApprovedUrl { get; set; }

        [JsonPropertyName("cancelUrl")]
        public string CancelUrl { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("product")]
        public Product Product { get; set; }

    }

    public class Product
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("qty")]
        public int Qty { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }
    }
}
