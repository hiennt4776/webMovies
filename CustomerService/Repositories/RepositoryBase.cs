
using dbMovies.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task Update(T entity);
    Task DeleteAsync(int id);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate);
}

public class Repository<T> : IRepository<T> where T : class

{
    protected readonly dbMoviesContext _context;
    protected readonly DbSet<T> _dbSet;


    public Repository(dbMoviesContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();

    }

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();


    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);


    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }


    public async Task Update(T entity)
    {
         _dbSet.Update(entity);
    }



    public async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is not null)
        {
            _dbSet.Remove(entity);
        }
    }

    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)  => await _dbSet.AsNoTracking().SingleOrDefaultAsync(predicate);


    public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate)  => await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

}
