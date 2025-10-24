using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace helperMovies.DTO
{
    public class PartnerDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("contactInfo")]
        public string ContactInfo { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("phone")]
        public string Phone { get; set; }
        [JsonPropertyName("address")]
        public string Address { get; set; }
        [JsonPropertyName("website")]
        public string Website { get; set; }
        [JsonPropertyName("taxCode")]
        public string TaxCode { get; set; }
        [JsonPropertyName("accountNumber")]
        public string AccountNumber { get; set; }
        [JsonPropertyName("bankName")]
        public string BankName { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }

    }
}
