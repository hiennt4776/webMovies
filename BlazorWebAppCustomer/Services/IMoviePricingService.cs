using Blazored.LocalStorage;
using BlazorWebAppCustomer.Data;
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

    public interface IMoviePricingService
    {
        Task<MoviePricingViewModel?> GetRentPricingAsync(int movieId);
        Task<MoviePricingViewModel?> GetBuyPricingAsync(int movieId);
        Task BuyMovieAsync(int movieId);
        Task RentMovieAsync(int pricingId);
        Task<List<MoviePricingViewModel>> GetRentPricesAsync(int movieId);
        Task<MoviePricingViewModel?> GetBuyPriceAsync(int movieId);
    }
    public class MoviePricingService : IMoviePricingService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiClient _apiClient;
        private readonly ApiSettings _settings;
        private readonly ILocalStorageService _localStorage;
        public MoviePricingService(HttpClient httpClient, ILocalStorageService localStorage, ApiClient apiClient, IOptions<ApiSettings> settings)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _apiClient = apiClient;
            _settings = settings.Value;
        }
        public async Task<MoviePricingViewModel?> GetRentPricingAsync(int movieId)
        {
            return await _apiClient.GetAsync<MoviePricingViewModel>(
                $"movie-pricing/{movieId}?type=RENT");
        }

        public async Task<MoviePricingViewModel?> GetBuyPricingAsync(int movieId)
        {
            return await _apiClient.GetAsync<MoviePricingViewModel>(
                $"movie-pricing/{movieId}?type=BUY");
        }

        public async Task BuyMovieAsync(int movieId)
        {
            await _apiClient.PostFormAsync(
                $"movie-transactions/buy/{movieId}", null);
        }


        public async Task RentMovieAsync(int pricingId)
        {
            await _apiClient.PostFormAsync(
                $"movie-transactions/rent/{pricingId}", null);
        }

        public async Task<List<MoviePricingViewModel>> GetRentPricesAsync(int movieId)
        {
            return await _apiClient.GetAsync<List<MoviePricingViewModel>>(
                $"MoviePricing/rent/{movieId}")
                ?? new List<MoviePricingViewModel>();
        }

        public async Task<MoviePricingViewModel?> GetBuyPriceAsync(int movieId)
        {
            return await _apiClient.GetAsync<MoviePricingViewModel>(
                $"MoviePricing/buy/{movieId}");
        }


    }
}
