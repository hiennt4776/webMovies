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
using System.Text.Json.Serialization;


namespace AdminService.Service
{

  
    public interface IContractService
    {
      
        Task<IEnumerable<Contract>> GetAllAsync();
        Task<ContractCreateDTO> CreateContractAsync(ContractCreateDTO dto);
        Task<ContractViewDTO> UpdateContractAsync(ContractEditDTO dto);
        Task AddContractDetailMovieAsync(ContractDetailMovie detail);
        Task<ContractViewDTO> GetByIdAsync(int id);
        public Task<PagedResult<ContractViewDTO>> GetPagedSortSearchAsync(int pageNumber, int pageSize, string? search, string? sortField, bool ascending);
        public Task<bool> DeleteAsync(int id);
        //Task<(byte[] FileBytes, string FileName)> DownloadFileAsync(int id);
        public Task<(byte[] FileBytes, string FileName, string ContentType)?> DownloadFileWithMetaAsync(int id);
    }

    public class ContractService : IContractService
    {
        private readonly dbMoviesContext _context;
        private readonly IContractRepository _contractRepository;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _dbu;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMovieService _movieService;
        private readonly IFileService _fileService;
        private readonly string _fileBasePath;
        private readonly IConfiguration _config;



        public ContractService(dbMoviesContext context, IContractRepository contractRepository,
                        IAuthService authService,
        IUnitOfWork dbu, JwtAuthService jwtAuthService, IHttpContextAccessor httpContextAccessor
            , IMovieService movieService, IFileService fileService, IOptions<FileSettings> settings,
         IConfiguration _config
            )
        {
            _context = context;
            _contractRepository = contractRepository;
            _authService = authService;
            _dbu = dbu;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
            _movieService = movieService;
            _fileService = fileService;
            _fileBasePath = _config["FileSettings:FilesPath"]!;
        }

        public async Task<ContractCreateDTO> CreateContractAsync(ContractCreateDTO contractCreateDTO)
        {
            var httpContext = _httpContextAccessor.HttpContext
             ?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);

            if (contractCreateDTO == null || contractCreateDTO.MovieInContract == null || contractCreateDTO.MovieInContract.Count == 0)
                throw new ArgumentNullException(nameof(contractCreateDTO));

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                decimal totalValue = contractCreateDTO.MovieInContract.Sum(m => m.Price);
                var contract = new Contract
                {
                    PartnerId = contractCreateDTO.PartnerId,
                    ContractNumber = contractCreateDTO.ContractNumber,
                    ContractDate = contractCreateDTO.ContractDate,
                    TotalValue = totalValue,
                    PaymentTerms = contractCreateDTO.PaymentTerms,
                    Notes = contractCreateDTO.Notes,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId,
                    IsDeleted = false
                };

                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();

                foreach (var movieDto in contractCreateDTO.MovieInContract)
                {
                    var movie = await _movieService.CreateMovieAsync(movieDto);

                    var detail = new ContractDetailMovie
                    {
                        Contract = contract,
                        MovieId = movie.Id,
                        Price = movieDto.Price,
                        Notes = movieDto.Notes,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userId,
                        IsDeleted = false
                    };

                    await AddContractDetailMovieAsync(detail);

                }
                if (contractCreateDTO.ContractFile != null)
                {


                    FileDTO fileDTO =  await _fileService.SaveFileAsync(contractCreateDTO.ContractFile, $"{CubDirectoryFileConstant.CONTRACT}\\{contract.Id}");
    
                    var fileRecord = new ContractFile
                    {
                        ContractId = contract.Id,
                        FileName = fileDTO.OriginalFileName,
                        FilePath = fileDTO.FullPath,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        IsDeleted = false
                    };
                    await AddContractDetailMovieAsync(fileRecord);

                }



