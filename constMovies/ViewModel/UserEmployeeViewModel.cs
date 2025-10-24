
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace helperMovies.ViewModel
{
    public class UserEmployeeViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("employee")]
        public EmployeeViewModel? Employee { get; set; }

        [JsonPropertyName("role")]
        public RoleViewModel Role { get; set; }

        [JsonPropertyName("isLocked")]
        public bool? IsLocked { get; set; }

        [JsonPropertyName("reasonLock")]
        public string ReasonLock { get; set; }
    }

    public class UserEmployeeCreateViewModel
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("employeeId")]
        public int EmployeeId { get; set; }

        [JsonPropertyName("roleId")]
        public int RoleId { get; set; }

        [JsonPropertyName("isLocked")]
        public bool IsLocked { get; set; }

        [JsonPropertyName("reasonLock")]
        public string? ReasonLock { get; set; }
    }


    public class UserEmployeeEditViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        //[JsonPropertyName("password")]
        //public string? Password { get; set; }

        [JsonPropertyName("roleId")]
        public int RoleId { get; set; }

        [JsonPropertyName("isLocked")]
        public bool IsLocked { get; set; }

        [JsonPropertyName("reasonLock")]
        public string? ReasonLock { get; set; }
    }

    public class UserEmployeeResetPasswordViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("roleName")]
        public string? RoleName { get; set; }

        [JsonPropertyName("isLocked")]
        public bool IsLocked { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }
    }


    public class ChangePasswordViewModel
    {
        public int UserId { get; set; }
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
