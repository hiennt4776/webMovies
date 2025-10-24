using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface IEmployeeRepository : IRepository<Employee>
{
    // Add custom methods for Entity here if needed
}

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(dbMoviesContext context) : base(context)
    {

    }

}
