using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface ICategoryRepository : IRepository<Category>
{
    // Add custom methods for Entity here if needed
}

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(dbMoviesContext context) : base(context)
    {

    }
}