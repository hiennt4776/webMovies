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
    public interface IWatchHistoryClientService
    {
        Task<decimal> GetWatchTimeAsync(int movieId);
        Task SaveWatchTimeAsync(int movieId, decimal time);
    }

    public class WatchHistoryClientService : IWatchHistoryClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiClient _apiClient;
        private readonly ApiSettings _settings;
        private readonly ILocalStorageService _localStorage;

        public WatchHistoryClientService(HttpClient httpClient, ILocalStorageService localStorage, ApiClient apiClient, IOptions<ApiSettings> settings)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _apiClient = apiClient;
            _settings = settings.Value;
        }

        public async Task<decimal> GetWatchTimeAsync(int movieId)
        {
            var response = await _apiClient.GetAsync($"watch/{movieId}");

            if (!response.IsSuccessStatusCode)
                return 0;

            return await response.Content.ReadFromJsonAsync<decimal>();
        }

        public async Task SaveWatchTimeAsync(int movieId, decimal time)
        {
            var watchHistoryViewModel = new WatchHistoryViewModel
            {
                MovieId = movieId,
                CurrentTime = time
            };

            await _apiClient.PostJsonAsync($"watch/update", watchHistoryViewModel);
        }
    }
}
