using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace helperMovies.DTO
{
    public class CustomerRegisterDTO
    {
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("phoneNumber")]
        public string Phone { get; set; } = string.Empty;
        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;
        [JsonPropertyName("dateOfBirth")]
        public DateOnly DateOfBirth { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
        [JsonPropertyName("confirmPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class CustomerDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("fullName")]
        public string FullName { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("phoneNumber")]
        public string Phone { get; set; }
        [JsonPropertyName("address")]
        public string Address { get; set; }
        [JsonPropertyName("dateOfBirth")]
        public DateOnly? DateOfBirth { get; set; }


        public string Username { get; set; }
        public bool? IsLocked { get; set; }
        public string ReasonLock { get; set; }

        public DateTime? CreatedDate { get; set; }
    }

    public class UserProfileDTO
    {
        public int UserCustomerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsLocked { get; set; }
    }
}
