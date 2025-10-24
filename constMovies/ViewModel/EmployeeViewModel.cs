using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace helperMovies.ViewModel
{
    public class EmployeeViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("fullName")]
        public string FullName { get; set; }
        [JsonPropertyName("gender")]
        public bool? Gender { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }
        [JsonPropertyName("address")]
        public string Address { get; set; }
        [JsonPropertyName("jobStatus")]
        public string JobStatus { get; set; }
        [JsonPropertyName("dateOfBirth")]
        public DateOnly? DateOfBirth { get; set; }
        [JsonPropertyName("salary")]
        public decimal? Salary { get; set; }
    }

    public class EmployeeCreateViewModel
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("roleId")]
        public int? RoleId { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; }
        [JsonPropertyName("gender")]
        public bool? Gender { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }
        [JsonPropertyName("address")]
        public string Address { get; set; }
        [JsonPropertyName("jobStatus")]
        public string JobStatus { get; set; }
        [JsonPropertyName("dateOfBirth")]
        public DateOnly? DateOfBirth { get; set; }
        [JsonPropertyName("salary")]
        public decimal? Salary { get; set; }
    }

    public class EmployeeUpdateViewModel
    
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; }

        [JsonPropertyName("gender")]
        public bool? Gender { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("phoneNumber")]

        public string PhoneNumber { get; set; }
        [JsonPropertyName("jobStatus")]
        public string JobStatus { get; set; }
        [JsonPropertyName("dateOfBirth")]
        public DateOnly? DateOfBirth { get; set; }
        [JsonPropertyName("salary")]
        public decimal? Salary { get; set; }


    }

    public class EmployeeUpdateByUserIdViewModel
    
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }


        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }
        [JsonPropertyName("address")]
        public string Address { get; set; }

    }

    public class EmployeeFilterViewModel
    {
        public string? Keyword { get; set; }
        public string? JobStatus { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
