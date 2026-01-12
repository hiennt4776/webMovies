using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface IInvoiceRepository : IRepository<Invoice>
{
    // Add custom methods for Entity here if needed
}

public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(dbMoviesContext context) : base(context)
    {

    }

}
