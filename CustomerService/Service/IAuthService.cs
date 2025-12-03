using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CustomerService.Service
{
    public interface IAuthService
    {
        int GetUserIdFromToken(HttpContext httpContext);
        string? GetUserNameFromToken(HttpContext httpContext);
    }

    public class AuthService : IAuthService
    {
        public int GetUserIdFromToken(HttpContext httpContext)
        {
            var claim = httpContext.User?.FindFirst("UserId");
            if (claim == null) throw new UnauthorizedAccessException("Token không có UserId");
            if (!int.TryParse(claim.Value, out int userId))
                throw new InvalidOperationException("UserId không hợp lệ trong token");
            return userId;
        }

        public string? GetUserNameFromToken(HttpContext httpContext)
        {
            return httpContext.User?.FindFirst("UserName")?.Value;
        }
    }
}
