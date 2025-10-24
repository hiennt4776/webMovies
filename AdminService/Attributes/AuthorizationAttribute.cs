using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using helperMovies.constMovies;
using helperMovies.constMovies;
using System.Net;

namespace AdminService.Attributes
{
    public class AuthorizationAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        // OR condition
        private readonly string? _requiredRoles;
        public AuthorizationAttribute(string policy, string? requiredRoles) : base(policy)
        {
            _requiredRoles = requiredRoles;
        }
        public AuthorizationAttribute(string policy) : base(policy)
        {
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user == null || user.Identity == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            var roles = user.Claims.Where(c => c.Type == JWTClaims.ROLES)
                .Select(c => c.Value)
                .ToArray();
            var isValid = _requiredRoles == null || _requiredRoles.Split(",").Any(r => roles.Contains(r));
            if (!isValid)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
