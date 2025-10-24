using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface IUserEmployeeRepository : IRepository<UserEmployee>
{
    // Add custom methods for Entity here if needed
}

public class UserEmployeeRepository : Repository<UserEmployee>, IUserEmployeeRepository
{
    public UserEmployeeRepository(dbMoviesContext context) : base(context)
    {

    }

}
