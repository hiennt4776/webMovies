using dbMovies.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public interface IUnitOfWork : IAsyncDisposable
{

    Task BeginTransactionAsync();
    Task CommitTransactionAsync();

    Task<int> SaveChangesAsync();
    Task<int> SaveChanges();

    Task  RollBackTransactionAsync();


}

public class UnitOfWork: IUnitOfWork
{

    private readonly dbMoviesContext _context;
    

    public UnitOfWork(dbMoviesContext context)
    {
        _context = context;

    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    public async Task<int> SaveChanges()
    {
        return  _context.SaveChanges();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _context.Database.CommitTransactionAsync();

    }

    public async Task RollBackTransactionAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }

}

