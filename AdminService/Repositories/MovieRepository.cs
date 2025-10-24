using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
public interface IMovieRepository : IRepository<Movie>
{
    // Add custom methods for Entity here if needed
}

public class MovieRepository : Repository<Movie>, IMovieRepository
{
    public MovieRepository(dbMoviesContext context) : base(context)
    {

    }



}
