using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface ICustomerRepository : IRepository<Customer>
{
    // Add custom methods for Entity here if needed
}

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(dbMoviesContext context) : base(context)
    {

    }

}
