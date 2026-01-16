using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace helperMovies.ViewModel
{
    public class CustomerRegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]

        [JsonPropertyName("phoneNumber")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required.")]
        [JsonPropertyName("dateOfBirth")]
        public DateOnly DateOfBirth { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [JsonPropertyName("confirmPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
    public class CustomerViewModel
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
}
