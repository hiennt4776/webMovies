using AdminService.Services;
using AdminService.Utils;
using Azure.Core;
using dbMovies.Models;
using helperMovies.constMovies;
using helperMovies.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;


namespace AdminService.Service
{
    public interface ICustomerService
    {
        Task<PagedResult<CustomerDTO>> GetCustomersAsync(
           string? keyword,
           int pageIndex,
           int pageSize);
    }
    public class CustomerService : ICustomerService
    {
        private readonly dbMoviesContext _context;
        public CustomerService(dbMoviesContext context)
        {
            _context = context;
        }
        public async Task<PagedResult<CustomerDTO>> GetCustomersAsync(
            string? keyword,
            int pageIndex,
            int pageSize)
        {
            var query =
                from c in _context.Customers
                join u in _context.UserCustomers
                    on c.Id equals u.CustomerId into cu
                from u in cu.DefaultIfEmpty()
                select new CustomerDTO
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    Email = c.Email,
                    Phone = c.Phone,
                    DateOfBirth = c.DateOfBirth,
                    CreatedDate = c.CreatedDate,
                    Username = u.Username,
                    IsLocked = u.IsLocked,
                    ReasonLock = u.ReasonLock
                };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.FullName.Contains(keyword) ||
                    x.Email.Contains(keyword) ||
                    x.Phone.Contains(keyword) ||
                    x.Username.Contains(keyword));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<CustomerDTO>
            {
                Items = items,
                TotalCount = totalItems,
                PageNumber = pageIndex,
                PageSize = pageSize
            };
        }
    }
}