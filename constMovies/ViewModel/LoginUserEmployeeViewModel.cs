using System.ComponentModel.DataAnnotations;

namespace helperMovies.ViewModel
{


    public class LoginEmployeeRequestViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class LoginEmployeeResponseViewModel
    {
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
