using Azure;
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

    public interface IFavoriteMovieService
    {
        Task<bool> ToggleFavoriteAsync(int movieId);
        Task<bool> IsFavoriteAsync(int movieId);
    }

    public class FavoriteMovieService : IFavoriteMovieService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly ApiClient _apiClient;
        private readonly ApiSettings _settings;
        public FavoriteMovieService(HttpClient httpClient, ApiClient apiClient, ILocalStorageService localStorage,  IOptions<ApiSettings> settings)
        
        {
            _httpClient = httpClient;
            _apiClient = apiClient;
            _localStorage = localStorage;

            _settings = settings.Value;
        }



        public Task<bool> ToggleFavoriteAsync(int movieId)
        {
            return _apiClient.PostAsync<bool>($"favorite/{movieId}");
        }

        public Task<bool> IsFavoriteAsync(int movieId)
        {
            return _apiClient.GetAsync<bool>($"favorite/{movieId}");
        }

    }
}