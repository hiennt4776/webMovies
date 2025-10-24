using AdminService.Services;
using dbMovies.Models;
using helperMovies.constMovies;
using helperMovies.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Intrinsics.Arm;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace AdminService.Service
{
    public interface IPartnerService
    {
        public Task<IEnumerable<PartnerDTO>> GetAllAsync();
        public Task<IEnumerable<PartnerDTO>> GetPartnerByStatus(string partnerStatusConstant);
        public Task<PagedResult<PartnerDTO>> GetPagedAsync(int pageNumber, int pageSize);
        public Task<PagedResult<PartnerDTO>> GetPagedSortSearchAsync(int pageNumber, int pageSize, string? search, string? sortColumn, bool ascending, string? status);
        public Task<PartnerDTO?> GetByIdAsync(int id);
        public Task<PartnerDTO> AddAsync(PartnerDTO dto);
        public Task<PartnerDTO?> UpdateAsync(int id, PartnerDTO dto);
        public Task<bool> DeleteAsync(int id);
    }
    public class PartnerService : IPartnerService
    {
        private readonly dbMoviesContext _context;
        private readonly IPartnerRepository _partnerRepository;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _dbu;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PartnerService( dbMoviesContext context, IPartnerRepository partnerRepository,
            IAuthService authService,
        IUnitOfWork dbu, JwtAuthService jwtAuthService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _partnerRepository = partnerRepository;
            _authService = authService;
            _dbu = dbu;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<PartnerDTO>> GetAllAsync()
        {
            return await _context.Partners
               .Where(p => p.IsDeleted == false)
               .OrderByDescending(p => p.CreatedDate)
               .Select(p => new PartnerDTO
               {
                   Id = p.Id,
                   Name = p.Name,
                   ContactInfo = p.ContactInfo,
                   Email = p.Email,
                   Phone = p.Phone,
                   Address = p.Address,
                   Website = p.Website,
                   TaxCode = p.TaxCode,
                   AccountNumber = p.AccountNumber,
                   BankName = p.BankName,
                   Status = p.Status

               })
               .ToListAsync();
        }

        public async Task<IEnumerable<PartnerDTO>> GetPartnerByStatus(string partnerStatusConstant)
        {
            return await _context.Partners
               .Where(p => p.IsDeleted == false && p.Status == partnerStatusConstant)
               .OrderByDescending(p => p.CreatedDate)
               .Select(p => new PartnerDTO
               {
                   Id = p.Id,
                   Name = p.Name,
                   ContactInfo = p.ContactInfo,
                   Email = p.Email,
                   Phone = p.Phone,
                   Address = p.Address,
                   Website = p.Website,
                   TaxCode = p.TaxCode,
                   AccountNumber = p.AccountNumber,
                   BankName = p.BankName,
                   Status = p.Status

               })
               .ToListAsync();
        }

        public async Task<PagedResult<PartnerDTO>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var baseQuery = _context.Partners
                .Where(c => c.IsDeleted == false);

            var totalCount = await baseQuery.CountAsync();

            var data = await baseQuery
                .OrderByDescending(p => p.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PartnerDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    ContactInfo = p.ContactInfo,
                    Email = p.Email,
                    Phone = p.Phone,
                    Address = p.Address,
                    Website = p.Website,
                    TaxCode = p.TaxCode,
                    AccountNumber = p.AccountNumber,
                    BankName = p.BankName,
                    Status = p.Status
                })
                .ToListAsync();

            return new PagedResult<PartnerDTO>
            {
                Items = data,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<PartnerDTO>> GetPagedSortSearchAsync(
            int pageNumber, int pageSize, string? search, string? sortColumn, bool ascending, string? status)
        {
            var query = _context.Partners.Where(p => p.IsDeleted == false);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || p.Email.Contains(search));

            if (!string.IsNullOrWhiteSpace(status) && status != "ALL")
                query = query.Where(p => p.Status == status);

            query = sortColumn switch
            {
                "Name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedDate)
            };

            var totalCount = await query.CountAsync();

            var data = await query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PartnerDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    ContactInfo = p.ContactInfo,
                    Email = p.Email,
                    Phone = p.Phone,
                    Address = p.Address,
                    Website = p.Website,
                    TaxCode = p.TaxCode,
                    AccountNumber = p.AccountNumber,
                    BankName = p.BankName,
                    Status = p.Status
                }).ToListAsync();

            return new PagedResult<PartnerDTO>
            {
                Items = data,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PartnerDTO?> GetByIdAsync(int id)
        {
            var p = await _partnerRepository.GetByIdAsync(id);
            if (p.IsDeleted== true) return null;

            return new PartnerDTO
            {
                Id = p.Id,
                Name = p.Name,
                ContactInfo = p.ContactInfo,
                Email = p.Email,
                Phone = p.Phone,
                Address = p.Address,
                Website = p.Website,
                TaxCode = p.TaxCode,
                AccountNumber = p.AccountNumber,
                BankName = p.BankName,
                Status = p.Status

            };
        }

        public async Task<PartnerDTO> AddAsync(PartnerDTO dto)
        {
            var httpContext = _httpContextAccessor.HttpContext
           ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);


            var partner = new Partner
            {
                Name = dto.Name,
                ContactInfo = dto.ContactInfo,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Website = dto.Website,
                TaxCode = dto.TaxCode,
                AccountNumber = dto.AccountNumber,
                BankName = dto.BankName,
                Status = dto.Status ?? PartnerStatusConstant.Active,
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                IsDeleted = false,
            };

            await _partnerRepository.AddAsync(partner);
            await _dbu.SaveChangesAsync();

            dto.Id = partner.Id;
          
            return dto;
        }

        public async Task<PartnerDTO?> UpdateAsync(int id, PartnerDTO dto)
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);
            var partner = await _partnerRepository.GetByIdAsync(id);
            if (partner == null) return null;

            partner.Name = dto.Name;
            partner.ContactInfo = dto.ContactInfo;
            partner.Email = dto.Email;
            partner.Phone = dto.Phone;
            partner.Address = dto.Address;
            partner.Website = dto.Website;
            partner.TaxCode = dto.TaxCode;
            partner.AccountNumber = dto.AccountNumber;
            partner.BankName = dto.BankName;
            partner.Status = dto.Status;
            partner.UpdatedBy = userId;
            partner.UpdatedDate = DateTime.Now;

            await _partnerRepository.Update(partner);
            await _dbu.SaveChangesAsync();

            partner.UpdatedDate = partner.UpdatedDate;
            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);



            var partner = await _partnerRepository.GetByIdAsync(id);
            if (partner == null) return false;

            partner.IsDeleted = true;
            partner.UpdatedBy = userId;
            partner.UpdatedDate = DateTime.Now;

            await _partnerRepository.Update(partner);
            await _dbu.SaveChangesAsync();
            return true;
        }
    }
}