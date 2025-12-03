using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface ISubscriptionPackageRepository : IRepository<SubscriptionPackageRepository>
{
    // Add custom methods for Entity here if needed
}

public class SubscriptionPackageRepository : Repository<SubscriptionPackageRepository>, ISubscriptionPackageRepository
{
    public SubscriptionPackageRepository(dbMoviesContext context) : base(context)
    {
    }
}
