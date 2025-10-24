using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace helperMovies.ViewModel
{
    public class PartnerViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("contactInfo")]
        public string ContactInfo { get; set; }

        [JsonPropertyName("email")]
        [EmailAddress(ErrorMessage = "Invalid email.")]
        public string Email { get; set; }


        [Phone(ErrorMessage = "Invalid phone number.")]
        [JsonPropertyName("phone")]
        public string Phone { get; set; }
        [JsonPropertyName("address")]
        public string Address { get; set; }


        [Url(ErrorMessage = "Invalid Website.")]
        [JsonPropertyName("website")]
        public string Website { get; set; }
        [JsonPropertyName("taxCode")]
        public string TaxCode { get; set; }
        [JsonPropertyName("accountNumber")]
        public string AccountNumber { get; set; }
        [JsonPropertyName("bankName")]
        public string BankName { get; set; }

        [RegularExpression("ACTIVE|INACTIVE|SUSPENDED", ErrorMessage = "Invalid Status.")]
        [JsonPropertyName("status")]
        public string Status { get; set; }

    }
}
