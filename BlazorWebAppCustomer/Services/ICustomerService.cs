using Blazored.LocalStorage;
using dbMovies.Models;
using helperMovies.DTO;
using helperMovies.ViewModel;
using Microsoft.JSInterop;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace BlazorWebAppCustomer.Services
{

    public interface ICustomerService
    {
        Task<List<FavoriteMovieViewModel>> GetFavoritesAsync();
        Task<List<InvoiceViewModel>> GetInvoicesAsync();
        Task<CustomerViewModel> GetCurrentUserNameAsync();

        Task<CustomerViewModel> GetProfileAsync();

    }
    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly IUserCustomerService _userCustomerService;
        private readonly ApiClient _apiClient;

        public CustomerService(HttpClient httpClient, ApiClient apiClient, ILocalStorageService localStorage, IUserCustomerService userCustomerService)
        {
            _httpClient = httpClient;
            _apiClient = apiClient;
            _localStorage = localStorage;
            _userCustomerService = userCustomerService;
        }


        public async Task<CustomerViewModel> GetCurrentUserNameAsync()
        {
            // Lấy token đúng cách
            var token = await _localStorage.GetItemAsync<string>("token");

            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("Please Login");

            // Xóa dấu " nếu nó bị bao
            token = token.Trim('"');

            // Gắn token vào header
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            Console.WriteLine("==== [CLIENT DEBUG HTTP REQUEST] ====");
            Console.WriteLine($"BaseAddress: {_httpClient.BaseAddress}");
            Console.WriteLine($"Request URL: {_httpClient.BaseAddress}UserEmployee/profile");
            Console.WriteLine($"Authorization: {_httpClient.DefaultRequestHeaders.Authorization}");

            Console.WriteLine("====================================");

            var response = await _httpClient.GetAsync("Customer/profile");

            if (response.IsSuccessStatusCode)
            {
                var pf = await response.Content.ReadFromJsonAsync<CustomerViewModel>();
                if (pf is null) throw new Exception("Không đọc được dữ liệu người dùng.");
                return pf;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn hoặc token không hợp lệ.");

            if (response.StatusCode == HttpStatusCode.Forbidden)
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập tài nguyên này.");

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new Exception("Không tìm thấy hồ sơ người dùng.");

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CustomerViewModel>();
        }

        public async Task<CustomerViewModel> GetProfileAsync()
        {
            return await _apiClient.GetAsync<CustomerViewModel>("customer/profile");
        }


        public async Task<List<InvoiceViewModel>> GetInvoicesAsync()
        {
            return await _apiClient.GetAsync<List<InvoiceViewModel>>("customer/invoices")
                   ?? new List<InvoiceViewModel>();
        }

        public async Task<List<FavoriteMovieViewModel>> GetFavoritesAsync()
        {
            return await _apiClient.GetAsync<List<FavoriteMovieViewModel>>("customer/favorites")
                   ?? new List<FavoriteMovieViewModel>();
        }



    }
}
