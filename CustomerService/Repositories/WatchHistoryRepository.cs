using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface IWatchHistoryRepository : IRepository<WatchHistory>
{
    // Add custom methods for Entity here if needed
}

public class WatchHistoryRepository : Repository<WatchHistory>, IWatchHistoryRepository
{
    public WatchHistoryRepository(dbMoviesContext context) : base(context)
    {

    }
}