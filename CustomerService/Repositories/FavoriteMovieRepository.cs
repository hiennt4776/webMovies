using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface IFavoriteMovieRepository : IRepository<FavoriteMovie>
{
    // Add custom methods for Entity here if needed
}

public class  FavoriteMovieRepository : Repository<FavoriteMovie>, IFavoriteMovieRepository
{
    public  FavoriteMovieRepository(dbMoviesContext context) : base(context)
    {

    }

}
