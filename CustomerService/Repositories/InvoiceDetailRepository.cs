using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
public interface IInvoiceDetailRepository : IRepository<InvoiceDetail>
{
    // Add custom methods for Entity here if needed
}

public class InvoiceDetailRepository : Repository<InvoiceDetail>, IInvoiceDetailRepository
{
    public InvoiceDetailRepository(dbMoviesContext context) : base(context)
    {

    }

}
