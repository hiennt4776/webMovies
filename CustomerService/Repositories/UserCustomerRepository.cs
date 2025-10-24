using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface IUserCustomerRepository : IRepository<UserCustomer>
{
    // Add custom methods for Entity here if needed
}

public class UserCustomerRepository : Repository<UserCustomer>, IUserCustomerRepository
{
    public UserCustomerRepository(dbMoviesContext context) : base(context)
    {

    }

}
