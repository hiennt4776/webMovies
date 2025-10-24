using Blazored.LocalStorage;
using dbMovies.Models;
using helperMovies.ViewModel;
using Microsoft.JSInterop;
using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace BlazorWebAppAdmin.Services
{

    public interface IEmployeeService
    {
        Task<PagedResult<EmployeeViewModel>> GetPagedSearchSortAsync(
                int pageNumber, int pageSize, string? search = null, string? sortColumn = null, string? sortOrder = null);
        Task<PagedResult<EmployeeViewModel>> GetEmployeesAsync(EmployeeFilterViewModel filter);
        Task<EmployeeViewModel> GetByIdAsync(int id);
        Task<EmployeeViewModel> AddEmployeeAsync(EmployeeCreateViewModel dto);
        Task<EmployeeUpdateViewModel> UpdateEmployeeAsync(int id, EmployeeViewModel dto);
        Task<bool> DeleteAsync(int id);
        Task<List<EmployeeViewModel>> GetAllEmployeesAsync();
    }
    public class EmployeeService : IEmployeeService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly IUserEmployeeService _userEmployeeService;
        private readonly ApiClient _apiClient;

        public EmployeeService(HttpClient httpClient, ApiClient apiClient, ILocalStorageService localStorage, IUserEmployeeService userEmployeeService)
        {
            _httpClient = httpClient;
            _apiClient = apiClient;
            _localStorage = localStorage;
            _userEmployeeService = userEmployeeService;
        }

        public async Task<PagedResult<EmployeeViewModel>> GetPagedSearchSortAsync(
            int pageNumber, int pageSize, string? search = null, string? sortColumn = null, string? sortOrder = null)
        {
            string url = $"Employee/paged?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrWhiteSpace(sortColumn))
                url += $"&sortColumn={Uri.EscapeDataString(sortColumn)}";
            if (!string.IsNullOrWhiteSpace(sortOrder))
                url += $"&sortOrder={Uri.EscapeDataString(sortOrder)}";

            return await _httpClient.GetFromJsonAsync<PagedResult<EmployeeViewModel>>(url);
        }

        public async Task<PagedResult<EmployeeViewModel>> GetEmployeesAsync(EmployeeFilterViewModel filter)
        {

            var query = $"Employee/filter?Keyword={filter.Keyword}&JobStatus={filter.JobStatus}&PageNumber={filter.PageNumber}&PageSize={filter.PageSize}";

            // Gọi qua ApiClient
            var response = await _apiClient.GetAsync(query);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Lỗi khi gọi API: {response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<PagedResult<EmployeeViewModel>>();
            if (result == null)
                throw new Exception("Không thể đọc dữ liệu trả về từ API.");

            return result;

            //var response = await _apiClient.GetFromJsonAsync<PagedResult<EmployeeViewModel>>($"Employee/filter{query}");
            //return response!;
        }
        public async Task<EmployeeViewModel> GetByIdAsync(int id)
        {
            string url = $"Employee/get/{id}";
            var response = await _apiClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<EmployeeViewModel>();
        }



        public async Task<EmployeeViewModel> AddEmployeeAsync(EmployeeCreateViewModel dto)
        {
            var response = await _apiClient.PostJsonAsync("Employee/create", dto);
            return await response.Content.ReadFromJsonAsync<EmployeeViewModel>();
        }
        public async Task<EmployeeUpdateViewModel> UpdateEmployeeAsync(int id, EmployeeViewModel dto)
        {
            var response = await _apiClient.PutJsonAsync($"Employee/update/{id}", dto);
            return await response.Content.ReadFromJsonAsync<EmployeeUpdateViewModel>();
 
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Employee/delete/{id}");
            return response.IsSuccessStatusCode;
        }


        public async Task<List<EmployeeViewModel>> GetAllEmployeesAsync()
        {
   

            string url = $"Employee/getAllEmployees";
            var response = await _apiClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<EmployeeViewModel>();

            return await response.Content.ReadFromJsonAsync<List<EmployeeViewModel>>() ?? new List<EmployeeViewModel>();

        }


    }
}
