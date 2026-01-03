using Blazored.LocalStorage;
using BlazorWebAppCustomer.Data;
using dbMovies.Models;
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
    public interface IInvoiceService
    {
        Task<int> CreateInvoiceAsync(int moviePricingId);
    }
    public class InvoiceService : IInvoiceService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiClient _apiClient;
        private readonly ApiSettings _settings;
        private readonly ILocalStorageService _localStorage;

        public InvoiceService(HttpClient httpClient, ILocalStorageService localStorage, ApiClient apiClient, IOptions<ApiSettings> settings)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _apiClient = apiClient;
            _settings = settings.Value;
        }

        public async Task<int> CreateInvoiceAsync(int moviePricingId)
        {
            var response = await _apiClient.PostJsonAsync(
                $"invoice/create",
                new CreateInvoiceRequestViewModel
                {
                    MoviePricingId = moviePricingId
                });

            var invoiceIdString = await response.Content.ReadAsStringAsync();
            return int.Parse(invoiceIdString);
        }
    }
}
