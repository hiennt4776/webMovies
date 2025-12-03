using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface IMoviePricingRepository : IRepository<MovieFile>
{
    // Add custom methods for Entity here if needed
}

public class MoviePricingRepository : Repository<MovieFile>,IMoviePricingRepository
{
    public MoviePricingRepository(dbMoviesContext context) : base(context)
    {
    }
}
