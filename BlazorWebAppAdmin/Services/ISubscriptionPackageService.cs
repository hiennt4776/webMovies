using Blazored.LocalStorage;
using BlazorWebAppAdmin.Services;
using helperMovies.ViewModel;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace BlazorWebAppAdmin.Services
{
    public interface ISubscriptionPackageService
    {
    }
    public class SubscriptionPackageService : ISubscriptionPackageService
    {
    
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly IUserEmployeeService _userService;
        private readonly ApiClient _apiClient;
        public SubscriptionPackageService(HttpClient httpClient, ApiClient apiClient, ILocalStorageService localStorage, IUserEmployeeService userService)
        {
            _httpClient = httpClient;
            _apiClient = apiClient;
            _localStorage = localStorage;
            _userService = userService;
        }
        // -------------------------
        // GET Paged + Search
        // -------------------------
        public async Task<PagedResult<SubscriptionPackageViewModel>> GetPagedAsync(
            string? search, int page, int pageSize)
        {
            var query = new Dictionary<string, string>()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString()
            };

            if (!string.IsNullOrWhiteSpace(search))
                query["search"] = search;

            var url = QueryHelpers.AddQueryString("SubscriptionPackage", query);

            var response = await _apiClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PagedResult<SubscriptionPackageViewModel>>();
        }

        // -------------------------
        // GET BY ID
        // -------------------------
        public async Task<SubscriptionPackageViewModel?> GetByIdAsync(int id)
        {
            var resp = await _apiClient.GetAsync($"SubscriptionPackage/{id}");
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadFromJsonAsync<SubscriptionPackageViewModel>();
        }

        // -------------------------
        // CREATE
        // -------------------------
        public async Task CreateAsync(SubscriptionPackageViewModel dto)
        {
            var resp = await _apiClient.PostJsonAsync("SubscriptionPackage", dto);
            resp.EnsureSuccessStatusCode();
        }

        // -------------------------
        // UPDATE
        // -------------------------
        public async Task UpdateAsync(int id, SubscriptionPackageViewModel dto)
        {
            var resp = await _apiClient.PutJsonAsync($"SubscriptionPackage/{id}", dto);
            resp.EnsureSuccessStatusCode();
        }

        // -------------------------
        // DELETE
        // -------------------------
        public async Task DeleteAsync(int id)
        {
            var resp = await _apiClient.DeleteAsync($"SubscriptionPackage/{id}");
            resp.EnsureSuccessStatusCode();
        }
    }

}
