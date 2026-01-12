using Azure;
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
    public interface ICustomerService
    {
        Task<PagedResult<CustomerViewModel>> GetCustomersAsync(
              string? keyword,
              int pageIndex,
              int pageSize);
    }

    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly ApiClient _apiClient;

        public CustomerService(HttpClient httpClient, ApiClient apiClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _apiClient = apiClient;
            _localStorage = localStorage;
        }

        public async Task<PagedResult<CustomerViewModel>> GetCustomersAsync(
              string? keyword,
              int pageIndex,
              int pageSize)
        {
            var url =
                $"Customer/getPageSearchCustomer?keyword={keyword}&pageIndex={pageIndex}&pageSize={pageSize}";

            //return await _apiClient.GetAsync<PagedResult<CustomerViewModel>>(url)
            //       ?? new();

            var response = await _apiClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Lỗi khi gọi API: {response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<PagedResult<CustomerViewModel>>();
            if (result == null)
                throw new Exception("Không thể đọc dữ liệu trả về từ API.");

            return result;
        }
     
    }

}