                await transaction.CommitAsync();
                return contractCreateDTO;
            }
            catch (Exception ex) {
                await transaction.RollbackAsync();
                throw;
            }

    
        
        }
        public async Task AddContractDetailMovieAsync(ContractFile contractFile)
        {
            _context.ContractFiles.Add(contractFile);
            await _context.SaveChangesAsync();
        }
        public async Task AddContractDetailMovieAsync(ContractDetailMovie detail)
        {
            _context.ContractDetailMovies.Add(detail);
            await _context.SaveChangesAsync();
        }
        public async Task<ContractViewDTO> UpdateContractAsync(ContractEditDTO dto)
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);

            var contract = await _context.Contracts
                .Include(c => c.ContractDetailMovies)
                .FirstOrDefaultAsync(c => c.Id == dto.Id);

            if (contract == null)
                throw new Exception("Không tìm thấy hợp đồng.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            // Cập nhật thông tin chung
            contract.PartnerId = dto.PartnerId;
            contract.ContractNumber = dto.ContractNumber;
            contract.ContractDate = dto.ContractDate;
            contract.PaymentTerms = dto.PaymentTerms;
            contract.Notes = dto.Notes;
            contract.UpdatedDate = DateTime.Now;
            contract.UpdatedBy = userId;

            // Xóa mềm phim cũ (update IsDeleted = true)
            var oldDetails = await _context.ContractDetailMovies
                .Where(d => d.ContractId == contract.Id && d.IsDeleted == false)
                .ToListAsync();

            foreach (var item in oldDetails)
            {
                item.IsDeleted = true;
                item.UpdatedDate = DateTime.Now;
                item.UpdatedBy = userId; // nếu có thông tin user đăng nhập
            }

            _context.ContractDetailMovies.UpdateRange(oldDetails);
            await _context.SaveChangesAsync();

            await SoftDeleteAllMoviesAndDetailsAsync(contract.Id, userId);

            await AddNewMoviesAndDetailsAsync(contract.Id, dto.MovieInContract, userId);

            // 🟧 Cập nhật lại tổng giá trị hợp đồng
            contract.TotalValue = await _context.ContractDetailMovies
                .Where(d => d.ContractId == contract.Id && d.IsDeleted == false)
                .SumAsync(d => d.Price);

            //Nếu có file hợp đồng mới
            if (dto.ContractFile != null && dto.ContractFile.Length > 0)
            {
                // ✅ Xóa mềm file hợp đồng cũ
                var oldFiles = await _context.ContractFiles
                    .Where(f => f.ContractId == contract.Id && f.IsDeleted == false)
                    .ToListAsync();

                foreach (var f in oldFiles)
                {
                    f.IsDeleted = true;
                    f.UpdatedDate = DateTime.Now;
                    f.UpdatedBy = userId;
                }

                FileDTO fileDTO = await _fileService.SaveFileAsync(dto.ContractFile, $"{CubDirectoryFileConstant.CONTRACT}\\{contract.Id}");
                var file = new ContractFile
                {
                    ContractId = contract.Id,
                    FileName = fileDTO.OriginalFileName,
                    FilePath = fileDTO.FullPath,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };
                _context.ContractFiles.Add(file);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new ContractViewDTO
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber,
                ContractDate = contract.ContractDate,
                TotalValue = contract.TotalValue,
                Partner = new PartnerDTO
                {
                    Id = contract.PartnerId.Value,
                    Name = await _context.Partners
                 .Where(p => p.Id == contract.PartnerId.Value)
                 .Select(p => p.Name)
                 .FirstOrDefaultAsync()
                },
                PaymentTerms = contract.PaymentTerms,
                Notes = contract.Notes
            };
        }

        private async Task SoftDeleteAllMoviesAndDetailsAsync(int contractId, int userID)
        {
            var now = DateTime.Now;

            // 🔹 Lấy danh sách chi tiết cũ + MovieId liên quan
            var oldDetails = await _context.ContractDetailMovies
                .Where(d => d.ContractId == contractId && d.IsDeleted == false)
                .ToListAsync();

            if (!oldDetails.Any())
                return;

            var movieIds = oldDetails
                .Where(d => d.MovieId.HasValue)
                .Select(d => d.MovieId!.Value)
                .Distinct()
                .ToList();

            // 🔹 Xóa mềm chi tiết hợp đồng
            foreach (var detail in oldDetails)
            {
                detail.IsDeleted = true;
                detail.UpdatedBy = userID;
                detail.UpdatedDate = now;
            }

            _context.ContractDetailMovies.UpdateRange(oldDetails);

            // 🔹 Xóa mềm phim liên quan
            if (movieIds.Count > 0)
            {
                var movies = await _context.Movies
                    .Where(m => movieIds.Contains(m.Id) && m.IsDeleted == false)
                    .ToListAsync();

                foreach (var movie in movies)
                {
                    movie.IsDeleted = true;
                    movie.UpdatedBy = userID;
                    movie.UpdatedDate = now;
                }

                _context.Movies.UpdateRange(movies);
            }

            await _context.SaveChangesAsync();
        }

        private async Task AddNewMoviesAndDetailsAsync(int contractId, List<MovieInContractDTO> movies, int userID)
        {
            if (movies == null || !movies.Any())
                return;

            var details = new List<ContractDetailMovie>();

            foreach (var m in movies)
            {
                // Thêm phim mới
                var movie = new Movie
                {
                    Title = m.Title,
                    CreatedBy = userID,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                await _context.Movies.AddAsync(movie);
                await _context.SaveChangesAsync(); // cần để có Movie.Id

                // Tạo chi tiết hợp đồng mới
                details.Add(new ContractDetailMovie
                {
                    ContractId = contractId,
                    MovieId = movie.Id,
                    Price = m.Price,
                    Notes = m.Notes,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userID,
                    IsDeleted = false
                });
            }

            await _context.ContractDetailMovies.AddRangeAsync(details);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Contract>> GetAllAsync()
        {
            return await _context.Contracts
                .Include(c => c.ContractDetailMovies)
                    .ThenInclude(d => d.Movie)
                .Include(c => c.ContractFiles)
                .ToListAsync();
        }
        public async Task<PagedResult<ContractViewDTO>> GetPagedSortSearchAsync(int pageNumber,int pageSize, string? search,string? sortField,bool ascending)
        {
            var query = _context.Contracts
                .Include(c => c.Partner)
                .Where(c => c.IsDeleted ==false && c.Partner.IsDeleted == false);

            // 🔍 Tìm kiếm theo số hợp đồng hoặc tên đối tác
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.ContractNumber.Contains(search) ||
                    c.Partner.Name.Contains(search));
            }

            // 🔽 Sắp xếp động
            query = sortField switch
            {
                "contractNumber" => ascending
                    ? query.OrderBy(c => c.ContractNumber)
                    : query.OrderByDescending(c => c.ContractNumber),

                "contractDate" => ascending
                    ? query.OrderBy(c => c.ContractDate)
                    : query.OrderByDescending(c => c.ContractDate),

                "totalValue" => ascending
                    ? query.OrderBy(c => c.TotalValue)
                    : query.OrderByDescending(c => c.TotalValue),

                "partnerName" => ascending
                    ? query.OrderBy(c => c.Partner.Name)
                    : query.OrderByDescending(c => c.Partner.Name),

                _ => query.OrderByDescending(c => c.Id) // mặc định
            };

            // 📄 Tổng số bản ghi
            var totalCount = await query.CountAsync();

            // 📄 Phân trang
            var contracts = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ContractViewDTO
                {
                    Id = c.Id,
                    ContractNumber = c.ContractNumber,
                    ContractDate = c.ContractDate,
                    TotalValue = c.TotalValue,
                    Partner = new PartnerDTO
                    {
                        Id = c.Partner.Id,
                        Name = c.Partner.Name,
                        Email = c.Partner.Email,
                        Phone = c.Partner.Phone
                    },
                    PaymentTerms = c.PaymentTerms,
                    Notes = c.Notes
                })
                .ToListAsync();

            return new PagedResult<ContractViewDTO>
            {
                Items = contracts,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ContractViewDTO> GetByIdAsync(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Partner)
                .Include(c => c.ContractDetailMovies).ThenInclude(d => d.Movie)
                .Include(c => c.ContractFiles)
                .Where(c=>c.Id == id && c.IsDeleted == false)
                .Select(c => new ContractViewDTO
                {
                    Id = c.Id,
                    ContractNumber = c.ContractNumber,
                    ContractDate = c.ContractDate,
                    TotalValue = c.TotalValue,
                    PaymentTerms = c.PaymentTerms,
                    Notes = c.Notes,

                    Partner = new PartnerDTO
                    {
                        Id = c.Partner.Id,
                        Name = c.Partner.Name
                    },

                    MovieInContract = c.ContractDetailMovies
                        .Where(d => d.IsDeleted == false)
                        .Select(d => new MovieInContractDTO
                        {
                            Title = d.Movie.Title,
                            Price = (decimal)d.Price,
                            Notes = d.Notes
                        }).ToList(),

                    ContractFilePath = c.ContractFiles
                        .Where(f => f.IsDeleted == false)
                        .Select(f => f.FilePath)
                        .FirstOrDefault(),

                    ContractFileName = c.ContractFiles
                        .Where(f => f.IsDeleted == false)
                        .Select(f => f.FileName)
                        .FirstOrDefault()
                })
            
                .FirstOrDefaultAsync();

            return contract;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var httpContext = _httpContextAccessor.HttpContext
     ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);



            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null) return false;

            contract.IsDeleted = true;
            contract.UpdatedBy = userId;
            contract.UpdatedDate = DateTime.Now;

            await _contractRepository.Update(contract);

            // Xóa mềm phim cũ (update IsDeleted = true)
            var oldDetails = await _context.ContractDetailMovies
                .Where(d => d.ContractId == contract.Id && d.IsDeleted == false)
                .ToListAsync();

            foreach (var item in oldDetails)
            {
                item.IsDeleted = true;
                item.UpdatedDate = DateTime.Now;
                item.UpdatedBy = userId; // nếu có thông tin user đăng nhập
            }

            _context.ContractDetailMovies.UpdateRange(oldDetails);
          

            await SoftDeleteAllMoviesAndDetailsAsync(contract.Id, userId);

            await _dbu.SaveChangesAsync();
            return true;
        }

        public async Task<(byte[] FileBytes, string FileName, string ContentType)?> DownloadFileWithMetaAsync(int id)
        {

            var contract = await _context.Contracts
                 .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);
            var contractFiles = await _context.ContractFiles.FirstOrDefaultAsync(c => c.ContractId == id && c.IsDeleted == false);

        
            if (contract == null)
                throw new FileNotFoundException("Không tìm thấy hợp đồng.");
           

            var fullPath = Path.Combine(_fileBasePath, contractFiles.FilePath);

            if (!File.Exists(fullPath))
                return null;
            if (string.IsNullOrEmpty(fullPath))
                throw new FileNotFoundException("Hợp đồng này không có tệp đính kèm.");

            if (!System.IO.File.Exists(fullPath))
                throw new FileNotFoundException("Tệp không tồn tại trong hệ thống.");

            var bytes = await File.ReadAllBytesAsync(fullPath);
            var fileName = contractFiles.FileName ?? Path.GetFileName(contractFiles.FilePath);
            var contentType = GetContentType(fileName);

            return (bytes, fileName, contentType);
        }


        // Helper xác định kiểu MIME theo phần đuôi
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

    }

}

