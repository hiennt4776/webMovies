

using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface IPartnerRepository : IRepository<Partner>
{
    // Add custom methods for Entity here if needed
}

public class PartnerRepository : Repository<Partner>, IPartnerRepository
{
    public PartnerRepository(dbMoviesContext context) : base(context)
    {

    }
}