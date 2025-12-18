using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface ISubscriptionPackageRepository : IRepository<SubscriptionPackage>
{
    // Add custom methods for Entity here if needed
}

public class SubscriptionPackageRepository : Repository<SubscriptionPackage>, ISubscriptionPackageRepository
{
    public SubscriptionPackageRepository(dbMoviesContext context) : base(context)
    {
    }
}
