using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface IRoleUserRepository : IRepository<RoleUser>
{
    // Add custom methods for Entity here if needed
}

public class IRoleUsersService : Repository<RoleUser>, IRoleUserRepository
{
    public IRoleUsersService(dbMoviesContext context) : base(context)
    {

    }
}