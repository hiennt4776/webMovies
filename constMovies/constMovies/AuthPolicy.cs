namespace helperMovies.constMovies
{
    public class AuthPolicy
    {
        public static readonly string RoleBasedPolicy = "RoleBasedPolicy";
    }
    public class JWTClaims
    {
        public static readonly string USERNAME = "Username";
        public static readonly string ROLES = "Roles";
    }
    public class UserRoleValue
    {
        public static readonly string ADMIN = "ADMIN";
        public static readonly string MEMBER = "MEMBER";
    }
}
