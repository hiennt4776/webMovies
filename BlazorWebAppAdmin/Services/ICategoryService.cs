using Blazored.LocalStorage;
using dbMovies.Models;
using helperMovies.ViewModel;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace BlazorWebAppAdmin.Services
{

    public interface ICategoryService
    {
        public Task<PagedResult<CategoryViewModel>> GetPagedAsync(int pageIndex, int pageSize);
        public Task<PagedResult<CategoryViewModel>> GetPagedSearchSortAsync(
           int pageNumber,
           int pageSize,
           string? search,
           string? sortColumn,
           bool ascending);
        public Task<CategoryViewModel> GetByIdAsync(int id);
        public Task<CategoryViewModel> AddAsync(CategoryViewModel dto);
        public Task<CategoryViewModel> UpdateAsync(int id, CategoryViewModel dto);
        public Task<bool> DeleteAsync(int id);


    }

    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly IUserEmployeeService _userService;
        private readonly ApiClient _apiClient;
        public CategoryService(HttpClient httpClient, ApiClient apiClient, ILocalStorageService localStorage, IUserEmployeeService userService)
        {
            _httpClient = httpClient;
            _apiClient = apiClient;
            _localStorage = localStorage;
            _userService = userService;
        }

        // Lấy danh sách theo trang
        public async Task<PagedResult<CategoryViewModel>> GetPagedAsync(int pageNumber, int pageSize)
        {
            string url = $"Category/getPageCategories?pageNumber={pageNumber}&pageSize={pageSize}";

            var response = await _apiClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error: {response.StatusCode}");

            return await response.Content.ReadFromJsonAsync<PagedResult<CategoryViewModel>>();
        }

        public async Task<PagedResult<CategoryViewModel>> GetPagedSearchSortAsync(
                int pageNumber, int pageSize, string? search, string? sortColumn, bool ascending)
        { 
            await _userService.AddAuthHeaderAsync(); 
            var query = new List<string> { $"Category/getPageSortSearchCategories?pageNumber={pageNumber}", $"pageSize={pageSize}", $"ascending={ascending.ToString().ToLower()}" }; 
            if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}"); 
            if (!string.IsNullOrWhiteSpace(sortColumn)) query.Add($"sortColumn={Uri.EscapeDataString(sortColumn)}"); 
            string url = $"{string.Join("&", query)}"; 
            // Gọi ApiClient thay cho _httpClient
            var response = await _apiClient.GetAsync(url); 
            if (!response.IsSuccessStatusCode) throw new Exception($"Error: {response.StatusCode}");
            // Deserialize JSON về object
            var result = await response.Content.ReadFromJsonAsync<PagedResult<CategoryViewModel>>();
            return result; 
        }


        public async Task<CategoryViewModel> GetByIdAsync(int id)
        {
            string url = $"Category/get/{id}";
            var response = await _apiClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CategoryViewModel>();
        }
        // Thêm mới
        public async Task<CategoryViewModel> AddAsync(CategoryViewModel dto)
        {
            string url = "Category/create";
            var response = await _apiClient.PostJsonAsync(url, dto);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CategoryViewModel>();
        }

        // Cập nhật
        public async Task<CategoryViewModel> UpdateAsync(int id, CategoryViewModel dto)
        {
            string url = $"Category/update/{id}";
            var response = await _apiClient.PutJsonAsync(url, dto);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CategoryViewModel>();
        }

        // Xóa
        public async Task<bool> DeleteAsync(int id)
        {
            string url = $"Category/delete/{id}";
            var response = await _apiClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }

    }
}