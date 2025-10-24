using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface IRoleRepository : IRepository<Role>
{
    // Add custom methods for Entity here if needed
}

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(dbMoviesContext context) : base(context)
    {

    }
}