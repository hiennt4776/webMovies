using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface IMovieFileRepository : IRepository<MovieFile>
{
    // Add custom methods for Entity here if needed
}

public class MovieFileRepository : Repository<MovieFile>,IMovieFileRepository
{
    public MovieFileRepository(dbMoviesContext context) : base(context)
    {
    }
}
