using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CustomerService.Service
{
    public interface IAuthService
    {
        int GetUserIdFromToken(HttpContext httpContext);
        string? GetUserNameFromToken(HttpContext httpContext);
        //int GetUserIdFromToken();
        //string? GetUserNameFromToken();
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

    //public class AuthService : IAuthService
    //{
    //    private readonly IHttpContextAccessor _httpContextAccessor;

    //    public AuthService(IHttpContextAccessor httpContextAccessor)
    //    {
    //        _httpContextAccessor = httpContextAccessor;
    //    }

    //    public int GetUserIdFromToken()
    //    {
    //        var httpContext = _httpContextAccessor.HttpContext;
    //        if (httpContext == null)
    //            throw new InvalidOperationException("HttpContext is null. Method must be called in an HTTP request.");

    //        var claim = httpContext.User.FindFirst("UserId");
    //        if (claim == null)
    //            throw new UnauthorizedAccessException("Token không có UserId");

    //        if (!int.TryParse(claim.Value, out int userId))
    //            throw new InvalidOperationException("UserId không hợp lệ trong token");

    //        return userId;
    //    }

    //    public string? GetUserNameFromToken()
    //    {
    //        return _httpContextAccessor.HttpContext?
    //            .User?
    //            .FindFirst("UserName")?
    //            .Value;
    //    }
    //}
}
