

using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface IContractRepository : IRepository<Contract>
{
    // Add custom methods for Entity here if needed
}

public class ContractRepository : Repository<Contract>, IContractRepository
{
    public ContractRepository(dbMoviesContext context) : base(context)
    {

    }
}