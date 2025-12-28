using Azure;
using Blazored.LocalStorage;
using BlazorWebAppCustomer.Data;
using BlazorWebAppCustomer.Services;
using dbMovies.Models;
using helperMovies.DTO;
using helperMovies.ViewModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;


namespace BlazorWebAppCustomer.Services
{
    public interface ISubscriptionPackageService
    {
        Task<List<SubscriptionPackageViewModel>> GetAllAsync();
        Task<int> PurchaseAsync(int packageId);
    }

    public class SubscriptionPackageService : ISubscriptionPackageService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiClient _apiClient;
        private readonly ApiSettings _settings;
        private readonly ILocalStorageService _localStorage;



        public SubscriptionPackageService(HttpClient httpClient, ApiClient apiClient, ILocalStorageService localStorage, IOptions<ApiSettings> settings)

        {
            _httpClient = httpClient;
            _apiClient = apiClient;
            _localStorage = localStorage;

            _settings = settings.Value;
        }


        public async Task<List<SubscriptionPackageViewModel>> GetAllAsync()
        {


            var url = $"{_settings.BaseUrl}SubscriptionPackage/active";
            var response = await _httpClient.GetFromJsonAsync<List<SubscriptionPackageViewModel>>(url) ?? new List<SubscriptionPackageViewModel>();

            return response;
        }

        public async Task<int> PurchaseAsync(int packageId)
        {
            var url = $"{_settings.BaseUrl}SubscriptionPackage/purchase/package?packageId={packageId}";
            var response = await _apiClient.PostJsonAsync(url, new { });
            var invoiceId = await response.Content.ReadFromJsonAsync<int>();
            if (invoiceId == 0)
                throw new Exception("Không tạo được hóa đơn");

            return invoiceId;

        }



    }

}